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
using System.Collections.ObjectModel;

namespace EPI.Comm.Net.Generic
{
   
    public class TcpNetServer<Theader> : TcpNetServer, IComm<Theader>
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
        private void OnClientClosed(object sender, Net.Events.TcpEventArgs e)
        {
            if(e.TcpNetClient is TcpNetClient<Theader> client && clients.Contains(client))
            {
                clients.Remove(client);
            }
        }

        private void OnClientAccpeted(object sender, Net.Events.TcpEventArgs e)
        {
            if (e.TcpNetClient is TcpNetClient<Theader> client && !clients.Contains(client))
            {
                clients.Add(client);
            }
        }

        private void TcpNetServerReceived(object sender, Net.Events.CommReceiveEventArgs e)
        {
        }
        public TcpNetServer(Func<Theader, int> getBodySize) : this(DefaultBufferSize, getBodySize)
        {

        }
        new public event PacketEventHandler<Theader> Received;
        new public event CommEventHandler ClientAccpeted;
        new public event CommEventHandler ClientClosed;
        #endregion

        public void Send(Theader header, byte[] body)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class ClientCollection<Theader> : ReadOnlyCollection<TcpNetClient<Theader>>
    {
        internal ClientCollection(IList<TcpNetClient<Theader>> list) : base(list)
        {
        }
    }
    public sealed class ClientCollection<Theader, Tfooter> : ReadOnlyCollection<TcpNetClient<Theader, Tfooter>>
    {
        internal ClientCollection(IList<TcpNetClient<Theader, Tfooter>> list) : base(list)
        {
        }
    }
}
