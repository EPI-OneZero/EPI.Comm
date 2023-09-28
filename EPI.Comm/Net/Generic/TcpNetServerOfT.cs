using EPI.Comm.Buffers;
using EPI.Comm.Net.Generic.Events;
using EPI.Comm.Net.Generic.Packets;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EPI.Comm.Net.Generic
{

    public class TcpNetServer<Theader> : TcpServerBase, IComm<Theader>
        where Theader : new()
    {
        #region Field & Property
        private List<TcpNetClient<Theader>> clients = new List<TcpNetClient<Theader>>();
        public ClientCollection<Theader> Clients { get; private set; }
        internal Func<Theader, int> GetBodySize { get; private set; }
        #endregion

        #region CTOR
        public TcpNetServer(int bufferSize, Func<Theader, int> getBodySize) : base(bufferSize)
        {
            Clients = new ClientCollection<Theader>(clients);
            GetBodySize = getBodySize;

        }
        public TcpNetServer(Func<Theader, int> getBodySize) : this(DefaultBufferSize, getBodySize)
        {
        }
        #endregion

        #region Send Receive
        public void Send(Theader header, byte[] body)
        {
            var result = Parallel.ForEach(clients, c =>
            {
                c.Send(header, body);
            });
        }
        private void OnClientReceived(object sender, PacketEventArgs<Theader> e)
        {
            Received?.Invoke(this, e);
        }
        public event PacketEventHandler<Theader> Received;
        #endregion

        #region Accept
        private protected override TcpClientBase CreateClient(TcpClient client)
        {
            var result = new TcpNetClient<Theader>(client, BufferSize, GetBodySize);
            return result;
        }
        private protected override void OnClientAccepted(TcpClientBase client)
        {
            var newClient = client as TcpNetClient<Theader>;
            if (IsValidClientType(newClient) && !clients.Contains(newClient))
            {
                AttachClient(newClient);
                ClientAccpeted?.Invoke(this, new TcpEventArgs<Theader>(newClient));
            }
        }
        private void AttachClient(TcpNetClient<Theader> client)
        {
            clients.Add(client);
            client.Received += OnClientReceived;

        }
        public event TcpEventHandler<Theader> ClientAccpeted;

        #endregion

        #region Close


        private protected override void OnClientDisconnected(TcpClientBase client)
        {
            var oldClient = client as TcpNetClient<Theader>;
            if (IsValidClientType(oldClient) && clients.Contains(oldClient))
            {
                DetachClient(oldClient);
                ClientDisconnected?.Invoke(this, new TcpEventArgs<Theader>(oldClient));
            }
        }
        private void DetachClient(TcpNetClient<Theader> client)
        {
            clients.Remove(client);
            client.Received -= OnClientReceived;
        }
        private bool IsValidClientType(TcpClientBase client)
        {
            return client is TcpNetClient<Theader>;
        }

        public event TcpEventHandler<Theader> ClientDisconnected;
        #endregion

        #region Event


        #endregion


    }

    public class TcpNetServer<Theader, Tfooter> : TcpServerBase, IComm<Theader, Tfooter>
       where Theader : new() where Tfooter : new()
    {
        #region Field & Property
        private List<TcpNetClient<Theader, Tfooter>> clients = new List<TcpNetClient<Theader, Tfooter>>();
        public ClientCollection<Theader, Tfooter> Clients { get; private set; }
        internal Func<Theader, int> GetBodySize { get; private set; }
        #endregion

        #region CTOR
        public TcpNetServer(int bufferSize, Func<Theader, int> getBodySize) : base(bufferSize)
        {
            Clients = new ClientCollection<Theader, Tfooter>(clients);
            GetBodySize = getBodySize;

        }
        public TcpNetServer(Func<Theader, int> getBodySize) : this(DefaultBufferSize, getBodySize)
        {
        }
        #endregion

        #region Send Receive
        public void Send(Theader header, byte[] body, Tfooter footer)
        {
            var result = Parallel.ForEach(clients, c =>
            {
                c.Send(header, body, footer);
            });
        }
        private void OnClientReceived(object sender, PacketEventArgs<Theader, Tfooter> e)
        {
            Received?.Invoke(this, e);
        }
        public event PacketEventHandler<Theader, Tfooter> Received;
        #endregion

        #region Accept
        private protected override TcpClientBase CreateClient(TcpClient client)
        {
            var result = new TcpNetClient<Theader, Tfooter>(client, BufferSize, GetBodySize);
            return result;
        }
        private protected override void OnClientAccepted(TcpClientBase client)
        {
            var newClient = client as TcpNetClient<Theader, Tfooter>;
            if (IsValidClientType(newClient) && !clients.Contains(newClient))
            {
                AttachClient(newClient);
                ClientAccpeted?.Invoke(this, new TcpEventArgs<Theader, Tfooter>(newClient));
            }
        }
        private void AttachClient(TcpNetClient<Theader, Tfooter> client)
        {
            clients.Add(client);
            client.Received += OnClientReceived;

        }
        public event TcpEventHandler<Theader, Tfooter> ClientAccpeted;

        #endregion

        #region Close
        private protected override void OnClientDisconnected(TcpClientBase client)
        {
            var oldClient = client as TcpNetClient<Theader, Tfooter>;
            if (IsValidClientType(oldClient) && clients.Contains(oldClient))
            {
                DetachClient(oldClient);
                ClientDisconnected?.Invoke(this, new TcpEventArgs<Theader, Tfooter>(oldClient));
            }
        }
        private void DetachClient(TcpNetClient<Theader, Tfooter> client)
        {
            clients.Remove(client);
            client.Received -= OnClientReceived;
        }
        private bool IsValidClientType(TcpClientBase client)
        {
            return client is TcpNetClient<Theader, Tfooter>;
        }

        public event TcpEventHandler<Theader, Tfooter> ClientDisconnected;
        #endregion
    }
}
