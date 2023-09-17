using EPI.Comm.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static EPI.Comm.CommException;

namespace EPI.Comm.Net
{
    public class TcpNetServer : CommBase, IComm, IDisposable
    {
        protected TcpListener Listener { get; set; }
        protected volatile bool isListening = false;
        protected int Port { get; set; }
        protected int BufferSize { get; set; }
        protected List<TcpNetClient> Clients { get; set; } = new List<TcpNetClient>();
        protected object startStopLock = new object();

        public TcpNetServer(int port, int bufferSize)
        {
            Port = port;
            BufferSize = bufferSize;
        }
        public TcpNetServer(int port) : this(port, DefaultBufferSize)
        {
            
        }
        /// <summary>
        ///  서버가 클라이언트를 Accept 시작
        /// </summary>
        public void Start()
        {
            lock (startStopLock)
            {
                if (!isListening)
                {
                    Listener = new TcpListener(System.Net.IPAddress.Any, Port);
                    Listener.Start();
                    ThreadUtil.Start(AcceptLoop);
                    isListening = true;
                }
            }
        }
        /// <summary>
        /// 서버가 클라이언트를 Accept 중지
        /// </summary>
        public void Stop()
        {
            lock (startStopLock)
            {
                if (isListening)
                {
                    Listener.Stop();
                    Listener = null;
                    DisposeAllClients();
                    isListening = false;
                }
            }
        }
        public event CommReceiveEventHandler Received;
        public event EventHandler Closed;
        public event EventHandler Accpeted;
        public void Send(string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            Send(bytes);
        }
        public void Send(byte[] bytes)
        {
            foreach (var client in Clients)
            {
                client?.Send(bytes);
            }
        }
        private void AcceptLoop()
        {
            while (true)
            {
                try
                {
                    Accept();
                }
                catch (CommException e)
                {

                    Debug.WriteLine(e.Message);
                    if (!isListening)
                    {
                        break;
                    }
                }
                finally 
                {
                    Debug.WriteLine(nameof(AcceptLoop));
                }
            }
        }
        private void Accept()
        {
            try
            {
                var tcpClient = Listener.AcceptTcpClient();
                var client = new TcpNetClient(tcpClient, BufferSize);
                SetClient(client);

                Accpeted?.Invoke(client, EventArgs.Empty);

            }
            catch (SocketException socketException)
            {
                throw CreateCommException(socketException);
            }
            catch (ObjectDisposedException disposedException)
            {
                throw CreateCommException(disposedException);
            }
            catch (NullReferenceException nullException)
            {
                throw CreateCommException(nullException);
            }
            finally
            {
                Debug.WriteLine(nameof(Accept));
            }
        }
        private void SetClient(TcpNetClient client)
        {
            client.Received += ClientReceived;
            client.Closed += ClientClosed;
            Clients.Add(client);
        }
        /// <summary>
        /// 클라이언트 연결해제
        /// </summary>
        /// <param name="client"></param>
        private void ClearClient(TcpNetClient client)
        {
            client.Received -= ClientReceived;
            client.Closed -= ClientClosed;
            if (Clients.Contains(client))
            {
                Clients.Remove(client);
                Closed?.Invoke(client, EventArgs.Empty);
                client.Dispose();
            }
        }

        private void ClientClosed(object sender, EventArgs e)
        {
            var client = sender as TcpNetClient;
            ClearClient(client);
         
        }
        private void DisposeAllClients()
        {
            var clients = Clients.ToArray();
            foreach (var client in clients)
            {
                ClearClient(client);
            }
        }
        private void ClientReceived(object sender, CommReceiveEventArgs e)
        {
            Received?.Invoke(sender, e);
        }

        #region IDISPOSE
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 관리형 상태(관리형 개체)를 삭제합니다.
                    Listener.Stop();
                    DisposeAllClients();
                }
                Listener = null;
                // TODO: 비관리형 리소스(비관리형 개체)를 해제하고 종료자를 재정의합니다.
                // TODO: 큰 필드를 null로 설정합니다.
                disposedValue = true;
            }
        }

        // // TODO: 비관리형 리소스를 해제하는 코드가 'Dispose(bool disposing)'에 포함된 경우에만 종료자를 재정의합니다.
        // ~Server()
        // {
        //     // 이 코드를 변경하지 마세요. 'Dispose(bool disposing)' 메서드에 정리 코드를 입력합니다.
        //     Dispose(disposing: false);
        // }


        private bool disposedValue;

        public void Dispose()
        {
            // 이 코드를 변경하지 마세요. 'Dispose(bool disposing)' 메서드에 정리 코드를 입력합니다.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
