using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EPI.Comm.Tcp.Generic
{
    public class Packet<Theader, Tfooter>
    {
        public Theader Header { get; set; }
        public byte[] Body { get; set; }
        public Tfooter Footer { get; set; }
        public Packet() 
        {

        }
    }
    public class Client<Theader> : Client where Theader : struct
    {
        public Client(string ip, int port, int bufferSize = ushort.MaxValue) : base(ip, port, bufferSize)
        {
        }
    }
    public class Client<Theader, Tfooter> : Client<Theader> where Theader : struct where Tfooter : struct
    {
        public Client(string ip, int port, int bufferSize = ushort.MaxValue) : base(ip, port, bufferSize)
        {
        }
    }
}
