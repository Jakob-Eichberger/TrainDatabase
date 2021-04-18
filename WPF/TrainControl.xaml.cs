using Helper;
using System;
using System.Windows;
using System.Windows.Controls;
namespace WPF_Application
{
    /// <summary>
    /// Interaction logic for TrainControl.xaml
    /// </summary>
    public partial class TrainControl : UserControl
    {
        public LokInfoData LokData { get; set; }
        public TrainControl(Z21 z21)
        {
            try
            {
                InitializeComponent();
                LokData = new();
            }
            catch (Exception e)
            {
                MessageBox.Show($"Beim öffnen der ansicht ist ein Fehler aufgetreten: {e.Message}");
            }
        }
    }
}
