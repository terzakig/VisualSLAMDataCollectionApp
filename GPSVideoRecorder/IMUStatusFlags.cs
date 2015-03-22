/////////////////// Data Collection of Video sequences and framestamped IMU and GPS readings ///////////////////////////////////
//
/////////////////////////////////  IMUStatus flags //////////////////////////////////////
// 
// This class corresponds to bit-flags contained in an IMU message. It "unwraps" the flag word and stores the values to corresponding Booleans
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
    class IMUStatusFlags
    {
        public ushort StatusWord;      // the 16 bit word of flags

        // ****** List of Boolean flags starting from bit 0 ************
        public Boolean Gyro1;                   // Bit 0  - Gyro 1 check
        public Boolean Gyro2;                   // Bit 1  - Gyro 2 check
        public Boolean Gyro3;                   // Bit 2  - Gyro 1 check

        public Boolean Accelerometer1;          // Bit 3  - Accelerometer 1 check
        public Boolean Accelerometer2;          // Bit 4  - Accelerometer 2 check
        public Boolean Accelerometer3;          // Bit 5  - Accelerometer 3 check

        public Boolean RAM;                     // Bit 6  - RAM check
        public Boolean EPROM;                   // Bit 7  - EPROM check

        public Boolean GyroFreq;                // Bit 8  - Gyro Freq check (whatever that is)
        public Boolean GyroRate;                // Bit 9  - Gyro Rate check (whatever that is)
        public Boolean GyroQuad;                // Bit 10 - Gyro Quad check (whatever that is)
        public Boolean GyroPD;                  // Bit 11 - Gyro PD   check (whatever that is)

        public Boolean Acc;                     // Bit 12 - Accelerometer   check 
        public Boolean AccTemp;                 // Bit 13 - Accelerometer temperature  check 

        // ****************** Bits 14 and 15 are the BIT (Built In Test) Mode considered in pairs and not individually *************************
        // 0 0 : Startup Commanded BIT reporting
        // 0 1 : Startup BIT in progress
        // 1 0 : Commanded BIT in progress
        // 1 1 : Periodic BIT reporting

        public Boolean BIT_Bit0;                // Bit 14
        public Boolean BIT_Bit1;                // Bit 15



        // constructor
        public IMUStatusFlags(ushort statusWord)
        {
            StatusWord = statusWord;

            fillBitFields();
        }


        private void fillBitFields()
        {
            Gyro1 = (StatusWord & 1) > 0;
            Gyro2 = ((StatusWord >> 1) & 1) > 0;
            Gyro3 = ((StatusWord >> 2) & 1) > 0;

            Accelerometer1 = ((StatusWord >> 3) & 1) > 0;
            Accelerometer2 = ((StatusWord >> 4) & 1) > 0;
            Accelerometer3 = ((StatusWord >> 5) & 1) > 0;

            RAM = ((StatusWord >> 6) & 1) > 0;
            EPROM = ((StatusWord >> 7) & 1) > 0;

            GyroFreq = ((StatusWord >> 8) & 1) > 0;
            GyroRate = ((StatusWord >> 9) & 1) > 0;
            GyroQuad = ((StatusWord >> 10) & 1) > 0;
            GyroPD = ((StatusWord >> 11) & 1) > 0;

            Acc = ((StatusWord >> 12) & 1) > 0;
            AccTemp = ((StatusWord >> 13) & 1) > 0;

            BIT_Bit0 = ((StatusWord >> 14) & 1) > 0;
            BIT_Bit1 = ((StatusWord >> 15) & 1) > 0;
        }
            
    }
}
