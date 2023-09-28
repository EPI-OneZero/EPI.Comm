﻿using EPI.Comm.Net.Events;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EPI.Comm.Net
{
    public class TcpNetServer : TcpServerBase, IComm
    {
        #region Field & Property
        private List<TcpNetClient> clients = new List<TcpNetClient>();
        public ClientCollection Clients { get; private set; }
        #endregion
        #region CTOR
        public TcpNetServer(int bufferSize) : base(bufferSize)
        {
            Clients = new ClientCollection(clients);
        }
        public TcpNetServer() : this(DefaultBufferSize)
        {

        }
        #endregion
        #region Send
        public void Send(string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            Send(bytes);
        }
        public void Send(byte[] bytes)
        {
            var result = Parallel.ForEach(clients, c =>
            {
                c.Send(bytes);
            });
        }
        #endregion
        #region Receive
        private void OnClientReceived(object sender, PacketEventArgs e)
        {
            Received?.Invoke(this, e);

        }
        public event PacketEventHandler Received;
        #endregion

        #region Accept
        private protected override TcpClientBase CreateClient(TcpClient client)
        {
            return new TcpNetClient(client, BufferSize);
        }

        private protected override void OnClientConnected(TcpClientBase client)
        {
            var newClient = client as TcpNetClient;
            if (IsValidClientType(newClient) && !clients.Contains(newClient))
            {
                AttachClient(newClient);
                ClientAccpeted?.Invoke(this, new TcpEventArgs(newClient));
            }
        }
        private void AttachClient(TcpNetClient client)
        {
            clients.Add(client);
            client.Received += OnClientReceived;

        }
        public event TcpEventHandler ClientAccpeted;
        #endregion

        #region Close
        private protected override void OnClientDisconnected(TcpClientBase client)
        {
            var oldClient = client as TcpNetClient;
            if (IsValidClientType(oldClient) && clients.Contains(oldClient))
            {
                DetachClient(oldClient);
                ClientDisconnected?.Invoke(this, new TcpEventArgs(oldClient));
            }
        }
        private bool IsValidClientType(TcpClientBase client)
        {
            return client is TcpNetClient;
        }
        private void DetachClient(TcpNetClient client)
        {
            clients.Remove(client);
            client.Received -= OnClientReceived;
        }
        public event TcpEventHandler ClientDisconnected;
        #endregion
    }
}
