using System;
using System.Collections.Generic;
using System.Net;
using static EPI.Comm.CommConfig;

namespace EPI.Comm.Net.Generic
{
    public class UdpNet<THeader> : UdpBase, IComm<THeader>
    {
        #region Field & Property
        public Func<THeader, int> GetBodySize { get; private set; }

        internal PacketMakerDictionary<THeader> PacketMakers { get; private set; } = new PacketMakerDictionary<THeader>();
        public bool IsBigEndian { get; set; }
        #endregion

        #region CTOR
        public UdpNet(Func<THeader, int> getBodySize) : this(DefaultBufferSize, getBodySize)
        {

        }
        public UdpNet(int bufferSize, Func<THeader, int> getBodySize) : base(bufferSize)
        {
            SetPacketProperties(getBodySize);
        }
        #endregion

        #region Method & Event
        private void SetPacketProperties(Func<THeader, int> getBodySize)
        {
            GetBodySize = getBodySize;
        }

        public void Send(THeader header, byte[] body)
        {
            var packetToSend = new PacketMaker<THeader>(GetBodySize, false)
            {
                Header = header,
                Body = body
            };
            var fullPacketBytes = packetToSend.SerializePacket(IsBigEndian);

            Send(fullPacketBytes);
        }
        protected override void OnReceived(PacketEventArgs e)
        {
            if (!PacketMakers.ContainsKey(e.From))
            {
                PacketMakers.Add(e.From, new PacketMaker<THeader>(GetBodySize, true));
            }
            PacketMakers[e.From].TryDeserializeLoop(e.FullPacket, IsBigEndian, () =>
            {
                Received?.Invoke(this, new PacketEventArgs<THeader>(e.From, PacketMakers[e.From]));
            });
        }
        public event PacketEventHandler<THeader> Received;
        protected override void OnStop()
        {
            PacketMakers.Clear();
            base.OnStop();
        }
        #endregion
    }
    public class UdpNet<THeader, TFooter> : UdpBase, IComm<THeader, TFooter>
    {
        #region Field & Property
        internal PacketMakerDictionary<THeader, TFooter> PacketMakers { get; private set; } = new PacketMakerDictionary<THeader, TFooter>();
        public Func<THeader, int> GetBodySize { get; private set; }
        public bool IsBigEndian { get; set; }
        #endregion

        #region CTOR
        public UdpNet(Func<THeader, int> getBodySize) : this(DefaultBufferSize, getBodySize)
        {
        }
        public UdpNet(int bufferSize, Func<THeader, int> getBodySize) : base(bufferSize)
        {
            SetPacketProperties(getBodySize);
        }
        #endregion

        #region Method & Event
        private void SetPacketProperties(Func<THeader, int> getBodySize)
        {
            GetBodySize = getBodySize;
        }
        public void Send(THeader header, byte[] body, TFooter footer)
        {
            var packetToSend = new PacketMaker<THeader, TFooter>(GetBodySize, false)
            {
                Header = header,
                Body = body,
                Footer = footer
            };
            var fullPacketBytes = packetToSend.SerializePacket(IsBigEndian);

            Send(fullPacketBytes);
        }

        protected override void OnReceived(PacketEventArgs e)
        {
            if (!PacketMakers.ContainsKey(e.From))
            {
                PacketMakers.Add(e.From, new PacketMaker<THeader, TFooter>(GetBodySize, true));
            }
            PacketMakers[e.From].TryDeserializeLoop(e.FullPacket, IsBigEndian, () =>
            {
                Received?.Invoke(this, new PacketEventArgs<THeader, TFooter>(e.From, PacketMakers[e.From]));
            });
        }
        public event PacketEventHandler<THeader, TFooter> Received;
        protected override void OnStop()
        {
            PacketMakers.Clear();
            base.OnStop();
        }
        #endregion
    }
    internal class PacketMakerDictionary<THeader> : Dictionary<IPEndPoint, PacketMaker<THeader>>
    {

    }
    internal class PacketMakerDictionary<THeader, TFooter> : Dictionary<IPEndPoint, PacketMaker<THeader, TFooter>>
    {

    }
}
