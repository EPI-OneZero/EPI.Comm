using EPI.Comm.Net.Generic.Packets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.CodeDom;
using System.Net;
using System.Runtime.InteropServices;
using static EPI.Comm.Utils.MarshalSerializer;
namespace UnitTest.Endian
{
    
    [TestClass]
    public class EndianTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var outer = new Outer();
            var size= Marshal.SizeOf(outer);
            var bytes = new byte[size];
            Serialize(outer, bytes, 0, size);
            var now0 = DateTime.Now;
            ReverseEndian<Outer>(bytes, 0);
            var now1 = DateTime.Now;
            outer.ReverseEndian();


            var now2 = DateTime.Now;
            var dt2 = now2 - now1;
            var dt1 = now1 - now0;
            Console.WriteLine(dt1.TotalMilliseconds);
            Console.WriteLine(dt2.TotalMilliseconds);
            var t = Deserialize<Outer>(bytes,false);
            Assert.AreEqual(outer, t);
        }

        [TestMethod]
        public void TestMethod2() 
        {
            Console.WriteLine(Marshal.SizeOf(typeof(Myenum)));
            var type = Enum.GetUnderlyingType(typeof(MyEnum));
            Console.WriteLine(Marshal.OffsetOf(typeof(Myenum), "a")) ;
            Console.WriteLine(Marshal.SizeOf(type)) ;
        }
       

    }
    #region Model
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Outer
    {
        public byte A = 0x01;
        public short B = 0x2345;
        public int C = 0x67890123;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public Inner[] Inners = new Inner[10];
        public long D = 0x4567890123456789;
        public Outer()
        {
            var random = new Random();
            B= (short)random.Next(short.MinValue,short.MaxValue);
            C= random.Next(int.MinValue,int.MaxValue);
            D = (((long)random.Next(int.MinValue, int.MaxValue)) << 32) | (long)random.Next(int.MinValue, int.MaxValue);
            for (int i = 0; i < Inners.Length; i++)
            {
                Inners[i] = new Inner();
            }
        }
        public void ReverseEndian()
        {
            B = IPAddress.HostToNetworkOrder(B);
            C = IPAddress.HostToNetworkOrder(C);
            D = IPAddress.HostToNetworkOrder(D);
            for (int i = 0; i < Inners.Length; i++)
            {
                Inners[i].ReverseEndian();
            }
        }
        public override bool Equals(object obj)
        {
            var other = obj as Outer;
            if(A !=other.A)
            {
                return false;
            }
            if(B != other.B)
            {
                return false;
            }
            if (C != other.C)
            {
                return false;
            }
            if (D != other.D)
            {
                return false;
            }
            for (int i = 0; i < Inners.Length; i++)
            {
                if (!Inners[i].Equals(other.Inners[i]))
                {
                    return false;
                }
            }
           
            return true;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Inner
    {
        public byte A = 0x01;
        public short B = 0x1234;
        public int C = 0x23456789;
        public long D = 0x7891234567803210;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst =5)]
        public short[] shorts = new short[5];
        public Inner()
        {
            for (int i = 0; i < shorts.Length; i++)
            {
                shorts[i] = (short)(i * 0xff);
            }
        }
        public void ReverseEndian()
        {
            B = IPAddress.HostToNetworkOrder(B);
            C = IPAddress.HostToNetworkOrder(C);
            D = IPAddress.HostToNetworkOrder(D);
            for (int i = 0; i < shorts.Length; i++)
            {
                shorts[i] = IPAddress.HostToNetworkOrder(shorts[i]);
            }
        }
        public override bool Equals(object obj)
        {
            var other = obj as Inner;
            if (A != other.A)
            {
                return false;
            }
            if (B != other.B)
            {
                return false;
            }
            if (C != other.C)
            {
                return false;
            }
            if (D != other.D)
            {
                return false;
            }
            return true;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
    #endregion
    #region Model2
    [StructLayout( LayoutKind.Sequential, Pack =1)]
    struct Myenum
    {
        int b;
        MyEnum a;
    }
    enum MyEnum :byte
    {
        a,b,c= 2
    }

    [StructLayout(LayoutKind.Sequential, Pack =1)]
    class AAA
    {

    }
    [StructLayout(LayoutKind.Sequential)]
    class BBB
    {
        int a;
        short b;
        int c;
        short d;
    }
    #endregion
}
