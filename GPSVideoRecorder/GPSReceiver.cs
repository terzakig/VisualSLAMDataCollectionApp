//////////////////////////////////////// GPS Receiver class ////////////////////////////////////////////////
///                                   Inherits NMEA receiver                                              //
///                                   
// George Terzakis 2011-13 - Plymouth University


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GPSVideoRecorder
{
    class GPSReceiver : NMEAReceiver
    {

        // the degrees are signed to indicate N-orth (+) / S-outh (-) hemisphere - E-ast (+) / W-est (-)
        public int LatSign, LongSign; // sign of Longitude and latitude
        public int LongDegrees;     // 3 digits
        public int LatDegrees;      // 2 digits
        public double LongMinutes;     // 2 + 4 digits (including the 4 deciminutes now)
        public double LatMinutes;      // 2 + 4 digits (including the 4 deciminutes now)
        //public int LongDeciminutes; // 4 digits
        //public int LatDeciminutes;  // 4 digits

        // latitude, longitude as floating points
        public double LatitudeAsDecimal, LongitudeAsDecimal;


        // UTC time as transmitted by last GPS reception
        public int LastUTCHour;       // 2 digits
        public int LastUTCMinute;     // 2 digits
        public int LastUTCSecond;     // 2 digits
        public int LasUTCMillisecond; // 3 digits

        // UTC Date and Time in Datetime format
        public DateTime UTCDateTime;

        // Data valid (3d argument field in GPRMC sentence)
        public Boolean MRCStatus;
        // Speed over Ground
        public double SpeedOverGround;
        // Course over ground
        public double CourseOverGround;

        // Magnetic Variation
        public double MagVariation;

        // constructor
        public GPSReceiver()
            : base()
        {

            // an approximate current location (somewhere in Plymouth)
            LongDegrees = +50; // 50 degrees North
            LongMinutes = 35.0;  // 22 minutes


            LatDegrees = -4;  // 4 degrees West
            LatMinutes = 07;  // 7 minutes


            // latitude and longitude as doubles
            LatitudeAsDecimal = 50.35;
            LongitudeAsDecimal = -4.07;

            // Date and time
            UTCDateTime = DateTime.Now;
            LastUTCHour = UTCDateTime.Hour;
            LastUTCMinute = UTCDateTime.Minute;
            LastUTCSecond = UTCDateTime.Second;
            LasUTCMillisecond = UTCDateTime.Millisecond;



            MRCStatus = false;
            SpeedOverGround = CourseOverGround = -1;

        }

        public GPSReceiver(string commport, int baudrate, Boolean checksum)
            : base(commport, baudrate, checksum)
        {
            // an approximate current location (somewhere in Plymouth)
            LongDegrees = +50; // 50 degrees North
            LongMinutes = 35.00;  // 22 minutes


            LatDegrees = -4;  // 4 degrees West
            LatMinutes = 07.00;  // 7 minutes


            // latitude and longitude as doubles
            LatitudeAsDecimal = 50.35;
            LongitudeAsDecimal = -4.07;

            // Date and time
            UTCDateTime = DateTime.Now;
            LastUTCHour = UTCDateTime.Hour;
            LastUTCMinute = UTCDateTime.Minute;
            LastUTCSecond = UTCDateTime.Second;
            LasUTCMillisecond = UTCDateTime.Millisecond;

            MRCStatus = false;

            SpeedOverGround = CourseOverGround = -1;
        }

        public override void processEvents()
        {
            // removing a sentence possibly store in the queue
            int qlen = SentenceQueue.Count();
            if (qlen > 0)
            {
                string sentence = SentenceQueue[qlen - 1];
                SentenceQueue.RemoveAt(qlen - 1);
                // decode the GPRMC sentence
                string[] tokens = parseNMEASentence(sentence);
                if (tokens[0] == "$GPRMC")
                    decodeGPRMCTokens(tokens,
                                        out LatitudeAsDecimal,
                                        out LatDegrees,
                                        out LatMinutes,
                                        out LongitudeAsDecimal,
                                        out LongDegrees,
                                        out LongMinutes,
                                        out SpeedOverGround,
                                        out CourseOverGround,
                                        out UTCDateTime);


                // ************** time retrieved and stored **********
            }
        }



        // a function that returns the latitude as string
        public String getLatitudeAsString()
        {
            String latitude = Convert.ToString(LatitudeAsDecimal);
            /*
            int num = LatDegrees;
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
            String longitude = Convert.ToString(LongitudeAsDecimal);
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



        //getGPSdata opverloaded to include a given frameindex
        public GPSDataInstance getGPSDataInstance(int frameindex)
        {
            GPSDataInstance gpsdata = new GPSDataInstance(frameindex,
                                                              LatitudeAsDecimal,
                                                              LongitudeAsDecimal,
                                                              SpeedOverGround,
                                                              CourseOverGround,
                                                              MRCStatus
                                                              );
            return gpsdata;
        }





        // The following function computes the distance between two  GPS coordinates using the Haversine formula
        public static double getGPSDistance(double latitude1, double longitude1,
                                            double latitude2, double longitude2)
        {

            double R = 6371.0; // Earth's radius in km

            var lat1 = latitude1 * Math.PI / 180;
            var lat2 = latitude2 * Math.PI / 180;
            var lon1 = longitude1 * Math.PI / 180;
            var lon2 = longitude2 * Math.PI / 180;

            var dLat = lat2 - lat1;
            var dLon = lon2 - lon1;


            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c;

            return d;
        }

        // GPS distance computation overload with GPSDatainstance structures
        public static double getGPSDistance(GPSDataInstance location1, GPSDataInstance location2)
        {

            return getGPSDistance(location1.Latitude, location1.Longitude, location2.Latitude, location2.Longitude);
        }

        // a function that computes the relative position in 2D Euclidean coordinates 
        public static double getRelativePosition(double orgLatitude, double orgLongitude,
                                                double locationLatitude, double locationLongitude,
                                                out double X, out double Y
                                                )
        {
            // using the convention x = Δlatitude and y = Δlongitude
            X = getGPSDistance(orgLatitude, orgLongitude,
                                      locationLatitude, orgLongitude);

            Y = getGPSDistance(orgLatitude, orgLongitude,
                                      orgLatitude, locationLongitude);

            return Math.Sqrt(X * X + Y * Y);
        }


        // relative position overload for GPSDataInstance structures (accurate for relatively close distances - e.g., several miles)
        public static double getRelativePosition(GPSDataInstance origin,
                                                 GPSDataInstance location,
                                                 out double X, out double Y
                                                )
        {
            X = getGPSDistance(origin.Latitude, origin.Longitude, location.Latitude, origin.Longitude);
            Y = getGPSDistance(origin.Latitude, origin.Longitude, origin.Latitude, location.Longitude);
            return Math.Sqrt(X * X + Y * Y);
        }


    }
}
