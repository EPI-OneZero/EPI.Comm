using EPI.Comm.Net.Events;
using EPI.Comm.UTils;
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
        private readonly object SendLock = new object();
        private readonly object ReceiveLock = new object();
        protected Socket Socket { get; set; }
        protected byte[] ReceiveBuffer { get; set; }
        public bool IsConnected => Socket?.Connected ?? false;
        public IPEndPoint LocalEndPoint => Socket?.LocalEndPoint as IPEndPoint;
        public IPEndPoint RemoteEndPoint => Socket?.RemoteEndPoint as IPEndPoint;
        private static readonly LingerOption lingerOption = new LingerOption(true, 0);
        internal NetSocket(Socket socket, int bufferSize)
        {
            Socket = socket;
            ReceiveBuffer = new byte[bufferSize];
            SetSocketOption(socket);
            InitCallback();
            ThreadUtil.Start(OnReceive);
        }
        
        private void SetSocketOption(Socket socket)
        {
            socket.ReceiveBufferSize = ReceiveBuffer.Length;
            socket.SendBufferSize = ReceiveBuffer.Length;
            socket.NoDelay = true;
            socket.LingerState = lingerOption;
        }
        private void InitCallback()
        {
        }
        public void Send(byte[] bytes)
        {
            try
            {
                lock (SendLock)
                {
                    Socket.Send(bytes);
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
      
        private byte[] Receive()
        {
            try
            {
                int count = Socket.Receive(ReceiveBuffer);
                byte[] result = new byte[count];
                Buffer.BlockCopy(ReceiveBuffer, 0, result, 0, count);
                if (count <= 0)
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
            while (IsConnected)
            {
                try
                {
                    byte[] recv;
                    lock (ReceiveLock)
                    {
                        recv = Receive();
                    }
                    Received?.Invoke(this, new CommReceiveEventArgs(RemoteEndPoint, recv));
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
    }
}
