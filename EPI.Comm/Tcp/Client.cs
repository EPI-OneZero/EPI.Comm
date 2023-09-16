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

namespace EPI.Comm.Tcp
{
    public class Client : CommBase, IComm, IDisposable
    {

        public string Ip { get; protected set; }
        public int Port { get; protected set; }
        public int BufferSize { get;protected set; }
        protected TcpClient TcpClient { get; set; }
        protected SocketHolder SocketHolder { get; set; }
        public IPEndPoint LocalEndPoint => SocketHolder?.LocalEndPoint;
        public IPEndPoint RemoteEndPoint => SocketHolder?.RemoteEndPoint;
        public bool IsConnected => SocketHolder?.IsConnected ?? false;
        public Client(string ip, int port, int bufferSize )
        {
            Ip = ip;
            Port = port;
            BufferSize = bufferSize;
        }
        public Client(string ip, int port) : this(ip, port, DefaultBufferSize)
        {

        }
        /// <summary>
        /// server가 acept한 client에 대한 생성자
        /// </summary>
        /// <param name="client"></param>
        /// <param name="bufferSize"></param>
        internal Client(TcpClient client, int bufferSize)
        {
            TcpClient = client;
            var endpoint = (IPEndPoint)client.Client.RemoteEndPoint;
            Ip = endpoint.Address.ToString();
            Port = endpoint.Port;
            BufferSize = bufferSize;
            SetSocketHolder(TcpClient);
        }
        /// <summary>
        /// 소켓 이벤트 연결
        /// </summary>
        /// <param name="client"></param>
        private void SetSocketHolder(TcpClient client)
        {
            SocketHolder = new SocketHolder(client.Client, BufferSize);
            SocketHolder.Received += SocketReceived;
            SocketHolder.Closed += SocketClosed;
        }
        /// <summary>
        /// 소켓 이벤트 연결해제
        /// </summary>
        private void ClearSocketHolder()
        {
            if (SocketHolder != null)
            {
                SocketHolder.Received -= SocketReceived;
                SocketHolder.Closed -= SocketClosed;
                SocketHolder = null;
            }
        }

        public void Connect()
        {
            try
            {
             
                if(!IsConnected)
                {
                    TcpClient = new TcpClient();
                    TcpClient.Connect(Ip, Port);
                    SetSocketHolder(TcpClient);
                    Connected?.Invoke(this, EventArgs.Empty);
                }
                else
                {
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

        private void SocketClosed(object sender, EventArgs e)
        {
            ClearSocketHolder();
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
            if(IsConnected)
            {
                ClearSocketHolder();
                TcpClient?.Close();
                TcpClient = null;
                RaiseCloseEvent();
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
            SocketHolder?.Send(bytes);
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
                    ClearSocketHolder();
                    TcpClient?.Dispose();
                    RaiseCloseEvent();
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
