using EPI.Comm.Net.Generic.Packets;
using System;
using System.Net;

namespace EPI.Comm.Net.Generic.Events
{
    public class PacketEventArgs<Theader, Tfooter> : EventArgs
    {
        public IPEndPoint From { get; private set; }
        public Packet<Theader, Tfooter> Packet { get; internal set; }
        internal PacketEventArgs(IPEndPoint from, Packet<Theader, Tfooter> packet)
        {
            From = from;
            Packet = packet;
        }
    }
    public delegate void PacketEventHandler<Theader, Tfooter>(object sender, PacketEventArgs<Theader, Tfooter> e);
    public class PacketEventArgs<Theader> : EventArgs
    {
        public IPEndPoint From { get; private set; }
        public Packet<Theader> Packet { get; internal set; }
        internal PacketEventArgs(IPEndPoint from, Packet<Theader> packet)
        {
            From = from;
            Packet = packet;
        }
    }

    public delegate void PacketEventHandler<Theader>(object sender, PacketEventArgs<Theader> e);

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
