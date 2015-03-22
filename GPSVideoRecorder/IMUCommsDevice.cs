/////////////////// Data Collection of Video sequences and framestamped IMU and GPS readings ///////////////////////////////////
//
/////////////////////////////////  Class IMUCommsDevice //////////////////////////////////////
// 
// Implements (1-way) communications with the SiIMU 02 (Atlantic Inertial Systems/UTC Aerospace Systems)
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
using System.Threading;



namespace GPSVideoRecorder
{
    class IMUCommsDevice
    {
        // ************ Constants ***********************

        
        //  Serial Comms relkated constants (only 1)
        public const int Serial_BaudRate_Standard = 460800; // standard Baud rate for SiIMU 02

        public const int Serial_MaxNumMsgInBuffer = 1000;   // maximum number of messages in the incoming buffer

        // ********** Class Members *************
        private System.IO.Ports.SerialPort CommPort;    // serial port

        // The communications "device" thread
        public Thread CommsThread;
 
        // the message queue
        //public IMUMessageQueue MessageQueue;
        //public Semaphore MsgQueueSemaphore; // a semaphore to block message queue access upon update

        // ********* Serial data reception - related variables ***********
        private int Data_RemainingBytes; // remaining bytes yet to be received for the message being currently received
        private byte[] Data_Rawmessage;  // raw byte storage for the message being currently received

        // ***************** Communications Operation Flags ********************
        public Boolean Flag_CommsActive;
        public Boolean Flag_InThreadLoop; // indicates that code is still executing inside the loop


        // last imu reading (for user -defined sampling intervals)
        public float Last_Acc1, Last_Acc2, Last_Acc3, Last_Angvel1, Last_Angvel2, Last_Angvel3;

        // constructor #1 (commport identified with its number)
        public IMUCommsDevice(int CommPortID)
        {
            // 1. Initializing Commport
            CommPort = new System.IO.Ports.SerialPort("COM" + Convert.ToString(CommPortID));
            //CommPort.BaudRate = Serial_BaudRate_Standard;
            CommPort.BaudRate = Serial_BaudRate_Standard;
            
            CommPort.WriteBufferSize = 10000;
            //CommPort.ReadBufferSize = Serial_MaxNumMsgInBuffer * IMUMessage.c_MessageLength;
            CommPort.Parity = System.IO.Ports.Parity.None;
            
            CommPort.Handshake = System.IO.Ports.Handshake.RequestToSend;
            CommPort.StopBits = System.IO.Ports.StopBits.Two;

            CommPort.ReadBufferSize = 10000;

            CommPort.ReceivedBytesThreshold = 20;
            
            
            // 2. Initializing state to St_Idle
            //State = St_Idle;

            // 3. The device is disabled
            Flag_CommsActive = false;

            // 4. Creating an empty Message queue 
           // MessageQueue = new IMUMessageQueue();

            // setting last nknown readings to zero
            Last_Acc1 = Last_Acc2 = Last_Acc3 = Last_Angvel1 = Last_Angvel2 = Last_Angvel3 = 0;

            // create the queue semaphore
            //MsgQueueSemaphore = new Semaphore(1, 1);
        }


        // constructor #2 (commport identified by a string)
        public IMUCommsDevice(string CommPortName)
        {
            // 1. Initializing Commport
            CommPort = new System.IO.Ports.SerialPort(CommPortName);
            //CommPort.BaudRate = Serial_BaudRate_Standard;
            CommPort.BaudRate = Serial_BaudRate_Standard;

            CommPort.WriteBufferSize = 10000;
            //CommPort.ReadBufferSize = Serial_MaxNumMsgInBuffer * IMUMessage.c_MessageLength;
            CommPort.Parity = System.IO.Ports.Parity.None;

            CommPort.Handshake = System.IO.Ports.Handshake.RequestToSend;
            CommPort.StopBits = System.IO.Ports.StopBits.Two;

            CommPort.ReceivedBytesThreshold = 2;


            // 2. Initializing state to St_Idle
            //State = St_Idle;

            // 3. The device is disabled
            Flag_CommsActive = false;

            // 4. Creating an empty Message queue 
            //MessageQueue = new IMUMessageQueue();

            // setting last nknown readings to zero
            Last_Acc1 = Last_Acc2 = Last_Acc3 = Last_Angvel1 = Last_Angvel2 = Last_Angvel3 = 0;

            // create the queue semaphore
            //MsgQueueSemaphore = new Semaphore(1, 1);
        }


        // start the comunications process
        public void startComms()
        {
            // 1. clear the queue and the Comm Port buffer

            //MessageQueue.clear();

            // 2. opening the serial port abd Raising active comms flag
            CommPort.Open();
            CommPort.DiscardInBuffer();
            Flag_CommsActive = true;

            //3. Create and start the thread
            CommsThread = new Thread(mainLoop);
            //CommsThread.Priority = ThreadPriority.Highest;
            //CommsThread.IsBackground = true;
            CommsThread.Start();

        }

        public void stopComms()
        {
            if (Flag_CommsActive)
            {
                Flag_CommsActive = false;
                while (Flag_InThreadLoop) ; // waiting until last iteration has finished

                // closing the commport
                CommPort.Close();

                // killing the thread
                CommsThread.Abort();

            }
        }



