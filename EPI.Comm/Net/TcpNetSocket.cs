using EPI.Comm.Log;
using EPI.Comm.Net.Events;
using EPI.Comm.Utils;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using static EPI.Comm.CommException;
namespace EPI.Comm.Net
{
    internal sealed class TcpNetSocket
    {
        #region Field & Property
        private readonly object SendLock = new object();
        private readonly object ReceiveLock = new object();
        private readonly KeepAlive KeepAliveConfig = new KeepAlive();
        internal Socket Socket { get; private set; }
        internal byte[] ReceiveBuffer { get; private set; }
        public bool IsConnected => Socket != null && Socket.Connected;
        public IPEndPoint LocalEndPoint { get; private set; }
        public IPEndPoint RemoteEndPoint { get; private set; }
        private static readonly LingerOption lingerOption = new LingerOption(true, 0);

        #endregion

        #region CTOR
        internal TcpNetSocket(Socket socket, int bufferSize)
        {
            Socket = socket;
            LocalEndPoint = socket.LocalEndPoint as IPEndPoint;
            RemoteEndPoint = socket.RemoteEndPoint as IPEndPoint;
            ReceiveBuffer = new byte[bufferSize];
            SetSocketOption(socket);
            DelegateUtil.Start(ReceiveLoop);
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

            socket.IOControl(IOControlCode.KeepAliveValues, KeepAliveConfig.Generate(), null);
        }
        public void Send(byte[] bytes)
        {
            try
            {
                lock (SendLock)
                {
                    var res = Socket.Send(bytes);
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
            catch (NullReferenceException e)
            {
                throw CreateCommException(e);
            }
            finally
            {
                //Logger.Default.WriteLineCaller();
            }

        }
        private byte[] Receive()
        {
            try
            {
                int count = Socket.Receive(ReceiveBuffer);
                if (count <= 0)
                {
                    throw CreateCommException($"연결 끊김 수신 바이트 수 : {count}");
                }
                byte[] result = new byte[count];
                Buffer.BlockCopy(ReceiveBuffer, 0, result, 0, count);
                return result;

            }
            catch(NullReferenceException e)
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
                Logger.Default.WriteLineCaller();
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
                    Received?.Invoke(this, new PacketEventArgs(RemoteEndPoint, recv));
                }
                catch (CommException e) // 연결을 끊었을 때
                {
                    Logger.Default.WriteLine(e.Message);
                    RaiseClosed();
                    break;
                }
                finally
                {
                    Logger.Default.WriteLineCaller();
                }

            }
        }
        private void RaiseClosed()
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Event
        public event PacketEventHandler Received;
        public event EventHandler Closed;
        #endregion
    }
    #region KeepAlive
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal sealed class KeepAlive
    {
        public uint OnOff = 1;
        public uint IntervalMilliseconds = 3 * 1000;
        public uint RetryMiliseconds = 100;

        public byte[] Generate()
        {
            var size = Marshal.SizeOf<KeepAlive>();
            var result = new byte[size];
            MarshalSerializer.Serialize(this, result, 0, size);
            return result;
        }
    }
    #endregion
}
