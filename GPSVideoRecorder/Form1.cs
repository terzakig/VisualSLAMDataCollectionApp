/////////////////// Data Collection of Video sequences and framestamped IMU and GPS readings ///////////////////////////////////
//
/////////////////////////////////  Form1 //////////////////////////////////////
// 
// Just a form implementing a simple GUI
//

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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Emgu.CV;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.Util;

using FlyCapture2Managed;


namespace GPSVideoRecorder
{
    public partial class Form1 : Form
    {
        
        // Latest camera intrinsics
        public IntrinsicCameraParameters LatestCamIntrinsics;

        // The calibrator clASS
        Flea3Calibrator calibrator;
        
        // registered  comm ports
        String[] CommPorts;

        // the GPS receiever object
        GPSReceiver gps;

        // the IMU communications device
        IMUCommsDevice imu;

        // the recording class / thread
        Flea3Recorder recorder;

        public string CurrentFileName;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            updateCommPortCombos();
            
            

            comboResolution.SelectedIndex = 0;

            Matrix<double> test = new Matrix<double>(3, 3);

            LatestCamIntrinsics = new IntrinsicCameraParameters();
            // initializing the intrinsics matrix to the identity
            LatestCamIntrinsics.IntrinsicMatrix.SetIdentity();

            //LatestCamIntrinsics.DistortionCoeffs = new Matrix<double>(4, 1);
            LatestCamIntrinsics.DistortionCoeffs.SetZero();

            gps = null;

            calibrator = null;

            imu = null;

            comboResolution.SelectedIndex = 2;
            

            // start the timer
            timer1.Start();
            
        }

        private void updateCommPortCombos() {
            CommPorts = System.IO.Ports.SerialPort.GetPortNames();
            comboBox1.Items.Clear();
            comboBox2.Items.Clear();
            comboBox1.Text = comboBox2.Text = "";

            int i;
            // adding comm ports to combo boxes
            for (i = 0; i < CommPorts.Length; i++) {
                comboBox1.Items.Add(CommPorts[i]);
                comboBox2.Items.Add(CommPorts[i]);
            }
            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
                comboBox2.SelectedIndex = 0;
            }

        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (gps == null)
            {
                gps = new GPSReceiver(comboBox1.Items[comboBox1.SelectedIndex].ToString(), (int)numericUpDown1.Value, true);
                gps.StartReceiver();

                // change the connection status control
                textBox4.Text = "Connected";
                //timer1.Start();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (gps != null)
            {
                gps.stopReceiver();
                if (gps.CommPort.IsOpen) gps.CommPort.Close();

                // change the connection status control
                textBox4.Text = "Disconnected";
                //timer1.Stop();
            }

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

            timer1.Stop();
            
            if (recorder != null)
                if (recorder.RecordingThreadActive)
                    recorder.stopCapturing();

            
            if (gps!=null) 
                if (gps.NMEAThreadActive) {
                    gps.stopReceiver();
                    if (gps.CommPort.IsOpen)
                        gps.CommPort.Close();
                }

            
            if (imu != null)
            {
                if (imu.Flag_CommsActive) imu.stopComms();
                imu = null;
            }
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (gps != null)
            {
                textBox1.Text = gps.MRCStatus ? "Valid" : "Invalid";
                textBox2.Text = Convert.ToString(gps.LatitudeAsDecimal); 
                textBox3.Text = Convert.ToString(gps.LongitudeAsDecimal);
                textBox5.Text = Convert.ToString(gps.SpeedOverGround);
            }

            if (imu != null)
                if (imu.Flag_CommsActive)
                {
                    textBox7.Text = Convert.ToString(imu.Last_Acc1);
                    textBox8.Text = Convert.ToString(imu.Last_Acc2);
                    textBox9.Text = Convert.ToString(imu.Last_Acc3);
                    textBox10.Text = Convert.ToString(imu.Last_Angvel1);
                    textBox11.Text = Convert.ToString(imu.Last_Angvel2);
                    textBox12.Text = Convert.ToString(imu.Last_Angvel3);
                }






        }

        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            while (dlg.ShowDialog() != DialogResult.OK);

            String folderName = dlg.SelectedPath;

