/////////////////// Data Collection of Video sequences and framestamped IMU and GPS readings ///////////////////////////////////
//
// Flea3Recorder class.cs: This is the class that corresponds to the video recording thread.
// It also uses a HighPrecisionTimer in order to poll the IMU at 0.07 s (150 Hz) betwen frame capturing events
// Besides the recording thread, the class uses a "Dumping" thread in order to asynchronously record frames.

// Copyright (C) 2012 George Terzakis
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;

using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using System.Diagnostics;


using FlyCapture2Managed;


namespace GPSVideoRecorder
{
    
    class Flea3Recorder
    {

        public const int MAX_FRAMEQUEUE_LEN = 20;

        public ManagedCamera Cam;
        public ManagedBusManager BusMgr;

        public List<ManagedPGRGuid> CamIDs;

        public GPSReceiver GpsReceiver;

        public IMUCommsDevice IMUcomms;

        public ManagedAVIRecorder AviRecorder;
        
        // a display bitmap object
        public System.Windows.Forms.PictureBox DisplayPicture;


        // The process main loop thread
        public Thread RecordingThread; 
        
        // NMEA thread active flag
        public Boolean RecordingThreadActive;

        // a flag that enforces recording to a file
        public Boolean RecordToFile;

        // a flag to indicate whether gravity was found (along the y axis)
        private Boolean Flag_GravityFound_Y;

        // a queue of frames...
        public ManagedImageRollingBuffer FrameQueue;
        
        
        // the thread that dumps the content of the queue into the file
        public Thread DumpingThread;

        // The IMU sampling PRECISION Timer
        //public Thread IMUSamplingThread;
        public PrecisionTimer IMUSamplingTimer;
            
        // GPS data storage
        public List<GPSDataInstance> GpsCaptureData;

        // IMU data storage
        public List<IMUDataInstance> IMUCapturedata;


        // frame index
        public int FrameIndex;

        
        // a flag for in/off-thread operation
        public Boolean OutOfRecordingThread, OutOfDumpingThread;

       // A timer used to sample the IMU during the intervals betweem frame captures
        //private System.Timers.Timer IMUSamplingTimer;


        public Flea3Recorder(GPSReceiver gpsReceiver, IMUCommsDevice imu)
        {
            int i;

            IMUcomms = imu;

            

            // 1. creating the camera object
            Cam = new ManagedCamera();
            // 2. creating the bus manager in order to handle (potentially) 
            // multiple cameras
            BusMgr = new ManagedBusManager();

            // 3. retrieving the List of all camera ids connected to the bus
            CamIDs = new List<ManagedPGRGuid>();
            int camCount = (int)BusMgr.GetNumOfCameras();
            for (i = 0; i < camCount; i++)
            {
                ManagedPGRGuid guid = BusMgr.GetCameraFromIndex((uint)i);
                CamIDs.Add(guid);
            }
            
            // 4. assigning values to properties
            GpsReceiver = gpsReceiver;

            // 5. init flags
            RecordingThreadActive = false;

            RecordToFile = false;

            OutOfRecordingThread = OutOfDumpingThread = true;

            // 6. Creating the Frame data queue
            FrameQueue = new ManagedImageRollingBuffer(MAX_FRAMEQUEUE_LEN);

            
            
        }


        // print the camera info on the console
        public void printCameraInfo()
        {
            if (Cam.IsConnected())
            {
                CameraInfo camInfo = Cam.GetCameraInfo();

                StringBuilder newStr = new StringBuilder();
                newStr.Append("\n*** CAMERA INFORMATION ***\n");
                newStr.AppendFormat("Serial number - {0}\n", camInfo.serialNumber);
                newStr.AppendFormat("Camera model - {0}\n", camInfo.modelName);
                newStr.AppendFormat("Camera vendor - {0}\n", camInfo.vendorName);
                newStr.AppendFormat("Sensor - {0}\n", camInfo.sensorInfo);
                newStr.AppendFormat("Resolution - {0}\n", camInfo.sensorResolution);

                Console.WriteLine(newStr);
            }
        }

        


