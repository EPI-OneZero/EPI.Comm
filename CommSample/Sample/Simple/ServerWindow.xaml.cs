﻿using EPI.Comm.Net;
using EPI.Comm.Net.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CommSample.Sample
{
    public partial class ServerWindow : Window
    {
        public ServerWindow()
        {
            InitializeComponent();
            Closed += ServerWindow_Closed;
            server = new TcpNetServer(int.Parse(port.Text));
            server.Received += Server_BytesReceived;
            server.ClientDisconnected += Server_Closed;
            server.ClientConnected += Server_Accpeted;

        }
        public static int ServerPort { get; set; } = 12345;
        private void Server_Accpeted(object sender, TcpEventArgs e)
        {
            var client = e.TcpNetClient;
            var local = client.LocalEndPoint;
            var remote = client.RemoteEndPoint;

            var ip0 = local.Address.ToString();
            var port0 = local.Port.ToString();

            var ip1 = remote.Address.ToString();
            var port1 = remote.Port.ToString();
            MessageBox.Show($"server local : {ip0} : {port0} , remote : {ip1} : {port1}");
        }

        private void ServerWindow_Closed(object sender, EventArgs e)
        {
            server.Stop();

        }

        private TcpNetServer server;
        private void Server_Closed(object sender, TcpEventArgs e)
        {
            ///MessageBox.Show("Server Client Closed");
        }
        private volatile int count = 0;
        private void Server_BytesReceived(object sender, PacketEventArgs e)
        {
            //server.Clients.FirstOrDefault().Send(e.ReceivedBytes);
            var bytes = new byte[]
                    {
                        1,2,3,4,5,6,7,8,
                        1,2,3,4,5,6,7,8,
                        1,2,3,4,5,6,7,8,
                        1,2,3,4,5,6,7,8,
                    };
            server.Send(bytes);
            Thread.Sleep(100);
            //Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    var build = new StringBuilder();
            //    build.AppendLine($"받은 바이트 수 :  {e.FullPacket.Length}");
            //    for (int i = 0; i < e.FullPacket.Length; i++)
            //    {
            //        if (i % 8 == 0)
            //        {
            //            build.AppendLine();
            //        }
            //        var b = e.FullPacket[i];
            //        build.Append($"{b:D3}\t");

            //    }
            //    build.AppendLine();
            //    recv.Text += build.ToString();

            //}));
        }

        private void Start(object sender, RoutedEventArgs e)
        {
            if (ushort.TryParse(port.Text, out ushort p))
            {
                if (!server.IsListening || server.Port != p)
                {
                    server.Stop();
                    server.StartListen(p);
                    ServerPort = p;
                }

            }
            else
            {
                MessageBox.Show("포트번호 이상");
            }

        }

        private void Stop(object sender, RoutedEventArgs e)
        {

            server.Stop();

        }

        private void Send(object sender, RoutedEventArgs e)
        {
            var bytes = new byte[]
                    {
                        1,2,3,4,5,6,7,8,
                        1,2,3,4,5,6,7,8,
                        1,2,3,4,5,6,7,8,
                        1,2,3,4,5,6,7,8,
                    };


            server.Send(bytes);
        }
        private void Clear(object sender, RoutedEventArgs e)
        {
            recv.Text = string.Empty;
        }
    }

}
