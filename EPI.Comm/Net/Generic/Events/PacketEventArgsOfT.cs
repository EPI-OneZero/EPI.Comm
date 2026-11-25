using System;
using System.Net;

namespace EPI.Comm.Net.Generic
{
    public class PacketEventArgs<THeader> : EventArgs
    {
        public IPEndPoint From { get; private set; }
        public byte[] FullPacket { get; private set; }
        public THeader Header { get; private set; }
        public byte[] Body { get; private set; }
        internal PacketEventArgs(IPEndPoint from, PacketMaker<THeader> packet)
        {
            From = from;
            Header = packet.Header;
            Body = packet.Body;
            FullPacket = packet.FullPacket;
        }
    }
    public delegate void PacketEventHandler<THeader>(object sender, PacketEventArgs<THeader> e);

    public class PacketEventArgs<THeader, TFooter> : EventArgs
    {
        public IPEndPoint From { get; private set; }
        public byte[] FullPacket { get; private set; }
        public THeader Header { get; private set; }
        public byte[] Body { get; private set; }
        public TFooter Footer { get; private set; }
        internal PacketEventArgs(IPEndPoint from, PacketMaker<THeader, TFooter> packet)
        {
            From = from;
            Header = packet.Header;
            Body = packet.Body;
            Footer = packet.Footer;
            FullPacket = packet.FullPacket;
        }
    }
    public delegate void PacketEventHandler<THeader, TFooter>(object sender, PacketEventArgs<THeader, TFooter> e);
}
