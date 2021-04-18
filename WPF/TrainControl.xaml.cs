using Helper;
using Model;
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

namespace WPF_Application
{
    /// <summary>
    /// Interaction logic for TrainControl.xaml
    /// </summary>
    public partial class TrainControl : Window
    {
        public Z21 z21;
        public Vehicle vehicle;
        public TrainControl(Z21 _z21, Vehicle _vehicle)
        {
            InitializeComponent();
            z21 = _z21;
            vehicle = _vehicle;
        }
    }
}
