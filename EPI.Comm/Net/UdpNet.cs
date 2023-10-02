using EPI.Comm.Net.Events;
using System;
using System.Collections.Generic;
using System.Text;
using static EPI.Comm.CommConfig;
namespace EPI.Comm.Net
{
    internal class UdpNet : UdpBase, IComm
    {
        public event PacketEventHandler Received;
        public UdpNet(int bufferSize) :base(bufferSize)
        {
        }
        public UdpNet() : base(DefaultBufferSize)
        {
        }
        public void Send(byte[] bytes)
        {
            SendBytes(bytes);
        }
        private protected override void OnReceived(PacketEventArgs e)
        {
            Received?.Invoke(this, e);
        }
    }
}
