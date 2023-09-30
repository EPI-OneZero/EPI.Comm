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
        public IPEndPoint LocalEndPoint => UdpClient?.Client?.LocalEndPoint as IPEndPoint;
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
                    RemoteEndPoint = new IPEndPoint(IPAddress.Parse(sendIp), sendPort);
                    SetSocketOption();
                    ThreadUtil.Start(ReceiveLoop);
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
        private void Receive()
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
            }
            catch (ObjectDisposedException e)
            {
                throw CommException.CreateCommException(e);
            }
            catch (NullReferenceException e)
            {
                throw CommException.CreateCommException(e);
            }
            catch (SocketException e)
            {
                throw CommException.CreateCommException(e);
            }
        }
        private void ReceiveLoop()
        {
            while (isStarted)
            {
                try
                {
                    Receive();
                }
                catch (CommException e)
                {
                    Logger.Default.WriteLine(e.Message);
                    break;
                }
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
