using System;

namespace EPI.Comm.Net.Generic.Events
{
    public class TcpEventArgs<Theader> : EventArgs where Theader : new()
    {
        public TcpNetClient<Theader> TcpNetClient { get; private set; }
        public TcpEventArgs(TcpNetClient<Theader> client)
        {
            TcpNetClient = client;
        }
    }
    public delegate void TcpEventHandler<Theader>(object sender, TcpEventArgs<Theader> e)
    where Theader : new();
    public class TcpEventArgs<Theader, Tfooter> : EventArgs where Theader : new() where Tfooter : new()
    {
        public TcpNetClient<Theader, Tfooter> TcpNetClient { get; private set; }
        public TcpEventArgs(TcpNetClient<Theader, Tfooter> client)
        {
            TcpNetClient = client;
        }
    }
    public delegate void TcpEventHandler<Theader, Tfooter>(object sender, TcpEventArgs<Theader, Tfooter> e)
        where Theader : new() where Tfooter : new();



}
