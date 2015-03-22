/////////////////// Data Collection of Video sequences and framestamped IMU and GPS readings ///////////////////////////////////
//
// Flea3Calibrator class: An OpenCV based calibration class specifically for the Pointgrey Flea3 USB3 Camera

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
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.UI;
using System.Threading;
using System.Drawing;
using System.Windows.Forms;

using FlyCapture2Managed;


namespace GPSVideoRecorder
{
    class Flea3Calibrator
    {
        // ******************** Flea3 vamera related properties *****************************
        // The camera
        public ManagedCamera Cam;
        // Bus manager
        public ManagedBusManager BusMgr;
        // List of detected camera ids
        public List<ManagedPGRGuid> CamIDs;
        // **********************************************************************************

        // state constants
        public const int ST_IDLE = 0;
        public const int ST_CALIBRATING = 1;

        // some constants for default configuration of the calibration
        public const int CHESS_HORIZ_CORNER_COUNT = 8;
        public const int CHESS_VERT_CORNER_COUNT = 5;

        // rectangle widfth and height
        public const float RECT_WIDTH = 3.0f;
        public const float RECT_HEIGHT = 3.0f;

        public const int FRAME_COUNT = 30;
        // constants ends


        public Thread CalibrationThread;

        // state of the process
        public int state;

        // flag 
        public Boolean CalibrationDone;

        // number of frames in the calibration 
        public int FrameCount;
        // the frame counter
        public int FrameCounter;

        // chessboard dimension
        public int ChessHorizCount, ChessVertCount;
        // square size
        public float RectWidth, RectHeight;

        // The detected points in all frames
        public PointF[][] Points;

        // The locations of the points on the calibration sheet (local 2D coordinate origin)
        public MCvPoint3D32f[][] ObjectPoints;

        // the extrinsics and intrinsics of the camera
        public ExtrinsicCameraParameters[] Extrinsics;
        public IntrinsicCameraParameters Intrinsics;

        // An image to draw the chessboard corners...
        public Image<Gray, Byte> DrawImage;


        // an image viewer
        //public ImageViewer imageViewer;

        // another drawing canvas
        public PictureBox DisplayBox;

        // The video mode (either 1600x1200 or 800x600 cropped)
        public int VidMode;


        // calibrator deafult constructor
        public Flea3Calibrator(PictureBox displaybox)
        {
            DisplayBox = displaybox;
             // 1. creating the camera object
            Cam = new ManagedCamera();
            // 2. creating the bus manager in order to handle (potentially) 
            // multiple cameras
            BusMgr = new ManagedBusManager();

            // 3. retrieving the List of all camera ids connected to the bus
            CamIDs = new List<ManagedPGRGuid>();
            int camCount = (int)BusMgr.GetNumOfCameras();
            for (int i = 0; i < camCount; i++)
            {
                ManagedPGRGuid guid = BusMgr.GetCameraFromIndex((uint)i);
                CamIDs.Add(guid);
            }
            

            FrameCount = FRAME_COUNT;
            ChessHorizCount = CHESS_HORIZ_CORNER_COUNT;
            ChessVertCount = CHESS_VERT_CORNER_COUNT;

            RectWidth = RECT_WIDTH;
            RectHeight = RECT_HEIGHT;

            // creatring the imageViewer to display the calibration frame sequence
            //imageViewer = new ImageViewer();
            

            state = ST_IDLE;

        }


        // calibrator constructor#2
        public Flea3Calibrator(int horiz_corner_count, int vert_corner_count, 
                                        float rect_width, float rect_height, int frame_count, PictureBox displaybox)
        {
            DisplayBox = displaybox;
            // 1. creating the camera object
            Cam = new ManagedCamera();
            // 2. creating the bus manager in order to handle (potentially) 
            // multiple cameras
            BusMgr = new ManagedBusManager();

            // 3. retrieving the List of all camera ids connected to the bus
            CamIDs = new List<ManagedPGRGuid>();
            int camCount = (int)BusMgr.GetNumOfCameras();
            for (int i = 0; i < camCount; i++)
            {
                ManagedPGRGuid guid = BusMgr.GetCameraFromIndex((uint)i);
                CamIDs.Add(guid);
            }
            

            FrameCount = frame_count;
            ChessHorizCount = horiz_corner_count;
            ChessVertCount = vert_corner_count;

            RectWidth = rect_width;
            RectHeight = rect_height;

            // creatring the imageViewer to display the calibration frame sequence
            //imageViewer = new ImageViewer();


            state = ST_IDLE;

        }


