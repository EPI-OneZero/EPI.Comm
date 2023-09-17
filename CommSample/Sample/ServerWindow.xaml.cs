using EPI.Comm;
using EPI.Comm.Net;
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
            server.Received += Server_BytesReceived;
            server.Closed += Server_Closed;
            server.Accpeted += Server_Accpeted;
           
        }

        private void Server_Accpeted(object sender, EventArgs e)
        {
            var client = sender as TcpNetClient;
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

        private TcpNetServer server = new TcpNetServer(5500);
        private void Server_Closed(object sender, EventArgs e)
        {
            MessageBox.Show("Server Client Closed");
        }

        private void Server_BytesReceived(object sender, CommReceiveEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                recv.Text += System.Text.Encoding.UTF8.GetString(e.ReceivedBytes) + "\r\n";

            }));
        }

        private void Start(object sender, RoutedEventArgs e)
        {
            server.Start();
        }

        private void Stop(object sender, RoutedEventArgs e)
        {
            server.Stop();
        }

        private void text_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes( text.Text);
                 
                server.Send(bytes);
            }
        }
        private void Clear(object sender, RoutedEventArgs e)
        {
            recv.Text= string.Empty;
        }
    }
  
}
