using EPI.Comm.UTils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static EPI.Comm.CommException;

namespace EPI.Comm.Net
{
    public abstract class TcpServerBase : CommBase, IDisposable
    {
        #region Field & Property
        protected TcpListener Listener { get; set; }
        public IPEndPoint LocalEndPoint => Listener?.Server?.LocalEndPoint as IPEndPoint;
        public int Port => ((IPEndPoint)Listener?.LocalEndpoint)?.Port ?? -1;
        public bool IsListening { get => isListening; }

        protected volatile bool isListening = false;
        protected int BufferSize { get; private set; }

        private List<TcpClientBase> clients = new List<TcpClientBase>();


        protected object startStopLock = new object();
        private volatile bool acceptLoopingOn = false;
        #endregion

        #region CTOR
        protected TcpServerBase(int bufferSize)
        {
            BufferSize = bufferSize;

        }
        #endregion

        #region Client Attach Detach 
        private void AttachClient(TcpClientBase client)
        {
            client.Disconnected += OnClientDisconnected;
            clients.Add(client);
        }
        private void DetachClient(TcpClientBase client)
        {
            client.Disconnected -= OnClientDisconnected;
            if (clients.Contains(client))
            {
                clients.Remove(client);
                OnDisconnected(client);
                client.Dispose();
            }
        }
        #endregion

        #region Send
        public void Send(string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            Send(bytes);
        }
        public void Send(byte[] bytes)
        {
            var result = Parallel.ForEach(clients, c =>
            {
                c.Send(bytes);
            });
        }


        #endregion

        #region StartStop
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
            if (isListening)
            {
                lock (startStopLock)
                {

                    isListening = false;
                    Listener.Stop();
                    Listener = null;
                    WaitAcceptLoopFinish();
                    DisposeAllClients();
                }
            }
        }
        private protected virtual void OnClientDisconnected(object sender, EventArgs e)
        {
            var client = sender as TcpClientBase;
            DetachClient(client);
        }
        private protected abstract void OnDisconnected(TcpClientBase client);
        private void DisposeAllClients()
        {
            var clients = this.clients?.ToArray() ?? new TcpClientBase[0];
            foreach (var client in clients)
            {
                DetachClient(client);
            }
        }

        #endregion

        #region Accept
        private void AcceptLoop()
        {
            if (!acceptLoopingOn)
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

        }
        private void Accept()
        {
            try
            {
                var tcpClient = Listener?.AcceptTcpClient();
                if (tcpClient != null)
                {
                    var client = CreateClient(tcpClient);
                    AttachClient(client);
                    OnAccepted(client);
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
        private void WaitAcceptLoopFinish()
        {
            while (acceptLoopingOn)
            {

            }
        }
        private protected abstract void OnAccepted(TcpClientBase client);


        private protected abstract TcpClientBase CreateClient(TcpClient client);

        #endregion

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
}
