using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GPSVideoRecorder
{
    class PrecisionTimerEventArgs : EventArgs
    {
        public double TimeElapsed; // the exact time elapsed when the event was triggered

        public PrecisionTimerEventArgs(double timeElapsed)
        {
            TimeElapsed = timeElapsed;
        }
    }
}
