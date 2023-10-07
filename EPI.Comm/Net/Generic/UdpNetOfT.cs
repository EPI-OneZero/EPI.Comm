using EPI.Comm.Buffers;
using EPI.Comm.Net.Events;
using EPI.Comm.Net.Generic.Events;
using EPI.Comm.Net.Generic.Packets;
using System;
using System.Collections.Generic;
using System.Net;
using static EPI.Comm.CommConfig;

namespace EPI.Comm.Net.Generic
{
    internal class UdpNet<Theader> : UdpBase, IComm<Theader>
    {
        #region Field & Property
        public Func<Theader, int> GetBodySize { get; private set; }

        internal PacketMakerDictionary<Theader> PacketMakers { get; private set; } = new PacketMakerDictionary<Theader>();
        public bool IsBigEndian { get; set; }
        #endregion

        #region CTOR
        public UdpNet(Func<Theader, int> getBodySize) : this(DefaultBufferSize, getBodySize)
        {

        }
        public UdpNet(int bufferSize, Func<Theader, int> getBodySize) : base(bufferSize)
        {
            SetPacketProperties(getBodySize);
        }
        #endregion

        #region Method & Event
        private void SetPacketProperties(Func<Theader, int> getBodySize)
        {
            GetBodySize = getBodySize;
        }

        public void Send(Theader header, byte[] body)
        {
            var packetToSend = new PacketMaker<Theader>(GetBodySize, false)
            {
                Header = header,
                Body = body
            };
            var fullPacketBytes = packetToSend.SerializePacket(IsBigEndian);

            Send(fullPacketBytes);
        }
        protected override void OnReceived(PacketEventArgs e)
        {
            if(!PacketMakers.ContainsKey(e.From))
            {
                PacketMakers.Add(e.From, new PacketMaker<Theader>(GetBodySize, true));
            }
            PacketMakers[e.From].TryDeserializeLoop(e.FullPacket, IsBigEndian, () =>
            {
                Received?.Invoke(this, new PacketEventArgs<Theader>(e.From, PacketMakers[e.From]));
            });
        }
        public event PacketEventHandler<Theader> Received;
        #endregion
    }
    internal class UdpNet<Theader, Tfooter> : UdpBase, IComm<Theader, Tfooter>
    {
        #region Field & Property
        internal PacketMakerDictionary<Theader, Tfooter> PacketMakers { get; private set; } = new PacketMakerDictionary<Theader, Tfooter>();
        public Func<Theader, int> GetBodySize { get; private set; }
        internal PacketMaker<Theader, Tfooter> PacketToReceive { get; set; }
        public bool IsBigEndian { get; set; }
        #endregion

        #region CTOR
        public UdpNet(Func<Theader, int> getBodySize) : this(DefaultBufferSize, getBodySize)
        {
        }
        public UdpNet(int bufferSize, Func<Theader, int> getBodySize) : base(bufferSize)
        {
            SetPacketProperties(getBodySize);
        }
        #endregion

        #region Method & Event
        private void SetPacketProperties(Func<Theader, int> getBodySize)
        {
            GetBodySize = getBodySize;
            PacketToReceive = new PacketMaker<Theader, Tfooter>(getBodySize, true);
        }
        public void Send(Theader header, byte[] body, Tfooter footer)
        {
            var packetToSend = new PacketMaker<Theader, Tfooter>(GetBodySize, false)
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
                PacketMakers.Add(e.From, new PacketMaker<Theader,Tfooter>(GetBodySize, true));
            }
            PacketMakers[e.From].TryDeserializeLoop(e.FullPacket, IsBigEndian, () =>
            {
                Received?.Invoke(this, new PacketEventArgs<Theader, Tfooter>(e.From, PacketMakers[e.From]));
            });
        }
        protected override void OnStop()
        {
            PacketToReceive.ClearReceiveBuffer();
            PacketToReceive.ClearPacketInfo();
            base.OnStop();
        }
        public event PacketEventHandler<Theader, Tfooter> Received;
        #endregion
    }
    internal class PacketMakerDictionary<Theader> : Dictionary<IPEndPoint, PacketMaker<Theader>>
    {

    }
    internal class PacketMakerDictionary<Theader, Tfooter> : Dictionary<IPEndPoint, PacketMaker<Theader,Tfooter>>
    {

    }
}
