using Helper;
using Infrastructure;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPF_Application;

namespace Wpf_Application
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly Z21 z21;
        private readonly Database db = new();
        private readonly List<TrainControl> TrainControls = new();
        public MainWindow()
        {
            try
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                InitializeComponent();
                z21 = Z21Connection.Get();
                TrainControls.Add(new TrainControl(z21, db.Vehicles.FirstOrDefault(e => e.Address == 9)));
                TrainControls[0].Show();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

    }
}
