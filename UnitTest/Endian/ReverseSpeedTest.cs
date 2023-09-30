using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Net;

namespace UnitTest.Endian
{
    [TestClass]
    public class ReverseSpeedTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            byte[] data = new byte[32];
            short[] longs= new short[16];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)i;
            }
            for (int i = 0; i < longs.Length; i++)
            {
                longs[i] = (short)i;
            }
            var now0 = DateTime.Now;
            for (int i = 0; i < 100000; i++)
            {
                Array.Reverse(data, 0, 2);
                Array.Reverse(data, 2, 2);
                Array.Reverse(data, 4, 2);
                Array.Reverse(data, 6, 2);
                Array.Reverse(data, 8, 2);
                Array.Reverse(data, 10, 2);
                Array.Reverse(data, 12, 2);
                Array.Reverse(data, 14, 2);

                Array.Reverse(data, 16, 2);
                Array.Reverse(data, 18, 2);
                Array.Reverse(data, 20, 2);
                Array.Reverse(data, 22, 2);
                Array.Reverse(data, 24, 2);
                Array.Reverse(data, 26, 2);
                Array.Reverse(data, 28, 2);
                Array.Reverse(data, 30, 2);
            }
            
            var now1 = DateTime.Now;
            for (int i = 0; i < 100000; i++)
            {
                longs[0] = IPAddress.HostToNetworkOrder(longs[0]);
                longs[1] = IPAddress.HostToNetworkOrder(longs[1]);
                longs[2] = IPAddress.HostToNetworkOrder(longs[2]);
                longs[4] = IPAddress.HostToNetworkOrder(longs[4]);
                longs[5] = IPAddress.HostToNetworkOrder(longs[5]);
                longs[6] = IPAddress.HostToNetworkOrder(longs[6]);
                longs[7] = IPAddress.HostToNetworkOrder(longs[7]);
                longs[8] = IPAddress.HostToNetworkOrder(longs[8]);
                longs[9] = IPAddress.HostToNetworkOrder(longs[9]);
                longs[10] = IPAddress.HostToNetworkOrder(longs[10]);
                longs[11] = IPAddress.HostToNetworkOrder(longs[11]);
                longs[12] = IPAddress.HostToNetworkOrder(longs[12]);
                longs[13] = IPAddress.HostToNetworkOrder(longs[13]);
                longs[14] = IPAddress.HostToNetworkOrder(longs[14]);
                longs[15] = IPAddress.HostToNetworkOrder(longs[15]);
            }

        
            var now2 = DateTime.Now;

            var dt1 = now1 - now0;
            var dt2 = now2 - now1;
            Console.WriteLine(dt1.TotalMilliseconds);
            Console.WriteLine(dt2.TotalMilliseconds);
        }
      
    }
}
