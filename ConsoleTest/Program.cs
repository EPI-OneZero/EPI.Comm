using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ConsoleTest
{
    [StructLayout(LayoutKind.Sequential ,Pack =1)]
    class MyClass2 : MyClass1
    {
        public byte C { get; set; }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    class MyClass1
    {
        public byte A { get; set; }
        public byte B { get; set; }
    }

    /// <summary>
    /// 프로그램
    /// </summary>
    public class Program
    {
        /// <summary>
        /// 메인함수
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var bytes = new byte[65535];
            var udp = new UdpClient();
            var ip = new IPEndPoint(IPAddress.Loopback, 7777);
            udp.Client.Bind(ip);

            udp.Connect(ip);
            udp.Send(new byte[4] { 1, 2, 3, 4 },4);
            while (true)
            {
                var recv = udp.Receive(ref ip);
                Console.WriteLine(recv);
            }
        }


    }
}
