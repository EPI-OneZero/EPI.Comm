using EPI.Comm.Net.Events;
using EPI.Comm.Net.Generic.Events;
using System.Net;

namespace EPI.Comm
{
    public static class CommConfig
    {
        public const int DefaultBufferSize = 8192;
    }
    public interface IComm
    {
        void Send(byte[] bytes);
        event PacketEventHandler Received;
    }

    public interface IComm<Theader, Tfooter> : IEndian
    {
        void Send(Theader header, byte[] body, Tfooter footer);
        event PacketEventHandler<Theader, Tfooter> Received;


    }
    public interface IComm<Theader> : IEndian
    {
        void Send(Theader header, byte[] body);
        event PacketEventHandler<Theader> Received;
    }
    public interface IEndian
    {
        bool IsBigEndian { get; set; }
    }
}
