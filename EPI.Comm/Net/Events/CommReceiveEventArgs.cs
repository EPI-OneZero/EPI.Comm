using System;
using System.Net;

namespace EPI.Comm.Net.Events
{
    public class CommReceiveEventArgs : EventArgs
    {
        public IPEndPoint From { get; private set; }
        public byte[] ReceivedBytes { get; private set; }

        public CommReceiveEventArgs(IPEndPoint from, byte[] receivedBytes)
        {
            From = from;
            ReceivedBytes = receivedBytes;
        }
    }
    public delegate void CommReceiveEventHandler(object sender, CommReceiveEventArgs e);
}
