using EPI.Comm.Net.Events;
using System.Net.Sockets;

namespace EPI.Comm.Net
{
    public class TcpNetClient : TcpClientBase, IComm
    {
        #region CTOR
        public TcpNetClient(int bufferSize) : base(bufferSize)
        {

        }
        public TcpNetClient() : this(DefaultBufferSize)
        {
        }

        internal TcpNetClient(TcpClient client, int bufferSize) : base(client, bufferSize)
        {

        }
        #endregion

        #region Receive
        private protected override void SocketReceived(object sender, SocketReceiveEventArgs e)
        {
            Received?.Invoke(this, new PacketEventArgs(e.From, e.ReceivedBytes));
        }
        public event PacketEventHandler Received;
        #endregion
    }
}
