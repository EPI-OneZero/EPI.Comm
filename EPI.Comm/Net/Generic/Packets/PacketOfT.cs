using EPI.Comm.Buffers;
using System;
using System.Runtime.InteropServices;
using static EPI.Comm.Utils.MarshalSerializer;
namespace EPI.Comm.Net.Generic.Packets
{
    public class Packet<Theader>
    {
        #region Field & Property
        private protected QueueBuffer queue;
        private enum DeserializeState
        {
            None,
            HeaderCompleted,
            BodyCompleted,
        }
        public byte[] FullPacket { get; private set; }
        public Theader Header { get; private set; }
        public byte[] Body { get; private set; }
        public int HeaderSize { get; private set; }
        public int BodySize => Body?.Length ?? 0;
        public virtual int FullSize => HeaderSize + BodySize;
        public Func<Theader, int> GetBodySize { get; set; }
        #endregion

        #region CTOR
        internal Packet(Func<Theader, int> getBodySize) : this(default(Theader), null, getBodySize)
        {

        }
        internal Packet(Theader header, byte[] body, Func<Theader, int> getBodySize)
        {
            GetBodySize = getBodySize;
            HeaderSize = Marshal.SizeOf<Theader>();
            queue = new QueueBuffer();
            Header = header;
            Body = body;
        }
        #endregion

        #region Method
        protected int CalculateBodySize()
        {
            return GetBodySize?.Invoke(Header) ?? 0;
        }
        private DeserializeState state = DeserializeState.None;
        internal bool TryDeserialize(IBuffer buffer,bool isBigEndian)
        {
            var success = TryDeserializePacket(buffer, isBigEndian);
            if (success)
            {
                FullPacket = queue.GetBytes(queue.Count);
            }
            return success;
        }
        private protected virtual bool TryDeserializePacket(IBuffer buffer, bool isBigEndian)
        {
            switch (state)
            {
                case DeserializeState.None:
                    if (TryDeserializeHeader(buffer, isBigEndian))
                    {
                        return TryDeserializeBody(buffer);
                    }
                    else return false;
                case DeserializeState.HeaderCompleted:
                    return TryDeserializeBody(buffer);
                case DeserializeState.BodyCompleted:
                    return true;
                default:
                    throw new InvalidOperationException($"Unexpected state: {state}");
            }
        }
        private bool TryDeserializeHeader(IBuffer buffer,bool isBigEndian)
        {
            if (IsEnoughSizeToDeserialize(buffer.Count, HeaderSize))
            {
                var bytes = buffer.GetBytes(HeaderSize);
                if (isBigEndian)
                {
                    ReverseEndian<Theader>(bytes, 0);
                }
                queue.AddBytes(bytes);
                Header = Deserialize<Theader>(bytes);
                state = DeserializeState.HeaderCompleted;
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool TryDeserializeBody(IBuffer buffer)
        {
            var bodySize = CalculateBodySize();
            if (IsEnoughSizeToDeserialize(buffer.Count, bodySize))
            {
                var bytes = buffer.GetBytes(bodySize);
                queue.AddBytes(bytes);
                Body = bytes;
                state = DeserializeState.BodyCompleted;
                return true;
            }
            else
            {
                return false;
            }
        }
       
        internal virtual byte[] SerializePacket(bool isBigEndian)
        {
            int bodySize = Body.Length;
            var headerDefinedBodySize = CalculateBodySize();
            if (headerDefinedBodySize == bodySize)
            {
                var fullPacketBytes = new byte[FullSize];
                Serialize(Header, fullPacketBytes, 0, HeaderSize);
                Buffer.BlockCopy(Body, 0, fullPacketBytes, HeaderSize, bodySize);
                if (isBigEndian)
                {
                    ReverseEndian<Theader>(fullPacketBytes, 0);
                }
                return fullPacketBytes;
            }
            else
            {
                throw new ArgumentOutOfRangeException
                    ($"Body length is differs from header definition.\r\n body length :{BodySize}, header definition : {headerDefinedBodySize}");
            }

        }
        #endregion
    }
    public sealed class Packet<Theader, Tfooter> : Packet<Theader>
    {
        #region Field & Property
        public override int FullSize => base.FullSize + FooterSize;
        public Tfooter Footer { get; private set; }
        public int FooterSize { get; private set; }
        #endregion

        #region CTOR
        internal Packet(Func<Theader, int> getBodySize) : this(default(Theader), null, default(Tfooter), getBodySize)
        {

        }
        internal Packet(Theader header, byte[] body, Tfooter footer, Func<Theader, int> getBodySize) : base(header, body, getBodySize)
        {
            Footer = footer;
            FooterSize = Marshal.SizeOf<Tfooter>();
        }
        #endregion

        #region Method
        private protected override bool TryDeserializePacket(IBuffer buffer, bool isBigEndian)
        {
            if (base.TryDeserializePacket(buffer, isBigEndian))
            {
                return TryDeserializeFooter(buffer, isBigEndian);
            }
            else
            {
                return false;
            }
        }
        private bool TryDeserializeFooter(IBuffer buffer, bool isBigEndian)
        {
            if (IsEnoughSizeToDeserialize(buffer.Count, FooterSize))
            {
                var bytes = buffer.GetBytes(FooterSize);
                if(isBigEndian)
                {
                    ReverseEndian<Tfooter>(bytes,0);
                }
                queue.AddBytes(bytes);
                Footer = Deserialize<Tfooter>(bytes);
                return true;
            }
            else
            {
                return false;
            }
        }

        internal override byte[] SerializePacket(bool isBigEndian)
        {
            var fullPacketBytes = base.SerializePacket(isBigEndian);
            Serialize(Footer, fullPacketBytes, HeaderSize + BodySize, FooterSize);
            if(isBigEndian)
            {
                ReverseEndian<Tfooter>(fullPacketBytes,HeaderSize + BodySize);
            }
            return fullPacketBytes;
        }
        #endregion
    }
}