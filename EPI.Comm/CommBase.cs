using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using EPI.Comm.Net.Events;
using EPI.Comm.Net.Generic.Events;

namespace EPI.Comm
{
    
    public abstract class CommBase
    {
        public const int DefaultBufferSize = 8192;
    }
    public interface IComm
    {
        void Send(byte[] bytes);
        event PacketEventHandler Received;
    }

    public interface IComm<Theader, Tfooter>
    {
        void Send(Theader header, byte[] body, Tfooter footer);
        event PacketEventHandler<Theader, Tfooter> Received;


    }
    public interface IComm<Theader>
    {
        void Send(Theader header, byte[] body);
        event PacketEventHandler<Theader> Received;

    }
}
