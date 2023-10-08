using EPI.Comm.Net.Events;
using static EPI.Comm.CommConfig;
namespace EPI.Comm.Net
{
    internal class UdpNet : UdpBase, IComm
    {
        public event PacketEventHandler Received;
        public UdpNet(int bufferSize) : base(bufferSize)
        {
        }
        public UdpNet() : base(DefaultBufferSize)
        {
        }

        protected override void OnReceived(PacketEventArgs e)
        {
            Received?.Invoke(this, e);
        }
    }
}
