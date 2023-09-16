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

namespace CommSample
{
    /// <summary>
    /// JustTestUC.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class JustTestWindow : Window
    {
        private JustTest JustTest = new JustTest();
        public JustTestWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            JustTest.SendToServer();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            JustTest.SendToClient();
        }
    }
}
