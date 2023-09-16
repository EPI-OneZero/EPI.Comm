using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPI.Comm.Tcp.Generic
{
    internal class PacketEventArgs<Theader,Tfooter> : EventArgs
    {
        public PacketEventArgs() { }
    }
}
