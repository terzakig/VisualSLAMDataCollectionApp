using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;

using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;

using FlyCapture2Managed;


namespace GPSVideoRecorder
{
    class ManagedImageRollingBuffer
    {
        public int MAX_LEN; // maximum length of the buffer
        public ManagedImage[] Buffer; // the buffer of manged images
        public int first, last; // first and last inded

        private Semaphore BufSemaphore; // a semaphore that locks/releases access to the Buffer
        private ManagedImage temp;      // an temporary image for storage of ca[ture
        // constructir
        public ManagedImageRollingBuffer(int maxlen)
        {
            int i;
            MAX_LEN = maxlen;
            Buffer = new ManagedImage[MAX_LEN];
            // pre-allocating buffers
            for (i = 0; i < MAX_LEN; i++)
                Buffer[i] = new ManagedImage();
           

            first = last = -1;
            BufSemaphore = new Semaphore(1, 1); // single access at a time

            temp = new ManagedImage(); // initialize the temporary image
        }

        public void clear()
        {
            int i;
            //BufSemaphore.WaitOne();

            first = last = -1;
            //BufSemaphore.Release();
        }

        public Boolean isEmpty()
        {
            //BufSemaphore.WaitOne();
            Boolean empty = ((first == -1) || (last == -1));
            //BufSemaphore.Release();

            return empty;
        }

        public int count() {
            if (isEmpty()) return 0;
            else if (first < last) return last - first +1;
            else return MAX_LEN - first + last + 1;
        }

        public void add(ManagedCamera cam)
        {

            cam.RetrieveBuffer(temp);

            BufSemaphore.WaitOne();
            
            if (isEmpty())
            {
                first = last = 0;
                //Buffer[last] = img;
                
            } else {
                last = (last + 1) % MAX_LEN;
                //Buffer[last] = img;
            }
            Buffer[last] = temp;
            
            BufSemaphore.Release();

        }

        public ManagedImage remove()
        {
            ManagedImage img = null;
            BufSemaphore.WaitOne();
            if ((first != -1) && (last != -1))
            { // not empty
                img = Buffer[first];
                //Buffer[first] = null;
                if (first == last) first = last = -1;
                else first = (first + 1) % MAX_LEN;
            }
            BufSemaphore.Release();

            return img;
        }

    }
}
