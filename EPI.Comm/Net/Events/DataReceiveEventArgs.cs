using System;
using System.Net;

namespace EPI.Comm.Net.Events
{
    public class PacketEventArgs : EventArgs
    {
        public object From { get; private set; }
        public byte[] ReceivedBytes { get; private set; }

        public PacketEventArgs(object from, byte[] receivedBytes)
        {
            From = from;
            ReceivedBytes = receivedBytes;
        }
    }
    public delegate void PacketEventHandler(object sender, PacketEventArgs e);
}
