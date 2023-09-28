using EPI.Comm.Buffers;
using EPI.Comm.Net.Events;
using EPI.Comm.Net.Generic.Events;
using EPI.Comm.Net.Generic.Packets;
using EPI.Comm.Utils;
using System;
using System.Net.Sockets;

namespace EPI.Comm.Net.Generic
{
    /// <summary>
    /// 
    /// 
    /// 
    /// </summary>
    /// <typeparam name="Theader">Marshal.SizeOf 가능 및 StructLayout Sequential 확인 필수</typeparam>
    public class TcpNetClient<Theader> : TcpClientBase, IComm<Theader>
        where Theader : new()
    {
        public int HeaderSize { get; private set; }
        internal IBuffer ReceiveBuffer { get; set; } = new QueueBuffer();
        internal Func<Theader, int> GetBodySize { get; private set; }
        internal Packet<Theader> Packet { get; set; }
        public TcpNetClient(int bufferSize, Func<Theader, int> getBodySize) : base(bufferSize)
        {
            HeaderSize = ObjectUtil.SizeOf<Theader>();
            GetBodySize = getBodySize;
        }
        public TcpNetClient(Func<Theader, int> getBodySize) : this(DefaultBufferSize, getBodySize)
        {
        }

        internal TcpNetClient(TcpClient client, int bufferSize, Func<Theader, int> getBodySize) : base(client, bufferSize)
        {
            HeaderSize = ObjectUtil.SizeOf<Theader>();
            GetBodySize = getBodySize;
        }
        public void Send(Theader header, byte[] body)
        {
            var headerDefinedBodySize = GetBodySize?.Invoke(header) ?? 0;
            var fullPacketBytes = Packet<Theader>.GeneratePacketBytes(header,body,HeaderSize,headerDefinedBodySize);
            SendBytes(fullPacketBytes);
        }

        public event PacketEventHandler<Theader> Received;
        private protected override void SocketReceived(object sender, SocketReceiveEventArgs e)
        {
            lock (this)
            {
                if (Packet == null)
                {
                    Packet = new Packet<Theader>(GetBodySize);
                }
                ReceiveBuffer.AddBytes(e.ReceivedBytes);
                if (Packet.TryDeserialize(ReceiveBuffer))
                {
                    Received?.Invoke(this, new PacketEventArgs<Theader>(e.From, Packet));
                }
            }

        }
    }

    /// <summary>
    ///  질문 : TcpNetClient<Theader, Tfooter>, TcpNetClient<Theader>
    ///  는 각각 TcpNetClient를 상속하는게 맞는가??
    ///  제네릭 서버나 제네릭 클라이언트는 논제네릭 서버와 클라이언트를 has a로 소유하는게 맞는가?
    ///  아니면 is a로 상속하는 게 맞는가
    /// </summary>
    /// <typeparam name="Theader">Marshal.SizeOf 가능 및 StructLayout Sequential 확인 필수</typeparam>
    /// <typeparam name="Tfooter">Marshal.SizeOf 가능 및 StructLayout Sequential 확인 필수</typeparam>
    public class TcpNetClient<Theader, Tfooter> : TcpClientBase, IComm<Theader, Tfooter>
        where Theader : new()
    {
        internal int HeaderSize { get; set; }
        internal int FooterSize { get; set; }
        internal IBuffer ReceiveBuffer { get; set; } = new QueueBuffer();
        private Packet<Theader, Tfooter> Packet { get; set; }
        internal Func<Theader, int> GetBodySize { get; private set; }
        public TcpNetClient(int bufferSize, Func<Theader, int> getBodySize) : base(bufferSize)
        {
            SetPacketProperties(getBodySize);
        }
        public TcpNetClient(Func<Theader, int> getBodySize) : this(DefaultBufferSize, getBodySize)
        {
        }
        internal TcpNetClient(TcpClient client, int bufferSize, Func<Theader, int> getBodySize) : base(client, bufferSize)
        {
            SetPacketProperties(getBodySize);
        }
        private void SetPacketProperties(Func<Theader, int> getBodySize)
        {
            HeaderSize = ObjectUtil.SizeOf<Theader>();
            FooterSize = ObjectUtil.SizeOf<Tfooter>();
            GetBodySize = getBodySize;
        }
        public void Send(Theader header, byte[] body, Tfooter footer)
        {
            var headerDefinedBodySize = GetBodySize?.Invoke(header) ?? 0;
            var fullPacketBytes = Packet<Theader, Tfooter>.GeneratePacketBytes(header, body, footer, HeaderSize, headerDefinedBodySize, FooterSize);

            SendBytes(fullPacketBytes);
        }
        private protected override void SocketReceived(object sender, SocketReceiveEventArgs e)
        {
            lock (this)
            {
                if (Packet == null)
                {
                    Packet = new Packet<Theader, Tfooter>(GetBodySize);
                }
                ReceiveBuffer.AddBytes(e.ReceivedBytes);

                if (Packet.TryDeserialize(ReceiveBuffer))
                {
                    var packet = Packet;
                    Packet = null;
                    Received?.Invoke(this, new PacketEventArgs<Theader, Tfooter>(e.From, packet));
                }
            }
        }

        public event PacketEventHandler<Theader, Tfooter> Received;
    }

}
