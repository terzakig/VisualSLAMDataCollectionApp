/////////////////// Data Collection of Video sequences and framestamped IMU and GPS readings ///////////////////////////////////
//
/////////////////////////////////  PrecisionTimer //////////////////////////////////////
// 
// A special PrecisionTimer that circumvents the problem of standard windows API timers (very erratic event triggering under 12 ms)
// This timer enables accurate IMU sampling in Windows 

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

using System.Runtime.InteropServices;
using System.Threading;
using System.ComponentModel;


namespace GPSVideoRecorder
{
    class PrecisionTimer
    {
        // declaring the timer event and event handler delegate
        // the timer overflow event
        public event EventHandler<PrecisionTimerEventArgs> TimerOverflow;
        // the function type of the event handler (FlearFrameHandler type)
        public delegate void PrecisionTimerEventHandler(object s, PrecisionTimerEventArgs e);

        // importing the QueryPerformanceCounter and Frequency functions
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);
        // done


        // the time in seconds
        public double Interval;

        // a boolean indicating that the timer is running
        public Boolean Enabled;

        // the counter thread (counts time using QueryPerformanceCounter and triggers the overflow events
        private Thread CounterThread;

        // the clock frequency
        private long ClockFrequency;

        // the start time and currenttime
        private long startTime, currentTime;

        // the out-of-loop flag
        private Boolean flag_OutOfLoop;

        // constructor
        public PrecisionTimer(double interval, PrecisionTimerEventHandler timerEventHandler)
        {
            Enabled = false;
            Interval = interval;

            flag_OutOfLoop = true;

            TimerOverflow += new EventHandler<PrecisionTimerEventArgs>(timerEventHandler);
        }

        // start the timer
        public void start()
        {
            if (QueryPerformanceFrequency(out ClockFrequency) == false)
            {
                throw new Win32Exception(); // timer not supported
            }
            else
            {
                CounterThread = new Thread(countingLoop);
                //CounterThread.Priority = ThreadPriority.Highest;
                Enabled = true;
                CounterThread.Start();
            }
        }


        public void stop()
        {
            Enabled = false; // timer inactive, leaving the mainb loop
            while (!flag_OutOfLoop) ; // wait until the loop has ended
            CounterThread.Abort(); // kill the thread
        }

        // the counting loop
        private void countingLoop()
        {
            QueryPerformanceCounter(out startTime);
            flag_OutOfLoop = false; // entering the loop
            while (Enabled)
            {
                QueryPerformanceCounter(out currentTime); // obtaining current time
                double diff = (currentTime - startTime) / (1.0 * ClockFrequency);
                if (diff >= Interval)
                { // creating a new (overflow) event
                    var handler = TimerOverflow;
                    PrecisionTimerEventArgs args = new PrecisionTimerEventArgs(diff);

                    if (handler != null)
                    {
                        handler(this, args);
                    }

                    QueryPerformanceCounter(out startTime); // resetting the startTime in order to capture the next overflow
                }
            }
            flag_OutOfLoop = true; // just left the loop. Clear to kill the thread
        }


    }
}
