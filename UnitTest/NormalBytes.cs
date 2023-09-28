using EPI.Comm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTest
{
    [TestClass]
    public class NormalBytes
    {
        public const int Port = 4101;

        public NormalBytes()
        {

        }
        [TestMethod]
        public void ClientsToServer()
        {
            var model = new TestModel();
            try
            {
                model.Init(Port);
                IOTest(model.Client, model.Server, model.Data);
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                model.Close();
            }
        }
        [TestMethod]
        public void ServersToClient()
        {
            var model = new TestModel();
            model.Init(Port);
           
            try
            {
                IOTest(model.Server, model.Client, model.Data);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                model.Close();
            }

        }
        private void IOTest(IComm sender, IComm receiver, byte[] sendBytes)
        {
            int count = 0;
            int fullCount = 100;
            byte[] recv = null;
            receiver.Received += OnReceived;
            try
            {
                for (int i = 0; i < fullCount; i++)
                {
                    sender.Send(sendBytes);
                    while (count <= i)
                    {
                        Thread.Sleep(1);
                    }
                    Assert.IsTrue(Enumerable.SequenceEqual(sendBytes, recv));
                }
                Console.WriteLine(count);
                Console.WriteLine(fullCount);
                Assert.AreEqual(count, fullCount);

            }
            catch (Exception)
            {

                throw;
            }
            void OnReceived(object s, EPI.Comm.Net.Events.PacketEventArgs e)
            {
                recv = e.ReceivedBytes;
                count++;
            }
        }
    }
}
