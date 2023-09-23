using EPI.Comm.Net.Generic;
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
    public delegate void CommEventHandler(object sender, TcpEventArgs e);
    public class TcpEventArgs<Theader> : EventArgs
    {
        public TcpNetClient<Theader> TcpNetClient { get; private set; }
        public TcpEventArgs(TcpNetClient<Theader> client)
        {
            TcpNetClient = client;
        }
    }
    public delegate void TcpEventHandler<Theader>(object sender, TcpEventArgs<Theader> e);
    public class TcpEventArgs<Theader, Tfooter> : EventArgs
    {
        public TcpNetClient<Theader, Tfooter> TcpNetClient { get; private set; }
        public TcpEventArgs(TcpNetClient<Theader, Tfooter> client)
        {
            TcpNetClient = client;
        }
    }
    public delegate void TcpEventHandler<Theader, Tfooter>(object sender, TcpEventArgs<Theader, Tfooter> e);
}
