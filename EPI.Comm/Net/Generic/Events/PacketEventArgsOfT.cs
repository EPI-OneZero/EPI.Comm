using EPI.Comm.Net.Generic.Packets;
using System;
using System.Net;

namespace EPI.Comm.Net.Generic.Events
{
    public class PacketEventArgs<Theader> : EventArgs
    {
        public IPEndPoint From { get; private set; }
        public byte[] FullPacket { get; private set; }
        public Theader Header { get; private set; }
        public byte[] Body { get; private set; }
        internal PacketEventArgs(IPEndPoint from, PacketMaker<Theader> packet)
        {
            From = from;
            Header = packet.Header;
            Body = packet.Body;
            FullPacket = packet.FullPacket;
        }
    }
    public delegate void PacketEventHandler<Theader>(object sender, PacketEventArgs<Theader> e);

    public class PacketEventArgs<Theader, Tfooter> : EventArgs
    {
        public IPEndPoint From { get; private set; }
        public byte[] FullPacket { get; private set; }
        public Theader Header { get; private set; }
        public byte[] Body { get; private set; }
        public Tfooter Footer { get; private set; }
        internal PacketEventArgs(IPEndPoint from, PacketMaker<Theader, Tfooter> packet)
        {
            From = from;
            Header = packet.Header;
            Body = packet.Body;
            Footer = packet.Footer;
            FullPacket = packet.FullPacket;
        }
    }
    public delegate void PacketEventHandler<Theader, Tfooter>(object sender, PacketEventArgs<Theader, Tfooter> e);
}
