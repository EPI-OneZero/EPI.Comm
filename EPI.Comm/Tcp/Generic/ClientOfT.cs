using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EPI.Comm.Tcp.Generic
{
    public struct EmptyFooter
    {
    }
    public class Client<Theader,TFooter> : Client
    {
        public Client(string ip, int port, int bufferSize, Func<Theader, int> getPacketSize) : base(ip, port, bufferSize)
        {

        }
        public Client(string ip, int port, Func<Theader, int> getPacketSize) : this(ip, port, 8192, getPacketSize)
        {

        }
    }
    public class Client<Theader> : Client<Theader, EmptyFooter>
    {
        public Client(string ip, int port, Func<Theader, int> getPacketSize) : base(ip, port, getPacketSize)
        {
        }

        public Client(string ip, int port, int bufferSize, Func<Theader, int> getPacketSize) : base(ip, port, bufferSize, getPacketSize)
        {
        }
    }
}
