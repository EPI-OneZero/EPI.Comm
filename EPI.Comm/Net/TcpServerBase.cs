using EPI.Comm.Log;
using EPI.Comm.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
namespace EPI.Comm.Net
{
    public abstract class TcpServerBase : IDisposable
    {
        #region Field & Property
        private readonly object startStopLock = new object();
        internal TcpListener Listener { get; private set; }
        public IPEndPoint LocalEndPoint => Listener?.Server?.LocalEndPoint as IPEndPoint;
        public int Port { get; private set; }
        public bool IsListening { get => isListening; }

        private volatile bool isListening;
        public int BufferSize { get; private set; }

        private List<TcpClientBase> clients = new List<TcpClientBase>();


        #endregion

        #region CTOR
        protected TcpServerBase(int bufferSize)
        {
            BufferSize = bufferSize;

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
                    ThreadUtil.Start(AcceptLoop);
                }
            }

        }

        #region Accept
        private void AcceptLoop()
        {
            try
            {
                while (isListening)
                {
                    var tcpClient = Listener.AcceptTcpClient();
                    if (tcpClient != null)
                    {
                        var client = CreateClient(tcpClient);
                        AttachClient(client);
                    }
                }
            }
            catch (SocketException e)
            {
                Logger.Default.WriteLine(e.Message);
            }
            finally
            {
                Logger.Default.WriteLineCaller();
                isListening = false;
            }

        }
        private protected virtual void AttachClient(TcpClientBase client)
        {
            clients.Add(client);
            client.Disconnected += OnDisconnected;
        }
        private protected abstract TcpClientBase CreateClient(TcpClient client);
        #endregion

        public void Stop()
        {
            lock (startStopLock)
            {
                if (isListening)
                {
                    isListening = false;
                    Listener.Stop();
                    while (isListening)
                    {
                    }
                    Listener = null;
                    Port = 0;

                    DisposeAllClients();
                }
            }

        }
        private void OnDisconnected(object sender, EventArgs e)
        {
            var client = sender as TcpClientBase;
            DetachClient(client);
        }

        private void DisposeAllClients()
        {
            var clients = this.clients.ToArray();
            foreach (var client in clients)
            {
                DetachClient(client);
            }
        }
        private protected virtual void DetachClient(TcpClientBase client)
        {
            client.Disconnected -= OnDisconnected;
            if (clients.Contains(client))
            {
                clients.Remove(client);
                client.Dispose();
            }
        }
        #endregion

      

        #region IDISPOSE
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Stop();
                }
                clients.Clear();
                Listener = null;
                disposedValue = true;
            }
        }
        private bool disposedValue;

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
