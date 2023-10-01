using EPI.Comm.Log;
using EPI.Comm.Net.Events;
using EPI.Comm.Utils;
using System;
using System.Net;
using System.Net.Sockets;
using static EPI.Comm.CommConfig;
namespace EPI.Comm.Net
{
    public abstract class UdpBase : IDisposable
    {
        private readonly object StartStopLock = new object();
        private readonly object SendLock = new object();
        private readonly object RecvLock = new object();
        public UdpClient UdpClient { get; private set; }
        public IPEndPoint LocalEndPoint { get; private set; }
        public IPEndPoint RemoteEndPoint { get; private set; }
        public int BufferSize { get; private set; }
        private volatile bool isStarted;

        public bool IsStarted => isStarted;
        protected UdpBase(int bufferSize)
        {
            BufferSize = bufferSize;
        }
        protected UdpBase() : this(DefaultBufferSize)
        {
        }
        public void Start(int recvPort, string sendIp, int sendPort)
        {
            lock (StartStopLock)
            {
                if (!isStarted)
                {
                    UdpClient = new UdpClient(new IPEndPoint(IPAddress.Any, recvPort));
                    LocalEndPoint = UdpClient.Client.LocalEndPoint as IPEndPoint;
                    RemoteEndPoint = new IPEndPoint(IPAddress.Parse(sendIp), sendPort);
                    SetSocketOption();
                    ThreadUtil.Start(() =>
                    {
                        while (Receive());
                    });
                    isStarted = true;
                }
            }
        }
        private void SetSocketOption()
        {
            UdpClient.Client.ReceiveBufferSize = BufferSize;
            UdpClient.Client.SendBufferSize = BufferSize;
        }
        public void Stop()
        {
            lock (StartStopLock)
            {
                UdpClient?.Dispose();
                UdpClient = null;
                LocalEndPoint = null;
                RemoteEndPoint = null;
                isStarted = false;
            }

        }
        public void JoinMulticastGroup(string ip)
        {
            UdpClient?.JoinMulticastGroup(IPAddress.Parse(ip));
        }
        public void DropMulticastGroup(string ip)
        {
            UdpClient?.DropMulticastGroup(IPAddress.Parse(ip));
        }

        internal void SendBytes(byte[] bytes)
        {
            try
            {
                lock (SendLock)
                {
                    UdpClient?.Send(bytes, bytes.Length, RemoteEndPoint);
                }
            }

            catch (SocketException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            catch (NullReferenceException)
            {
            }
            finally
            {
                Logger.Default.WriteLineCaller();
            }
        }
        private bool Receive()
        {
            try
            {
                var from = new IPEndPoint(IPAddress.Any, 0);
                byte[] recv = null;
                lock (RecvLock)
                {
                    recv = UdpClient.Receive(ref from);
                }
                OnReceived(new PacketEventArgs(new IPEndPoint(from.Address, from.Port), recv));
                return true;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
            catch (NullReferenceException)
            {
                return false;
            }
            catch (SocketException)
            {
                return false;
            }
        }
     
        private protected abstract void OnReceived(PacketEventArgs e);
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
