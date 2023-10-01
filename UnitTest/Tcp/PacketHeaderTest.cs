using EPI.Comm;
using EPI.Comm.Net.Generic;
using EPI.Comm.Net.Generic.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using UnitTest.Models;

namespace UnitTest.Tcp
{

    [TestClass]
    public class PacketHeaderTest
    {
        public TcpNetServer<Header> Server { get; set; }
        public TcpNetClient<Header> Client { get; set; }
        public List<PacketWithHeader> Data { get; set; }

        public PacketHeaderTest()
        {
        }
        [TestInitialize]
        public void Init()
        {
            const int Port = 5555;
            Server = new TcpNetServer<Header>(Header.GetBodySize) { IsBigEndian = true };
            Client = new TcpNetClient<Header>(Header.GetBodySize) { IsBigEndian = true };
            int packetCount = 1;
            Data = new List<PacketWithHeader>(packetCount);
            for (int i = 0; i < packetCount; i++)
            {
                var packet = new PacketWithHeader();
                packet.SetRandom();
                Data.Add(packet);
            }
            Server.StartListen(Port);
            Client.Connect(IPAddress.Loopback.ToString(), Port);
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
        private void IOTest(IComm<Header> sender, IComm<Header> receiver, List<PacketWithHeader> packets)
        {
            int count = 0;
            int fullCount = packets.Count;
            PacketWithHeader recv = null;

            receiver.Received += OnReceived;
            try
            {
                for (int i = 0; i < fullCount; i++)
                {
                    sender.Send(packets[i].Header, packets[i].Body);
                    while (count <= i)
                    {
                        Thread.Sleep(1);
                    }
                    Assert.AreEqual(packets[i], recv);
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
            void OnReceived(object s, PacketEventArgs<Header> e)
            {
                recv = new PacketWithHeader() { Header = e.Header, Body = e.Body, FullPacket =e.FullPacket };
                Console.WriteLine($"{recv}");
                count++;
            }
        }

    }


}
