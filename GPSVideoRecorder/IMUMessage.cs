using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace GPSVideoRecorder
{


   

    //[StructLayout(LayoutKind.Explicit)] 

    class IMUMessage
    {
        // **************** Message-releated constants *******************
        public const ushort c_Header = 0x55AA; // message header

        public const ushort c_MessageLength = 68; // Message Length: Should be always, 34 words * 2 bytes = 68 bytes


        /////////////////  IMU Message fields ///////////////////////////
        public ushort Header;                    // Must be 0x55AA
        public ushort Count;                     // Message count (I guess the count includes the rest of the message, POSSIBLY INCLUDING the checksum bytes)
        // **** Axis 1 *****
        public float Axis1_Rate;                 // Axis 1 Angular Velocity (single precision) - Degrees / second        
        public float Axis1_Quad;                 // .... ?                                     - Degrees / Second
        public float Axis1_Frequency;            // Axis 1 Frequency (possibly Sampling???)     - Hz
        public float Axis1_Accelerometer;        // Axis 1 Acceleration                        - g's
        public float Axis1_AccelerometerTemp;    // Axis 1 Accelerometer temperature           - Celsius

        // **** Axis 2 *****
        public float Axis2_Rate;                 // Axis 2 Angular Velocity (single precision) - Degrees / second
        public float Axis2_Quad;                 // .... ?                                     - Degrees / Second
        public float Axis2_Frequency;            // Axis 2 Frequency (possibly Sampling???)     - Hz
        public float Axis2_Accelerometer;        // Axis 2 Acceleration                        - g's
        public float Axis2_AccelerometerTemp;    // Axis 2 Accelerometer temperature           - Celsius

        // **** Axis 3 *****
        public float Axis3_Rate;                  // Axis 3 Angular Velocity (single precision) - Degrees / second
        public float Axis3_Quad;                  // .... ?                                     - Degrees / Second
        public float Axis3_Frequency;             // Axis 3 Frequency (possibly Sampling???)    - Hz
        public float Axis3_Accelerometer;         // Axis 3 Acceleration                        - g's
        public float Axis3_AccelerometerTemp;     // Axis 3 Accelerometer temperature           - Celsius

        public IMUStatusFlags Status; // The Built In Test results and the tests on Gyro and accelerometer

        public ushort Checksum;                   // Checksum word (again, is this a CRC checksum, or just a sum???)



        // ******************** And some static methods *********************

        // a method to get a floating point number out of 4 bytes in MSB-comes-first order
        public static float getFloatFromBytes(byte rawByte3, byte rawByte2, byte rawByte1, byte rawByte0)
        {
            // creating an array with least to most significant ordering
            byte[] inverseOrder = new byte[4];
            inverseOrder[0] = rawByte0;
            inverseOrder[1] = rawByte1;
            inverseOrder[2] = rawByte2;
            inverseOrder[3] = rawByte3;

            float result = 0f;

            unsafe
            {
                fixed (void* voidPtr = &inverseOrder[0])
                {
                    
                    float* floatPtr = (float*)voidPtr;
                    result = *floatPtr;
                }
            }

            return result;
        }

        // "unwrap" a raw message 
        public static IMUMessage unwrapRawMessage(byte[] rawMsg)
        {
            // creating a new IMU Message object
            IMUMessage msg = new IMUMessage();

            // 1. Header
            msg.Header = (ushort)((ushort)(rawMsg[0] << 8) + rawMsg[1]);
            msg.Count = (ushort)((ushort)(rawMsg[2] << 8) + rawMsg[3]);

            // 2. axis 1 rate
            msg.Axis1_Rate = getFloatFromBytes(rawMsg[4], rawMsg[5], rawMsg[6], rawMsg[7]);

            // 3. axis 1 quad
            msg.Axis1_Quad = getFloatFromBytes(rawMsg[8], rawMsg[9], rawMsg[10], rawMsg[11]);

            // 4. axis 1 frequency
            msg.Axis1_Frequency = getFloatFromBytes(rawMsg[12], rawMsg[13], rawMsg[14], rawMsg[15]);

            // 5. axis 1 accelerometer
            msg.Axis1_Accelerometer = getFloatFromBytes(rawMsg[16], rawMsg[17], rawMsg[18], rawMsg[19]);

            // 6. axis 1 accelerometer
            msg.Axis1_AccelerometerTemp = getFloatFromBytes(rawMsg[20], rawMsg[21], rawMsg[22], rawMsg[23]);

            // 7. axis 2 rate
            msg.Axis2_Rate = getFloatFromBytes(rawMsg[24], rawMsg[25], rawMsg[26], rawMsg[27]);

            // 8. axis 2 quad
            msg.Axis2_Quad = getFloatFromBytes(rawMsg[28], rawMsg[29], rawMsg[30], rawMsg[31]);

            // 9. axis 2 frequency
            msg.Axis2_Frequency = getFloatFromBytes(rawMsg[32], rawMsg[33], rawMsg[34], rawMsg[35]);

            // 10. axis 2 accelerometer
            msg.Axis2_Accelerometer = getFloatFromBytes(rawMsg[36], rawMsg[37], rawMsg[38], rawMsg[39]);

            // 11. axis 2 accelerometer temperature
            msg.Axis2_AccelerometerTemp = getFloatFromBytes(rawMsg[40], rawMsg[41], rawMsg[42], rawMsg[43]);

            // 12. axis 3 rate
            msg.Axis3_Rate = getFloatFromBytes(rawMsg[44], rawMsg[45], rawMsg[46], rawMsg[47]);

            // 13. axis 3 quad
            msg.Axis3_Quad = getFloatFromBytes(rawMsg[48], rawMsg[49], rawMsg[50], rawMsg[51]);

            // 14. axis 3 frequency
            msg.Axis3_Frequency = getFloatFromBytes(rawMsg[52], rawMsg[53], rawMsg[54], rawMsg[55]);

            // 15. axis 3 accelrometer
            msg.Axis3_Accelerometer = getFloatFromBytes(rawMsg[56], rawMsg[57], rawMsg[58], rawMsg[59]);

            // 16. axis 3 accelrometer temperature
            msg.Axis3_AccelerometerTemp = getFloatFromBytes(rawMsg[60], rawMsg[61], rawMsg[62], rawMsg[63]);


            // 17. Built In Mode and Test Results
            msg.Status = new IMUStatusFlags((ushort)((rawMsg[64] << 8) + rawMsg[65]));

            // 18. The checksum
            msg.Checksum = (ushort)((rawMsg[66] << 8) + rawMsg[67]);

            return msg;

        }









    }

}
