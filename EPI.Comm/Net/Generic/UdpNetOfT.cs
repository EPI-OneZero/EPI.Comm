﻿using EPI.Comm.Buffers;
using EPI.Comm.Net.Events;
using EPI.Comm.Net.Generic.Events;
using EPI.Comm.Net.Generic.Packets;
using System;
using System.Collections.Generic;
using System.Text;
using static EPI.Comm.CommConfig;
namespace EPI.Comm.Net.Generic
{
    internal class UdpNet<Theader> : UdpBase,IComm<Theader>
    {
        #region Field & Property
        internal IBuffer ReceiveBuffer { get; set; } = new QueueBuffer();
        public int HeaderSize { get; private set; }
        public Func<Theader, int> GetBodySize { get; private set; }
        internal Packet<Theader> PacketToReceive { get; set; }
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
            PacketToReceive = new Packet<Theader>(getBodySize);
            HeaderSize = PacketToReceive.HeaderSize;
        }

        public void Send(Theader header, byte[] body)
        {
            var packetToSend = new Packet<Theader>(GetBodySize)
            {
                Header = header,
                Body = body
            };
            var fullPacketBytes = packetToSend.SerializePacket(IsBigEndian);

            SendBytes(fullPacketBytes);
        }
        private protected override void OnReceived(PacketEventArgs e)
        {
            lock (this)
            {
                ReceiveBuffer.AddBytes(e.FullPacket);
                while (PacketToReceive.TryDeserialize(ReceiveBuffer, IsBigEndian))
                {
                    Received?.Invoke(this, new PacketEventArgs<Theader>(e.From, PacketToReceive));
                    PacketToReceive.Clear();
                }
            }
        }
        public event PacketEventHandler<Theader> Received;
        #endregion
    }
    internal class UdpNet<Theader, Tfooter> : UdpBase, IComm<Theader, Tfooter>
    {
        #region Field & Property
        public int HeaderSize { get; private set; }
        public int FooterSize { get; private set; }
        public Func<Theader, int> GetBodySize { get; private set; }
        internal IBuffer ReceiveBuffer { get; set; } = new QueueBuffer();
        internal Packet<Theader, Tfooter> PacketToReceive { get; set; }
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
            PacketToReceive = new Packet<Theader, Tfooter>(getBodySize);
            HeaderSize = PacketToReceive.HeaderSize;
            FooterSize = PacketToReceive.FooterSize;
        }
        public void Send(Theader header, byte[] body, Tfooter footer)
        {
            var packetToSend = new Packet<Theader, Tfooter>(GetBodySize)
            {
                Header = header,
                Body = body,
                Footer = footer
            };
            var fullPacketBytes = packetToSend.SerializePacket(IsBigEndian);

            SendBytes(fullPacketBytes);
        }

        private protected override void OnReceived(PacketEventArgs e)
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
        private protected override void OnStop()
        {
            ReceiveBuffer.Clear();
            PacketToReceive.Clear();
            base.OnStop();
        }
        public event PacketEventHandler<Theader, Tfooter> Received;
        #endregion
    }
}