using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

namespace WPF_Application
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {

        public bool ValueChanged { get; set; }

        public string ControllerIp
        {
            get => Settings.ControllerIP.ToString();
            set
            {
                try
                {
                    ValueChanged = value != ControllerIp;
                    Settings.ControllerIP = IPAddress.Parse(value);
                }
                catch
                {
                    ValueChanged = false;
                    MessageBox.Show($"'{value}' ist keine valide Ip!");
                }
            }
        }

        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void settingsw_Closed(object sender, EventArgs e)
        {
            if (ValueChanged)
            {
                MessageBox.Show("Geänderte Einstellungen werden teilweise erst nach einem Restart übernommen!", "Geänderte Settings", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
