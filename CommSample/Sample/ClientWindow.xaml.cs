using EPI.Comm;
using EPI.Comm.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// ClientWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ClientWindow : Window
    {
       
        public ClientWindow()
        {
            InitializeComponent();
            client.Received += Client_BytesReceived;
            client.Closed += Client_Closed;
            client.Connected += Client_Connected; ;
            Closed += ClientWindow_Closed;
        }

        private void Client_Connected(object sender, EventArgs e)
        {
            var client = sender as Client;
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
            client.Stop();
        }

        private Client client = new Client("127.0.0.1", 5500);
        private void Client_Closed(object sender, EventArgs e)
        {
            MessageBox.Show("Client Closed");
        }

        private void Client_BytesReceived(object sender, CommReceiveEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                recv.Text = System.Text.Encoding.UTF8.GetString(e.ReceivedBytes) + "\r\n";
                recv.Text =e.ReceivedBytes.Length + "\r\n";

            }));
        }

        private void Start(object sender, RoutedEventArgs e)
        {
            client.Start();
        }

        private void Stop(object sender, RoutedEventArgs e)
        {
            client.Stop();
        }

        private void text_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                var bytes = text.Text;
                client.Send(bytes);
            }
      
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
