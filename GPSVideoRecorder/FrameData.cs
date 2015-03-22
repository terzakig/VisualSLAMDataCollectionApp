using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;


using FlyCapture2Managed;

// this class is used to store a captured ftrame and the corresponding GPS data instance
namespace GPSVideoRecorder
{
    class FrameData
    {
        // the image
        public ManagedImage Img;

        // the GPS data
        public GPSDataInstance GPSData;

        public FrameData(ManagedImage img, GPSDataInstance gpsData)
        {
            Img = img;
            GPSData = gpsData;
        }

    }
}
