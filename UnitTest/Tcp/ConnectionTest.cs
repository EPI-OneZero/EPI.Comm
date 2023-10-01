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
            const int port = 5552;
            var loopback = IPAddress.Loopback.ToString();
            server.StartListen(port);
            try
            {
                for (int i = 0; i < 3; i++)
                {
                    client.Connect(loopback, port);
                    Assert.IsTrue(client.IsConnected);
                    client.Stop();
                    Thread.Sleep(10);
                    Assert.IsFalse(client.IsConnected);
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                server.Dispose();
                client.Dispose();
            }
        }
        [TestMethod]
        public void ClientAutoConnect()
        {
            var server = new TcpNetServer();
            var client = new TcpNetClient();
            client.AutoConnect = true;
            const int port = 4103;
            var loopback = IPAddress.Loopback.ToString();
          
            try
            {
                server.StartListen(port);
                client.Connect(loopback, port);
                for (int i = 0; i < 3; i++)
                {
                    server.Stop();
                    Thread.Sleep(100);
                    Assert.IsFalse(client.IsConnected);
                    server.StartListen(port);
                    Thread.Sleep(500);
                    Assert.IsTrue(client.IsConnected);
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                server.Dispose();
                client.Dispose();
            }
            
            
        }
    }
}
