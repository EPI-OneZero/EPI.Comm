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

namespace EPI.Comm.Tcp
{
    public class SocketHolder : CommBase, IComm
    {
        protected Socket Socket { get; set; }
        protected byte[] Buffer { get; set; }
        protected AsyncCallback SendCallback { get; private set; }
        public bool IsConnected => Socket?.Connected ?? false;
        public IPEndPoint LocalEndPoint => Socket?.LocalEndPoint as IPEndPoint;
        public IPEndPoint RemoteEndPoint => Socket?.RemoteEndPoint as IPEndPoint;
        internal SocketHolder(Socket socket) : this(socket, ushort.MaxValue)
        {

        }
        internal SocketHolder(Socket socket, int bufferSize)
        {
            Socket = socket;
            Buffer = new byte[bufferSize];
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
            socket.ReceiveBufferSize = Buffer.Length;
            socket.SendBufferSize = Buffer.Length;
            socket.NoDelay = true;
            //var y = socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger);
            //var yy = socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger);
            socket.LingerState = new LingerOption(true, 0);
            //var x = socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger);
            //var xx = socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger);
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
                int count = Socket.Receive(Buffer);
                byte[] result = new byte[count];
                Array.Copy(Buffer, result, count);
                if(count <= 0)
                {
                    throw CreateCommException();
                }
                return result;
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
