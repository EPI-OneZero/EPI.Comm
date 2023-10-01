using EPI.Comm.Buffers;
using EPI.Comm.Net.Events;
using EPI.Comm.Net.Generic.Events;
using EPI.Comm.Net.Generic.Packets;
using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using static EPI.Comm.CommConfig;

namespace EPI.Comm.Net.Generic
{
    public class TcpNetClient<Theader> : TcpClientBase, IComm<Theader>
        where Theader : new()
    {
        #region Field & Property
        public int HeaderSize { get; private set; }
        internal IBuffer ReceiveBuffer { get; set; } = new QueueBuffer();
        internal Func<Theader, int> GetBodySize { get; private set; }
        internal Packet<Theader> PacketToSend { get; set; }
        internal Packet<Theader> PacketToReceive { get; set; }
        public bool IsBigEndian { get; set; }
        #endregion

        #region CTOR
        public TcpNetClient(int bufferSize, Func<Theader, int> getBodySize) : base(bufferSize)
        {
            HeaderSize = Marshal.SizeOf<Theader>();
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
            ReceiveBuffer.Clear();
            base.OnSocketDisconnected();
        }
        private protected override void OnSocketConnected()
        {
            ReceiveBuffer.Clear();
            base.OnSocketConnected();
        }
        private void SetPacketProperties(Func<Theader, int> getBodySize)
        {
            GetBodySize = getBodySize;
            PacketToSend = new Packet<Theader>(GetBodySize);
            PacketToReceive = new Packet<Theader>(GetBodySize);
            HeaderSize = Marshal.SizeOf<Theader>();

        }
        public void Send(Theader header, byte[] body)
        {
            PacketToSend.Header = header;
            PacketToSend.Body = body;
            var fullPacketBytes = PacketToSend.SerializePacket(IsBigEndian);
            SendBytes(fullPacketBytes);
        }
        private protected override void SocketReceived(object sender, PacketEventArgs e)
        {
            lock (this)
            {
                ReceiveBuffer.AddBytes(e.FullPacket);
                while (PacketToReceive.TryDeserialize(ReceiveBuffer,IsBigEndian))
                {
                    var packet = PacketToReceive;
                    Received?.Invoke(this, new PacketEventArgs<Theader>(e.From, packet));
                    PacketToReceive.Clear();
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
        public int HeaderSize { get; private set; }
        public int FooterSize { get; private set; }
        internal IBuffer ReceiveBuffer { get; set; } = new QueueBuffer();
        internal Packet<Theader, Tfooter> PacketToSend { get; set; }
        internal Packet<Theader, Tfooter> PacketToReceive { get; set; }
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
            PacketToSend = new Packet<Theader, Tfooter>(getBodySize);
            PacketToReceive = new Packet<Theader, Tfooter>(getBodySize);
            HeaderSize = Marshal.SizeOf<Theader>();
            FooterSize = Marshal.SizeOf<Tfooter>();

        }
        private protected override void OnSocketDisconnected()
        {
            ReceiveBuffer.Clear();
            base.OnSocketDisconnected();
        }
        private protected override void OnSocketConnected()
        {
            ReceiveBuffer.Clear();
            base.OnSocketConnected();
        }
        public void Send(Theader header, byte[] body, Tfooter footer)
        {
            PacketToSend.Header = header;
            PacketToSend.Body = body;
            PacketToSend.Footer = footer;
            var fullPacketBytes = PacketToSend.SerializePacket(IsBigEndian);

            SendBytes(fullPacketBytes);
        }
        private protected override void SocketReceived(object sender, PacketEventArgs e)
        {
            lock (this)
            {
                ReceiveBuffer.AddBytes(e.FullPacket);
                while (PacketToReceive.TryDeserialize(ReceiveBuffer, IsBigEndian))
                {
                    Received?.Invoke(this, new PacketEventArgs<Theader, Tfooter>(e.From, PacketToReceive));
                    PacketToReceive.Clear();
                }
            }
        }

        public event PacketEventHandler<Theader, Tfooter> Received;
        #endregion
    }
}
