using EPI.Comm.Net;
using System.Net;

namespace UnitTest
{
    class TestModel
    {
        public TcpNetServer Server { get; set; }
        public TcpNetClient Client { get; set; }
        public byte[] Data { get; set; }
        public TestModel()
        {
          
        }
        public void Init(int port)
        {
            Server = InitServer();
            Client = InitClient();
            Data = GetBytes();
            Server.StartListen(port);
            Client.Connect(IPAddress.Loopback.ToString(), port);
        }
        private TcpNetClient InitClient()
        {
            return new TcpNetClient();
        }
        private TcpNetServer InitServer()
        {
            return new TcpNetServer();
        }
        private byte[] GetBytes()
        {
            var result = new byte[256];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (byte)i;
            }
            return result;
        }
        public void Close()
        {
            Server?.Dispose();
            Client?.Dispose();
        }
    }
}
