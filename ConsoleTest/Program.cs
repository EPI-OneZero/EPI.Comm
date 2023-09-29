using EPI.Comm.Net;
using EPI.Comm.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ConsoleTest
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class MyClass
    {
        public byte A;
        public byte B;
        public byte C;
        Queue<string> x;
        public MyClass(byte a, byte b, byte c)
        {
            A = a;
            B = b;
            C = c;
        }
    }

    /// <summary>
    /// 프로그램
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            int x = 1;

            var fields = TypeUtil.GetFields(typeof(DateTime));
            foreach (var field in fields) 
            {
                PrintInfo(field);
                Console.WriteLine(field.GetValue(DateTime.MinValue));
                Console.WriteLine(field.GetValue(DateTime.MaxValue));
            }
        }
        private static void PrintInfo(FieldInfo field)
        {
            Console.WriteLine(field.Name);
            Console.WriteLine(field.FieldType.Name);
        }
    }
    class DServer : TcpServerBase
    {
        public DServer(int bufferSize) : base(bufferSize)
        {
        }

        internal override TcpClientBase CreateClient(TcpClient client)
        {
            throw new NotImplementedException();
        }

        internal override void OnClientConnected(TcpClientBase client)
        {
            throw new NotImplementedException();
        }

        internal override void OnClientDisconnected(TcpClientBase client)
        {
            throw new NotImplementedException();
        }
    }
}
