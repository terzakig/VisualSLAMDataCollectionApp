using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;



namespace GPSVideoRecorder
{
    class IMUMessageQueue
    {
        public const int c_StdMaxLength = 100; // by default 100 messages maximum in the queue


        private List<IMUMessage> Messages;
        public int MaxLength;

        // A semaphore to lock the queue while being updated, either for writing or for reading
        private Semaphore QueueAccessLock;

        public IMUMessageQueue()
        {
            Messages = new List<IMUMessage>();

            MaxLength = c_StdMaxLength;

            // creating the locking semaphore
            QueueAccessLock = new Semaphore(1, 1);
        }

        public IMUMessageQueue(int maxlength) {
            Messages = new List<IMUMessage>();

            MaxLength = maxlength;

            // creating the access lock semaphore
            QueueAccessLock = new Semaphore(1, 1);
        }


        // clears the queue
        public void clear()
        {
            Messages.Clear();
        }

        // retrives number of items oin the queue
        public int Count()
        {
            return Messages.Count();
        }

       // check if empty
        public Boolean isEmpty()
        {
            return (Messages.Count() > 0);
        }

        // add item
        public void addMessage(IMUMessage msg)
        {
            
            // locking queue acess vi the sepamphore
            QueueAccessLock.WaitOne();

            Messages.Add(msg);

            if (Messages.Count() > MaxLength)
                Messages.RemoveAt(0);

            // releasing access
            QueueAccessLock.Release();
            
        }


        // remove an item
        public IMUMessage removeMessage()
        {
            IMUMessage msg = null;

            // locking queue
            QueueAccessLock.WaitOne();

            if (!isEmpty())
            {
                msg = Messages[0];

                Messages.Remove(msg);
            }

            // releasing queue
            QueueAccessLock.Release();

            return msg;
        }



        // retrieve the queue messages as raw data in a matrix Nx6 st: every line is, a1 a2 a3 w1 w2 w3
        public float[,] getQueueRawData(int fromIndex, int toIndex)
        {
            int i;
            int N = toIndex - fromIndex + 1;
            float[,] imudata = new float[N, 6];

            for (i = fromIndex; i <= toIndex; i++)
            {
                imudata[i - fromIndex, 0] = Messages[i].Axis1_Accelerometer; // a1
                imudata[i - fromIndex, 1] = Messages[i].Axis2_Accelerometer; // a2
                imudata[i - fromIndex, 2] = Messages[i].Axis3_Accelerometer; // a3

                imudata[i - fromIndex, 3] = Messages[i].Axis1_Rate; // w1
                imudata[i - fromIndex, 1] = Messages[i].Axis2_Rate; // w2
                imudata[i - fromIndex, 2] = Messages[i].Axis3_Rate; // w3
            }

            return imudata;
        }

        public float[,] getQueueRawData()
        {
            int i;
            int N = Messages.Count();

            float[,] imudata = null;

            if (N > 0)
            {
                imudata = new float[N, 6];

                for (i = 0; i < N; i++)
                {
                    imudata[i, 0] = Messages[i].Axis1_Accelerometer; // a1
                    imudata[i, 1] = Messages[i].Axis2_Accelerometer; // a2
                    imudata[i, 2] = Messages[i].Axis3_Accelerometer; // a3

                    imudata[i, 3] = Messages[i].Axis1_Rate; // w1
                    imudata[i, 1] = Messages[i].Axis2_Rate; // w2
                    imudata[i, 2] = Messages[i].Axis3_Rate; // w3
                }
            }
            return imudata;
        }

    }
}
