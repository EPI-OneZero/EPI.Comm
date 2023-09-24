using System;
using System.Net;

namespace EPI.Comm.Net.Events
{
    /// <summary>
    /// 질문: 이놈 빼고도 모든 EventArgs들 이놈들 이름은 적절한가? ,  
    /// </summary>
    public class PacketEventArgs : EventArgs
    {
        public IPEndPoint From { get; private set; }
        public byte[] ReceivedBytes { get; private set; }

        public PacketEventArgs(IPEndPoint from, byte[] receivedBytes)
        {
            From = from;
            ReceivedBytes = receivedBytes;
        }
    }
    public delegate void PacketEventHandler(object sender, PacketEventArgs e);

    public class SocketReceiveEventArgs
    {
        public IPEndPoint From { get; private set; }
        public byte[] ReceivedBytes { get; private set; }
        public SocketReceiveEventArgs(IPEndPoint from, byte[] receivedBytes)
        {
            From = from;
            ReceivedBytes = receivedBytes;
        }
    }
    public delegate void SocketReceiveEventHandler(object sender, SocketReceiveEventArgs e);
}
