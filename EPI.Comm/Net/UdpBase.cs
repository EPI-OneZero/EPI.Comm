using EPI.Comm.Utils;
using System;
using System.Net;
using System.Net.Sockets;
namespace EPI.Comm.Net
{
    public abstract class UdpBase : IDisposable
    {
        #region Field & Property
        public static uint MulticastMin { get; private set; } = ConvertAddressToUint("224.0.0.0");
        public static uint MulticastMax { get; private set; } = ConvertAddressToUint("239.255.255.255");

        private readonly object StartStopLock = new object();
        private readonly object SendLock = new object();
        public UdpClient UdpClientSender { get; private set; }
        public UdpClient UdpClientReceiver { get; private set; }
        public IPEndPoint LocalEndPoint { get; private set; }
        public IPEndPoint RemoteEndPoint { get; private set; }
        public int BufferSize { get; private set; }
        private volatile bool isStarted;
        public bool IsStarted => isStarted;
        #endregion

        #region CTOR
        protected UdpBase(int bufferSize)
        {
            BufferSize = bufferSize;
        }
        #endregion

        #region Method
        public void Start(int recvPort, string sendIp, int sendPort)
        {
            lock (StartStopLock)
            {
                if (!isStarted)
                {
                    UdpClientSender = new UdpClient();
                    UdpClientReceiver = new UdpClient(new IPEndPoint(IPAddress.Any, recvPort));
                
                    LocalEndPoint = UdpClientReceiver.Client.LocalEndPoint as IPEndPoint;
                    RemoteEndPoint = new IPEndPoint(IPAddress.Parse(sendIp), sendPort);

                    SetSocketOption(UdpClientSender.Client);
                    SetSocketOption(UdpClientReceiver.Client);
                    ThreadUtil.Start(() =>
                    {
                        while (TryReceive()) ;
                    });
                    isStarted = true;
                }
            }
        }

        public void SetBroadCast(bool enable)
        {
            if (UdpClientSender != null)
            {
                UdpClientSender.EnableBroadcast = enable;
            }
        }
        public void JoinMulticast(string ip, bool multicastLoopback)
        {
            CheckInMulticastRange(ip);

            UdpClientReceiver.JoinMulticastGroup(IPAddress.Parse(ip));
            UdpClientReceiver.MulticastLoopback = multicastLoopback;
        }
        public void DropMulticast(string ip)
        {
            CheckInMulticastRange(ip);
            UdpClientReceiver.DropMulticastGroup(IPAddress.Parse(ip));
        }
        private static void CheckInMulticastRange(string ip)
        {
            uint ipUint = ConvertAddressToUint(ip);
            if (MulticastMin > ipUint || ipUint > MulticastMax)
            {
                throw new ArgumentOutOfRangeException($"{ip}는 멀티캐스트 대역이 아닙니다.");
            }
        }
        private static uint ConvertAddressToUint(string ip)
        {
            var addressBytes = IPAddress.Parse(ip).GetAddressBytes();
            Array.Reverse(addressBytes);
            return BitConverter.ToUInt32(addressBytes, 0);
        }
        private void SetSocketOption(Socket socket)
        {
            socket.ReceiveBufferSize = BufferSize;
            socket.SendBufferSize = BufferSize;
        }
        public void Stop()
        {

            lock (StartStopLock)
            {
                if (isStarted)
                {
                    UdpClientReceiver?.Dispose();
                    UdpClientSender?.Dispose();
                    UdpClientSender = null;
                    UdpClientReceiver = null;
                    LocalEndPoint = null;
                    RemoteEndPoint = null;
                    OnStop();
                    isStarted = false;
                }

            }
        }
        protected virtual void OnStop()
        {
        }
        public void Send(byte[] bytes)
        {
            try
            {
                lock (SendLock)
                {
                    _ = UdpClientSender?.Send(bytes, bytes.Length, RemoteEndPoint);
                }
            }
            catch (SocketException e)
            {
                Logger.Default.WriteLine(e.Message);
            }
            catch (ObjectDisposedException e)
            {
                Logger.Default.WriteLine(e.Message);
            }
            catch (NullReferenceException e)
            {
                Logger.Default.WriteLine(e.Message);
            }
            finally
            {
                Logger.Default.WriteLineCaller();
            }
        }
        private bool TryReceive()
        {
            bool result;
            try
            {
                var from = new IPEndPoint(IPAddress.Any, 0);
                byte[] recv = null;
                recv = UdpClientReceiver.Receive(ref from);
                OnReceived(new PacketEventArgs(new IPEndPoint(from.Address, from.Port), recv));
                result = true;
            }
            catch (ObjectDisposedException)
            {
                result = false;
            }
            catch (NullReferenceException)
            {
                result = false;
            }
            catch (SocketException)
            {
                result = false;
            }
            return result;
        }
        protected abstract void OnReceived(PacketEventArgs e);
        #endregion

        #region IDISPOSE
        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Stop();
                }

                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