        public void startCalibration(int vidmode, ManagedPGRGuid camGuid)
        {
            int i, j;

            VidMode = vidmode;

            // Starting the camera
            // 1. connect to the camera 
            Cam.Connect(camGuid);
            
            // 2. setting up the video mode
            if (VidMode == 0)
            {
                Cam.SetVideoModeAndFrameRate(VideoMode.VideoMode1600x1200Yuv422, FrameRate.FrameRate30);
                
            }
            else if (VidMode == 1)
            {
                Cam.SetVideoModeAndFrameRate(VideoMode.VideoMode1600x1200Rgb, FrameRate.FrameRate15);
                
            }
            else if (VidMode == 2)
            {
                Format7ImageSettings fset = new Format7ImageSettings();
                fset.height = 540;
                fset.width = 960;
                fset.offsetX = 40;
                fset.offsetY = 118;
                fset.mode = Mode.Mode4;

                fset.pixelFormat = PixelFormat.PixelFormatRgb8;

                Cam.SetFormat7Configuration(fset, 40.0f); // this equivalent to 24 fps

            }



            // creating the thread
            CalibrationThread = new Thread(MainLoop);

            // zeroing the index of frame counter
            FrameCounter = 0;

            // creating the point detected storage array
            Points  = new PointF[FrameCount][];
            ObjectPoints = new MCvPoint3D32f[FrameCount][];

            // showing the image viewer
            //imageViewer.Show();

            // clearing flag
            CalibrationDone = false;

            for (i=0; i<FrameCount; i++) {
                ObjectPoints[i] = new MCvPoint3D32f[ChessHorizCount * ChessVertCount];
                for (j=0; j<ChessVertCount * ChessHorizCount; j++) {
                    ObjectPoints[i][j].x = (float)(RectWidth * ( j % ChessHorizCount ));
                    ObjectPoints[i][j].y = (float)(RectHeight * ( j / ChessHorizCount));
                    ObjectPoints[i][j].z = 0;                
                }
            }

            // starting the camera capture 
            Cam.StartCapture();

            state = ST_CALIBRATING;

            CalibrationThread.Start();
        }


        public void MainLoop()
        {
            // Managed Image MUST BE OUT OF THE LOOP! (For some reason...)
            ManagedImage rawImage = new ManagedImage();
            ManagedImage convertedImage = new ManagedImage();
            //System.Drawing.Bitmap bitmap;


            while (state == ST_CALIBRATING)
            {

                // retrieving an image using the Flea3 API
                Cam.RetrieveBuffer(rawImage);


                // Convert the raw image to a System.Drawing.Bitmap                
                rawImage.Convert(PixelFormat.PixelFormatBgr, convertedImage);
                System.Drawing.Bitmap bitmap = convertedImage.bitmap;

                // Suimply create a new openCV frame with the bitmap
                Image<Bgr, Byte> frame = new Image<Bgr, byte>(bitmap);

                Image<Gray, Byte> grayFrame = frame.Convert<Gray, Byte>();


                // and creating the drawImage frame

                DrawImage = grayFrame.Clone();


                // decalring array of points for all RGB components
                PointF[][] corners = new PointF[3][];

                // finding corners in the frame
                // left frame

                // bool result = CameraCalibration.FindChessboardCorners(grayFrame,
                //                                                      new Size(ChessHorizCount, ChessVertCount), Emgu.CV.CvEnum.CALIB_CB_TYPE.FAST_CHECK, out corners[0]);

                corners[0] = CameraCalibration.FindChessboardCorners(grayFrame,
                                                                     new Size(ChessHorizCount, ChessVertCount), Emgu.CV.CvEnum.CALIB_CB_TYPE.FAST_CHECK);
                                                                      //Emgu.CV.CvEnum.CALIB_CB_TYPE.ADAPTIVE_THRESH | Emgu.CV.CvEnum.CALIB_CB_TYPE.NORMALIZE_IMAGE
                                                                      //);

                bool result = !(corners[0] == null);
                


                if (result)
                {
                    FrameCounter++;
                    //finding corners with sub pixel accuracy
                    grayFrame.FindCornerSubPix(corners, new Size(10, 10), new Size(-1, -1), new MCvTermCriteria(0.01));
                    // now draing the corners                    

                    /* CameraCalibration.DrawChessboardCorners(DrawImage,
                                                            new Size(ChessHorizCount, ChessVertCount),
                                                            corners[0], 
                                                            true
                                                            ); */
                    CameraCalibration.DrawChessboardCorners(DrawImage,
                                                            new Size(ChessHorizCount, ChessVertCount),
                                                            corners[0]
                                                            );
                    
                    // adding the detected points to the list
                    Points[FrameCounter - 1] = corners[0];

                   
                    
                }

                

                // assiging the image to the imageviewer (so that it shows)
                //imageViewer.Image = DrawImage;
                DisplayBox.Image = (Image)DrawImage.Bitmap;
                
                
                if (FrameCounter >= FrameCount)
                {
                    state = ST_IDLE;

                    calibrate();

                    Console.WriteLine("Calibration now is complete. You may NOW kill the thread!");
                }
            }
        }

        
        // the function that calibrates
        public void calibrate()
        {
            //Extrinsics = new ExtrinsicCameraParameters();
            Intrinsics = new IntrinsicCameraParameters();

            //int width = (int)Cap.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_WIDTH);
            //int height = (int)Cap.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT);

            int width = ((VidMode == 0) || (VidMode == 1)) ? 1600 : 800;
            int height = ((VidMode == 0) || (VidMode == 1)) ? 1200 : 600;
            


            CameraCalibration.CalibrateCamera(ObjectPoints, Points, new Size(width, height), Intrinsics,
                                              Emgu.CV.CvEnum.CALIB_TYPE.DEFAULT, out Extrinsics);
        
            // clearing the display 
            DisplayBox.Image = null;
            // raising a flag
            CalibrationDone = true;


        }


        // the following suspends the thread, resets the flag and destroys the capture and imageviewer
        public void stopCalibration()
        {
            CalibrationThread.Abort();

            state = ST_IDLE;

            CalibrationDone = false;

            //imageViewer.Close();
            //imageViewer.Dispose();

            Cam.StopCapture();
            Cam.Disconnect();

            //CalibrationThread.Abort();
        }

    }
}
