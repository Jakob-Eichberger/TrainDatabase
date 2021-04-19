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
using WPF_Application.DbImport;

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
                db.FillDatabase();

                InitializeComponent();
                z21 = Z21Connection.Get();

                new TrainControl(z21, db?.Vehicles?.FirstOrDefault(e => e.Address == 9) ?? throw new ApplicationException("Lok ned do")).Show();

            }
            catch (Exception e)
            {
                MessageBox.Show($"{e.Message}\n inner: {e?.InnerException?.Message ?? ""}");
            }
        }

        private void DB_Import_Z21_new(object sender, RoutedEventArgs e) => new DB_Import_Z21().Show();
    }
}
