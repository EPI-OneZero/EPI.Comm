using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using static EPI.Comm.CommConfig;
namespace EPI.Comm.Net.Generic
{
    public class TcpNetServer<THeader> : TcpServerBase, IComm<THeader>
        where THeader : new()
    {
        #region Field & Property
        public ClientCollection<THeader> Clients => new ClientCollection<THeader>(SafeCopyClients<TcpNetClient<THeader>>());
        internal Func<THeader, int> GetBodySize { get; private set; }
        protected bool isBigEndian;
        public bool IsBigEndian
        {
            get
            {
                return isBigEndian;
            }
            set
            {
                isBigEndian = value;
                foreach (var client in Clients)
                {
                    client.IsBigEndian = isBigEndian;
                }
            }
        }
        #endregion

        #region CTOR
        public TcpNetServer(int bufferSize, Func<THeader, int> getBodySize) : base(bufferSize)
        {
            GetBodySize = getBodySize;

        }
        public TcpNetServer(Func<THeader, int> getBodySize) : this(DefaultBufferSize, getBodySize)
        {
        }
        #endregion

        #region Send Receive
        public void Send(THeader header, byte[] body)
        {
            var packetMakerToSend = new PacketMaker<THeader>(GetBodySize, false)
            {
                Header = header,
                Body = body,
            };
            var fullPacketBytes = packetMakerToSend.SerializePacket(IsBigEndian);
            Parallel.ForEach(Clients, c =>
            {
                c.Send(fullPacketBytes);
            });
        }
        private void OnClientReceived(object sender, PacketEventArgs<THeader> e)
        {
            Received?.Invoke(this, e);
        }
        public event PacketEventHandler<THeader> Received;
        #endregion

        #region Accept
        protected override TcpClientBase CreateClient(TcpClient client)
        {
            var result = new TcpNetClient<THeader>(client, BufferSize, GetBodySize)
            { IsBigEndian = IsBigEndian };
            return result;
        }

        protected override void AttachClient(TcpClientBase client)
        {
            base.AttachClient(client);
            var newClient = client as TcpNetClient<THeader>;
            newClient.Received += OnClientReceived;
            ClientConnected?.Invoke(this, new TcpEventArgs<THeader>(newClient));
        }
        public event TcpEventHandler<THeader> ClientConnected;

        #endregion

        #region Close

        protected override void DetachClient(TcpClientBase client)
        {
            base.DetachClient(client);

            var oldClient = client as TcpNetClient<THeader>;
            oldClient.Received -= OnClientReceived;
            ClientDisconnected?.Invoke(this, new TcpEventArgs<THeader>(oldClient));
        }

        public event TcpEventHandler<THeader> ClientDisconnected;
        #endregion
    }
    public class TcpNetServer<THeader, TFooter> : TcpServerBase, IComm<THeader, TFooter>
       where THeader : new() where TFooter : new()
    {
        #region Field & Property
        public ClientCollection<THeader, TFooter> Clients => new ClientCollection<THeader, TFooter>(SafeCopyClients<TcpNetClient<THeader, TFooter>>());
        internal Func<THeader, int> GetBodySize { get; private set; }
        protected bool isBigEndian;
        public bool IsBigEndian
        {
            get
            {
                return isBigEndian;
            }
            set
            {
                isBigEndian = value;
                foreach (var client in Clients)
                {
                    client.IsBigEndian = isBigEndian;
                }
            }
        }
        #endregion

        #region CTOR
        public TcpNetServer(int bufferSize, Func<THeader, int> getBodySize) : base(bufferSize)
        {
            GetBodySize = getBodySize;
        }
        public TcpNetServer(Func<THeader, int> getBodySize) : this(DefaultBufferSize, getBodySize)
        {
        }
        #endregion

        #region Send Receive
        public void Send(THeader header, byte[] body, TFooter footer)
        {
            var packetMakerToSend = new PacketMaker<THeader, TFooter>(GetBodySize, false)
            {
                Header = header,
                Body = body,
                Footer = footer
            };
            var fullPacketBytes = packetMakerToSend.SerializePacket(IsBigEndian);
            Parallel.ForEach(Clients, c =>
            {
                c.Send(fullPacketBytes);
            });
        }
        private void OnClientReceived(object sender, PacketEventArgs<THeader, TFooter> e)
        {
            Received?.Invoke(this, e);
        }
        public event PacketEventHandler<THeader, TFooter> Received;
        #endregion

        #region Accept
        protected override TcpClientBase CreateClient(TcpClient client)
        {
            var result = new TcpNetClient<THeader, TFooter>(client, BufferSize, GetBodySize)
            { IsBigEndian = IsBigEndian };
            return result;
        }
        protected override void AttachClient(TcpClientBase client)
        {
            base.AttachClient(client);
            var newClient = client as TcpNetClient<THeader, TFooter>;
            newClient.Received += OnClientReceived;
            ClientConnected?.Invoke(this, new TcpEventArgs<THeader, TFooter>(newClient));
        }
        public event TcpEventHandler<THeader, TFooter> ClientConnected;
        #endregion

        #region Close
        protected override void DetachClient(TcpClientBase client)
        {
            base.DetachClient(client);
            var oldClient = client as TcpNetClient<THeader, TFooter>;
            oldClient.Received -= OnClientReceived;
            ClientDisconnected?.Invoke(this, new TcpEventArgs<THeader, TFooter>(oldClient));
        }
        public event TcpEventHandler<THeader, TFooter> ClientDisconnected;
        #endregion
    }
}
