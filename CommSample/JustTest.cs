using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CommSample
{
    internal class JustTest
    {
        public TcpClient Client { get; set; }
        public TcpListener Listener { get; set; }
        public Socket server;
        public JustTest()
        {
            Listener = new TcpListener(new IPEndPoint(IPAddress.Any, 5500));
          
            Listener.Start();
            Task.Run(() => 
            {
                //Client = new TcpClient("127.0.0.1", 5500);
                Client = new TcpClient();
                Client.Connect(IPAddress.Parse("127.0.0.1"), 5500);
            });
         
           
            Thread.Sleep(1000);
            server = Listener.AcceptSocket();
            SetSocket(Client.Client);
            SetSocket(server);
            ReceiveStart(Client.Client);
            ReceiveStart(server);
        }
        private void ReceiveStart(Socket socket)
        {
            var threadstart= new Thread(new ThreadStart(()=>
            {
                while (true)
                {
                    var bytes = new byte[1024];
                    var recv = socket.Receive(bytes, SocketFlags.None);
                    MessageBox.Show($"recvSize: {recv}");
                }
            })); 
            threadstart.Start();
        }
        private void SetSocket(Socket socket)
        {
            socket.SendBufferSize = 1024;
            socket.ReceiveBufferSize = 1024;
        }
        public void SendToServer()
        {
            Client.Client.Send(new byte[] { 1,2,3,4,});
            Client.Client.Send(new byte[] { 1,2,3,4,});
        }
        public void SendToClient()
        {
            server.Send(new byte[] { 1, 2, 3, 4, });

        }
    }
}
