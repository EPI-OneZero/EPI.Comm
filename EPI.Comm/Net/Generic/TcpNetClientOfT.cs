using EPI.Comm.Buffers;
using EPI.Comm.Net.Events;
using EPI.Comm.Net.Generic.Events;
using EPI.Comm.Net.Generic.Packets;
using System;
using System.Net.Sockets;
using static EPI.Comm.CommConfig;

namespace EPI.Comm.Net.Generic
{
    public class TcpNetClient<Theader> : TcpClientBase, IComm<Theader>
        where Theader : new()
    {
        #region Field & Property
        private readonly object recvLock = new object();
        internal Func<Theader, int> GetBodySize { get; private set; }
        internal PacketMaker<Theader> PacketMakerToReceive { get; set; }
        public bool IsBigEndian { get; set; }
        #endregion

        #region CTOR
        public TcpNetClient(int bufferSize, Func<Theader, int> getBodySize) : base(bufferSize)
        {
            SetPacketProperties(getBodySize);
        }
        public TcpNetClient(Func<Theader, int> getBodySize) : this(DefaultBufferSize, getBodySize)
        {
        }

        internal TcpNetClient(TcpClient client, int bufferSize, Func<Theader, int> getBodySize) : base(client, bufferSize)
        {
            SetPacketProperties(getBodySize);
        }
        #endregion

        #region Method & Event
        private protected override void OnSocketDisconnected()
        {
            PacketMakerToReceive.ClearReceiveBuffer();
            PacketMakerToReceive.ClearPacketInfo();
            base.OnSocketDisconnected();
        }

        private void SetPacketProperties(Func<Theader, int> getBodySize)
        {
            GetBodySize = getBodySize;

            PacketMakerToReceive = new PacketMaker<Theader>(getBodySize, true);
        }
        public void Send(Theader header, byte[] body)
        {
            var packetMakerToSend = new PacketMaker<Theader>(GetBodySize, false)
            {
                Header = header,
                Body = body
            };
            var fullPacketBytes = packetMakerToSend.SerializePacket(IsBigEndian);
            Send(fullPacketBytes);
        }
        private protected override void SocketReceived(object sender, PacketEventArgs e)
        {
            lock (recvLock)
            {
                PacketMakerToReceive.AddBytes(e.FullPacket);
                while (PacketMakerToReceive.TryDeserialize(IsBigEndian))
                {
                    Received?.Invoke(this, new PacketEventArgs<Theader>(e.From, PacketMakerToReceive));
                    PacketMakerToReceive.ClearPacketInfo();
                }
            }
        }

        public event PacketEventHandler<Theader> Received;
        #endregion
    }
    public class TcpNetClient<Theader, Tfooter> : TcpClientBase, IComm<Theader, Tfooter>
        where Theader : new()
    {
        #region Field & Property
        private readonly object recvLock = new object();
        public Func<Theader, int> GetBodySize { get; private set; }
        internal PacketMaker<Theader, Tfooter> PacketMakerToReceive { get; set; }
        public bool IsBigEndian { get; set; }
        #endregion

        #region CTOR
        public TcpNetClient(int bufferSize, Func<Theader, int> getBodySize) : base(bufferSize)
        {
            SetPacketProperties(getBodySize);
        }
        public TcpNetClient(Func<Theader, int> getBodySize) : this(DefaultBufferSize, getBodySize)
        {
        }
        internal TcpNetClient(TcpClient client, int bufferSize, Func<Theader, int> getBodySize) : base(client, bufferSize)
        {
            SetPacketProperties(getBodySize);
        }
        #endregion

        #region Method & Event
        private void SetPacketProperties(Func<Theader, int> getBodySize)
        {
            GetBodySize = getBodySize;
            PacketMakerToReceive = new PacketMaker<Theader, Tfooter>(getBodySize, true);

        }
        private protected override void OnSocketDisconnected()
        {
            PacketMakerToReceive.ClearReceiveBuffer();
            PacketMakerToReceive.ClearPacketInfo();
            base.OnSocketDisconnected();
        }
        public void Send(Theader header, byte[] body, Tfooter footer)
        {
            var packetMakerToSend = new PacketMaker<Theader, Tfooter>(GetBodySize, false)
            {
                Header = header,
                Body = body,
                Footer = footer
            };
            var fullPacketBytes = packetMakerToSend.SerializePacket(IsBigEndian);

            Send(fullPacketBytes);
        }
        private protected override void SocketReceived(object sender, PacketEventArgs e)
        {
            lock (recvLock)
            {
                PacketMakerToReceive.AddBytes(e.FullPacket);
                while (PacketMakerToReceive.TryDeserialize(IsBigEndian))
                {
                    Received?.Invoke(this, new PacketEventArgs<Theader, Tfooter>(e.From, PacketMakerToReceive));
                    PacketMakerToReceive.ClearPacketInfo();
                }
            }
        }
        public event PacketEventHandler<Theader, Tfooter> Received;
        #endregion
    }
}
