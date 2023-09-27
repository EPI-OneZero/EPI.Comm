using EPI.Comm.Buffers;
using EPI.Comm.Net.Events;
using EPI.Comm.Net.Generic.Events;
using EPI.Comm.Net.Generic.Packets;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections;
using System.Net;

namespace EPI.Comm.Net.Generic
{

    public class TcpNetServer<Theader> : TcpNetServer, IComm<Theader>
        where Theader : new()
    {
        private List<TcpNetClient<Theader>> clients = new List<TcpNetClient<Theader>>();
        new public ClientCollection<Theader> Clients { get; private set; }
        internal IBuffer ReceiveBuffer { get; set; } = new QueueBuffer();
        internal Func<Theader, int> GetBodySize { get; private set; }
        internal Packet<Theader> Packet { get; set; }
        public TcpNetServer(int bufferSize, Func<Theader, int> getBodySize) : base(bufferSize)
        {
            Clients = new ClientCollection<Theader>(clients);
            GetBodySize = getBodySize;

        }
        public TcpNetServer(Func<Theader, int> getBodySize) : this(DefaultBufferSize, getBodySize)
        {
        }

        #region Override
        private protected override TcpNetClient CreateClient(TcpClient client)
        {
            var result = new TcpNetClient<Theader>(client, BufferSize, GetBodySize);
            return result;
        }

        private protected override void OnAccepted(object sender, TcpEventArgs e)
        {
            var newClient = e.TcpNetClient as TcpNetClient<Theader>;
            if (IsValidClientType(newClient) && !clients.Contains(newClient))
            {
                AttachClient(newClient);
                ClientAccpeted?.Invoke(this, new TcpEventArgs<Theader>(newClient));
            }
        }
        private void AttachClient(TcpNetClient<Theader> client)
        {
            clients.Add(client);
            client.Received += ClientReceived;

        }

        private void ClientReceived(object sender, PacketEventArgs<Theader> e)
        {
        }

        private void DetachClient(TcpNetClient<Theader> client)
        {
            clients.Remove(client);
            client.Received -= ClientReceived;
        }
        private protected override void OnClosed(object sender, TcpEventArgs e)
        {
            var oldClient = e.TcpNetClient as TcpNetClient<Theader>;
            if (IsValidClientType(oldClient) && clients.Contains(oldClient))
            {
                DetachClient(oldClient);
                ClientClosed?.Invoke(this, new TcpEventArgs<Theader>(oldClient));
            }
        }
        private bool IsValidClientType(TcpNetClient client)
        {
            return client is TcpNetClient<Theader>;
        }

        private protected override void OnClientReceived(object sender, PacketEventArgs e)
        {

        }


        #endregion
        #region 이벤트
        new public event PacketEventHandler<Theader> Received;
        new public event TcpEventHandler<Theader> ClientAccpeted;
        new public event TcpEventHandler<Theader> ClientClosed;
        #endregion
        public void Send(Theader header, byte[] body)
        {
        }
    }
}
