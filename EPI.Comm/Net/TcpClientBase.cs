﻿using EPI.Comm.Log;
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

        protected TcpClient TcpClient { get; private set; }
        protected TcpNetSocket NetSocket { get; private set; }
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
        private volatile bool isSocketAttached;
        private volatile bool isConnecting;
        private volatile string ipToConnect;
        private volatile int portToConnect;
        public bool IsConnected => isSocketAttached;
        private readonly AutoConnectHelper connectHelper;

        #endregion

        #region CTOR
        protected TcpClientBase(int bufferSize)
        {
            connectHelper = new AutoConnectHelper(this);
            BufferSize = bufferSize;
            AutoConnect = true;
        }
        protected TcpClientBase(TcpClient client, int bufferSize)
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
            isSocketAttached = true;
        }
        private void DetachSocket()
        {
            isSocketAttached = false;
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
        public void Send(byte[] bytes)
        {
            NetSocket?.Send(bytes);
        }
        protected abstract void SocketReceived(object sender, PacketEventArgs e);
        #endregion

        #region Connect
        public void Connect(string ip, int port)
        {
            SetRemoteEndPoint(ip, port);
            InternalConnect();

        }
        internal void InternalConnect()
        {
            if (!(isConnecting || isSocketAttached))
            {
                lock (ConnectLock)
                {
                    isConnecting = true;
                    TcpClient = new TcpClient();
                    if (Connect(TcpClient))
                    {
                        AttachSocket(TcpClient.Client);
                        OnConnected();
                        Connected?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        TcpClient?.Dispose();
                        TcpClient = null;
                        connectHelper?.RunAutoConnectIfUserWant();
                    }
                    isConnecting = false;
                }
            }
        }
        private bool Connect(TcpClient client)
        {
            try
            {
                var asyncHandle = client.BeginConnect(IPAddress.Parse(ipToConnect), portToConnect, null, null);
                var returned = asyncHandle.AsyncWaitHandle.WaitOne(5000);
                return client.Connected && returned;
            }
            catch (SocketException)
            {
                return false;
            }
            finally
            {
                Logger.Default.WriteLine($"{nameof(IsConnected)} : {IsConnected}");
            }
        }

        private void SetRemoteEndPoint(string ip, int port)
        {
            connectHelper.EnableUserRequestConnect();
            ipToConnect = ip;
            portToConnect = port;
        }

        public event EventHandler Connected;
        protected virtual void OnConnected()
        {

        }
        #endregion

        #region Stop
        public void Stop()
        {
            connectHelper?.StopAutoConnectIfLoopOn();
            lock (ConnectLock)
            {
                TcpClient?.Dispose();
                TcpClient = null;
                while (isSocketAttached)
                {
                }
            }
        }

        protected virtual void OnDisconnected()
        {

        }
        private void SocketClosed(object sender, EventArgs e)
        {
            DetachSocket();
            TcpClient?.Dispose();
            TcpClient = null;
            OnDisconnected();
            Disconnected?.Invoke(this, EventArgs.Empty);
            connectHelper?.RunAutoConnectIfUserWant();
        }
        public event EventHandler Disconnected;
        #endregion

        #region AutoConnect
        private sealed class AutoConnectHelper
        {
            public bool AutoConnect { get; set; }
            private volatile bool isAutoConnectLoopOn;
            private volatile bool userRequestConnect;
            public TcpClientBase Tcp { get; private set; }

            public AutoConnectHelper(TcpClientBase tcp)
            {
                Tcp = tcp;
            }
            public void EnableUserRequestConnect()
            {
                userRequestConnect = true;
            }
            public void RunAutoConnectIfUserWant()
            {
                if (AutoConnect && userRequestConnect && !isAutoConnectLoopOn)
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
            }
            private void AutoConnectLoop()
            {
                while (AutoConnect && userRequestConnect)
                {
                    Tcp.InternalConnect();
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
