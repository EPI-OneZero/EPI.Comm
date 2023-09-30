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
    public class PacketHeaderFooterTest
    {
        public TcpNetServer<Header, Footer> Server { get; set; }
        public TcpNetClient<Header, Footer> Client { get; set; }
        public List<PacketWithHeaderFooter> Data { get; set; }


        public PacketHeaderFooterTest()
        {
        }
        [TestInitialize]
        public void Init()
        {
            const int Port = 5555;
            Server = new TcpNetServer<Header, Footer>(Header.GetBodySize) { IsBigEndian = true };
            Client = new TcpNetClient<Header, Footer>(Header.GetBodySize) { IsBigEndian = true };
            int packetCount = 10;
            Data = new List<PacketWithHeaderFooter>(packetCount);
            for (int i = 0; i < packetCount; i++)
            {
                var packet = new PacketWithHeaderFooter();
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
        private void IOTest(IComm<Header, Footer> sender, IComm<Header, Footer> receiver, List<PacketWithHeaderFooter> packets)
        {
            int count = 0;
            int fullCount = packets.Count;
            PacketWithHeaderFooter recv = null;
            receiver.Received += OnReceived;
            try
            {
                for (int i = 0; i < fullCount; i++)
                {
                    sender.Send(packets[i].Header, packets[i].Body, packets[i].Footer);
                    while (count <= i)
                    {
                        Thread.Sleep(1);
                    }
                    try
                    {
                        Assert.AreEqual(packets[i], recv);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine(packets[i].Header.BodySize);
                        Console.WriteLine(packets[i].Footer.Etx);
                        Console.WriteLine(packets[i].Body.Length);
                        Console.WriteLine(recv.Header.BodySize);
                        Console.WriteLine(recv.Footer.Etx);
                        Console.WriteLine(recv.Body.Length);
                        throw;
                    }
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
            void OnReceived(object s, PacketEventArgs<Header, Footer> e)
            {
                recv = new PacketWithHeaderFooter() { Header = e.Packet.Header, Body = e.Packet.Body, Footer = e.Packet.Footer };
                count++;
            }
        }
    }


}