        // start capturing
        public void startCapture(ManagedPGRGuid camGuid, int vidMode,System.Windows.Forms.PictureBox displayPicture, 
                                 String fileName,Boolean record2file)
        {
            int i;

            Flag_GravityFound_Y = false; // garvity is not known

            // CLEARING THE FRAME QUEUE NO MATTER WHAT...
            FrameQueue.clear();


            RecordToFile = record2file;
            
            // creating the GPS data list
            GpsCaptureData = new List<GPSDataInstance>();
            // creating the IMU data List
            IMUCapturedata = new List<IMUDataInstance>();

            // resetting frame index
            FrameIndex = 0;

            // 1. connect to the camera 
            Cam.Connect(camGuid);

            int fps_i = 0;
            if (vidMode == 0)
            {
                Cam.SetVideoModeAndFrameRate(VideoMode.VideoMode1600x1200Yuv422, FrameRate.FrameRate30);
                fps_i = 30;
            }
            else if (vidMode == 1) {
                Cam.SetVideoModeAndFrameRate(VideoMode.VideoMode1600x1200Rgb, FrameRate.FrameRate15);
                fps_i = 15;
                }
            else if (vidMode == 2)
            {
                Format7ImageSettings fset = new Format7ImageSettings();
                fset.height = 540;
                fset.width = 960;
                fset.offsetX = 40;
                fset.offsetY = 118;
                fset.mode = Mode.Mode4;

                fset.pixelFormat = PixelFormat.PixelFormatRgb8;

                Cam.SetFormat7Configuration(fset, 40.0f); // this equivalent to 24 fps
                
                fps_i = 24;
            }
            

            if (RecordToFile)
            {
                // 3. Creating the avi recorder object
                AviRecorder = new ManagedAVIRecorder();

                MJPGOption option = new MJPGOption();

                float fps = (float)fps_i;

                option.frameRate = fps;
                option.quality = 100;  // 100 for superb quality
                AviRecorder.AVIOpen(fileName, option);
            }


            // 4. setting the frame buffering option
            // leave it for now...


            // 5. start the capturing
            Cam.StartCapture();

            // MUST discard the first few frames!
            ManagedImage rawImage = new ManagedImage();
            for (i = 0; i < 10;  i++)
            {
                Cam.RetrieveBuffer(rawImage);
            }

            // 6. set the display bitmap 
            DisplayPicture = displayPicture;

            // 7. starting sampling, recording and dumping threads
            

            // IMU sampling thread
            IMUSamplingTimer = new PrecisionTimer(.0075, this.IMUSamplingEvent); // sampling frequency at 150 Hz

            RecordingThreadActive = true;
            OutOfRecordingThread = true;

            IMUSamplingTimer.start();
            RecordingThread = new Thread(this.mainLoop);
            //RecordingThread.Priority = ThreadPriority.Highest;
            RecordingThread.Start();
            
            
            // creating the thread for the dumping
            DumpingThread = new System.Threading.Thread(this.dumpingLoop);

            while (OutOfRecordingThread); // must wait until the recording thread enters the loop, otherwise the dumping will never start!

            DumpingThread.Start();
            
            
        }


        
        // the recording thread main loop
        public void mainLoop()
        {

            OutOfRecordingThread = false;
            ManagedImage rawImage = new ManagedImage();
            ManagedImage convertedImage = new ManagedImage();
            GPSDataInstance newGpsData;

            

            while (RecordingThreadActive)
            {

                // 1. retrieving a frame from the buffer

                if ((Flag_GravityFound_Y && (IMUcomms != null)) || (IMUcomms == null)) // record only if gravity is found or if there is no IMU
                {

                    FrameQueue.add(Cam);
                    //Cam.RetrieveBuffer(rawImage);

                    // increasing frame index
                    FrameIndex++;

                    //tempImage = new ManagedImage(rawImage);

                    //FrameQueue.add(tempImage);

                    //FrameQueue.Buffer[FrameQueue.last].Convert(PixelFormat.PixelFormatBgr, convertedImage);
                    //System.Drawing.Bitmap bitmap = convertedImage.bitmap;
                    //DisplayPicture.Image = (Image)convertedImage.bitmap;


                    // adding gps data in the GPS data list
                    if (GpsReceiver != null)
                    {

                        newGpsData = new GPSDataInstance(FrameIndex, 
                                                         GpsReceiver.LatitudeAsDecimal,
                                                         GpsReceiver.LongitudeAsDecimal,
                                                         GpsReceiver.SpeedOverGround,
                                                         GpsReceiver.CourseOverGround,
                                                         GpsReceiver.MRCStatus);
                        GpsCaptureData.Add(newGpsData);
                    }

                }

            }

            DisplayPicture.Image = null;
            OutOfRecordingThread = true;
        }



