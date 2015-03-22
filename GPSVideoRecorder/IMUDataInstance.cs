using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;


namespace GPSVideoRecorder
{
    class IMUDataInstance
    {
        // the frame id
        public int FrameIndex;

        // the raw data as a matrix
        public float a1, a2, a3, w1, w2, w3;

        // constructor #1
        public IMUDataInstance(int frameIndex, float acc1, float acc2, float acc3, float angv1, float angv2, float angv3)
        {
            FrameIndex = frameIndex;
            
            a1 = acc1; a2 = acc2; a3 = acc3;
            w1 = angv1; w2 = angv2; w3 = angv3;
        }

        // constructor #4 (from line string)
        public IMUDataInstance(string line)
        {
            string[] strs = line.Split(new char[] { ',' });


            // frame index
            FrameIndex = Convert.ToInt32(strs[0]);

            // acceleration1
            a1 = (float)Convert.ToDouble(strs[1]);
            
            // acceleration2
            a2 = (float)Convert.ToDouble(strs[2]);
            
            // acceleration3
            a3 = (float)Convert.ToDouble(strs[3]);

            // angular velocity1
            w1 = (float)Convert.ToDouble(strs[4]);

            // angular velocity2
            w2 = (float)Convert.ToDouble(strs[5]);

            // angular velocity3
            w3 = (float)Convert.ToDouble(strs[6]);
            
        }

        public string getIMUDataAsCommaDelimetedLine()
        {
            string ret = Convert.ToString(FrameIndex) + "," +
                         Convert.ToString(a1) + "," +
                         Convert.ToString(a2) + "," +
                         Convert.ToString(a3) + "," +
                         Convert.ToString(w1) + "," +
                         Convert.ToString(w2) + "," +
                         Convert.ToString(w3);

            return ret;
        }


        public string getIMUDataAsCommaDelimetedLineNoFrameIndex()
        {
            string ret = Convert.ToString(a1) + "," +
                         Convert.ToString(a2) + "," +
                         Convert.ToString(a3) + "," +
                         Convert.ToString(w1) + "," +
                         Convert.ToString(w2) + "," +
                         Convert.ToString(w3);

            return ret;
        }



    }
}
