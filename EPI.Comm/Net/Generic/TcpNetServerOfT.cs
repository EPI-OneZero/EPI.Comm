﻿using EPI.Comm.Net.Generic.Events;
using EPI.Comm.Net.Generic.Packets;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using static EPI.Comm.CommConfig;
namespace EPI.Comm.Net.Generic
{
    public class TcpNetServer<Theader> : TcpServerBase, IComm<Theader>
        where Theader : new()
    {
        #region Field & Property
        private readonly List<TcpNetClient<Theader>> clients = new List<TcpNetClient<Theader>>();
        public ClientCollection<Theader> Clients => new ClientCollection<Theader>(clients.ToArray());
        internal Func<Theader, int> GetBodySize { get; private set; }
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
        public TcpNetServer(int bufferSize, Func<Theader, int> getBodySize) : base(bufferSize)
        {
            GetBodySize = getBodySize;

        }
        public TcpNetServer(Func<Theader, int> getBodySize) : this(DefaultBufferSize, getBodySize)
        {
        }
        #endregion

        #region Send Receive
        public void Send(Theader header, byte[] body)
        {
            var packetMakerToSend = new PacketMaker<Theader>(GetBodySize, false)
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
        private void OnClientReceived(object sender, PacketEventArgs<Theader> e)
        {
            Received?.Invoke(this, e);
        }
        public event PacketEventHandler<Theader> Received;
        #endregion

        #region Accept
        protected override TcpClientBase CreateClient(TcpClient client)
        {
            var result = new TcpNetClient<Theader>(client, BufferSize, GetBodySize)
            { IsBigEndian = IsBigEndian };
            return result;
        }

        protected override void AttachClient(TcpClientBase client)
        {
            base.AttachClient(client);
            var newClient = client as TcpNetClient<Theader>;
            clients.Add(newClient);
            newClient.Received += OnClientReceived;
            ClientConnected?.Invoke(this, new TcpEventArgs<Theader>(newClient));
        }
        public event TcpEventHandler<Theader> ClientConnected;

        #endregion

        #region Close

        protected override void DetachClient(TcpClientBase client)
        {
            base.DetachClient(client);

            var oldClient = client as TcpNetClient<Theader>;
            clients.Remove(oldClient);
            oldClient.Received -= OnClientReceived;
            ClientDisconnected?.Invoke(this, new TcpEventArgs<Theader>(oldClient));
        }

        public event TcpEventHandler<Theader> ClientDisconnected;
        #endregion
    }
    public class TcpNetServer<Theader, Tfooter> : TcpServerBase, IComm<Theader, Tfooter>
       where Theader : new() where Tfooter : new()
    {
        #region Field & Property
        private readonly List<TcpNetClient<Theader, Tfooter>> clients = new List<TcpNetClient<Theader, Tfooter>>();
        public ClientCollection<Theader, Tfooter> Clients => new ClientCollection<Theader, Tfooter>(clients.ToArray());
        internal Func<Theader, int> GetBodySize { get; private set; }
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
        public TcpNetServer(int bufferSize, Func<Theader, int> getBodySize) : base(bufferSize)
        {
            GetBodySize = getBodySize;
        }
        public TcpNetServer(Func<Theader, int> getBodySize) : this(DefaultBufferSize, getBodySize)
        {
        }
        #endregion

        #region Send Receive
        public void Send(Theader header, byte[] body, Tfooter footer)
        {
            var packetMakerToSend = new PacketMaker<Theader, Tfooter>(GetBodySize, false)
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
        private void OnClientReceived(object sender, PacketEventArgs<Theader, Tfooter> e)
        {
            Received?.Invoke(this, e);
        }
        public event PacketEventHandler<Theader, Tfooter> Received;
        #endregion

        #region Accept
        protected override TcpClientBase CreateClient(TcpClient client)
        {
            var result = new TcpNetClient<Theader, Tfooter>(client, BufferSize, GetBodySize)
            { IsBigEndian = IsBigEndian };
            return result;
        }
        protected override void AttachClient(TcpClientBase client)
        {
            base.AttachClient(client);
            var newClient = client as TcpNetClient<Theader, Tfooter>;
            clients.Add(newClient);
            newClient.Received += OnClientReceived;
            ClientConnected?.Invoke(this, new TcpEventArgs<Theader, Tfooter>(newClient));
        }
        public event TcpEventHandler<Theader, Tfooter> ClientConnected;
        #endregion

        #region Close
        protected override void DetachClient(TcpClientBase client)
        {
            base.DetachClient(client);
            var oldClient = client as TcpNetClient<Theader, Tfooter>;
            clients.Remove(oldClient);
            oldClient.Received -= OnClientReceived;
            ClientDisconnected?.Invoke(this, new TcpEventArgs<Theader, Tfooter>(oldClient));
        }
        public event TcpEventHandler<Theader, Tfooter> ClientDisconnected;
        #endregion
    }
}
