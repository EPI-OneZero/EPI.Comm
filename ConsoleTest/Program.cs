using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTest
{
    internal class Program
    {
        [StructLayout(LayoutKind.Sequential,CharSet = CharSet.Unicode, Pack =1)]
        public class MyClass
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
            public string c;
            public int A;
            public short a;
            public int B;
            public short b;
        }
        static void Main(string[] args)
        {
            var handle = GCHandle.Alloc(new char[] { '가', '나', '\0','b', (char)1,(char)0,(char)3, (char)2, (char)0,(char)4 }, GCHandleType.Pinned);
            var size = Marshal.SizeOf(typeof(MyClass));
            object s = Marshal.PtrToStructure<MyClass>(handle.AddrOfPinnedObject());
            handle.Free();
        }
    }
}
