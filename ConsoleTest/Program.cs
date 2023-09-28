using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
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
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        class MyClass
        {
            public int A;
            public int B;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public ushort[] Shorts;
            public MyClass()
            {
                Shorts = new ushort[3] { 65, 0, 66 };
            }
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        class MyClass2
        {
            public int A;
            public int B;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public char[] Chars;
            public MyClass2()
            {
                Chars = new char[3];
            }
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct MyStruct
        {
            public int AAA;
            public int BBB;
            public string C;
            public string D;

        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        class MyStruct2
        {
            public int AAA;
            public int BBB;
            public string C;
            public string D;

        }
        public static void Main(string[] args)
        {
            try
            {
                var m1 = new MyClass() { A = 1, B = 2 };
                var m3 = new MyStruct() { AAA = 1, BBB = 2, C = "0123456789", D = "9876543210" };
                var size1 = Marshal.SizeOf(m3);
                byte[] bytes = new byte[size1];
                var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                var ptr1 = handle.AddrOfPinnedObject();
                Marshal.StructureToPtr(m3, handle.AddrOfPinnedObject(), false);
                var t = Marshal.PtrToStructure<MyStruct2>(ptr1);
                var t2 = Marshal.PtrToStructure<MyStruct2>(ptr1);
                Marshal.DestroyStructure(ptr1, typeof(MyStruct));
                var t3 =  Marshal.PtrToStructure<MyStruct2>(ptr1);
                var f1 = object.ReferenceEquals(m3.C, t.C);
                var f2 = object.ReferenceEquals(m3.D, t.D);
                Marshal.DestroyStructure(handle.AddrOfPinnedObject(), typeof(MyStruct));
                handle.Free();


            }
            catch (Exception)
            {

            }

        }
        public static void ReverseEndian<T>(T t)
        {

        }


    }
}
