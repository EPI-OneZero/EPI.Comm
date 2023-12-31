﻿using EPI.Comm.Net.Events;
using System.Net.Sockets;
using static EPI.Comm.CommConfig;
namespace EPI.Comm.Net
{
    public class TcpNetClient : TcpClientBase, IComm
    {
        public TcpNetClient(int bufferSize) : base(bufferSize)
        {

        }
        public TcpNetClient() : this(DefaultBufferSize)
        {
        }

        internal TcpNetClient(TcpClient client, int bufferSize) : base(client, bufferSize)
        {

        }

        protected override void SocketReceived(object sender, PacketEventArgs e)
        {
            Received?.Invoke(this, e);
        }
        public event PacketEventHandler Received;
    }
}
