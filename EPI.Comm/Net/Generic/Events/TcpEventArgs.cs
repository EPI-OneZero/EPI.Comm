using System;

namespace EPI.Comm.Net.Generic
{
    public class TcpEventArgs<THeader> : EventArgs where THeader : new()
    {
        public TcpNetClient<THeader> TcpNetClient { get; private set; }
        public TcpEventArgs(TcpNetClient<THeader> client)
        {
            TcpNetClient = client;
        }
    }
    public delegate void TcpEventHandler<THeader>(object sender, TcpEventArgs<THeader> e)
    where THeader : new();
    public class TcpEventArgs<THeader, TFooter> : EventArgs where THeader : new() where TFooter : new()
    {
        public TcpNetClient<THeader, TFooter> TcpNetClient { get; private set; }
        public TcpEventArgs(TcpNetClient<THeader, TFooter> client)
        {
            TcpNetClient = client;
        }
    }
    public delegate void TcpEventHandler<THeader, TFooter>(object sender, TcpEventArgs<THeader, TFooter> e)
        where THeader : new() where TFooter : new();



}
