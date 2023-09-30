using EPI.Comm.Log;
using EPI.Comm.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using static EPI.Comm.CommException;
namespace EPI.Comm.Net
{
    public abstract class TcpServerBase : IDisposable
    {
        #region Field & Property
        protected TcpListener Listener { get; set; }
        public IPEndPoint LocalEndPoint => Listener?.Server?.LocalEndPoint as IPEndPoint;
        public int Port { get; private set; }
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
            client.Disconnected += OnDisconnected;
            clients.Add(client);
        }
        private void DetachClient(TcpClientBase client)
        {
            client.Disconnected -= OnDisconnected;
            if (clients.Contains(client))
            {
                clients.Remove(client);
                OnClientDisconnected(client);
                client.Dispose();
            }
        }
        #endregion

        #region StartStop
        public void StartListen(int port)
        {
            lock (startStopLock)
            {
                if (!isListening)
                {
                    Listener = new TcpListener(IPAddress.Any, port);
                    Listener.Start();
                    Port = port;
                    isListening = true;
                    DelegateUtil.Start(AcceptLoop);
                }
            }


        }
        public void Stop()
        {
            lock (startStopLock)
            {
                if (isListening)
                {
                    isListening = false;
                    Listener.Stop();
                    Listener = null;
                    Port = 0;
                    WaitAcceptLoopFinish();
                    DisposeAllClients();
                }
            }

        }
        private void OnDisconnected(object sender, EventArgs e)
        {
            var client = sender as TcpClientBase;
            DetachClient(client);
        }
        private protected abstract void OnClientDisconnected(TcpClientBase client);
        private void DisposeAllClients()
        {
            var clients = this.clients.ToArray();
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
                        var tcpClient = Accept();
                        if (tcpClient != null)
                        {
                            var client = CreateClient(tcpClient);
                            AttachClient(client);
                            OnClientConnected(client);
                        }
                    }
                    catch (CommException e)
                    {
                        Logger.Default.WriteLine(e.Message);
                        if (!isListening)
                        {
                            break;
                        }
                    }
                    finally
                    {
                        Logger.Default.WriteLineCaller();
                    }
                }
                acceptLoopingOn = false;
            }

        }
        private TcpClient Accept()
        {
            try
            {
                var tcpClient = Listener.AcceptTcpClient();
                return tcpClient;
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
                Logger.Default.WriteLineCaller();
            }
        }
        private void WaitAcceptLoopFinish()
        {
            while (acceptLoopingOn)
            {

            }
        }
        private protected abstract void OnClientConnected(TcpClientBase client);
        private protected abstract TcpClientBase CreateClient(TcpClient client);

        #endregion

        #region IDISPOSE
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Listener?.Stop();
                    DisposeAllClients();
                }
                clients.Clear();
                Listener = null;
                disposedValue = true;
            }
        }
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
