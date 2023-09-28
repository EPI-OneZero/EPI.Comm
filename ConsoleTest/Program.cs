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
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        class MyClass
        {
            public int A;
            public int B;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public ushort[] Shorts;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        class MyClass2
        {
            public int A;
            public int B;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public char[] Chars;
        }

        public static void Main(string[] args)
        {

        }
        public static void ReverseEndian<T>(T t)
        {

        }


    }
}
