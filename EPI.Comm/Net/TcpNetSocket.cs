using EPI.Comm.Net.Events;
using EPI.Comm.Utils;
using EPI.Comm.UTils;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using static EPI.Comm.CommException;

namespace EPI.Comm.Net
{
    internal class TcpNetSocket : CommBase
    {
        #region Field & Property
        private readonly object SendLock = new object();
        private readonly object ReceiveLock = new object();
        private readonly KeepAlive KeepAliveConfig = new KeepAlive();
        protected Socket Socket { get; set; }
        protected byte[] ReceiveBuffer { get;private set; }
        public bool IsConnected => Socket?.Connected ?? false;
        public IPEndPoint LocalEndPoint => Socket?.LocalEndPoint as IPEndPoint;
        public IPEndPoint RemoteEndPoint => Socket?.RemoteEndPoint as IPEndPoint;
        private static readonly LingerOption lingerOption = new LingerOption(true, 0);

        #endregion

        #region CTOR
        internal TcpNetSocket(Socket socket, int bufferSize)
        {
            Socket = socket;
            ReceiveBuffer = new byte[bufferSize];
            SetSocketOption(socket);
            ThreadUtil.Start(ReceiveLoop);
        }

        #endregion

        #region Method
        private void SetSocketOption(Socket socket)
        {
            socket.ReceiveBufferSize = ReceiveBuffer.Length;
            socket.SendBufferSize = ReceiveBuffer.Length;
            socket.NoDelay = true;
            socket.LingerState = lingerOption;
            SetKeepAlive(socket);
        }
        private void SetKeepAlive(Socket socket)
        {
            var size = Marshal.SizeOf<KeepAlive>();

            socket.IOControl(IOControlCode.KeepAliveValues, KeepAliveConfig.Generate(), null);
        }
        public void Send(byte[] bytes)
        {
            try
            {
                lock (SendLock)
                {
                   var res= Socket.Send(bytes);
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
            catch(NullReferenceException e)
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
                    throw CreateCommException($"연결 끊김 수신 바이트 수 :{count}");
                }
                return result;

            }
            catch (NullReferenceException e)
            {
                throw CreateCommException(e);
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
        private void ReceiveLoop()
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
                    Received?.Invoke(this, new SocketReceiveEventArgs(RemoteEndPoint, recv));
                }
                catch (CommException e) // 연결을 끊었을 때
                {
                    Debug.WriteLine(e.Message);
                    RaiseClosed();
                    break;
                }
                finally
                {
                    Debug.WriteLine(nameof(ReceiveLoop));
                }

            }
        }
        private void RaiseClosed()
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Event
        public event SocketReceiveEventHandler Received;
        public event EventHandler Closed;
        #endregion
    }
    #region KeepAlive
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal class KeepAlive
    {
        public uint OnOff = 1;
        public uint IntervalMilliseconds = 3 * 1000;
        public uint RetryMiliseconds = 100;

        public byte[] Generate()
        {
            var size = Marshal.SizeOf<KeepAlive>();
            var result = new byte[size];
            PacketSerializer.SerializeByMarshal(this, result, 0, size);
            return result;
        }
    }
    #endregion
}
