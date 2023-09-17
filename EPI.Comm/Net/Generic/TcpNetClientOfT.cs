using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EPI.Comm.Net.Generic
{
    public class TcpNetClient<Theader, TFooter> : TcpNetClient<Theader>
    {
        public TcpNetClient(string ip, int port, int bufferSize, Func<Theader, int> getBodySize) : base(ip, port, bufferSize, getBodySize)
        {
            base.PacketReceived += BasePacketReceived;
        }

      
        public TcpNetClient(string ip, int port, Func<Theader, int> getPacketSize) : this(ip, port, 8192, getPacketSize)
        {

        }
        private void BasePacketReceived(object sender, PacketEventArgs<Theader> e)
        {

        }

        new public event EventHandler<PacketEventArgs<Theader, TFooter>> PacketReceived;
    }
    public class TcpNetClient<Theader> : TcpNetClient
    {
        protected Func<Theader, int> GetBodySizeFunc { get; private set; }
        private Queue<byte> buffer;
        internal int BufferCount => buffer.Count;
        internal int HeaderSize { get; set; }
        public TcpNetClient(string ip, int port, int bufferSize, Func<Theader, int> getBodySize) : base(ip, port, bufferSize)
        {
            HeaderSize = Marshal.SizeOf(typeof(Theader));
            buffer = new Queue<byte>(bufferSize);
            Received += ClientReceived;
        }
        public TcpNetClient(string ip, int port, Func<Theader, int> getBodySize) : this(ip, port, DefaultBufferSize, getBodySize)
        {

        }
        private Packet<Theader> HeaderPacket { get; set; }
        private SerializeState serializeState = SerializeState.Initial;
        private object recvLock = new object();
        private void ClientReceived(object sender, CommReceiveEventArgs e)
        {
            lock (recvLock)
            {
                Enqueue(e.ReceivedBytes);
                serializeState = SerializePacket(serializeState);
                if (serializeState == SerializeState.BodySerialized)
                {
                    PacketReceived?.Invoke(this, new PacketEventArgs<Theader>(HeaderPacket));
                    serializeState = SerializeState.Initial;
                    HeaderPacket = null;
                }
            }
           
        }
        private SerializeState SerializePacket(SerializeState state)
        {
            switch (state)
            {
                case SerializeState.Initial:
                    HeaderPacket = new Packet<Theader>();
                    if (SerializeHeader(HeaderPacket))
                    {
                        return SerializePacket(SerializeState.HeaderSerialized);
                    }
                    else
                    {
                        return state;
                    }
                case SerializeState.HeaderSerialized:
                    if (SerializeBody(HeaderPacket))
                    {
                        return SerializeState.BodySerialized;
                    }
                    else
                    {
                        return state;
                    }
                default:
                    throw new InvalidOperationException();
            }
        }
        private bool SerializeHeader(Packet<Theader> packet)
        {
            if (PacketSerializer.IsEnoughSize(BufferCount, 0, HeaderSize))
            {
                var headerBytes = Dequeue(HeaderSize);
               packet.Header = PacketSerializer.Deserialize<Theader>(headerBytes, HeaderSize, 0);
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool SerializeBody(Packet<Theader> packet)
        {
            int bodySize = GetBodySize(HeaderPacket.Header);
            if (bodySize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bodySize));
            }
            if (PacketSerializer.IsEnoughSize(BufferCount, 0, bodySize))
            {
                packet.Body = Dequeue(bodySize);
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public event EventHandler<PacketEventArgs<Theader>> PacketReceived;
        private int GetBodySize(Theader t)
        {
            return GetBodySizeFunc?.Invoke(t) ?? 0;
        }
        protected void Enqueue(byte[] bytes)
        {
            foreach (var b in bytes)
            {
                buffer.Enqueue(b);
            }
        }
        protected byte[] Dequeue(int size)
        {
            var bytes = new byte[size];
            for (int i = 0; i < size; i++)
            {
                bytes[i] = buffer.Dequeue();
            }
            return bytes;
        }

    }
    internal enum SerializeState
    {
        Initial,
        HeaderSerialized,
        BodySerialized,
        FooterSerialized
    }
    internal static class PacketSerializer
    {

        
      

        public static bool IsEnoughSize(int sourceSize, int targetSize, int offset)
        {
            return sourceSize - offset >= targetSize;
        }
        public static byte[] Deserialize(byte[] bytes, int targetSize, int offset)
        {
            if (IsEnoughSize(bytes?.Length ?? 0, offset, targetSize))
            {
                var result = new byte[targetSize];
                if(targetSize > 0)
                {
                    Array.Copy(bytes, offset, result, 0, targetSize);
                }
                return result;
            }
            else
            {
                throw new IndexOutOfRangeException(nameof(bytes));
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <param name="targetSize"></param>
        /// <param name="offset"></param>
        /// <param name="caller"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public static T Deserialize<T>(byte[] bytes, int targetSize, int offset, [CallerMemberName] string caller = "")
        {
            if (IsEnoughSize(bytes?.Length ?? 0, offset, targetSize))
            {
                var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                var result = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject() + offset);
                handle.Free();
                return result;
            }
            else
            {
                throw new IndexOutOfRangeException($"{caller} : nameof(bytes)");
            }
        }
    }
}