        // the thread main loop
        private void mainLoop()
        {
            Flag_InThreadLoop = true; // entering the thread main loop

            while (Flag_CommsActive)
            {
                processData();
                //processEvents();
                
            }

            Flag_InThreadLoop = false; // leaving the thread main loop
        }


        // The function that assembles incoming serial data and puts in a queue
        private void processData()
        {
            int i;
            // retrieving the number of available bytes in the serial port buffer
            int numBytesToRead = CommPort.BytesToRead;
            if (numBytesToRead > 0)
            //do
            {
                if (Data_RemainingBytes == 0)
                {// i.e., the previous message is complete (or we are commencing reception now)
                    if (numBytesToRead >= 2)
                    { // we have -at least-the message header in the buffer. 
                        
                        // Her's the CORRECT ORDER (i.e., the oldest byte is always read first from the buffer):
                        byte Header_High = (byte)CommPort.ReadByte();
                        byte Header_Low = (byte)CommPort.ReadByte();

                        // allocating space for the incoming message in raw form (i.e., byte array)
                        Data_Rawmessage = new byte[IMUMessage.c_MessageLength];

                        ushort head = (ushort)(Header_High * 256 + Header_Low);

                        if (head == IMUMessage.c_Header) // the buffer contains gibberish. Empty it.
                            //CommPort.DiscardInBuffer();
                        //else
                        {// the two bytes were equal to a header and therefore the remaining bytes for this message should be 34 - 2 = 32
                            Data_RemainingBytes = IMUMessage.c_MessageLength - 2;  // the size of the message MINUS the 2 bytes of the header
                            Data_Rawmessage[0] = Header_High; // storing in a MSB-comes-first-order
                            Data_Rawmessage[1] = Header_Low;
                        }
                    }
                }
                else if (numBytesToRead >= Data_RemainingBytes)
                { // the buffer contains the rest of the message for sure...
                    // reading the rest of the message into the allocated raw message buffer 
                    CommPort.Read(Data_Rawmessage, 2, Data_RemainingBytes);

                    //for (i = 2; i < IMUMessage.c_MessageLength; i++)
                    //    Data_Rawmessage[i] = (byte)CommPort.ReadByte();

                    // resetting the remaining bytes to 0
                    Data_RemainingBytes = 0;

                    // unwrapping the message (hopefully correctly...)
                    IMUMessage msg = IMUMessage.unwrapRawMessage(Data_Rawmessage);

                    // ************* checking for cheksum errors ********************
                    ushort estimatedChecksum = 0;
                    UInt32 sum = 0;

                    for (i = 0; i < IMUMessage.c_MessageLength/2 - 1; i++)
                    {
                        ushort u_word = (ushort)((ushort)(Data_Rawmessage[i*2] << 8) + (ushort)Data_Rawmessage[2*i + 1]);
                        sum += u_word;
                    }

                    int temp = (int)(0 - (sum & 0xffff));

                    estimatedChecksum = (ushort)temp;
                    
                    

                    // ****************************************************************

                    
                    // adding the message to the queue
                    //MsgQueueSemaphore.WaitOne(); // blocking the queue from external thread access
                    
                    //MessageQueue.addMessage(msg);
                    
                    //MsgQueueSemaphore.Release(); // releasing the structure for access

                    // and updating the last known reading
                    Last_Acc1 = msg.Axis1_Accelerometer; Last_Acc2 = msg.Axis2_Accelerometer; Last_Acc3 = msg.Axis3_Accelerometer;
                    Last_Angvel1 = msg.Axis1_Rate; Last_Angvel2 = msg.Axis2_Rate; Last_Angvel3 = msg.Axis3_Rate;


                    // for demonstration
                    /*Console.Write("Estimated checksum : ");
                    Console.Write(estimatedChecksum);
                    Console.Write(" and actual checksum : "); 
                    Console.WriteLine( msg.Checksum);

                    Console.Write("Axis 1 rate :");
                    Console.WriteLine(msg.Axis1_Rate);


                    Console.Write("Axis 1 Temperature :");
                    Console.WriteLine(msg.Axis1_AccelerometerTemp);

                    Console.Write("Axis 1 Frequency :");
                    Console.WriteLine(msg.Axis2_Frequency);

                    Console.Write("Axis 2 Temperature :");
                    Console.WriteLine(msg.Axis2_AccelerometerTemp);

                    Console.Write("Axis 1 Quad:");
                    Console.WriteLine(msg.Axis1_Quad);

                    Console.Write("Axis 1 Acceleration: ");
                    Console.WriteLine(msg.Axis1_Accelerometer);

                    Console.Write("Axis 2 Acceleration: ");
                    Console.WriteLine(msg.Axis2_Accelerometer);

                    Console.Write("Axis 3 Acceleration: ");
                    Console.WriteLine(msg.Axis3_Accelerometer);
                        */
                }

                // updating the number of bytes left in the buffer
                //numBytesToRead = CommPort.BytesToRead;

            } //while ((numBytesToRead >= Data_RemainingBytes) && (Data_RemainingBytes > 0));

        }


        // return the connection status of the serial port object
        public Boolean isPortOpen()
        {
            return CommPort.IsOpen;
        }



    }
}
