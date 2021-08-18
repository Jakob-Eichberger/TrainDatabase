using Helper;
using Microsoft.EntityFrameworkCore;
using Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WPF_Application;
using WPF_Application.CentralStation.Z21;
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
        static readonly Mutex mutex = new(true, "{8F6F0AC4-B9A1-45fd-A8CF-72F04E6BDE8F}");
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
                if (!mutex.WaitOne(TimeSpan.Zero, true))
                {
                    MessageBox.Show("Achtung mehr als eine Instanz der Software kann nicht geöffnet werden!");
                    Application.Current.Shutdown();
                    return;
                }
                Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
#if RELEASE
                if (MessageBoxResult.No == MessageBox.Show("Achtung! Es handelt sich bei der Software um eine Alpha version! Es können und werden Bugs auftreten, wenn Sie auf JA drücken, stimmen Sie zu, dass der Entwickler für keinerlei Schäden, die durch die Verwendung der Software entstehen könnten, haftbar ist!", "Haftungsausschluss", MessageBoxButton.YesNo, MessageBoxImage.Information))
                {
                    Close();
                }
#endif
                Theme = new();
                this.DataContext = this;
                InitializeComponent();

                if (Debugger.IsAttached)
                    ShowConsoleWindow();
                CheckIfDbHasItemsDrawVehicle();
                CreatController();
                RemoveUnneededImages();
            }
            catch (Exception e)
            {
                Close();
                Logger.Log($"Fehler beim start", e, Environment.StackTrace);
                MessageBox.Show($"Fehler beim Start!.\n Error: '{e?.Message ?? ""}' \nIm Ordner ...\\Log\\ finden Sie ein Logfile. Bitte an contact@jakob-eichberger.at schicken.");
            }
        }

        private void CreatController()
        {
            Controller = new Z21(Settings.ControllerIP, Settings.ControllerPort);
            Controller.LogOn();
        }

        private void CheckIfDbHasItemsDrawVehicle()
        {
            db.Database.EnsureCreated();
            if (db.Vehicles.Any())
            {
                DrawAllVehicles(db.Vehicles.OrderBy(e => e.Position).ToList());
            }
            else
            {
                if (MessageBoxResult.Yes == MessageBox.Show("Sie haben noch keine Daten in der Datenbank. Möchten Sie jetzt welche importieren?", "Datenbank importieren", MessageBoxButton.YesNo, MessageBoxImage.Question))
                {
                    new Importer.Z21(db).Show();
                }
            }
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Log("", e.Exception, Environment.StackTrace);
            e.Handled = true;
            MessageBox.Show("Es ist ein unerwarteter Fehler aufgetreten!");
        }

        private void DB_Import_new(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageBox.Show("Sind Sie sicher dass Sie eine neue Datenbank importieren wollen? Wenn Sie Ja drücken kann die aktuelle Datenbank nicht mehr verwendet werden.", "Frage", MessageBoxButton.YesNo, MessageBoxImage.Question))
            {
                VehicleGrid.Children.Clear();
                db.Database.EnsureDeleted();
                var importer = new Importer.Z21(db);
                this.Close();
                importer.Show();
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
                    Logger.Log($"Deleting file failed", ex, Environment.StackTrace);
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
                Logger.Log($"-", ex, Environment.StackTrace);
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
                Logger.Log($"-", ex, Environment.StackTrace);
                MessageBox.Show($"Beim öffnen ist ein unerwarteter Fehler aufgetreten! Fehlermeldung: {ex?.Message}", "Error beim öffnen");
            }
        }

        private void Search()
        {
            if (!string.IsNullOrWhiteSpace(tbSearch.Text))
                DrawAllVehicles(db.Vehicles.Include(e => e.Category).ToList().Where(i => (i.Address + i.Article_Number + i.Category?.Name + i.Owner + i.Railway + i.Description + i.Full_Name + i.Name + i.Type).ToLower().Contains(tbSearch.Text.ToLower())).OrderBy(e => e.Position));
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
            Application.Current.Shutdown();
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

        #region Console
        [DllImport(@"kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport(@"kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport(@"user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SwHide = 0;
        const int SwShow = 5;

        public static void ShowConsoleWindow()
        {
            var handle = GetConsoleWindow();

            if (handle == IntPtr.Zero)
            {
                AllocConsole();
            }
            else
            {
                ShowWindow(handle, SwShow);
            }
        }
        #endregion
    }
}
