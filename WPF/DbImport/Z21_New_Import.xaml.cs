using Infrastructure;
using Microsoft.Win32;
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

namespace WPF_Application.DbImport
{
    /// <summary>
    /// Interaction logic for Z21_New_Import.xaml
    /// </summary>
    public partial class Z21_New_Import : Window
    {
        private readonly Database db = new();

        public Z21_New_Import()
        {
            InitializeComponent();
        }

        private void btnOpenFileDalog_Click(object sender, RoutedEventArgs e)
        {
            string path = "";
            OpenFileDialog ofp = new();
            if (ofp.ShowDialog() == true)
            {
                path = ofp.FileName;
            }
            new DBImport.Z21_New_Import(db, path);
        }


    }
}
