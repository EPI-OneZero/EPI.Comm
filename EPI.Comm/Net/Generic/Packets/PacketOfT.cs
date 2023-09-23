using EPI.Comm.Buffers;
using System;
using System.Runtime.InteropServices;
using static EPI.Comm.Net.Generic.Packets.PacketSerializer;
namespace EPI.Comm.Net.Generic.Packets
{
    public class Packet<Theader> 
    {
        public Theader Header { get; private set; }
        public byte[] Body { get; private set; }
        public int HeaderSize { get; private set; }
        public virtual int FullSize => HeaderSize + Body?.Length ?? 0;
        public Func<Theader, int> GetBodySize { get; set; }
        internal Packet(Func<Theader, int> getBodySize)
        {
            GetBodySize = getBodySize;
            HeaderSize = Marshal.SizeOf(Header);
        }
        private protected bool HeaderDeserialized { get; private set; } = false;
        internal virtual bool TryDeserializePacket(IBuffer buffer)
        {
            if (HeaderDeserialized)
            {
                return TryDeserializeBody(buffer, GetBodySize?.Invoke(Header) ?? 0);
            }
            else
            {
                if (TryDeserializeHeader(buffer))
                {
                    HeaderDeserialized = true;
                    return TryDeserializePacket(buffer);
                }
                else return false;
            }
        }
        private bool TryDeserializeHeader(IBuffer buffer)
        {
            if (IsEnoughSize(buffer.Count, HeaderSize, 0))
            {
                Header = Deserialize<Theader>(buffer.GetBytes(HeaderSize), HeaderSize, 0);
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool TryDeserializeBody(IBuffer buffer, int bodySize)
        {
            if (IsEnoughSize(buffer.Count, bodySize, 0))
            {
                Body = buffer.GetBytes(bodySize);

                return true;
            }
            else
            {
                return false;
            }
        }
    }
    public sealed class Packet<Theader, Tfooter> : Packet<Theader>
    {
        public override int FullSize => base.FullSize + Marshal.SizeOf<Tfooter>();
        public Tfooter Footer { get; private set; }
        public int FooterSize { get; private set; }
        internal Packet(Func<Theader, int> getBodySize) : base(getBodySize)
        {
            FooterSize = Marshal.SizeOf<Tfooter>();
        }
        internal override bool TryDeserializePacket(IBuffer buffer)
        {
            if (base.TryDeserializePacket(buffer))
            {
                return TryDeserializeFooter(buffer);
            }
            else
            {
                return false;
            }
        }
        private bool TryDeserializeFooter(IBuffer buffer)
        {
            if (IsEnoughSize(buffer.Count, HeaderSize, 0))
            {
                Footer = Deserialize<Tfooter>(buffer.GetBytes(HeaderSize), HeaderSize, 0);
                return true;
            }
            else
            {
                return false;
            }
        }

    }

    
}
