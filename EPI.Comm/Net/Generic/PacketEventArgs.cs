using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPI.Comm.Net.Generic
{
    public class PacketEventArgs<Theader,Tfooter> : EventArgs
    {
        public Packet<Theader,Tfooter> Packet { get; internal set; }
        public PacketEventArgs() { }
    }
    public class PacketEventArgs<Theader> : EventArgs
    {
        public Packet<Theader> Packet { get; internal set; }
        public PacketEventArgs(Packet<Theader> packet) 
        {
            Packet = packet;
        }
    }
}
