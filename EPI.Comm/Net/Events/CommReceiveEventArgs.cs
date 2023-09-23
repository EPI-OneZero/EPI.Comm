using System;

namespace EPI.Comm.Net.Events
{
    public class CommReceiveEventArgs : EventArgs
    {
        public byte[] ReceivedBytes { get; private set; }

        public CommReceiveEventArgs(byte[] receivedBytes)
        {
            ReceivedBytes = receivedBytes;
        }
    }
    public delegate void CommReceiveEventHandler(object sender, CommReceiveEventArgs e);
}
