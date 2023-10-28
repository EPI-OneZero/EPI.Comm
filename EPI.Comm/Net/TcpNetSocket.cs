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
    public sealed class TcpNetSocket
    {
        #region Field & Property
        private readonly object SendLock = new object();
        private static readonly byte[] KeepAliveConfig = new KeepAlive().Generate();
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
            ThreadUtil.Start(()=>
            {
                while (IsConnected && TryReceive())
                {
                }

                Closed?.Invoke(this, EventArgs.Empty);
            });
        }
        #endregion

        #region Method & Event
        private void SetSocketOption(Socket socket)
        {
            socket.ReceiveBufferSize = ReceiveBuffer.Length;
            socket.SendBufferSize = ReceiveBuffer.Length;
            socket.NoDelay = true;
            socket.LingerState = lingerOption;
            SetKeepAlive(socket);
        }
        private static void SetKeepAlive(Socket socket)
        {

            socket.IOControl(IOControlCode.KeepAliveValues, KeepAliveConfig, null);
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
                Logger.Default.WriteLine(e.Message);
            }
            catch (ObjectDisposedException e)
            {
                Logger.Default.WriteLine(e.Message);
            }
            finally
            {
                Logger.Default.WriteLineCaller();
            }

        }
        private bool TryReceive()
        {
            try
            {
                int count = Socket.Receive(ReceiveBuffer);
                if (count <= 0)
                {
                    return false;
                }
                byte[] result = new byte[count];
                Buffer.BlockCopy(ReceiveBuffer, 0, result, 0, count);
                Received?.Invoke(this, new PacketEventArgs(RemoteEndPoint, result));
                return true;

            }
            catch (SocketException e)
            {
                return false;
            }
            catch (ObjectDisposedException e)
            {
                return false;
            }
            finally
            {
                Logger.Default.WriteLineCaller();
            }
        }
        public event PacketEventHandler Received;
        public event EventHandler Closed;
        #endregion

        #region KeepAlive
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal sealed class KeepAlive
        {
            public uint OnOff = 1;
            public uint IntervalMilliseconds = 3 * 1000;
            public uint RetryMilliseconds = 100;

            public byte[] Generate()
            {
                const int Size = 12;
                var result = new byte[Size];
                MarshalSerializer.Serialize(this, result, Size);
                return result;
            }
        }
        #endregion
    }

}