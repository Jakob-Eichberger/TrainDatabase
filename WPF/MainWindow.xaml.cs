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
        private static readonly Mutex mutex = new(true, "{8F6F0AC4-B9A1-45fd-A8CF-72F04E6BDE8F}");

        private readonly Database db = new();

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
                    Application.Current.Shutdown();
                    return;
                }
#endif
                this.DataContext = this;
                InitializeComponent();

                if (Settings.OpenDebugConsoleOnStart)
                    ShowConsoleWindow();

                DrawVehiclesIfAnyExist();
                CreatController();
                RemoveUnneededImages();
                db.CollectionChanged += Db_CollectionChanged;
            }
            catch (Exception e)
            {
                Close();
                Logger.Log($"Fehler beim start", e);
                MessageBox.Show($"Fehler beim Start!.\n Error: '{e?.Message ?? ""}' \nIm Ordner ...\\Log\\ finden Sie ein Logfile. Bitte an contact@jakob-eichberger.at schicken.");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        ModelTrainController.CentralStationClient? Controller { get; set; }

        public void DrawAllVehicles(IEnumerable<Vehicle> list)
        {
            VehicleGrid.Children.Clear();
            foreach (var item in list.OrderBy(e => e.Position))
            {
                if (item is null) continue;
                Border border = new()
                {
                    Padding = new(2),
                    Margin = new(10),
                    BorderThickness = new(1),
                    BorderBrush = Brushes.Black,
                    Tag = item
                };
                border.MouseDown += Border_MouseDown;

                StackPanel sp = new()
                {
                    Height = 120,
                    Width = 250,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Background = Brushes.White,
                    ContextMenu = new(),
                    Tag = item
                };

                var path = $"{Directory.GetCurrentDirectory()}\\Data\\VehicleImage\\{item?.ImageName}";
                if (File.Exists(path))
                {
                    sp.Children.Add(new Image()
                    {
                        Source = LoadPhoto(path),
                        Width = 250,
                        Height = 100,
                        Tag = item
                    });
                }

                TextBlock tb = new() { Tag = item };
                tb.Text = !string.IsNullOrWhiteSpace(item?.FullName) ? item?.FullName : (!string.IsNullOrWhiteSpace(item?.Name) ? item?.Name : $"Adresse: {item?.Address}");
                sp.Children.Add(tb);
                sp.ContextMenu.Items.Add(GetControlVehicleMenuItem(item));
                border.Child = sp;

                VehicleGrid.Children.Add(border);
            }
        }

        public void RemoveUnneededImages() => Task.Run(() =>
        {
            try
            {
                var images = db.Vehicles.Select(e => e.ImageName).ToList();
                string directory = $"{Directory.GetCurrentDirectory()}\\Data\\VehicleImage";
                Directory.CreateDirectory(directory);
                foreach (var item in Directory.GetFiles($"{directory}\\"))
                    if (!images.Any(e => e == Path.GetFileName(item)))
                        File.Delete(item);
            }
            catch { }
        });

        protected void OnPropertyChanged([CallerMemberName] string name = null!) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private static BitmapImage LoadPhoto(string path)
        {
            BitmapImage bmi = new();
            bmi.BeginInit();
            bmi.CacheOption = BitmapCacheOption.OnLoad;
            bmi.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bmi.UriSource = new(path);
            bmi.EndInit();
            return bmi;
        }

        private async void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && sender.GetType()?.GetProperty("Tag")?.GetValue(sender, null) is Vehicle vehicle)
            {
                await Task.Delay(100);
                OpenNewTrainControlWindow(vehicle);
            }
        }

        void ControlLoko_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var menu = e.Source as MenuItem;
                var vehicle = menu?.Tag as Vehicle;
                if (vehicle is not null && Controller is not null)
                    OpenNewTrainControlWindow(vehicle);
                else
                    MessageBox.Show("Öffnen nicht möglich da Vehicle null ist!", "Error");
            }
            catch (Exception ex)
            {
                Logger.Log($"Beim öffnen ist ein unerwarteter Fehler aufgetreten", ex);
                MessageBox.Show($"Beim öffnen ist ein unerwarteter Fehler aufgetreten! Fehlermeldung: {ex?.Message}", "Error beim öffnen");
            }
        }

        private void CreatController()
        {
            Controller = new Z21(Settings.ControllerIP, Settings.ControllerPort);
            Controller.LogOn();
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Log("", e.Exception);
            e.Handled = true;
            MessageBox.Show("Es ist ein unerwarteter Fehler aufgetreten!");
        }

        private void Db_CollectionChanged(object? sender, EventArgs e) => Dispatcher.Invoke(() => Search());

        private void DB_Import_new(object sender, RoutedEventArgs e) => new Importer.Z21Import(db).ShowDialog();

        private void DrawVehiclesIfAnyExist()
        {
            db.Database.EnsureCreated();
            if (db.Vehicles.Any())
                Search();
            else
            {
                if (MessageBoxResult.Yes == MessageBox.Show("Sie haben noch keine Daten in der Datenbank. Möchten Sie jetzt welche importieren?", "Datenbank importieren", MessageBoxButton.YesNo, MessageBoxImage.Question))
                {
                    var importer = new Importer.Z21Import(db);
                    importer.ShowDialog();
                    Search();
                }
            }
        }

        private MenuItem GetControlVehicleMenuItem(Vehicle item)
        {
            MenuItem miControlLoko = new();
            miControlLoko.Header = item.Type switch
            {
                VehicleType.Lokomotive => "Lok steuern",
                VehicleType.Steuerwagen => "Steuerwagen steuern",
                VehicleType.Wagen => "Wagen steuern",
                _ => "Fahrzeug steuern",
            };
            miControlLoko.Click += ControlLoko_Click;
            miControlLoko.Tag = item;
            return miControlLoko;
        }

        private void MeasureLoko_Click(object sender, RoutedEventArgs e) => new Einmessen(db, Controller).Show();

        private void Mw_Closing(object sender, CancelEventArgs e)
        {
            if (Controller is not null)
                Controller.LogOFF();
            Application.Current.Shutdown();
        }

        private void OpenNewTrainControlWindow(Vehicle? vehicle)
        {
            if (Application.Current.Windows.OfType<TrainControl>().FirstOrDefault(e => e.Vehicle.Id == vehicle?.Id) is TrainControl trainControl)
            {
                trainControl.Activate();
                trainControl.RefreshSource();
            }
            else
                new TrainControl(Controller, vehicle, db).Show();
        }

        private void OpenVehicleManagement_Click(object sender, RoutedEventArgs e) => new VehicleManagement(db).Show();

        private void Search() => DrawAllVehicles(db.Vehicles.Include(e => e.Category).ToList().Where(i => i.IsActive && ($"{i.Address} {i.ArticleNumber} {i.Category?.Name} {i.Owner} {i.Railway} {i.FullName} {i.Name} {i.Type}").ToLower().Contains(tbSearch.Text.ToLower().Trim())).OrderBy(e => e.Position));

        private void Settings_Click(object sender, RoutedEventArgs e) => new SettingsWindow().Show();

        private void TbSearch_TextChanged(object sender, TextChangedEventArgs e) => Search();

        #region Console
        const int SwHide = 0;

        const int SwShow = 5;

        public static void ShowConsoleWindow()
        {
            var handle = GetConsoleWindow();

            if (handle == IntPtr.Zero)
                AllocConsole();
            else
                ShowWindow(handle, SwShow);
        }

        [DllImport(@"kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport(@"kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport(@"user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        #endregion
    }
}
