using Helper;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        readonly ModelTrainController.ModelTrainController controler = Z21Connection.Get();
        private readonly Database db = new();
        public event PropertyChangedEventHandler? PropertyChanged;
        private Brush primary = Brushes.White;
        private Brush secondary = Brushes.DarkGray;
        private Brush textColour = Brushes.Black;


        public Brush Primary
        {
            get => primary; set
            {
                primary = value;
                OnPropertyChanged();
            }
        }
        public Brush Secondary
        {
            get => secondary; set
            {
                secondary = value;
                OnPropertyChanged();
            }
        }
        public Brush TextColour
        {
            get => textColour; set
            {
                textColour = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            try
            {
                if (MessageBoxResult.No == MessageBox.Show("Achtung! Es handelt sich bei der Software um eine Alpha version! Es können und werden Bugs auftreten, wenn Sie auf JA drücken, stimmen Sie zu, dass der Entwickler für keinerlei Schäden, die durch die Verwendung der Software entstehen könnten, haftbar ist!", "Haftungsausschluss", MessageBoxButton.YesNo, MessageBoxImage.Information))
                {
                    Close();
                }

                InitializeComponent();

                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                db.FillDatabase();

                if (db.Vehicles.Any())
                    DrawAllVehicles(db.Vehicles.OrderBy(e => e.Position).ToList());
                else
                    if (MessageBoxResult.Yes == MessageBox.Show("Sie haben noch keine Daten in der Datenbank. Möchten Sie jetzt welche importieren?", "Datenbank importieren", MessageBoxButton.YesNo, MessageBoxImage.Question))
                    new Import_Overview().Show();
            }
            catch (Exception e)
            {
                MessageBox.Show($"{e.Message}\n inner: {e?.InnerException?.Message ?? ""}");
            }
        }

        private void DB_Import_Z21_new(object sender, RoutedEventArgs e) => new Z21_New_Import().Show();

        public void DrawAllVehicles(IEnumerable<Vehicle> list)
        {
            VehicleGrid.Children.Clear();
            foreach (var item in list)
            {
                if (item is null) return;
                Border border = new();
                border.Padding = new(2);
                border.Margin = new(10);
                border.BorderThickness = new(1);
                border.BorderBrush = Brushes.Black;

                StackPanel sp = new();
                try
                {
                    string path = $"{Directory.GetCurrentDirectory()}\\Data\\VehicleImage\\";
                    path += string.IsNullOrWhiteSpace(item?.ImageName) ? "default.png" : item?.ImageName;
                    Image image = new();
                    BitmapImage bitmap = new();
                    bitmap.BeginInit();
                    bitmap.UriSource = new(path);
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

                MenuItem miControlLoko = new();
                miControlLoko.Header = item.Type == VehilceType.Lokomotive ? "Lok steuern" : (item.Type == VehilceType.Steuerwagen ? "Steuerwagen steuern" : "Wagen steuern");
                miControlLoko.Click += ControlLoko_Click;
                sp.ContextMenu.Items.Add(miControlLoko);
                miControlLoko.Tag = item;
                MenuItem miEditLoko = new();
                miEditLoko.Header = item.Type == VehilceType.Lokomotive ? "Lok bearbeiten" : (item.Type == VehilceType.Steuerwagen ? "Steuerwagen bearbeiten" : "Wagen bearbeiten");
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

        private void TbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tbSearch.Text))
                DrawAllVehicles(db.Vehicles.Include(e => e.Category).Where(i => (i.Address + i.ArticleNumber + i.Category.Name + i.Owner + i.Railway + i.Description + i.FullName + i.Name + i.Type).ToLower().Contains(tbSearch.Text.ToLower())).OrderBy(e => e.Position));
            else
                DrawAllVehicles(db.Vehicles.OrderBy(e => e.Position));
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
