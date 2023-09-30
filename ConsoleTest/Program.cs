using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EPI.Comm.Net;
using EPI.Comm.Utils;

namespace ConsoleTest
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct MyObject
    {
        public byte A;
        public int B;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public int[] C;
        public fixed byte D[10];
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
        public string S;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    struct MyStruct
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
        public string S; // charset ansi =  1바이트
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    struct MyStruct2
    {
        [MarshalAs(UnmanagedType.LPStr, SizeConst = 10)]
        public string S;
    }
    class MyClass
    {

    }
    /// <summary>
    /// 프로그램
    /// </summary>
    unsafe public class Program
    {
        public static void Main(string[] args)
        {
        }
        
    }
}
