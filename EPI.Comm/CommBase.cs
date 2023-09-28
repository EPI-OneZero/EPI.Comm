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
