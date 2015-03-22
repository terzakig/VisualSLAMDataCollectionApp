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
