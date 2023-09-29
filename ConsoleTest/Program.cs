using System;
using System.Collections;
using System.Collections.Generic;
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
        [MarshalAs(UnmanagedType.ByValArray, SizeConst =2)]
        public int[] C;
        public fixed byte D[10];
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst =10)]
        public string S;
    }

    /// <summary>
    /// 프로그램
    /// </summary>
    unsafe public class Program
    {
        public static void Main(string[] args)
        {

            var fields = GetFields(typeof(MyObject));
            foreach (var field in fields) 
            {
                PrintInfo(field);
            }
        }
        private static void PrintInfo(FieldInfo field)
        {
            Console.WriteLine("Name : " + field.Name);
            Console.WriteLine("Type : " + field.FieldType.Name);
            try
            {
                Console.WriteLine("Offset : " + Marshal.OffsetOf(field.ReflectedType, field.Name));
                try
                {
                    Console.WriteLine("Size : " + Marshal.SizeOf(field.FieldType));
                }
                catch (Exception)
                {
                }
                Console.WriteLine(field.FieldType.IsPrimitive);
                if (!field.FieldType.IsPrimitive)
                {
                    Console.WriteLine();
                    var fields = GetFields(field.FieldType);
                    foreach (var f in fields)
                    {
                        PrintInfo(f);
                    }
                }
            }
            catch (Exception e) { }
            finally { Console.WriteLine(); }
        }
        public static FieldInfo[] GetFields(Type type)
        {
            var result = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return result;
        }
    }
}
