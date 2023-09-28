using System;

namespace EPI.Comm.Net.Events
{
    public class TcpEventArgs : EventArgs
    {
        public TcpNetClient TcpNetClient { get; private set; }
        public TcpEventArgs(TcpNetClient client)
        {
            TcpNetClient = client;
        }
    }
    public delegate void TcpEventHandler(object sender, TcpEventArgs e);
}
