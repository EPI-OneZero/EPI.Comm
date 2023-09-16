using CommSample.Sample;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CommSample
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Application.Current.Exit += Current_Exit;
            
        }

        private void Current_Exit(object sender, ExitEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private ServerWindow serverWindow = null;
        private ClientWindow clientWindow = null;
        private void OpenServerWindow(object sender, RoutedEventArgs e)
        {
            serverWindow = CreateWindow(serverWindow, () => serverWindow = null);
        }
        private void OpenClientWindow(object sender, RoutedEventArgs e)
        {
            clientWindow = CreateWindow(clientWindow, () => clientWindow = null);
        }
        private T CreateWindow<T>(T source, Action closeCallback) where  T : Window, new()
        {
            if(source != null)
            {
                source.Focus();
                return source;
            }
            else
            {
                var result = new T();
                result.Closed+= (s,e)=> closeCallback?.Invoke();
                result.Show();
                return result;
            }
          
        }
    }
}
