/////////////////// Data Collection of Video sequences and framestamped IMU and GPS readings ///////////////////////////////////
//
// GPSDataInstance class.cs: A structure for storing GPS data

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

namespace GPSVideoRecorder
{
    class GPSDataInstance
    {

        // the degrees are signed to indicate N-orth (+) / S-outh (-) hemisphere - E-ast (+) / W-est (-)
        public double Latitude, Longitude;
        // Speed over Ground
        public double SpeedOverGround; // float
        // Course over ground
        public double CourseOverGround; // float


        // Valid signal reading flag
        public Boolean Valid;


        // frame index
        public int FrameIndex;

        // constructor #1 (by numbers)
        public GPSDataInstance(int frameindex, double latitude, double longitude, double speedovgr, double courseoverg, Boolean valid)
        {
            FrameIndex = frameindex;

            // obtaining Longitude and Latitude in decimal form
            Latitude = latitude;
            Longitude = longitude;

            SpeedOverGround = speedovgr;
            CourseOverGround = courseoverg;
            Valid = valid;
        }



        // a function that returns the latitude as string
        public String getLatitudeAsString()
        {
            String latitude = Convert.ToString(Latitude);

            /*int num = LatDegrees;
            // Latitude Degrees (2 digits)
            int temp = num / 10;
            latitude += Convert.ToString(temp);
            num = num - temp * 10;
            latitude += Convert.ToString(num);

            // Latitude minutes (2 digits)
            num = LatMinutes;
            temp = num / 10;
            latitude += Convert.ToString(temp);
            num = num - temp * 10;
            latitude += Convert.ToString(num);


            // Latitude Deciminutes (4 digits)
            num = LatDeciminutes;
            temp = num / 1000;
            latitude += Convert.ToString(temp);
            num = num - temp * 1000;
            temp = num / 100;
            latitude += Convert.ToString(temp);
            num = num - temp * 100;
            temp = num / 10;
            latitude += Convert.ToString(temp);
            num = num - temp * 10;
            latitude += Convert.ToString(num);
            */
            return latitude;
        }

        // a function that returns the longitude as string
        public String getLongitudeAsString()
        {
            String longitude = Convert.ToString(Longitude);
            // Longitude Degrees (3 digits)
            /*int num = LongDegrees;
            int temp = num / 100;
            longitude += Convert.ToString(temp);
            num = num - temp * 100;
            temp = num / 10;
            longitude += Convert.ToString(temp);
            num = num - temp * 10;
            longitude += Convert.ToString(num);

            // Longitude minutes (2 digits)        
            num = LongMinutes;
            temp = num / 10;
            longitude += Convert.ToString(temp);
            num = num - temp * 10;
            longitude += Convert.ToString(num);


            // Latitude Deciminutes (4 digits)
            num = LongDeciminutes;
            temp = LongDeciminutes / 1000;
            longitude += Convert.ToString(temp);
            num = num - temp * 1000;
            temp = num / 100;
            longitude += Convert.ToString(temp);
            num = num - temp * 100;
            temp = num / 10;
            longitude += Convert.ToString(temp);
            num = num - temp * 10;
            longitude += Convert.ToString(num);
            */
            return longitude;
        }



        public string getGpsDataAsCommaDelimetedLine()
        {
            string lineStr = "";
            lineStr += Convert.ToString(FrameIndex) + "," +
                       (Valid ? "V," : "I,") +
                       getLongitudeAsString() + "," +
                       getLatitudeAsString() + "," +
                       Convert.ToString(SpeedOverGround) + "," +
                       Convert.ToString(CourseOverGround);


            return lineStr;
        }

        public string getGPSDataAsCommDelimetedLineNoFrameIndex()
        {
            string lineStr = "";
            lineStr += (Valid ? "V," : "I,") +
                       getLongitudeAsString() + "," +
                       getLatitudeAsString() + "," +
                       Convert.ToString(SpeedOverGround) + "," +
                       Convert.ToString(CourseOverGround);


            return lineStr;
        }




    }
}
