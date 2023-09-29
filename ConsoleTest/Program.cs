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
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        class MyClass
        {
            public int A;
            public int B;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public ushort[] Shorts;
            public int D;
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
            public int D;
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
            var fields = ObjectUtil.GetFields(typeof(MyClass));

            foreach (var item in fields)
            {
                Console.WriteLine($"{item.Name}  {item.FieldType} {GetFieldOffset(item)} " +
                    $"{item.GetCustomAttribute(typeof(MarshalAsAttribute))?.GetType()?.Name ?? string.Empty}");
              
            }
            var x = IntPtr.Size;
        }
        public static void ReverseEndian<T>(T t)
        {
        }
        public static int GetFieldOffset(FieldInfo fi) =>
                    GetFieldOffset(fi.FieldHandle);

        public static int GetFieldOffset(RuntimeFieldHandle h) =>
                            Marshal.ReadInt32(h.Value + (4 + IntPtr.Size)) & 0xFFFFFF;


    }
}
