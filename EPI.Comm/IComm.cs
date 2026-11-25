using EPI.Comm.Net;
using EPI.Comm.Net.Generic;

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
    public interface IComm<THeader, TFooter> : ICommSend, IEndian
    {
        void Send(THeader header, byte[] body, TFooter footer);
        event PacketEventHandler<THeader, TFooter> Received;
    }
    public interface IComm<THeader> : ICommSend, IEndian
    {
        void Send(THeader header, byte[] body);
        event PacketEventHandler<THeader> Received;
    }
    public interface IEndian
    {
        bool IsBigEndian { get; set; }
    }
}
