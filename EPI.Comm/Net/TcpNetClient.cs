using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using EPI.Comm.Net.Events;

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
        public bool IsConnected => NetSocket?.IsConnected ?? false;
        public bool AutoReconnect { get; set; }
        public TcpNetClient(int bufferSize)
        {
           
            BufferSize = bufferSize;
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
                        TcpClient = new TcpClient();
                        TcpClient.Connect(ip, port);
                        AttachSocket(TcpClient);
                        Connected?.Invoke(this, EventArgs.Empty);
                    }
                }
                catch (SocketException e)
                {
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
            TcpClient?.Close();
            TcpClient = null;
            RaiseCloseEvent();
        }

        private void SocketReceived(object sender, CommReceiveEventArgs e)
        {
            Received?.Invoke(this, e);
        }
        /// <summary>
        /// 연결 해제
        /// </summary>
        public void Stop()
        {
            lock (ConnectLock)
            {
                if (IsConnected)
                {
                    DetachSocket();
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
            Closed?.Invoke(this, EventArgs.Empty);
        }
        public void Send(string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            Send(bytes);
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
        public event EventHandler Closed;
        public event EventHandler Connected;
        public event CommReceiveEventHandler Received;
        #region IDISPOSE
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Stop();
                    //DetachSocket();
                    //TcpClient?.Dispose();
                    //RaiseCloseEvent();
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
