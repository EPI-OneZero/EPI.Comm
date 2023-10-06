using EPI.Comm.Buffers;
using System;
using System.Runtime.InteropServices;
using static EPI.Comm.Utils.MarshalSerializer;
namespace EPI.Comm.Net.Generic.Packets
{
    internal class Packet<Theader>
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
        public Theader Header { get; internal set; }
        public byte[] Body { get; internal set; }
        public int HeaderSize { get; private set; }
        public int BodySize => Body?.Length ?? 0;
        public virtual int FullSize => HeaderSize + BodySize;
        public Func<Theader, int> GetBodySize { get; set; }
        #endregion

        #region CTOR
        internal Packet(Func<Theader, int> getBodySize)
        {
            GetBodySize = getBodySize;
            HeaderSize = Marshal.SizeOf<Theader>();
            queue = new QueueBuffer();
        }

        #endregion

        #region Method
        private int CalculateBodySize()
        {
            return GetBodySize?.Invoke(Header) ?? 0;
        }
        private DeserializeState state = DeserializeState.None;
        internal bool TryDeserialize(IBuffer buffer, bool isBigEndian)
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
        private bool TryDeserializeHeader(IBuffer buffer, bool isBigEndian)
        {
            if (buffer.Count >= HeaderSize)
            {
                var bytes = buffer.GetBytes(HeaderSize);
                queue.AddBytes(bytes);

                Header = Deserialize<Theader>(bytes, isBigEndian);
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
            if (buffer.Count >= bodySize)
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
                Serialize(Header, fullPacketBytes, 0, HeaderSize, isBigEndian);
                Buffer.BlockCopy(Body, 0, fullPacketBytes, HeaderSize, bodySize);

                return fullPacketBytes;
            }
            else
            {
                throw new ArgumentOutOfRangeException
                    ($"바디의 크기가 헤더 정의와 다릅니다. 헤더에서 계산된 바디 크기 : {headerDefinedBodySize} 바이트");
            }
        }
        internal virtual void Clear()
        {
            Header = default(Theader);
            Body = null;
            state = DeserializeState.None;
            queue.Clear();
        }
        #endregion
    }
    internal sealed class Packet<Theader, Tfooter> : Packet<Theader>
    {
        #region Field & Property
        public override int FullSize => base.FullSize + FooterSize;
        public Tfooter Footer { get; internal set; }
        public int FooterSize { get; private set; }
        #endregion

        #region CTOR
        internal Packet(Func<Theader, int> getBodySize) : base(getBodySize)
        {
            FooterSize = Marshal.SizeOf<Tfooter>();
        }
        #endregion

        #region Method
        private protected override bool TryDeserializePacket(IBuffer buffer, bool isBigEndian)
        {
            if (base.TryDeserializePacket(buffer, isBigEndian) && buffer.Count >= FooterSize)
            {
                var bytes = buffer.GetBytes(FooterSize);
                queue.AddBytes(bytes);
                Footer = Deserialize<Tfooter>(bytes, isBigEndian);

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
            Serialize(Footer, fullPacketBytes, base.FullSize, FooterSize, isBigEndian);
            return fullPacketBytes;
        }

        internal override void Clear()
        {
            base.Clear();
            Footer = default(Tfooter);
        }
        #endregion
    }
}