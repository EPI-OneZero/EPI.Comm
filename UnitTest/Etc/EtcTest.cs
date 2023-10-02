using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace UnitTest.Etc
{
    [TestClass]
    public class EtcTest
    {
        [TestMethod]
        public void Test()
        {
            var t1 = typeof(MyStruct);
            var t2 = typeof(MyClass);
            //Console.WriteLine(Marshal.SizeOf(t1));
            Console.WriteLine(Marshal.SizeOf(t2));

            var xf = t2.GetField("x");
            var ft = xf.FieldType;
            var xff = ft.GetFields().First().FieldType;
            Console.WriteLine(xf.Name);
            Console.WriteLine(ft.Name);
            Console.WriteLine(xff.Name);
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    unsafe struct MyStruct
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public char[] x;
        public char a;

    }
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    unsafe struct MyClass
    {
        public fixed char x[2];
    }

}
