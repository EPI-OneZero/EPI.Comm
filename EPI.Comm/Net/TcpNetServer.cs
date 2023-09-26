using EPI.Comm.Net.Events;
using EPI.Comm.UTils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static EPI.Comm.CommException;

namespace EPI.Comm.Net
{
    /// <summary>
    /// 질문: TcpNetServer has a TcpNetClient. 
    ///  TcpNetClient has a NetSocket인데 이 구조 적절한가?
    /// </summary>
    public class TcpNetServer : CommBase, IComm, IDisposable
    {
        protected TcpListener Listener { get; set; }
        public IPEndPoint LocalEndPoint => Listener?.Server?.LocalEndPoint as IPEndPoint;
        public int Port  => ((IPEndPoint)Listener?.LocalEndpoint)?.Port ?? -1;
        public bool IsListening { get => isListening; }

        protected volatile bool isListening = false;
        protected int BufferSize { get; set; }
        private List<TcpNetClient> clients = new List<TcpNetClient>();
        public ClientCollection Clients { get; private set; }

        protected object startStopLock = new object();

        public TcpNetServer(int bufferSize)
        {
            BufferSize = bufferSize;
            Clients = new ClientCollection(clients);
        }
        public TcpNetServer() : this( DefaultBufferSize)
        {
            
        }
        /// <summary>
        ///  서버가 클라이언트를 Accept 시작
        /// </summary>
        public void StartListen(int port)
        {
            lock (startStopLock)
            {
                if (!isListening)
                {
                    Listener = new TcpListener(IPAddress.Any, port);
                    Listener.Start();
                    isListening = true;
                    ThreadUtil.Start(AcceptLoop);
                }
            }
        }
        /// <summary>
        /// 서버가 클라이언트를 Accept 중지, 및 모든 연결 해제
        /// </summary>
        public void Stop()
        {
            lock (startStopLock)
            {
                if (isListening)
                {
                    isListening = false;
                    Listener.Stop();
                    Listener = null;
                    DisposeAllClients();
               
                    while (acceptLoopingOn)
                    {
                    }
                }
            }
        }
        
        public void Send(string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            Send(bytes);
        }
        public void Send(byte[] bytes)
        {

            var result = Parallel.ForEach(clients, c=>
            {
                c.Send(bytes);
            });
        }
        private volatile bool acceptLoopingOn = false;
        private void AcceptLoop()
        {
            acceptLoopingOn = true;
            while (isListening)
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
            acceptLoopingOn = false;
        }
        private void Accept()
        {
            try
            {
                var tcpClient = Listener?.AcceptTcpClient();
                if(tcpClient!=null)
                {
                    var client = CreateClient(tcpClient);
                    AttachClient(client);
                    OnAccepted(this, new TcpEventArgs(client));
                }
               

            }
            catch (ObjectDisposedException disposedException)
            {
                throw CreateCommException(disposedException);
            }
            catch (SocketException socketException)
            {
                throw CreateCommException(socketException);
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
        private protected virtual void OnAccepted(object sender, TcpEventArgs e)
        {
            ClientAccpeted?.Invoke(this, e);
        }
        private protected virtual TcpNetClient CreateClient(TcpClient client)
        {
            return new TcpNetClient(client, BufferSize);
        }
        private void AttachClient(TcpNetClient client)
        {
            client.Received += OnClientReceived;
            client.Disconnected += OnClientClosed;
            clients.Add(client);
        }
        private void DetachClient(TcpNetClient client)
        {
            client.Received -= OnClientReceived;
            client.Disconnected -= OnClientClosed;
            if (clients.Contains(client))
            {
                clients.Remove(client);
                OnClosed(this, new TcpEventArgs(client));
                client.Dispose();
            }
        }

        private void OnClientClosed(object sender, EventArgs e)
        {
            var client = sender as TcpNetClient;
            DetachClient(client);
        }
        private protected virtual void OnClosed(object sender, TcpEventArgs e)
        {
            ClientClosed?.Invoke(this, e);
        }

        private void DisposeAllClients()
        {
            var clients = this.clients?.ToArray() ?? new TcpNetClient[0];
            foreach (var client in clients)
            {
                DetachClient(client);
            }
        }
        private protected virtual void OnClientReceived(object sender, PacketEventArgs e)
        {
            Received?.Invoke(this, e);
        }
        public event PacketEventHandler Received;
        public event TcpEventHandler ClientClosed;
        public event TcpEventHandler ClientAccpeted;
        #region IDISPOSE
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 관리형 상태(관리형 개체)를 삭제합니다.
                    Listener?.Stop();
                    DisposeAllClients();
                }
                clients.Clear();
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
    public sealed class ClientCollection : ReadOnlyCollection<TcpNetClient>
    {

        internal ClientCollection(IList<TcpNetClient> list) : base(list)
        {
        }
    }
}
