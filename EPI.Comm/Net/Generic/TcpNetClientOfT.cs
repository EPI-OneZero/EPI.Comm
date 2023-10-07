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
        internal Func<Theader, int> GetBodySize { get; private set; }
        internal PacketMaker<Theader> PacketToReceive { get; set; }
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
            PacketToReceive.ClearReceiveBuffer();
            PacketToReceive.ClearPacketInfo();
            base.OnSocketDisconnected();
        }

        private void SetPacketProperties(Func<Theader, int> getBodySize)
        {
            GetBodySize = getBodySize;

            PacketToReceive = new PacketMaker<Theader>(getBodySize);
        }
        public void Send(Theader header, byte[] body)
        {
            var packetToSend = new PacketMaker<Theader>(GetBodySize)
            {
                Header = header,
                Body = body
            };
            var fullPacketBytes = packetToSend.SerializePacket(IsBigEndian);
            Send(fullPacketBytes);
        }
        private protected override void SocketReceived(object sender, PacketEventArgs e)
        {
            lock (this)
            {
                while (PacketToReceive.TryDeserialize(e.FullPacket, IsBigEndian))
                {
                    Received?.Invoke(this, new PacketEventArgs<Theader>(e.From, PacketToReceive));
                    PacketToReceive.ClearPacketInfo();
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
        public Func<Theader, int> GetBodySize { get; private set; }
        internal PacketMaker<Theader, Tfooter> PacketToReceive { get; set; }
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
            PacketToReceive = new PacketMaker<Theader, Tfooter>(getBodySize);

        }
        private protected override void OnSocketDisconnected()
        {
            PacketToReceive.ClearReceiveBuffer();
            PacketToReceive.ClearPacketInfo();
            base.OnSocketDisconnected();
        }
        public void Send(Theader header, byte[] body, Tfooter footer)
        {
            var packetToSend = new PacketMaker<Theader, Tfooter>(GetBodySize)
            {
                Header = header,
                Body = body,
                Footer = footer
            };
            var fullPacketBytes = packetToSend.SerializePacket(IsBigEndian);

            Send(fullPacketBytes);
        }
        private protected override void SocketReceived(object sender, PacketEventArgs e)
        {
            lock (this)
            {
                while (PacketToReceive.TryDeserialize(e.FullPacket, IsBigEndian))
                {
                    Received?.Invoke(this, new PacketEventArgs<Theader, Tfooter>(e.From, PacketToReceive));
                    PacketToReceive.ClearPacketInfo();
                }
            }
        }

        public event PacketEventHandler<Theader, Tfooter> Received;
        #endregion
    }
}
