using EPI.Comm.Buffers;
using EPI.Comm.Net.Events;
using EPI.Comm.Net.Generic.Events;
using EPI.Comm.Net.Generic.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EPI.Comm.Net.Generic
{
    public class TcpNetClient<Theader> : TcpNetClient, IComm<Theader>
    {
        internal int HeaderSize { get; set; }
        internal IBuffer ReceiveBuffer { get; set; } = new QueueBuffer();
        internal Func<Theader, int> GetBodySize { get; private set; }
        internal Packet<Theader> Packet { get; set; }
        public TcpNetClient(int bufferSize, Func<Theader, int> getBodySize) : base(bufferSize)
        {
            HeaderSize = Marshal.SizeOf(typeof(Theader));
            GetBodySize = getBodySize;
            base.Received += ClientReceived;
        }
        public TcpNetClient(Func<Theader, int> getBodySize) : this(DefaultBufferSize, getBodySize)
        {
        }

        internal TcpNetClient(TcpClient client, int bufferSize, Func<Theader, int> getBodySize) : base(client, bufferSize)
        {
            HeaderSize = Marshal.SizeOf(typeof(Theader));
            GetBodySize = getBodySize;
            base.Received += ClientReceived;
        }
        public void Send(Theader header, byte[] body)
        {
            var fullPacketBytes = new byte[HeaderSize + body.Length];
            PacketSerializer.Serialize(header, fullPacketBytes, 0, HeaderSize);
            Buffer.BlockCopy(body, 0, fullPacketBytes, HeaderSize, body.Length);
            Send(fullPacketBytes);
        }
        private void ClientReceived(object sender, CommReceiveEventArgs e)
        {
            Packet = new Packet<Theader>(GetBodySize);
            ReceiveBuffer.AddBytes(e.ReceivedBytes);
            if (Packet.TryDeserializePacket(ReceiveBuffer))
            {
                Received?.Invoke(this, new PacketEventArgs<Theader>(Packet));
            }
        }
        new public event PacketEventHandler<Theader> Received;
    }
    public class TcpNetClient<Theader, Tfooter> : TcpNetClient, IComm<Theader,Tfooter>
    {
        internal int HeaderSize { get; set; }
        internal int FooterSize { get; set; }
        internal IBuffer ReceiveBuffer { get; set; } = new QueueBuffer();
        private Packet<Theader, Tfooter> Packet { get; set; }
        internal Func<Theader, int> GetBodySize { get; private set; }
        public TcpNetClient(int bufferSize, Func<Theader, int> getBodySize) : base(bufferSize)
        {
            GetBodySize = getBodySize;
            HeaderSize = Marshal.SizeOf(typeof(Theader));
            FooterSize = Marshal.SizeOf(typeof(Tfooter));
            base.Received += ClientReceived;
        }
        public TcpNetClient(Func<Theader, int> getBodySize) : this(DefaultBufferSize, getBodySize)
        {
        }
        internal TcpNetClient(TcpClient client, int bufferSize, Func<Theader, int> getBodySize) : base(client, bufferSize)
        {
            HeaderSize = Marshal.SizeOf(typeof(Theader));
            GetBodySize = getBodySize;
            base.Received += ClientReceived;
        }
        public void Send(Theader header, byte[] body, Tfooter footer)
        {
            int bodySize = body.Length;
            var fullPacketBytes = new byte[HeaderSize + bodySize + FooterSize];

            PacketSerializer.Serialize(header, fullPacketBytes, 0, HeaderSize);

            Buffer.BlockCopy(body, 0, fullPacketBytes, HeaderSize, bodySize);

            PacketSerializer.Serialize(footer, fullPacketBytes, HeaderSize + bodySize, FooterSize);

            Send(fullPacketBytes);
        }
        private void ClientReceived(object sender, CommReceiveEventArgs e)
        {
            lock (this)
            {
                if (Packet == null)
                {
                    Packet = new Packet<Theader, Tfooter>(GetBodySize);
                }
                ReceiveBuffer.AddBytes(e.ReceivedBytes);

                if (Packet.TryDeserializePacket(ReceiveBuffer))
                {
                    var packet = Packet;
                    Packet = null;
                    Received?.Invoke(this, new PacketEventArgs<Theader, Tfooter>(packet));
                }
            }
           
        }
        new public event PacketEventHandler<Theader, Tfooter> Received;
    }
   
}
