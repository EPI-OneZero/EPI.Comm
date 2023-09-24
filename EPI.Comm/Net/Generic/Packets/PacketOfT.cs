using EPI.Comm.Buffers;
using EPI.Comm.Utils;
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
        public int BodySize => Body?.Length ?? 0;
        public virtual int FullSize => HeaderSize + BodySize;
        public Func<Theader, int> GetBodySize { get; set; }
        internal Packet(Func<Theader, int> getBodySize)
        {
            GetBodySize = getBodySize;
            HeaderSize = ObjectUtil.SizeOf<Theader>();
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
                Header = DeserializeByMarshal<Theader>(buffer.GetBytes(HeaderSize), HeaderSize, 0);
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
        public override int FullSize => base.FullSize + ObjectUtil.SizeOf<Tfooter>();
        public Tfooter Footer { get; private set; }
        public int FooterSize { get; private set; }
        internal Packet(Func<Theader, int> getBodySize) : base(getBodySize)
        {
            FooterSize = ObjectUtil.SizeOf<Tfooter>();
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
                Footer = DeserializeByMarshal<Tfooter>(buffer.GetBytes(HeaderSize), HeaderSize, 0);
                return true;
            }
            else
            {
                return false;
            }
        }

    }

    
}
