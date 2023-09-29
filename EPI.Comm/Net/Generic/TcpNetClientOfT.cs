﻿using EPI.Comm.Buffers;
using EPI.Comm.Net.Events;
using EPI.Comm.Net.Generic.Events;
using EPI.Comm.Net.Generic.Packets;
using EPI.Comm.Utils;
using System;
using System.Net.Sockets;
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
        internal Packet<Theader> Packet { get; set; }
        #endregion
        #region CTOR
        public TcpNetClient(int bufferSize, Func<Theader, int> getBodySize) : base(bufferSize)
        {
            HeaderSize = TypeUtil.SizeOf<Theader>();
            GetBodySize = getBodySize;
        }
        public TcpNetClient(Func<Theader, int> getBodySize) : this(DefaultBufferSize, getBodySize)
        {
            SetPacketProperties(getBodySize);
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
            Packet = new Packet<Theader>(GetBodySize);
            HeaderSize = TypeUtil.SizeOf<Theader>();

        }
        public void Send(Theader header, byte[] body)
        {
            var headerDefinedBodySize = GetBodySize?.Invoke(header) ?? 0;
            var packet = new Packet<Theader>(header, body, GetBodySize);
            var fullPacketBytes = packet.SerializePacket();
            SendBytes(fullPacketBytes);
        }
        private protected override void SocketReceived(object sender, SocketReceiveEventArgs e)
        {
            lock (this)
            {
                ReceiveBuffer.AddBytes(e.ReceivedBytes);
                while (Packet.TryDeserialize(ReceiveBuffer))
                {

                    var packet = Packet;

                    Received?.Invoke(this, new PacketEventArgs<Theader>(e.From, packet));
                    Packet = new Packet<Theader>(GetBodySize);
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
        internal int HeaderSize { get; set; }
        internal int FooterSize { get; set; }
        internal IBuffer ReceiveBuffer { get; set; } = new QueueBuffer();
        private Packet<Theader, Tfooter> Packet { get; set; }
        internal Func<Theader, int> GetBodySize { get; private set; }
        #endregion

        #region CTOR
        public TcpNetClient(int bufferSize, Func<Theader, int> getBodySize) : base(bufferSize)
        {
            SetPacketProperties(getBodySize);
        }
        public TcpNetClient(Func<Theader, int> getBodySize) : this(CommConfig.DefaultBufferSize, getBodySize)
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
            Packet = new Packet<Theader, Tfooter>(GetBodySize);
            HeaderSize = TypeUtil.SizeOf<Theader>();
            FooterSize = TypeUtil.SizeOf<Tfooter>();

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
            var headerDefinedBodySize = GetBodySize?.Invoke(header) ?? 0;
            var packet = new Packet<Theader, Tfooter>(header, body, footer, GetBodySize);
            var fullPacketBytes = packet.SerializePacket();

            SendBytes(fullPacketBytes);
        }
        private protected override void SocketReceived(object sender, SocketReceiveEventArgs e)
        {
            lock (this)
            {
                ReceiveBuffer.AddBytes(e.ReceivedBytes);
                while (Packet.TryDeserialize(ReceiveBuffer))
                {

                    var packet = Packet;

                    Received?.Invoke(this, new PacketEventArgs<Theader, Tfooter>(e.From, packet));
                    Packet = new Packet<Theader, Tfooter>(GetBodySize);
                }
            }
        }

        public event PacketEventHandler<Theader, Tfooter> Received;
        #endregion
    }

}
