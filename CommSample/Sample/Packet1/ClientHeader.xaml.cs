﻿using EPI.Comm;
using EPI.Comm.Net.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace CommSample.Sample.Packet1
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    class MyHeader
    {
        public int src;
        public int dst;
        public int code;
        public int size;
    }
    public partial class ClientHeader : Window
    {
        private TcpNetClient<MyHeader> client;
        public ClientHeader()
        {
            InitializeComponent();
            client = new TcpNetClient<MyHeader>(c => c.size);
            client.Received += Client_Received;
            this.Closed += ClientHeader_Closed;
        }

        private void ClientHeader_Closed(object sender, EventArgs e)
        {
            client?.Dispose();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            client.Connect("127.0.0.1", 4101);
        }
        private void Client_Received(object sender, EPI.Comm.Net.Generic.Events.PacketEventArgs<MyHeader> e)
        {
            var src = e.Packet.Header.src;
            var dst = e.Packet.Header.dst;
            var code = e.Packet.Header.code;
            var size = e.Packet.Header.size;
            var data = e.Packet.Body;
            count++;
            if (count < 10000)
            {
                Button_Click1(null, null);
            }
            else
            {
                var dt = DateTime.Now - time;
                var ms = dt.TotalSeconds;
                MessageBox.Show(((long)ms).ToString());
            }

        }
        private volatile int count = 0;
        private DateTime time;

       
        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            if (count == 0)
            {
                time = DateTime.Now;
            }

            //MyHeader header = new MyHeader()
            //{
            //    src = 1,
            //    dst = 2,
            //    code = 3,
            //    size = 32,
            //};
            //byte[] data = new byte[] 
            //{
            //    1, 2, 3, 4,
            //    1, 2, 3, 4,
            //    1, 2, 3, 4,
            //    1, 2, 3, 4,

            //    1, 2, 3, 4,
            //    1, 2, 3, 4,
            //    1, 2, 3, 4,
            //    1, 2, 3, 4,

            //};
            //client.Send(header, data);
            client.SendBytes(new byte[]
           {
                1,0,0,0,
                2,0,0,0,
                3,0,0,0,
                32,0,0,0,

                1,2,3,4,
                1,2,3,4,
                1,2,3,4,
                1,2,3,4,

                1,2,3,4,
                1,2,3,4,
                1,2,3,4,
                1,2,3,4,
           });
        }
    }
}
