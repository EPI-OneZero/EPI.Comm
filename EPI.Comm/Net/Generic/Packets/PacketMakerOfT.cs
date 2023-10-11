using EPI.Comm.Buffers;
using System;
using System.Runtime.InteropServices;
using static EPI.Comm.Utils.MarshalSerializer;
namespace EPI.Comm.Net.Generic.Packets
{
    internal class PacketMaker<Theader>
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
        public Theader Header { get; internal set; }
        public byte[] Body { get; internal set; }
        public int HeaderSize { get; private set; } = Marshal.SizeOf<Theader>();
        public int BodySize => Body?.Length ?? 0;
        public virtual int FullSize => HeaderSize + BodySize;
        public Func<Theader, int> GetBodySize { get; set; }
        #endregion

        #region CTOR
        internal PacketMaker(Func<Theader, int> getBodySize, bool enableDeserialize)
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
                while (TryDeserialize(isBigEndian))
                {
                    callback();
                    ClearPacketInfo();
                }
            }
        }
        private bool TryDeserialize(bool isBigEndian)
        {

            var success = TryDeserializePacket(ReceiveBuffer, isBigEndian);
            if (success)
            {
                FullPacket = FullPacketBuffer.GetBytes(FullPacketBuffer.Count);
            }
            return success;
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
                Serialize(Header, fullPacketBytes, 0, isBigEndian);
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
            Header = default(Theader);
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
    internal sealed class PacketMaker<Theader, Tfooter> : PacketMaker<Theader>
    {
        #region Field & Property
        public override int FullSize => base.FullSize + FooterSize;
        public Tfooter Footer { get; internal set; }
        public int FooterSize { get; private set; } = Marshal.SizeOf<Tfooter>();
        #endregion

        #region CTOR
        internal PacketMaker(Func<Theader, int> getBodySize, bool enableDeserialize) : base(getBodySize, enableDeserialize)
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
                Footer = Deserialize<Tfooter>(bytes, isBigEndian);

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
            Serialize(Footer, fullPacketBytes, base.FullSize, isBigEndian);
            return fullPacketBytes;
        }

        public override void ClearPacketInfo()
        {
            base.ClearPacketInfo();
            Footer = default(Tfooter);
        }
        #endregion
    }
}