using EPI.Comm.Log;
using EPI.Comm.Net.Events;
using EPI.Comm.Utils;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
namespace EPI.Comm.Net
{
    public abstract class TcpClientBase : IDisposable
    {
        #region Field & Property
        private readonly object ConnectLock = new object();

        internal TcpClient TcpClient { get; private set; }
        internal TcpNetSocket NetSocket { get; private set; }
        public int BufferSize { get; private set; }
        public IPEndPoint LocalEndPoint { get; private set; }
        public IPEndPoint RemoteEndPoint { get; private set; }
        public bool AutoConnect
        {
            get => connectHelper?.AutoConnect ?? false;
            set
            {
                if (connectHelper != null)
                    connectHelper.AutoConnect = value;
            }
        }
        private volatile bool isConnecting;
        private string ipToConnect;
        private int portToConnect;
        public bool IsConnected => TcpClient?.Connected ?? false;
        private readonly AutoConnectHelper connectHelper;
        #endregion

        #region CTOR
        protected TcpClientBase(int bufferSize)
        {
            connectHelper = new AutoConnectHelper(this);
            BufferSize = bufferSize;
            AutoConnect = true;
        }
        protected TcpClientBase(TcpClient client, int bufferSize) : base()
        {
            TcpClient = client;
            BufferSize = bufferSize;
            AttachSocket(TcpClient.Client);
        }
        #endregion

        #region Socket Attach Detach
        private void AttachSocket(Socket client)
        {
            NetSocket = new TcpNetSocket(client, BufferSize);

            NetSocket.Received += SocketReceived;
            NetSocket.Closed += SocketClosed;
            LocalEndPoint = NetSocket.LocalEndPoint;
            RemoteEndPoint = NetSocket.RemoteEndPoint;
        }


        private void DetachSocket()
        {
            if (NetSocket != null)
            {
                NetSocket.Received -= SocketReceived;
                NetSocket.Closed -= SocketClosed;
                NetSocket = null;
                LocalEndPoint = null;
                RemoteEndPoint = null;
            }
        }
        #endregion

        #region Send Receive
        internal void SendBytes(byte[] bytes)
        {
            try
            {
                NetSocket?.Send(bytes);
            }
            catch (CommException e)
            {
                Logger.Default.WriteLine(e.Message);
            }
        }
        private protected abstract void SocketReceived(object sender, PacketEventArgs e);
        #endregion

        #region Connect
        public void Connect(string ip, int port)
        {
            SetRemoteEndPoint(ip, port);
            if (!(isConnecting || IsConnected))
            {
                lock (ConnectLock)
                {
                    isConnecting = true;
                    try
                    {
                        TcpClient = new TcpClient();
                        Connect(TcpClient);
                    }
                    catch (CommException e)
                    {
                        TcpClient?.Dispose();
                        TcpClient = null;
                        RunAutoConnectIfUserWant();
                        Logger.Default.WriteLine(e.Message);
                    }
                    finally
                    {
                        isConnecting = false;
                        Logger.Default.WriteLineCaller();
                    }
                }
            }

        }
        private void Connect(TcpClient client)
        {
            try
            {
                var asyncHandle = client.BeginConnect(IPAddress.Parse(ipToConnect), portToConnect, null, null);
                var returned = asyncHandle.AsyncWaitHandle.WaitOne(5000);
                if (IsConnected && returned)
                {
                    AttachSocket(client.Client);
                    OnSocketConnected();
                }
                else
                {
                    throw CommException.CreateCommException("Connection Time Out");
                }
            }
            catch (SocketException e)
            {

                throw CommException.CreateCommException(e);
            }
            finally
            {
                Logger.Default.WriteLine($"{nameof(IsConnected)} : {IsConnected}");
            }
        }

        private void SetRemoteEndPoint(string ip, int port)
        {
            connectHelper.SetEndPoint(ip, port, true);
            ipToConnect = ip;
            portToConnect = port;
        }
        private protected virtual void OnSocketConnected()
        {
            Connected?.Invoke(this, EventArgs.Empty);
        }
        public event EventHandler Connected;

        #endregion

        #region Stop
        public void Stop()
        {
            StopAutoConnectIfLoopOn();
            lock (ConnectLock)
            {
                bool raiseEvent = IsConnected;
                DetachSocket();
                TcpClient?.Dispose();
                TcpClient = null;
                if (raiseEvent)
                {
                    OnSocketDisconnected();
                }

            }
        }

        private protected virtual void OnSocketDisconnected()
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
        }
        private void SocketClosed(object sender, EventArgs e)
        {
            DetachSocket();
            TcpClient?.Dispose();
            TcpClient = null;
            OnSocketDisconnected();

            RunAutoConnectIfUserWant();
        }
        public event EventHandler Disconnected;
        #endregion

        #region AutoConnect
        private void StopAutoConnectIfLoopOn()
        {

            connectHelper?.StopAutoConnectIfLoopOn();

        }
        private void RunAutoConnectIfUserWant()
        {
            connectHelper?.RunAutoConnectIfUserWant();
        }

        private sealed class AutoConnectHelper
        {
            public bool AutoConnect { get; set; }
            private volatile bool isAutoConnectLoopOn;
            private volatile bool userRequestConnect;
            private volatile string userConnectIp;
            private volatile int userConnectPort = -1;
            public TcpClientBase Tcp { get; private set; }

            public AutoConnectHelper(TcpClientBase tcp)
            {
                Tcp = tcp;
            }
            public void SetEndPoint(string ip, int port, bool requestConnect)
            {
                userConnectIp = ip;
                userConnectPort = port;
                userRequestConnect = requestConnect;
            }
            public void RunAutoConnectIfUserWant()
            {
                if (AutoConnect && userRequestConnect && !isAutoConnectLoopOn)
                    RunAutoConnect();
            }
            private void RunAutoConnect()
            {
                ThreadUtil.Start(() =>
                {
                    lock (this)
                    {
                        isAutoConnectLoopOn = true;
                        AutoConnectLoop();
                        isAutoConnectLoopOn = false;
                    }

                });

            }
            private void AutoConnectLoop()
            {
                while (AutoConnect && userRequestConnect)
                {
                    Tcp.Connect(userConnectIp, userConnectPort);
                    if (Tcp.IsConnected)
                    {
                        break;
                    }
                    Thread.Sleep(3000);
                }
            }
            public void StopAutoConnectIfLoopOn()
            {
                userRequestConnect = false;
                while (isAutoConnectLoopOn)
                {
                }
            }
        }
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
                TcpClient = null;
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
