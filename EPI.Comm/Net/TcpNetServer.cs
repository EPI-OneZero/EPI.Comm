using EPI.Comm.Net.Events;
using System.Collections.Generic;
using System.Net.Sockets;

namespace EPI.Comm.Net
{
    /// <summary>
    /// 질문: TcpNetServer has a TcpNetClient. 
    ///  TcpNetClient has a NetSocket인데 이 구조 적절한가?
    /// </summary>
    public class TcpNetServer : TcpServerBase, IComm
    {
        public List<TcpNetClient> clients { get; private set; } = new List<TcpNetClient>();
        public ClientCollection Clients { get; private set; }
        #region CTOR
        public TcpNetServer(int bufferSize) : base(bufferSize)
        {
            Clients = new ClientCollection(clients);
        }
        public TcpNetServer() : this(DefaultBufferSize)
        {

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

        private protected override void OnAccepted(TcpClientBase client)
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
        private protected override void OnDisconnected(TcpClientBase client)
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
            ClientDisconnected?.Invoke(this, new TcpEventArgs(client));
        }
        public event TcpEventHandler ClientDisconnected;
        #endregion
    }
}
