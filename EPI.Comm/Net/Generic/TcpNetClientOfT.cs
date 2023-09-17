using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EPI.Comm.Net.Generic
{
    public class TcpNetClient<Theader,TFooter> : TcpNetClient
    {
        public TcpNetClient(string ip, int port, int bufferSize, Func<Theader, int> getPacketSize) : base(ip, port, bufferSize)
        {

        }
        public TcpNetClient(string ip, int port, Func<Theader, int> getPacketSize) : this(ip, port, 8192, getPacketSize)
        {

        }
        public event EventHandler<PacketEventArgs<Theader,TFooter>> PacketReceived;
    }
    public class TcpNetClient<Theader> : TcpNetClient
    {
        public TcpNetClient(string ip, int port, int bufferSize, Func<Theader, int> getBodySize) : base(ip, port, bufferSize)
        {
            base.Received += Client_Received;
        }

        private void Client_Received(object sender, CommReceiveEventArgs e)
        {
          
        }

        public TcpNetClient(string ip, int port, Func<Theader, int> getPacketSize) : this(ip, port, 8192, getPacketSize)
        {

        }
        public event EventHandler<PacketEventArgs<Theader>> PacketReceived;
    }
}
