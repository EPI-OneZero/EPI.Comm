using EPI.Comm.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Threading;

namespace UnitTest.Tcp
{
    [TestClass]
    public class ConnectionTest
    {
        [TestMethod]
        public void ClientConnect()
        {
            var server = new TcpNetServer();
            var client = new TcpNetClient();
            const int port = 4101;
            var loopback = IPAddress.Loopback.ToString();
            server.StartListen(port);

            for (int i = 0; i < 10; i++)
            {
                client.Connect(loopback, port);
                Assert.IsTrue(client.IsConnected);
                client.Stop();
                Assert.IsFalse(client.IsConnected);
            }
            server.Dispose();
            client.Dispose();
        }
        [TestMethod]
        public void ClientAutoConnect()
        {
            var server = new TcpNetServer();
            var client = new TcpNetClient();
            client.AutoConnect = true;
            const int port = 4101;
            var loopback = IPAddress.Loopback.ToString();
            server.StartListen(port);
            client.Connect(loopback, port);
            for (int i = 0; i < 5; i++)
            {
                server.Stop();
                Thread.Sleep(20);
                Assert.IsFalse(client.IsConnected);
                server.StartListen(port);
                Thread.Sleep(1000);
                Assert.IsTrue(client.IsConnected);
            }
            server.Dispose();
            client.Dispose();
        }
    }
}
