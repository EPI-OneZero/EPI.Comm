using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Runtime.Remoting.Messaging;
using System.Reflection;

namespace ConsoleTest
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    class MyClass
    {
        public int A  = 1;
        internal int B  = 2;
        private int C = 3;
        public override string ToString()
        {
            Queue<string> queue = new Queue<string>();
            return base.ToString(); 
        }
    }

    enum MyEnum
    {
        a,
        b,
        c,
    }
    unsafe internal class Program
    {
        static void Main(string[] args)
        {
            MyEnum x = MyEnum.a;
            switch (x)
            {
                case MyEnum.a:
                    x= MyEnum.b;
                    break;
                case MyEnum.b:
                    break;
                case MyEnum.c:
                    break;
                default:
                    break;
            }


        }
    }
}
