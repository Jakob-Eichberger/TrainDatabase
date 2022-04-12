using Helper;
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

        public LogWindow()
        {
            DataContext = this;
            InitializeComponent();

            Logger.OnMessageLogged += Logger_OnMessageLogged;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public Queue<string> Items { get; } = new();

        public void OnPropertyChanged() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));

        private void Logger_OnMessageLogged(object? sender, MessageLoggedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Items.Enqueue($"{e.DateTime:dd/MM/yy hh:mm:ss} - {e.Message}");

                while (Items.Count > 500)
                {
                    Items.Dequeue();
                }

                OnPropertyChanged();
                Lv.Items.Refresh();
                Sv?.ScrollToBottom();
            });
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            WindowState = WindowState.Minimized;
        }
    }

    public class QueueLogItem : StackPanel
    {
        public QueueLogItem(MessageLoggedEventArgs item)
        {
            HorizontalAlignment = HorizontalAlignment.Stretch;
            Orientation = Orientation.Horizontal;
            Item = item;
            Loaded += QueueLogItem_Loaded;
        }

        public MessageLoggedEventArgs Item { get; }

        private void QueueLogItem_Loaded(object sender, RoutedEventArgs e)
        {
            Children.Add(new Label() { Content = Item.DateTime.ToString("dd/MM/yy hh:mm:ss") });
            Children.Add(new Label() { Content = $"{Item.Message}\n" });
        }
    }
}
