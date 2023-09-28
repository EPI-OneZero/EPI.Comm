using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.InteropServices;

namespace UnitTest
{
    [StructLayout(LayoutKind.Sequential, Pack =1)]
    class MyClass
    {
        public int A = 1;
        public int B = 2;
        public int C = 3;
        public int D = 4;
    }
    [TestClass]
    public class UnitTest1
    {
        const int Count = 10000000;
        [TestMethod]
        public void TestMethod1()
        {
            var obj = new MyClass();
            var size = Marshal.SizeOf(obj);
            for (int i = 0; i < Count; i++)
            {
                var bytes = new byte[size];
                var ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(obj, ptr, false);
                Marshal.Copy(ptr,bytes,0,size);
                Marshal.DestroyStructure(ptr, typeof(MyClass));
                Marshal.FreeHGlobal(ptr);
            }
        }

        [TestMethod]
        public void TestMethod2()
        {
            var obj = new MyClass();
            var size = Marshal.SizeOf(obj);
            for (int i = 0; i < Count; i++)
            {
                var bytes = new byte[size];
                var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                var ptr = handle.AddrOfPinnedObject();
                Marshal.StructureToPtr(obj, ptr, false);
                Marshal.DestroyStructure(ptr, typeof(MyClass));
                handle.Free();

            }
        }
    }
}
