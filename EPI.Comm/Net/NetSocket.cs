using EPI.Comm.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static EPI.Comm.CommException;

namespace EPI.Comm.Net
{
    internal class NetSocket : CommBase, IComm
    {
        protected Socket Socket { get; set; }
        protected byte[] ArrayBuffer { get; set; }
        protected AsyncCallback SendCallback { get; private set; }
        public bool IsConnected => Socket?.Connected ?? false;
        public IPEndPoint LocalEndPoint => Socket?.LocalEndPoint as IPEndPoint;
        public IPEndPoint RemoteEndPoint => Socket?.RemoteEndPoint as IPEndPoint;
        private static readonly LingerOption lingerOption = new LingerOption(true, 0);
        internal NetSocket(Socket socket, int bufferSize)
        {
            Socket = socket;
            ArrayBuffer = new byte[bufferSize];
            SetSocketOption(socket);
            InitCallback();
            ThreadUtil.Start(OnReceive);
        }
        public void Close()
        {
            Socket?.Close();
        }
        private void SetSocketOption(Socket socket)
        {
            socket.ReceiveBufferSize = ArrayBuffer.Length;
            socket.SendBufferSize = ArrayBuffer.Length;
            socket.NoDelay = true;
            socket.LingerState = lingerOption;
        }
        private void InitCallback()
        {
            SendCallback = new AsyncCallback(OnSend);
        }
        public void Send(byte[] bytes)
        {
            Socket.Send(bytes);
        }
        internal IAsyncResult SendAsync(byte[] bytes)
        {
            var result = Socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, SendCallback, Socket);
            return result;
        }
        private byte[] Receive()
        {
            try
            {
                lock (this)
                {
                    int count = Socket.Receive(ArrayBuffer);
                    byte[] result = new byte[count];
                    Buffer.BlockCopy(ArrayBuffer, 0, result, 0, count);
                    if (count <= 0)
                    {
                        throw CreateCommException();
                    }
                    return result;
                }

            }
            catch (SocketException e)
            {
                throw CreateCommException(e);
            }
            catch (ObjectDisposedException e)
            {
               throw CreateCommException(e);
            }
            finally
            {
                Debug.WriteLine(nameof(Receive));
            }

        }
        private void OnReceive()
        {
            while (true)
            {
                try
                {
                    var recv = Receive();
                    Received?.Invoke(this, new CommReceiveEventArgs(recv));
                }
                catch(CommException e) // 연결을 끊었을 때
                {
                    Debug.WriteLine(e.Message);
                    RaiseClosed();
                    break;
                }
                finally
                {
                    Debug.WriteLine(nameof(OnReceive));
                }

            }
        }
        private void RaiseClosed()
        {
            Closed?.Invoke(this, EventArgs.Empty);

        }
        public event CommReceiveEventHandler Received;
        public event EventHandler Closed;
        private void OnSend(IAsyncResult result)
        {

        }

    }
}
