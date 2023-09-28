using System.Net.Sockets;

namespace EPI.Comm.Net
{
    public class UdpNetClient
    {
        public UdpClient UdpClient { get; private set; }
        public UdpNetClient()
        {
        }
    }
}
