using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using EPI.Comm.Net.Events;

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
        /// <summary>
        /// server가 acept한 client에 대한 생성자
        /// </summary>
        /// <param name="client"></param>
        /// <param name="bufferSize"></param>
        internal TcpNetClient(TcpClient client, int bufferSize) : base(client, bufferSize)
        {

        }
        #endregion
        private protected override void SocketReceived(object sender, SocketReceiveEventArgs e)
        {
            Received?.Invoke(this, new PacketEventArgs(e.From, e.ReceivedBytes));
        }
        public event PacketEventHandler Received;
    }
}
