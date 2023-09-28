using EPI.Comm.Buffers;
using EPI.Comm.Utils;
using System;
using static EPI.Comm.Utils.PacketSerializer;
namespace EPI.Comm.Net.Generic.Packets
{
    public class Packet<Theader>
    {
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
        internal Packet(Func<Theader, int> getBodySize)
        {
            GetBodySize = getBodySize;
            HeaderSize = ObjectUtil.SizeOf<Theader>();
            queue = new QueueBuffer();
        }
        private int CalculateBodySize()
        {
            return GetBodySize?.Invoke(Header) ?? 0;
        }
        private DeserializeState state = DeserializeState.None;
        private protected bool HeaderDeserialized { get; private set; } = false;
        private protected bool BodyDeserialized { get; private set; } = false;
        internal bool TryDeserialize(IBuffer buffer)
        {
            var success = TryDeserializePacket(buffer);
            if (success)
            {
                FullPacket = queue.GetBytes(queue.Count);
            }
            return success;
        }
        private protected virtual bool TryDeserializePacket(IBuffer buffer)
        {
            switch (state)
            {
                case DeserializeState.None:
                    if (TryDeserializeHeader(buffer))
                    {
                        return TryDeserializeBody(buffer, CalculateBodySize());
                    }
                    else return false;
                case DeserializeState.HeaderCompleted:
                    return TryDeserializeBody(buffer, CalculateBodySize());
                case DeserializeState.BodyCompleted:
                    return true;
                default:
                    throw new InvalidOperationException($"Unexpected state: {state}");
            }
        }
        private bool TryDeserializeHeader(IBuffer buffer)
        {
            if (IsEnoughSizeToDeserialize(buffer.Count, HeaderSize, 0))
            {
                var bytes = buffer.GetBytes(HeaderSize);
                queue.AddBytes(bytes);
                Header = DeserializeByMarshal<Theader>(bytes, HeaderSize, 0);
                state = DeserializeState.HeaderCompleted;
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool TryDeserializeBody(IBuffer buffer, int bodySize)
        {
            if (IsEnoughSizeToDeserialize(buffer.Count, bodySize, 0))
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

    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="Theader"></typeparam>
    /// <typeparam name="Tfooter"></typeparam>
    public sealed class Packet<Theader, Tfooter> : Packet<Theader>
    {
        public override int FullSize => base.FullSize + FooterSize;
        public Tfooter Footer { get; private set; }
        public int FooterSize { get; private set; }
        internal Packet(Func<Theader, int> getBodySize) : base(getBodySize)
        {
            FooterSize = ObjectUtil.SizeOf<Tfooter>();
        }
        private protected override bool TryDeserializePacket(IBuffer buffer)
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
            if (IsEnoughSizeToDeserialize(buffer.Count, FooterSize, 0))
            {
                var bytes = buffer.GetBytes(FooterSize);
                queue.AddBytes(bytes);
                Footer = DeserializeByMarshal<Tfooter>(bytes, FooterSize, 0);
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
