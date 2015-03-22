/////////////////// Data Collection of Video sequences and framestamped IMU and GPS readings ///////////////////////////////////
//
/////////////////////////////////  PrecisionTimerEventArgs class //////////////////////////////////////
// 
// The necessary declaration of the data straucture containing just the time elapsed since the previous timer overflow inheriting EventArgs class
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