        public void IMUSamplingEvent(Object s, PrecisionTimerEventArgs e)
        {
            if (IMUcomms != null)
                if (IMUcomms.Flag_CommsActive)
                {
                    Matrix<float> imudata = new Matrix<float>(1, 6);
                    imudata.Data[0, 0] = IMUcomms.Last_Acc1; imudata.Data[0, 1] = IMUcomms.Last_Acc2; imudata.Data[0, 2] = IMUcomms.Last_Acc3;
                    imudata.Data[0, 3] = IMUcomms.Last_Angvel1; imudata.Data[0, 4] = IMUcomms.Last_Angvel2; imudata.Data[0, 5] = IMUcomms.Last_Angvel3;

                    if (!Flag_GravityFound_Y) // check for gravity along the Y axis
                        Flag_GravityFound_Y = (Math.Abs(imudata.Data[0, 0] - 1.0) < 0.001);
                    if (Flag_GravityFound_Y) // record the data   
                    {
                        IMUDataInstance newIMUData = new IMUDataInstance(FrameIndex, 
                                                                         IMUcomms.Last_Acc1, IMUcomms.Last_Acc2, IMUcomms.Last_Acc3, 
                                                                         IMUcomms.Last_Angvel1, IMUcomms.Last_Angvel2, IMUcomms.Last_Angvel3);
                        IMUCapturedata.Add(newIMUData);
                    }


                }

        }

        
        // the dumping thread loop
        public void dumpingLoop()
        {
            OutOfDumpingThread = false;
            ManagedImage convertedImage = new ManagedImage();
            ManagedImage mgdimg;

            while (!OutOfRecordingThread || (!FrameQueue.isEmpty()))
            {

                // recording the image straight into the file
                if (!FrameQueue.isEmpty())
                {
                     mgdimg = FrameQueue.remove();

                    // displying the image (via the DisplayBitmap)
                    // Convert the raw image
                    mgdimg.Convert(PixelFormat.PixelFormatBgr, convertedImage);

                    DisplayPicture.Image = (Image)convertedImage.bitmap;

                    if (RecordToFile)
                        AviRecorder.AVIAppend(mgdimg);
                    
                }


            }
            // displying the image (via the DisplayBitmap)
            // Convert the raw image
            //ManagedImage convertedImage = new ManagedImage();
            //FrameQueue[0].Convert(PixelFormat.PixelFormatBgr, convertedImage);
            //System.Drawing.Bitmap bitmap = convertedImage.bitmap;

            //Image<Bgr, Byte> img = new Image<Bgr, byte>(bitmap);


            //DisplayPicture.Image = (Image)bitmap;









            //if (RecordToFile) AviRecorder.AVIClose();

            //DisplayPicture.Image = null;
            OutOfDumpingThread = true;

        }


        // deprecated...
        public void stopCapturing()
        {
            RecordingThreadActive = false;
            
            // wait until the dumping thread has finished
            while (!OutOfRecordingThread);

            //1.  kill trhe recording thread
            RecordingThread.Abort();

            // 2. kill the IMU sampling thread
            IMUSamplingTimer.stop();


            while (!OutOfDumpingThread);

            
            // now kill the dumping thread as well
            DumpingThread.Abort();

            RecordingThreadActive = false;

            this.DisplayPicture = null;

            
            //2. stop capturing
            Cam.StopCapture();

            Cam.Disconnect();

            if (RecordToFile)
            {
                //2. close the file
                AviRecorder.AVIClose();
            }

        }


    }

}
