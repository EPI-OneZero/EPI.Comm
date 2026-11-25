using System;
using System.Net.Sockets;
using static EPI.Comm.CommConfig;

namespace EPI.Comm.Net.Generic
{
    public class TcpNetClient<THeader> : TcpClientBase, IComm<THeader>
        where THeader : new()
    {
        #region Field & Property
        internal Func<THeader, int> GetBodySize { get; private set; }
        internal PacketMaker<THeader> PacketMakerToReceive { get; set; }
        public bool IsBigEndian { get; set; }
        #endregion

        #region CTOR
        public TcpNetClient(int bufferSize, Func<THeader, int> getBodySize) : base(bufferSize)
        {
            SetPacketProperties(getBodySize);
        }
        public TcpNetClient(Func<THeader, int> getBodySize) : this(DefaultBufferSize, getBodySize)
        {
        }

        internal TcpNetClient(TcpClient client, int bufferSize, Func<THeader, int> getBodySize) : base(client, bufferSize)
        {
            SetPacketProperties(getBodySize);
        }
        #endregion

        #region Method & Event
        protected override void OnDisconnected()
        {
            PacketMakerToReceive.ClearReceiveBuffer();
            PacketMakerToReceive.ClearPacketInfo();
            base.OnDisconnected();
        }

        private void SetPacketProperties(Func<THeader, int> getBodySize)
        {
            GetBodySize = getBodySize;

            PacketMakerToReceive = new PacketMaker<THeader>(getBodySize, true);
        }
        public void Send(THeader header, byte[] body)
        {
            var packetMakerToSend = new PacketMaker<THeader>(GetBodySize, false)
            {
                Header = header,
                Body = body
            };
            var fullPacketBytes = packetMakerToSend.SerializePacket(IsBigEndian);
            Send(fullPacketBytes);
        }
        protected override void SocketReceived(object sender, PacketEventArgs e)
        {

            PacketMakerToReceive.TryDeserializeLoop(e.FullPacket, IsBigEndian, () =>
            {
                Received?.Invoke(this, new PacketEventArgs<THeader>(e.From, PacketMakerToReceive));
            });
        }
        public event PacketEventHandler<THeader> Received;
        #endregion
    }
    public class TcpNetClient<THeader, TFooter> : TcpClientBase, IComm<THeader, TFooter>
        where THeader : new()
    {
        #region Field & Property
        public Func<THeader, int> GetBodySize { get; private set; }
        internal PacketMaker<THeader, TFooter> PacketMakerToReceive { get; set; }
        public bool IsBigEndian { get; set; }
        #endregion

        #region CTOR
        public TcpNetClient(int bufferSize, Func<THeader, int> getBodySize) : base(bufferSize)
        {
            SetPacketProperties(getBodySize);
        }
        public TcpNetClient(Func<THeader, int> getBodySize) : this(DefaultBufferSize, getBodySize)
        {
        }
        internal TcpNetClient(TcpClient client, int bufferSize, Func<THeader, int> getBodySize) : base(client, bufferSize)
        {
            SetPacketProperties(getBodySize);
        }
        #endregion

        #region Method & Event
        private void SetPacketProperties(Func<THeader, int> getBodySize)
        {
            GetBodySize = getBodySize;
            PacketMakerToReceive = new PacketMaker<THeader, TFooter>(getBodySize, true);

        }
        protected override void OnDisconnected()
        {
            PacketMakerToReceive.ClearReceiveBuffer();
            PacketMakerToReceive.ClearPacketInfo();
            base.OnDisconnected();
        }
        public void Send(THeader header, byte[] body, TFooter footer)
        {
            var packetMakerToSend = new PacketMaker<THeader, TFooter>(GetBodySize, false)
            {
                Header = header,
                Body = body,
                Footer = footer
            };
            var fullPacketBytes = packetMakerToSend.SerializePacket(IsBigEndian);

            Send(fullPacketBytes);
        }
        protected override void SocketReceived(object sender, PacketEventArgs e)
        {
            PacketMakerToReceive.TryDeserializeLoop(e.FullPacket, IsBigEndian, () =>
            {
                Received?.Invoke(this, new PacketEventArgs<THeader, TFooter>(e.From, PacketMakerToReceive));
            });
        }
        public event PacketEventHandler<THeader, TFooter> Received;
        #endregion
    }
}
