using Helper;
using Microsoft.EntityFrameworkCore;
using Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WPF_Application;
using WPF_Application.Helper;
using WPF_Application.Infrastructure;

namespace Wpf_Application
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly Database db = new();
        public event PropertyChangedEventHandler? PropertyChanged;

        private Theme theme = default!;

        ModelTrainController.CentralStationClient? Controller { get; set; }

        public Theme Theme
        {
            get => theme;
            set
            {
                theme = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            try
            {

#if RELEASE
                if (MessageBoxResult.No == MessageBox.Show("Achtung! Es handelt sich bei der Software um eine Alpha version! Es können und werden Bugs auftreten, wenn Sie auf JA drücken, stimmen Sie zu, dass der Entwickler für keinerlei Schäden, die durch die Verwendung der Software entstehen könnten, haftbar ist!", "Haftungsausschluss", MessageBoxButton.YesNo, MessageBoxImage.Information))
                {
                    Close();
                }
#endif
                Theme = new();
                this.DataContext = this;
                InitializeComponent();
                db.Database.EnsureCreated();

                if (db.Vehicles.Any())
                    DrawAllVehicles(db.Vehicles.OrderBy(e => e.Position).ToList());
                else
                if (MessageBoxResult.Yes == MessageBox.Show("Sie haben noch keine Daten in der Datenbank. Möchten Sie jetzt welche importieren?", "Datenbank importieren", MessageBoxButton.YesNo, MessageBoxImage.Question))
                {
                    new Importer.ImportSelecter(db).Show();
                    this.Close();
                }
                //switch (Settings.CentralStation)
                //{
                //    case CentralStationType.None:
                //        Controler = null!;
                //        break;
                //    case CentralStationType.Z21:
                Controller = new Z21(Settings.ControllerIP, Settings.ControllerPort);
                Controller.LogOn();
                //        break;
                //    case CentralStationType.ECoS:
                //        //Controler = new Z21(new StartData() { LanAdresse = Settings.ControllerIP.ToString(), LanPort = Settings.ControllerPort });
                //        break;
                //}
                RemoveUnneededImages();
            }
            catch (System.Net.Sockets.SocketException)
            {
                Close();
                MessageBox.Show("Achtung mehr als eine Instanz der Software kann nicht geöffnet werden!");
            }
            catch (Exception e)
            {
                Close();
                Logger.Log($"Fehler beim start", true, e);
                MessageBox.Show($"Es ist ein Fehler beim öffnen der Applikation aufgetreten.\nException Message: '{e?.Message ?? ""}' \nIm Ordner '{Directory.GetCurrentDirectory() + @"\Log" + @"\Error"}' finden Sie ein Logfile. Bitte an jakob.eichberger@gmx.net als Zipfile schicken. (Dann kann ich's fixen ;) ) ");
            }
        }

        private void DB_Import_new(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageBox.Show("Sind Sie sicher dass Sie eine neue Datenbank importieren wollen? Wenn Sie Ja drücken kann die aktuelle Datenbank nicht mehr verwendet.", "Frage", MessageBoxButton.YesNo, MessageBoxImage.Question))
            {
                VehicleGrid.Children.Clear();
                db.Database.EnsureDeleted();
                var x = new Importer.ImportSelecter(db);
                this.Close();
                x.Show();
            }
        }

        public void DrawAllVehicles(IEnumerable<Vehicle> list)
        {
            VehicleGrid.Children.Clear();
            foreach (var item in list)
            {
                if (item is null) continue;
                Border border = new();
                border.Padding = new(2);
                border.Margin = new(10);
                border.BorderThickness = new(1);
                border.BorderBrush = Brushes.Black;

                StackPanel sp = new();
                try
                {
                    string path = $"{Directory.GetCurrentDirectory()}\\Data\\VehicleImage\\{item?.Image_Name}";
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
                catch { }

                TextBlock tb = new();
                tb.Text = !string.IsNullOrWhiteSpace(item?.Full_Name) ? item?.Full_Name : (!string.IsNullOrWhiteSpace(item?.Name) ? item?.Name : $"Adresse: {item?.Address}");
                sp.Height = 120;
                sp.Width = 250;
                sp.Children.Add(tb);
                sp.HorizontalAlignment = HorizontalAlignment.Left;
                sp.VerticalAlignment = VerticalAlignment.Top;
                sp.Background = Brushes.White;

                sp.ContextMenu = new();
                MenuItem miControlLoko = new();
                miControlLoko.Header = item?.Type == VehicleType.Lokomotive ? "Lok steuern" : (item?.Type == VehicleType.Steuerwagen ? "Steuerwagen steuern" : "Wagen steuern");
                miControlLoko.Click += ControlLoko_Click;
                miControlLoko.Tag = item;
                sp.ContextMenu.Items.Add(miControlLoko);
                MenuItem miEditLoko = new();
                miEditLoko.Header = item?.Type == VehicleType.Lokomotive ? "Lok bearbeiten" : (item?.Type == VehicleType.Steuerwagen ? "Steuerwagen bearbeiten" : "Wagen bearbeiten");
                miEditLoko.Tag = item;
                miEditLoko.Click += EditLoko_Click;
                sp.ContextMenu.Items.Add(miEditLoko);
                border.Child = sp;
                VehicleGrid.Children.Add(border);
            }
        }

        public void RemoveUnneededImages()
        {
            Task.Run(() =>
            {
                try
                {
                    List<string> images = db.Vehicles.Select(e => e.Image_Name).ToList();
                    foreach (var item in Directory.GetFiles($"{Directory.GetCurrentDirectory()}\\Data\\VehicleImage\\"))
                    {
                        if (!images.Where(e => e == Path.GetFileName(item)).Any())
                            File.Delete(item);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"Deleting file failed", true, ex);
                }

            });
        }

        void ControlLoko_Click(Object sender, RoutedEventArgs e)
        {
            try
            {
                var menu = ((MenuItem)e.Source);
                Vehicle? vehicle = (menu.Tag as Vehicle);
                if (vehicle is not null && Controller is not null)
                    new TrainControl(Controller, vehicle, db).Show();
                else
                    MessageBox.Show("Öffnen nicht möglich da Vehicle null ist!", "Error");
            }
            catch (Exception ex)
            {
                Logger.Log($"-", true, ex);
                MessageBox.Show($"Beim öffnen ist ein unerwarteter Fehler aufgetreten! Fehlermeldung: {ex?.Message}", "Error beim öffnen");
            }
        }

        void EditLoko_Click(Object sender, RoutedEventArgs e)
        {
            try
            {
                var menu = ((MenuItem)e.Source);
                Vehicle? vehicle = (menu.Tag as Vehicle);
                if (vehicle is not null)
                {
                    EditVehicleWindow evw = new(db, vehicle);
                    if (evw.ShowDialog() ?? false)
                    {
                        Search();
                        RemoveUnneededImages();
                    }
                }
                else
                    MessageBox.Show("Öffnen nicht möglich da Vehicle null ist!", "Error");
            }
            catch (Exception ex)
            {
                Logger.Log($"-", true, ex);
                MessageBox.Show($"Beim öffnen ist ein unerwarteter Fehler aufgetreten! Fehlermeldung: {ex?.Message}", "Error beim öffnen");
            }
        }

        private void Search()
        {
            if (!string.IsNullOrWhiteSpace(tbSearch.Text))
                DrawAllVehicles(db.Vehicles.Include(e => e.Category).Where(i => (i.Address + i.Article_Number + i.Category.Name + i.Owner + i.Railway + i.Description + i.Full_Name + i.Name + i.Type).ToLower().Contains(tbSearch.Text.ToLower())).OrderBy(e => e.Position));
            else
                DrawAllVehicles(db.Vehicles.Include(e => e.Category).OrderBy(e => e.Position));
        }

        private void TbSearch_TextChanged(object sender, TextChangedEventArgs e) => Search();

        protected void OnPropertyChanged([CallerMemberName] string name = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void Mw_Closing(object sender, CancelEventArgs e)
        {
            if (Controller is not null)
            {
                Controller.LogOFF();
                Controller = null;
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e) => new SettingsWindow().Show();

        private void NewVehicle_Click(object sender, RoutedEventArgs e)
        {
            EditVehicleWindow w = new(db);
            if (w.ShowDialog() ?? false)
            {
                Search();
                RemoveUnneededImages();
            }
        }

        private void MeasureLoko_Click(object sender, RoutedEventArgs e)
        {
            new Einmessen(db, Controller).Show();
        }
    }
}