            textOutputFolder.Text = folderName;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            /*if (gps == null)
            {
                try
                {
                    gps = new GPSReceiver(comboBox1.Items[comboBox1.SelectedIndex].ToString(), (int)numericUpDown1.Value, true);

                    gps.StartReceiver();
                    timer1.Start();
                }
                catch (System.ArgumentOutOfRangeException ex)
                {
                    MessageBox.Show("Make sure your GPS is connected and choose serial port!!! Starting capture without GPS", "GPS problem!", MessageBoxButtons.OK);
                }
            }
            else if (!gps.CommPort.IsOpen)
            {
                try
                {
                    gps = new GPSReceiver(comboBox1.Items[comboBox1.SelectedIndex].ToString(), (int)numericUpDown1.Value, true);

                    gps.StartReceiver();
                    timer1.Start();
                }
                catch (System.ArgumentOutOfRangeException ex)
                {
                    MessageBox.Show("Make sure your GPS is connected and choose serial port!!! Starting capture without GPS", "GPS problem!", MessageBoxButtons.OK);
                }
            } */
            try
            {
                StreamReader sr = new StreamReader("CalibParams.txt");
                string line = sr.ReadLine();
                string[] row1 = line.Split(new char[] {','});

                LatestCamIntrinsics.IntrinsicMatrix[0, 0] = Convert.ToDouble(row1[0]);
                LatestCamIntrinsics.IntrinsicMatrix[0, 1] = Convert.ToDouble(row1[1]);
                LatestCamIntrinsics.IntrinsicMatrix[0, 2] = Convert.ToDouble(row1[2]);


               

                LatestCamIntrinsics.IntrinsicMatrix[1, 0] = Convert.ToDouble(row1[3]);
                LatestCamIntrinsics.IntrinsicMatrix[1, 1] = Convert.ToDouble(row1[4]);
                LatestCamIntrinsics.IntrinsicMatrix[1, 2] = Convert.ToDouble(row1[5]);


                
                LatestCamIntrinsics.IntrinsicMatrix[2, 0] = Convert.ToDouble(row1[6]);
                LatestCamIntrinsics.IntrinsicMatrix[2, 1] = Convert.ToDouble(row1[7]);
                LatestCamIntrinsics.IntrinsicMatrix[2, 2] = Convert.ToDouble(row1[8]);

                line = sr.ReadLine();
                string[] dcoeffs = line.Split(new char[] { ',' });
                LatestCamIntrinsics.DistortionCoeffs[0, 0] = Convert.ToDouble(dcoeffs[0]);
                LatestCamIntrinsics.DistortionCoeffs[1, 0] = Convert.ToDouble(dcoeffs[1]);
                LatestCamIntrinsics.DistortionCoeffs[2, 0] = Convert.ToDouble(dcoeffs[2]);
                LatestCamIntrinsics.DistortionCoeffs[3, 0] = Convert.ToDouble(dcoeffs[3]);

                sr.Close();


                // dumping out the new parameters from the file
                line = "[ "+row1[0]+" , "+row1[1] + " , " + row1[2] + " ]" + "\n"+
                       "[ "+row1[3]+" , "+row1[4] + " , " + row1[5] + " ]" + "\n"+
                       "[ "+row1[6]+" , "+row1[7] + " , " + row1[8] + " ]" + "\n"+
                       "-------------------------------------------------" + "\n"+
                       "["+dcoeffs[0]+" , "+ dcoeffs[1] + " , "+ dcoeffs[2] + " , " + dcoeffs[3]+"]";
                MessageBox.Show(line, "New camera Intrinsics and Distortion Coefficients loaded from the file", MessageBoxButtons.OK);
            }
            catch (FileNotFoundException ex) {

                // dumping out the new parameters from the file
                string line = "[ " + Convert.ToString(LatestCamIntrinsics.IntrinsicMatrix[0,0]) + " , 0 , "+Convert.ToString(LatestCamIntrinsics.IntrinsicMatrix[0,2])+ " ]" + "\n" +
                       "[ 0 , " + Convert.ToString(LatestCamIntrinsics.IntrinsicMatrix[1,1]) + " , " + Convert.ToString(LatestCamIntrinsics.IntrinsicMatrix[1,2])+ " ]" + "\n" +
                       "[ 0 , 0, 1 ]" + "\n" +
                       "-------------------------------------------------" + "\n" +
                       "[" + Convert.ToString(LatestCamIntrinsics.DistortionCoeffs[0,0]) + 
                       " , " + Convert.ToString(LatestCamIntrinsics.DistortionCoeffs[1,0]) + 
                       " , " + Convert.ToString(LatestCamIntrinsics.DistortionCoeffs[2,0]) + 
                       " , " + Convert.ToString(LatestCamIntrinsics.DistortionCoeffs[3,0]) + "]";
                MessageBox.Show(line, "Could not retrieve intrinsics from file. Using the values previously stored in memory...",  MessageBoxButtons.OK);

            };

