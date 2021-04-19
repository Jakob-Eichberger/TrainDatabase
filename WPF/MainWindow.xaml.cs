using Helper;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WPF_Application;
using WPF_Application.DbImport;

namespace Wpf_Application
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly ModelTrainController.ModelTrainController controler = Z21Connection.Get();
        private readonly Database db = new();

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                db.FillDatabase();

                if (db.Vehicles.Any())
                {
                    DrawAllVehicles(db.Vehicles.ToList());
                }
                else
                {
                    MessageBoxResult rs = MessageBox.Show("Sie haben noch keine Daten in der Datenbank. Möchten Sie jetzt welche importieren?", "Datenbank importieren", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (rs == MessageBoxResult.Yes)
                    {
                        new Import_Overview().Show();
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"{e.Message}\n inner: {e?.InnerException?.Message ?? ""}");
            }
        }

        private void DB_Import_Z21_new(object sender, RoutedEventArgs e) => new DB_Import_Z21().Show();

        public void DrawAllVehicles(IEnumerable<Vehicle> list)
        {
            VehicleGrid.Children.Clear();
            foreach (var item in list)
            {
                Border border = new();
                border.Padding = new(2);
                border.Margin = new(10);
                border.BorderThickness = new(1);
                border.BorderBrush = Brushes.Black;

                StackPanel sp = new();
                //sp.Orientation = Orientation.Vertical;

                try
                {
                    string path = $"{Directory.GetCurrentDirectory()}\\Data\\VehicleImage\\";
                    path += string.IsNullOrWhiteSpace(item?.ImageName) ? "default.png" : item?.ImageName;
                    Image image = new();
                    BitmapImage bitmap = new();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(path);
                    bitmap.EndInit();
                    image.Source = bitmap;
                    image.Width = 250;
                    image.Height = 100;
                    sp.Children.Add(image);
                }
                catch (Exception ex)
                {
                    Logger.Log($"{DateTime.UtcNow}: Image for Lok with adress '{item?.Address}' not found. Message: {ex.Message}", LoggerType.Warning);
                }
                TextBlock x = new();
                x.Text = !string.IsNullOrWhiteSpace(item?.FullName) ? item?.FullName : (!string.IsNullOrWhiteSpace(item?.Name) ? item?.Name : $"Adresse: {item?.Address}");
                sp.Height = 120;
                sp.Width = 250;
                sp.Children.Add(x);
                sp.HorizontalAlignment = HorizontalAlignment.Left;
                sp.VerticalAlignment = VerticalAlignment.Top;

                sp.ContextMenu = new();

                MenuItem miControlLoko = new MenuItem();
                miControlLoko.Header = "Lok steueren";
                miControlLoko.Click += ControlLoko_Click;
                sp.ContextMenu.Items.Add(miControlLoko);
                miControlLoko.Tag = item;

                MenuItem miEditLoko = new MenuItem();
                miEditLoko.Header = "Lok bearbeiten";
                miEditLoko.Click += EditLoko_Click;

                sp.ContextMenu.Items.Add(miEditLoko);
                border.Child = sp;
                VehicleGrid.Children.Add(border);

            }
        }

        void ControlLoko_Click(Object sender, RoutedEventArgs e)
        {
            try
            {
                var menu = ((MenuItem)e.Source);
                Vehicle? vehicle = (menu.Tag as Vehicle);
                if (vehicle is not null)
                    new TrainControl(controler, vehicle).Show();
                else
                    MessageBox.Show("Öffnen nicht möglich da Vehilce null ist!", "Erro");
            }
            catch (Exception ex)
            {
                Logger.Log($"{DateTime.UtcNow}: Method {nameof(ControlLoko_Click)} throw an exception! \nMessage: {ex.Message}\nIE: {ex.InnerException}\nIE Message: {ex.InnerException.Message}", LoggerType.Error);
                MessageBox.Show($"Beim öffnen ist ein Fehler aufgetreten! Fehlermeldung: {ex.Message}", "Error beim öffnen");
            }

        }

        void EditLoko_Click(Object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Does not work yet! Sorry!");
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tbSearch.Text))
                DrawAllVehicles(db.Vehicles.Include(e => e.Category).Where(i => (i.Address + i.ArticleNumber + i.Category.Name + i.Owner + i.Railway + i.Description + i.FullName + i.Name).ToLower().Contains(tbSearch.Text.ToLower())));
            else
                DrawAllVehicles(db.Vehicles);
        }
    }
}
