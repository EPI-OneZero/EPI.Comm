using EPI.Comm.Net;
using EPI.Comm.Net.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;

namespace CommSample.Sample
{
    /// <summary>
    /// ClientWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ClientWindow : Window
    {

        public ClientWindow()
        {
            InitializeComponent();
            InitClient();

        }
        private void InitClient()
        {
            client = new TcpNetClient();
            client.Received += Client_BytesReceived;
            client.Disconnected += Client_Closed;
            client.Connected += Client_Connected; ;
            Closed += ClientWindow_Closed;
        }

        private void Client_Connected(object sender, EventArgs e)
        {
            var client = sender as TcpNetClient;
            var local = client.LocalEndPoint;
            var remote = client.RemoteEndPoint;

            var ip0 = local.Address.ToString();
            var port0 = local.Port.ToString();

            var ip1 = remote.Address.ToString();
            var port1 = remote.Port.ToString();
            MessageBox.Show($"client local : {ip0} : {port0} , remote : {ip1} : {port1}");
        }

        private void ClientWindow_Closed(object sender, EventArgs e)
        {
            client?.Stop();
        }

        private TcpNetClient client = new TcpNetClient();
        private void Client_Closed(object sender, EventArgs e)
        {
            MessageBox.Show("Client Closed");
        }

        private void Client_BytesReceived(object sender, PacketEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var build = new StringBuilder();
                build.AppendLine($"받은 바이트 수 :  {e.ReceivedBytes.Length}");
                for (int i = 0; i < e.ReceivedBytes.Length; i++)
                {
                    if (i % 8 == 0)
                    {
                        build.AppendLine();
                    }
                    var b = e.ReceivedBytes[i];
                    build.Append(b + " ");

                }
                build.AppendLine();
                recv.Text += build.ToString();

            }));
        }

        private void Start(object sender, RoutedEventArgs e)
        {
            var s = ip.Text;
            var p = port.Text;
            var t = new Thread(() =>
            {
                client.Connect(s, int.Parse(p));

            });
            t.Start();
            //if(client.IsConnected)
            //{
            //    client.Send(new byte[] { 1,2,3});
            //    client.Stop();
            //}
        }

        private void Stop(object sender, RoutedEventArgs e)
        {
            var t = new Thread(() =>
            {
                client?.Stop();
            });
            t.Start();

        }

        private void Send(object sender, RoutedEventArgs e)
        {
            var split = text.Text.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            var bytes = new List<byte>();

            foreach (var s in split)
            {
                try
                {
                    var b = byte.Parse(s);
                    bytes.Add(b);
                }
                catch (Exception)
                {

                }
            }
            client.Send(bytes.ToArray());
        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            recv.Text = string.Empty;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }

}
