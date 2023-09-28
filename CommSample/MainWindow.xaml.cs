using CommSample.Sample;
using CommSample.Sample.Packet1;
using System;
using System.Diagnostics;
using System.Windows;

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
        private ClientHeader clientHeaderWindow = null;
        private void OpenServerWindow(object sender, RoutedEventArgs e)
        {
            serverWindow = CreateWindow(serverWindow, () => serverWindow = null);
        }
        private void OpenClientWindow(object sender, RoutedEventArgs e)
        {
            clientWindow = CreateWindow(clientWindow, () => clientWindow = null);
        }
        private T CreateWindow<T>(T source, Action closeCallback) where T : Window, new()
        {
            if (source != null)
            {
                source.Focus();
                return source;
            } 
            else
            {
                var result = new T();
                result.Closed += (s, e) => closeCallback?.Invoke();
                result.Show();
                return result;
            }

        }

        private void OpenClientHeaderWindow(object sender, RoutedEventArgs e)
        {
            clientHeaderWindow = CreateWindow(clientHeaderWindow, () => clientHeaderWindow = null);
        }
    }
}
