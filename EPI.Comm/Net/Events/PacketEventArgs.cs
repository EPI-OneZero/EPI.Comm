using System;
using System.Net;

namespace EPI.Comm.Net.Events
{
    public class PacketEventArgs : EventArgs
    {
        public byte[] ReceivedBytes { get; private set; }

        public PacketEventArgs(byte[] receivedBytes)
        {
            ReceivedBytes = receivedBytes;
        }
    }
    public delegate void PacketEventHandler(object sender, PacketEventArgs e);

    internal class SocketReceiveEventArgs
    {
        public IPEndPoint From { get; private set; }
        public byte[] ReceivedBytes { get; private set; }
        public SocketReceiveEventArgs(IPEndPoint from, byte[] receivedBytes)
        {
            From = from;
            ReceivedBytes = receivedBytes;
        }
    }
    internal delegate void SocketReceiveEventHandler(object sender, SocketReceiveEventArgs e);
}