            if (MessageBox.Show("Do you wish to do a calibration before recording ?", "Calibration Alert!", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                // calibrate first
                calibrator = new Flea3Calibrator(pictureBox1);
                calibrator.startCalibration(comboResolution.SelectedIndex, calibrator.CamIDs[0]);
            }
            else
            {
               recorder = new Flea3Recorder(gps, imu);
                



                // the file name format: "dd-mm-yyyy.hh-mm-ss"
                String fname = "";
                if (checkRecord.Checked)
                {

                    int day = DateTime.Now.Day;
                    fname = fname + ((day < 10) ? "0" + Convert.ToString(day) + "-" : Convert.ToString(day) + "-");

                    int month = DateTime.Now.Month;
                    fname = fname + ((month < 10) ? "0" + Convert.ToString(month) + "-" : Convert.ToString(month) + "-");

                    int year = DateTime.Now.Year;
                    fname = fname + Convert.ToString(year) + '.';

                    int hour = DateTime.Now.Hour;
                    fname = fname + ((hour < 10) ? "0" + Convert.ToString(hour) + "-" : Convert.ToString(hour) + "-");

                    int minute = DateTime.Now.Minute;
                    fname = fname + ((minute < 10) ? "0" + Convert.ToString(minute) : Convert.ToString(minute));

                    fname = textOutputFolder.Text + "\\" + fname;
                }

                CurrentFileName = fname;


                // ok, starting the recording!
                // using the first camera in the list
                if (recorder.CamIDs.Count() > 0)
                {

                    recorder.startCapture(recorder.CamIDs[0], comboResolution.SelectedIndex, pictureBox1, fname+".avi", checkRecord.Checked);
                }
                else
                    Console.WriteLine(" No camera found on the bus!!! Capturing aborted!");

            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            
            int i;
            if (recorder != null)
                if (recorder.RecordingThreadActive)
                {
                    recorder.stopCapturing();


                    //while (!recorder.OutOfDumpingThread) ;
                    //recorder.stopCapturing();

                    // ********************************** recording the gps information to a text file ******************************************************
                    string txtFileName = CurrentFileName + ".gps.txt";
                    StreamWriter sw = new StreamWriter(txtFileName);
                    int gpsdataCount = recorder.GpsCaptureData.Count();

                    string test = Convert.ToString(LatestCamIntrinsics.IntrinsicMatrix.Data[0, 0]);
                    // 
                    string intrinsicsStr = Convert.ToString(LatestCamIntrinsics.IntrinsicMatrix.Data[0, 0]) + ",";
                    intrinsicsStr += Convert.ToString(LatestCamIntrinsics.IntrinsicMatrix.Data[0, 1]) + ",";
                    intrinsicsStr += Convert.ToString(LatestCamIntrinsics.IntrinsicMatrix.Data[0, 2]) + ",";
                    intrinsicsStr += Convert.ToString(LatestCamIntrinsics.IntrinsicMatrix.Data[1, 0]) + ",";
                    intrinsicsStr += Convert.ToString(LatestCamIntrinsics.IntrinsicMatrix.Data[1, 1]) + ",";
                    intrinsicsStr += Convert.ToString(LatestCamIntrinsics.IntrinsicMatrix.Data[1, 2]) + ",";
                    intrinsicsStr += Convert.ToString(LatestCamIntrinsics.IntrinsicMatrix.Data[2, 0]) + ",";
                    intrinsicsStr += Convert.ToString(LatestCamIntrinsics.IntrinsicMatrix.Data[2, 1]) + ",";
                    intrinsicsStr += Convert.ToString(LatestCamIntrinsics.IntrinsicMatrix.Data[2, 2]);

                    sw.WriteLine(intrinsicsStr);

                    string distcoeffsStr = Convert.ToString(LatestCamIntrinsics.DistortionCoeffs[0, 0]) + "," +
                                            Convert.ToString(LatestCamIntrinsics.DistortionCoeffs[1, 0]) + "," +
                                            Convert.ToString(LatestCamIntrinsics.DistortionCoeffs[2, 0]) + "," +
                                            Convert.ToString(LatestCamIntrinsics.DistortionCoeffs[3, 0]);
                    sw.WriteLine(distcoeffsStr);



                    for (i = 0; i < gpsdataCount; i++)
                    {
                        string lineStr = recorder.GpsCaptureData[i].getGpsDataAsCommaDelimetedLine();
                        sw.WriteLine(lineStr);
                    }
                    sw.Close();

                    // ************************************ Storing IMU data - if present - in a separate file **************************

                    if (recorder.IMUCapturedata.Count() > 0)
                    {
                        txtFileName = CurrentFileName + ".imu.txt";
                        sw = new StreamWriter(txtFileName);
                        int imudataCount = recorder.IMUCapturedata.Count();


                        sw.WriteLine(intrinsicsStr);

                        sw.WriteLine(distcoeffsStr);


                        for (i = 0; i < imudataCount; i++)
                        {
                            string lineStr = recorder.IMUCapturedata[i].getIMUDataAsCommaDelimetedLine();
                            sw.WriteLine(lineStr);
                        }
                        sw.Close();
                    }

                }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            calibrator = new Flea3Calibrator(pictureBox1);
            calibrator.startCalibration(comboResolution.SelectedIndex, calibrator.CamIDs[0]);
        }

        private void button7_Click(object sender, EventArgs e)
        {

            Matrix<double> K = new Matrix<double>(3, 3);
            K.SetZero();
            Matrix<double> D = new Matrix<double>(8, 1);
            D.SetZero();

            if (calibrator != null)
            {
                int i, j;
                for (i = 0; i < 3; i++)
                    for (j = 0; j < 3; j++)
                        K.Data[i, j] = calibrator.Intrinsics.IntrinsicMatrix.Data[i, j];
                for (i = 0; i < 4; i++)
                    D.Data[i, 0] = calibrator.Intrinsics.DistortionCoeffs.Data[i, 0];

                calibrator.stopCalibration();
                
             

                // saving calibration parameters
                StreamWriter sw = new StreamWriter("CalibParams.txt");
                
                string intrinsicsStr = Convert.ToString(K.Data[0, 0]) + ",";
                       intrinsicsStr += Convert.ToString(K.Data[0, 1]) + ",";
                       intrinsicsStr += Convert.ToString(K.Data[0, 2]) + ",";
                       intrinsicsStr += Convert.ToString(K.Data[1, 0]) + ",";
                       intrinsicsStr += Convert.ToString(K.Data[1, 1]) + ",";
                       intrinsicsStr += Convert.ToString(K.Data[1, 2]) + ",";
                       intrinsicsStr += Convert.ToString(K.Data[2, 0]) + ",";
                       intrinsicsStr += Convert.ToString(K.Data[2, 1]) + ",";
                       intrinsicsStr += Convert.ToString(K.Data[2, 2]);
                
                sw.WriteLine(intrinsicsStr);

                string distcoeffsStr = Convert.ToString(D.Data[0, 0]) + "," +
                                        Convert.ToString(D.Data[1, 0]) + "," +
                                        Convert.ToString(D.Data[2, 0]) + "," +
                                        Convert.ToString(D.Data[3, 0]);
                sw.WriteLine(distcoeffsStr);

                sw.Close();

                LatestCamIntrinsics.IntrinsicMatrix.Data[0, 0] = K.Data[0, 0];
                LatestCamIntrinsics.IntrinsicMatrix.Data[0, 2] = K.Data[0, 2];
                LatestCamIntrinsics.IntrinsicMatrix.Data[1, 1] = K.Data[1, 1];
                LatestCamIntrinsics.IntrinsicMatrix.Data[1, 2] = K.Data[1, 2];
                LatestCamIntrinsics.IntrinsicMatrix.Data[2, 2] = 1;

                LatestCamIntrinsics.DistortionCoeffs.Data[0, 0] = D.Data[0, 0];
                LatestCamIntrinsics.DistortionCoeffs.Data[1, 0] = D.Data[1, 0];
                LatestCamIntrinsics.DistortionCoeffs.Data[2, 0] = D.Data[2, 0];
                LatestCamIntrinsics.DistortionCoeffs.Data[3, 0] = D.Data[3, 0];
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {

            Graphics g = Graphics.FromImage(pictureBox1.Image);
            g.Clear(Color.White);
        }

        private void button8_Click_1(object sender, EventArgs e)
        {
            if (comboBox2.Items.Count > 0)
            {
                string imuport = (string)comboBox2.Items[comboBox2.SelectedIndex];

                imu = new IMUCommsDevice(imuport);
                

                
                // starting imu communications
                imu.startComms();

                textBox6.Text = "Connected";

            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            updateCommPortCombos();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            updateCommPortCombos();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (imu != null)
            {
                if (imu.Flag_CommsActive) imu.stopComms();
                imu = null;

                textBox6.Text = "Disconnected";

            }
        }



    }
}
