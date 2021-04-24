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

        public string ControlerIp
        {
            get { return Settings.ControlerIP.ToString(); }
            set
            {
                try
                {
                    Settings.ControlerIP = IPAddress.Parse(value);
                }
                catch (FormatException)
                {
                    ErroMessageLabel.Content = "IP Incorrect";
                }
                ErroMessageLabel.Content = "-";
            }
        }
        public SettingsWindow()
        {
            InitializeComponent();
        }
    }
}
