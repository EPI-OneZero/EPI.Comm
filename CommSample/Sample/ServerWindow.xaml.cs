using EPI.Comm.Net;
using EPI.Comm.Net.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CommSample.Sample
{
    /// <summary>
    /// ServerWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ServerWindow : Window
    {
        public ServerWindow()
        {
            InitializeComponent();
            Closed += ServerWindow_Closed;
            server = new TcpNetServer(int.Parse(port.Text));
            server.Received += Server_BytesReceived;
            server.ClientClosed += Server_Closed;
            server.ClientAccpeted += Server_Accpeted;

        }
        public static int ServerPort { get; set; } = 4101;
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
            MessageBox.Show("Server Client Closed");
        }

        private void Server_BytesReceived(object sender, CommReceiveEventArgs e)
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
            server.Send(bytes.ToArray());
        }
        private void Clear(object sender, RoutedEventArgs e)
        {
            recv.Text = string.Empty;
        }
    }

}
