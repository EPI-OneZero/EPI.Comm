using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

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
