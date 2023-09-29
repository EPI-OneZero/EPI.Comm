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
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
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
            var s1 = Marshal.SizeOf(typeof(MyStruct));
            var s2 = Marshal.SizeOf(typeof(MyStruct2));
            var fields = GetFields(typeof(MyObject));
            var type = typeof(MyObject);
            var s = type.IsAnsiClass;
            var layout = GetStructLayout(typeof(MyObject));
            var layout1 = GetStructLayout(typeof(MyStruct));
            var layout2 = GetStructLayout(typeof(MyStruct2));
            var lay = typeof(MyClass).StructLayoutAttribute;
            foreach (var field in fields) 
            {
                PrintInfo(field);
            }
        }
        private static void PrintInfo(FieldInfo field)
        {
            Console.WriteLine("Name : " + field.Name);
            Console.WriteLine("Type : " + field.FieldType.Name);
            var inter = field.FieldType.GetInterface("IEnumerable`1");
            Console.WriteLine("GenericTypeArguments");
            foreach (var item in inter?.GenericTypeArguments?? new Type[0])
            {
                Console.WriteLine($"{item.Name}");
            }
            var marshalAs = GetMarshalAs(field);
            if(marshalAs != null)
            {
                Console.WriteLine("MarshalAs");
                Console.WriteLine(marshalAs.Value);
                Console.WriteLine(marshalAs.SizeConst);
            }
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
        public static MarshalAsAttribute GetMarshalAs(MemberInfo field)
        {
            var result = field.GetCustomAttribute<MarshalAsAttribute>();
            return result;
        }
        public static TypeAttributes GetStructLayout(Type type)
        {
            var result = type.Attributes;
            var x = type.GetCustomAttributes();
            return result;
        }
    }
}
