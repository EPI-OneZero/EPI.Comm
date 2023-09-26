using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EPI.Comm.Net.Events;
using EPI.Comm.UTils;

namespace EPI.Comm.Net
{
    public partial class TcpNetClient : CommBase, IComm, IDisposable
    {
        private readonly object ConnectLock = new object();
        public int BufferSize { get; set; }
        protected TcpClient TcpClient { get; set; }
        internal NetSocket NetSocket { get; private set; }
        public IPEndPoint LocalEndPoint => NetSocket?.LocalEndPoint;
        public IPEndPoint RemoteEndPoint => NetSocket?.RemoteEndPoint;

        private volatile bool isConnectedFlag = false;
        private bool IsSocketConnected => (NetSocket?.IsConnected ?? false);
        public bool IsConnected
        {
            get
            {
                return isConnectedFlag && IsSocketConnected;
            }
            set 
            { 
                isConnectedFlag = value;
            }
        }
        public bool AutoConnect
        {
            get => connectHelper?.AutoConnect ?? false;
            set
            {
                if (connectHelper != null)
                    connectHelper.AutoConnect = value;
            }
        }

        private AutoConnectHelper connectHelper;
        public TcpNetClient(int bufferSize)
        {
            connectHelper = new AutoConnectHelper(this);
            BufferSize = bufferSize;
            AutoConnect = true;
        }
        public TcpNetClient() : this(DefaultBufferSize)
        {
        }
        /// <summary>
        /// server가 acept한 client에 대한 생성자
        /// </summary>
        /// <param name="client"></param>
        /// <param name="bufferSize"></param>
        internal TcpNetClient(TcpClient client, int bufferSize)
        {
            TcpClient = client;
            BufferSize = bufferSize;
            isConnectedFlag = true;
            AttachSocket(TcpClient);
        }
        private void AttachSocket(TcpClient client)
        {
            NetSocket = new NetSocket(client.Client, BufferSize);
            NetSocket.Received += SocketReceived;
            NetSocket.Closed += SocketClosed;
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

        public void Connect(string ip, int port)
        {
            lock (ConnectLock)
            {
                try
                {
                    if (!IsConnected)
                    {
                        connectHelper.SetEndPoint(ip, port, true);
                        TcpClient = new TcpClient();
                        var asyncHandle = TcpClient.BeginConnect(IPAddress.Parse(ip), port, null, null);
                        var returned = asyncHandle.AsyncWaitHandle.WaitOne(10000);
                        if(returned|| IsSocketConnected)
                        {
                            IsConnected = true;
                            AttachSocket(TcpClient);
                            Connected?.Invoke(this, EventArgs.Empty);
                        }
                        else
                        {
                            TcpClient?.Dispose();
                            RunAutoConnectIfUserWant();
                        }


                    }
                }
                catch (SocketException e)
                {
                    TcpClient?.Dispose();
                    RunAutoConnectIfUserWant();
                    Debug.WriteLine(e.Message);
                }
                finally
                {
                    Debug.WriteLine(nameof(Connect));
                }
            }


        }
        
        private void SocketClosed(object sender, EventArgs e)
        {
            DetachSocket();
            IsConnected = false;
            TcpClient?.Close();
            TcpClient = null;
            RaiseCloseEvent();

            RunAutoConnectIfUserWant();
        }
        private void StopAutoConnectIfLoopOn()
        {
           
            connectHelper?.StopAutoConnectIfLoopOn();
           
        }
        private void RunAutoConnectIfUserWant()
        {
            connectHelper?.RunAutoConnectIfUserWant();
        }
    

        private protected virtual void SocketReceived(object sender, PacketEventArgs e)
        {
            Received?.Invoke(this, e);
        }
        /// <summary>
        /// 연결 해제
        /// </summary>
        public void Stop()
        {
            StopAutoConnectIfLoopOn();
           
            lock (ConnectLock)
            {
                if (IsConnected)
                {
                    DetachSocket();
                    IsConnected = false;
                    TcpClient?.Close();
                    TcpClient = null;
                    RaiseCloseEvent();

                }
            }


        }
        /// <summary>
        ///연결을 끊었을 때
        /// </summary>
        private void RaiseCloseEvent()
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
        }
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
        public event EventHandler Disconnected;
        public event EventHandler Connected;
        public event PacketEventHandler Received;
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

        private class AutoConnectHelper
        {
            public bool AutoConnect { get; set; }
            private volatile bool isAutoConnectLoopOn = false;
            private volatile bool userRequestConnect = false;
            private volatile string userConnectIp = null;
            private volatile int userConnectPort = -1;
            public TcpNetClient Tcp { get; private set; }
   
            public AutoConnectHelper(TcpNetClient tcp)
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
                        isAutoConnectLoopOn = false;
                    }
                    
                });

            }
            public void StopAutoConnectIfLoopOn()
            {
                userRequestConnect = false;
                while (isAutoConnectLoopOn)
                {
                }
            }
        }
    }

}
