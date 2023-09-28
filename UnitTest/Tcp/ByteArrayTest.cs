using EPI.Comm;
using EPI.Comm.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

namespace UnitTest.Tcp
{

    [TestClass]
    public class ByteArrayTest
    {
        public TcpNetServer Server { get; set; }
        public TcpNetClient Client { get; set; }
        public List<byte[]> Data { get; set; }
        public ByteArrayTest()
        {
        }
        [TestInitialize]
        public void Init()
        {
            const int Port = 4101;
            Server = new TcpNetServer();
            Client = new TcpNetClient();
            Data = new List<byte[]>();
            for (int i = 0; i < 10; i++)
            {
                Data.Add(GetBytes());
            }
            Server.StartListen(Port);
            Client.Connect(IPAddress.Loopback.ToString(), Port);
        }
        private byte[] GetBytes()
        {
            var random = new Random();
            var result = new byte[256];
            random.NextBytes(result);
            return result;
        }
        [TestCleanup]
        public void Close()
        {
            Server?.Dispose();
            Client?.Dispose();
        }
        [TestMethod]
        public void ClientsToServer()
        {
            IOTest(Client, Server, Data);
        }
        [TestMethod]
        public void ServersToClient()
        {
            IOTest(Server, Client, Data);
        }
        private void IOTest(IComm sender, IComm receiver, List<byte[]> sendBytes)
        {
            int count = 0;
            int fullCount = sendBytes.Count;
            byte[] recv = null;
            receiver.Received += OnReceived;
            try
            {
                for (int i = 0; i < fullCount; i++)
                {
                    sender.Send(sendBytes[i]);
                    while (count <= i)
                    {
                        Thread.Sleep(1);
                    }
                    Assert.IsTrue(Enumerable.SequenceEqual(sendBytes[i], recv));
                }
                Console.WriteLine(count);
                Console.WriteLine(fullCount);
                Assert.AreEqual(count, fullCount);

            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                receiver.Received -= OnReceived;
            }
            void OnReceived(object s, EPI.Comm.Net.Events.PacketEventArgs e)
            {
                recv = e.ReceivedBytes;
                count++;
            }
        }
    }
}
