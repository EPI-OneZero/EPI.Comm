using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ConsoleTest
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MyClass
    {
        public byte A;
        public byte B;
        public byte C;
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

        class MyClass
        {
            public void Test()
            {
                Console.WriteLine("old");
            }
        }
        class MyClass2 : MyClass
        {
            new public void Test()
            {
                Console.WriteLine("new");
            }
        }
        /// <summary>
        /// 메인함수
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            MyClass2 a = new MyClass2();
            MyClass b = a;
            a.Test();
            b.Test();
        }


    }
}
