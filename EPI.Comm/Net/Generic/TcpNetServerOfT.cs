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

            base.Received += TcpNetServerReceived;
            base.ClientAccpeted += OnClientAccpeted;
            base.ClientClosed += OnClientClosed;
        }
        private protected override TcpNetClient CreateClient(TcpClient client)
        {
            var result = new TcpNetClient<Theader>(client, BufferSize, GetBodySize);
            return result;
        }
        #region Event
        private void OnClientClosed(object sender, TcpEventArgs e)
        {
            var oldClient = e.TcpNetClient as TcpNetClient<Theader>;
            if (IsValidClientType(oldClient) && clients.Contains(oldClient))
            {
                clients.Remove(oldClient);
            }
        }

        private void OnClientAccpeted(object sender, TcpEventArgs e)
        {
            var newClient = e.TcpNetClient as TcpNetClient<Theader>;
            if (IsValidClientType(newClient) && !clients.Contains(newClient))
            {
                clients.Add(newClient);
            }
        }
        private bool IsValidClientType(TcpNetClient client)
        {
            return client is TcpNetClient<Theader>;
        }
        private void TcpNetServerReceived(object sender, DataReceiveEventArgs e)
        {
        }
        public TcpNetServer(Func<Theader, int> getBodySize) : this(DefaultBufferSize, getBodySize)
        {

        }
        new public event PacketEventHandler<Theader> Received;
        new public event TcpEventHandler ClientAccpeted;
        new public event TcpEventHandler ClientClosed;
        #endregion

        public void Send(Theader header, byte[] body)
        {
            throw new NotImplementedException();
        }
    }
}
