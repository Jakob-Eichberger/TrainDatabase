using Helper;
using Service;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WPF_Application
{
    /// <summary>
    /// Interaction logic for LogWindow.xaml
    /// </summary>
    public partial class LogWindow : Window, INotifyPropertyChanged
    {

        public LogWindow(LogEventBus logService)
        {
            LogService = logService;
            DataContext = this;
            InitializeComponent();

            LogService.OnMessageLogged += Logger_OnMessageLogged;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Text { get; set; } = "";

        private Queue<string> Messages { get; } = new();
        public LogEventBus LogService { get; }

        public void OnPropertyChanged() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));

        private void Logger_OnMessageLogged(object? sender, MessageLoggedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Messages.Enqueue($"{e.DateTime:dd/MM/yy hh:mm:ss} - {e.Message?.Replace("\n", "\n\t  ")}");

                while (Messages.Count > 100)
                {
                    Messages.Dequeue();
                }
                Text = string.Join("\n", Messages);

                OnPropertyChanged();
                Sv?.ScrollToBottom();
            });
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            WindowState = WindowState.Minimized;
        }
    }
}
