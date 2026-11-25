using EPI.Comm.Buffers;
using System;
using System.Runtime.InteropServices;
using static EPI.Comm.Utils.MarshalSerializer;
namespace EPI.Comm.Net.Generic
{
    internal class PacketMaker<THeader>
    {
        #region Field & Property
        private enum DeserializeState
        {
            None,
            HeaderCompleted,
            BodyCompleted,
        }
        private DeserializeState state = DeserializeState.None;
        private protected IBuffer FullPacketBuffer { get; set; }
        private protected IBuffer ReceiveBuffer { get; set; }
        public byte[] FullPacket { get; private set; }
        public THeader Header { get; internal set; }
        public byte[] Body { get; internal set; }
        public int HeaderSize { get; private set; } = Marshal.SizeOf<THeader>();
        public int BodySize => Body?.Length ?? 0;
        public virtual int FullSize => HeaderSize + BodySize;
        public Func<THeader, int> GetBodySize { get; set; }
        #endregion

        #region CTOR
        internal PacketMaker(Func<THeader, int> getBodySize, bool enableDeserialize)
        {
            GetBodySize = getBodySize;
            if (enableDeserialize)
            {
                FullPacketBuffer = new QueueBuffer();
                ReceiveBuffer = new QueueBuffer();
            }

        }

        #endregion

        #region Method
        private int CalculateBodySize()
        {
            return GetBodySize?.Invoke(Header) ?? 0;
        }
        internal void TryDeserializeLoop(byte[] bytes, bool isBigEndian, Action callback)
        {
            lock (this)
            {
                ReceiveBuffer.AddBytes(bytes);
                while (TryDeserializePacket(ReceiveBuffer, isBigEndian))
                {
                    FullPacket = FullPacketBuffer.GetBytes(FullPacketBuffer.Count);
                    callback();
                    ClearPacketInfo();
                }
            }
        }
        protected virtual bool TryDeserializePacket(IBuffer buffer, bool isBigEndian)
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
                FullPacketBuffer.AddBytes(bytes);
                if (isBigEndian)
                {
                    ReverseEndian<THeader>(bytes, 0);
                }
                Header = Deserialize<THeader>(bytes);
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
                FullPacketBuffer.AddBytes(bytes);
                Body = bytes;
                state = DeserializeState.BodyCompleted;
                return true;
            }
            else
            {
                return false;
            }
        }
        public virtual byte[] SerializePacket(bool isBigEndian)
        {
            int bodySize = Body.Length;
            var headerDefinedBodySize = CalculateBodySize();
            if (headerDefinedBodySize == bodySize)
            {
                var fullPacketBytes = new byte[FullSize];

                Serialize(Header, fullPacketBytes, 0);
                if (isBigEndian)
                {
                    ReverseEndian<THeader>(fullPacketBytes, 0);
                }
                Buffer.BlockCopy(Body, 0, fullPacketBytes, HeaderSize, bodySize);

                return fullPacketBytes;
            }
            else
            {
                throw new ArgumentOutOfRangeException
                    ($"헤더에 넣은 바디의 길이가 잘못되었습니다. 헤더에 넣은 바디 크기 : {headerDefinedBodySize} 바이트, 실제 길이 : {bodySize} 바이트");
            }
        }
        public virtual void ClearPacketInfo()
        {
            Header = default(THeader);
            Body = null;
            state = DeserializeState.None;
            FullPacketBuffer?.Clear();
        }
        public void ClearReceiveBuffer()
        {
            ReceiveBuffer?.Clear();
        }
        #endregion
    }
    internal sealed class PacketMaker<THeader, TFooter> : PacketMaker<THeader>
    {
        #region Field & Property
        public override int FullSize => base.FullSize + FooterSize;
        public TFooter Footer { get; internal set; }
        public int FooterSize { get; private set; } = Marshal.SizeOf<TFooter>();
        #endregion

        #region CTOR
        internal PacketMaker(Func<THeader, int> getBodySize, bool enableDeserialize) : base(getBodySize, enableDeserialize)
        {
        }
        #endregion

        #region Method

        protected override bool TryDeserializePacket(IBuffer buffer, bool isBigEndian)
        {
            if (base.TryDeserializePacket(buffer, isBigEndian) && buffer.Count >= FooterSize)
            {
                var bytes = buffer.GetBytes(FooterSize);
                FullPacketBuffer.AddBytes(bytes);
                if (isBigEndian)
                {
                    ReverseEndian<TFooter>(bytes, 0);
                }
                Footer = Deserialize<TFooter>(bytes);

                return true;
            }
            else
            {
                return false;
            }
        }

        public override byte[] SerializePacket(bool isBigEndian)
        {
            var fullPacketBytes = base.SerializePacket(isBigEndian);
            Serialize(Footer, fullPacketBytes, base.FullSize);
            if (isBigEndian)
            {
                ReverseEndian<TFooter>(fullPacketBytes, base.FullSize);
            }
            return fullPacketBytes;
        }

        public override void ClearPacketInfo()
        {
            base.ClearPacketInfo();
            Footer = default(TFooter);
        }
        #endregion
    }
}