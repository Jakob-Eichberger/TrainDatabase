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
using WPF_Application.Infrastructure;

namespace Importer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ImportSelecter : Window
    {
        public Database Db { get; set; }

        public ImportSelecter(Database db)
        {
            Db = db;
            InitializeComponent();
        }

        private void newZ21_Click(object sender, RoutedEventArgs e)
        {
            new Z21(Db).Show();
            this.Close();
        }
    }
}
