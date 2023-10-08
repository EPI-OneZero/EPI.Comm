using EPI.Comm.Net.Events;
using EPI.Comm.Net.Generic.Events;

namespace EPI.Comm
{
    public static class CommConfig
    {
        public const int DefaultBufferSize = 8192;
    }
    public interface IComm : ICommSend, ICommReceive
    {

    }
    public interface ICommSend
    {
        void Send(byte[] bytes);
    }
    public interface ICommReceive
    {
        event PacketEventHandler Received;
    }
    public interface IComm<Theader, Tfooter> : ICommSend, IEndian
    {
        void Send(Theader header, byte[] body, Tfooter footer);
        event PacketEventHandler<Theader, Tfooter> Received;
    }
    public interface IComm<Theader> : ICommSend, IEndian
    {
        void Send(Theader header, byte[] body);
        event PacketEventHandler<Theader> Received;
    }
    public interface IEndian
    {
        bool IsBigEndian { get; set; }
    }
}
