using EPI.Comm.Net.Events;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using static EPI.Comm.CommConfig;
namespace EPI.Comm.Net
{
    public class TcpNetServer : TcpServerBase, IComm
    {
        #region Field & Property
        private readonly List<TcpNetClient> clients = new List<TcpNetClient>();
        public ClientCollection Clients => new ClientCollection(clients.ToArray());
        #endregion

        #region CTOR
        public TcpNetServer(int bufferSize) : base(bufferSize)
        {
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
        private protected override void AttachClient(TcpClientBase client)
        {
            base.AttachClient(client);
            var newClient = client as TcpNetClient;
            clients.Add(newClient);
            newClient.Received += OnClientReceived;
            ClientConnected?.Invoke(this, new TcpEventArgs(newClient));

        }
        public event TcpEventHandler ClientConnected;
        #endregion

        #region Close
        private protected override void DetachClient(TcpClientBase client)
        {
            base.DetachClient(client);
            var oldClient = client as TcpNetClient;
            clients.Remove(oldClient);
            oldClient.Received -= OnClientReceived;
            ClientDisconnected?.Invoke(this, new TcpEventArgs(oldClient));

        }
        public event TcpEventHandler ClientDisconnected;
        #endregion
    }
}
