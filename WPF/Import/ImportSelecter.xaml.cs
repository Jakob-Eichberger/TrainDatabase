using System.Windows;
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
