using EPI.Comm.Net.Events;
using EPI.Comm.UTils;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace EPI.Comm.Net
{
    public abstract class TcpClientBase : CommBase, IDisposable
    {
        #region Field & Property
        private readonly object ConnectLock = new object();
        private readonly KeepAlive KeepAliveConfig = new KeepAlive();
        protected TcpClient TcpClient { get; set; }
        internal NetSocket NetSocket { get; private set; }
        public int BufferSize { get; private set; }
        public IPEndPoint LocalEndPoint => NetSocket?.LocalEndPoint;
        public IPEndPoint RemoteEndPoint => NetSocket?.RemoteEndPoint;
        public bool AutoConnect
        {
            get => connectHelper?.AutoConnect ?? false;
            set
            {
                if (connectHelper != null)
                    connectHelper.AutoConnect = value;
            }
        }
        private volatile bool isConnecting = false;
        private string ipToConnect;
        private int portToConnect;
        public bool IsConnected => (NetSocket?.IsConnected ?? false);
        private AutoConnectHelper connectHelper;
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
            SetKeepAlive(client);
            NetSocket = new NetSocket(client, BufferSize);
            NetSocket.Received += SocketReceived;
            NetSocket.Closed += SocketClosed;
        }
        private void SetKeepAlive(Socket socket)
        {
            var size = Marshal.SizeOf<KeepAlive>();
            var result = new byte[size];

            socket.IOControl(IOControlCode.KeepAliveValues, KeepAliveConfig.Generate(), result);
        }
        private void DetachSocket()
        {
            if (NetSocket != null)
            {
                NetSocket.Received -= SocketReceived;
                NetSocket.Closed -= SocketClosed;
                NetSocket = null;
            }
        }
        #endregion

        #region Send Receive
        public void Send(byte[] bytes)
        {
            try
            {
                NetSocket?.Send(bytes);
            }
            catch (CommException e)
            {
                Debug.WriteLine(e.Message);
            }
        }
        private protected abstract void SocketReceived(object sender, SocketReceiveEventArgs e);

        #endregion

        #region Connect
        public void Connect(string ip, int port)
        {
            SetRemoteEndPoint(ip, port);
            if (!isConnecting)
            {
                lock (ConnectLock)
                {
                    isConnecting = true;
                    try
                    {
                        if (!IsConnected)
                        {
                            TcpClient = new TcpClient();
                            Connect(TcpClient);
                        }
                    }
                    catch (CommException e)
                    {
                        TcpClient?.Dispose();
                        RunAutoConnectIfUserWant();
                        Debug.WriteLine(e.Message);
                    }
                    finally
                    {
                        isConnecting = false;
                        Debug.WriteLine(nameof(Connect));
                    }
                }
            }

        }
        private void Connect(TcpClient client)
        {
            try
            {
                var asyncHandle = client.BeginConnect(IPAddress.Parse(ipToConnect), portToConnect, null, null);
                var returned = asyncHandle.AsyncWaitHandle.WaitOne(10000);
                if (IsConnected || returned)
                {
                    AttachSocket(client.Client);
                    Connected?.Invoke(this, EventArgs.Empty);
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
                Debug.WriteLine($"{nameof(IsConnected)} : {IsConnected}");
            }
        }

        private void SetRemoteEndPoint(string ip, int port)
        {
            connectHelper.SetEndPoint(ip, port, true);
            ipToConnect = ip;
            portToConnect = port;
        }

        public event EventHandler Connected;

        #endregion

        #region Stop
        public void Stop()
        {
            StopAutoConnectIfLoopOn();
            lock (ConnectLock)
            {
                if (IsConnected)
                {
                    DetachSocket();
                    TcpClient?.Dispose();
                    TcpClient = null;
                    RaiseDisconnectEvent();

                }
            }
        }

        private void RaiseDisconnectEvent()
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
        }
        private void SocketClosed(object sender, EventArgs e)
        {
            DetachSocket();
            TcpClient?.Dispose();
            TcpClient = null;
            RaiseDisconnectEvent();

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

        private class AutoConnectHelper
        {
            public bool AutoConnect { get; set; }
            private volatile bool isAutoConnectLoopOn = false;
            private volatile bool userRequestConnect = false;
            private volatile string userConnectIp = null;
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
                    if (Tcp != null)
                    {
                        Tcp?.Connect(userConnectIp, userConnectPort);
                        if (Tcp?.IsConnected ?? true)
                        {
                            break;
                        }
                    }
                    else
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
                    // TODO: 관리형 상태(관리형 개체)를 삭제합니다.
                }
                TcpClient = null;
                // TODO: 비관리형 리소스(비관리형 개체)를 해제하고 종료자를 재정의합니다.
                // TODO: 큰 필드를 null로 설정합니다.
                disposedValue = true;
            }
        }

        // // TODO: 비관리형 리소스를 해제하는 코드가 'Dispose(bool disposing)'에 포함된 경우에만 종료자를 재정의합니다.
        // ~Client()
        // {
        //     // 이 코드를 변경하지 마세요. 'Dispose(bool disposing)' 메서드에 정리 코드를 입력합니다.
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 이 코드를 변경하지 마세요. 'Dispose(bool disposing)' 메서드에 정리 코드를 입력합니다.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
