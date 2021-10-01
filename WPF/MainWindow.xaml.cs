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
                    Application.Current.Shutdown();
                    return;
                }
#endif
                Theme = new();
                this.DataContext = this;
                InitializeComponent();

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

        private void Db_CollectionChanged(object? sender, EventArgs e) => Dispatcher.Invoke(() => Search());

        private void CreatController()
        {
            Controller = new Z21(Settings.ControllerIP);
            Controller.LogOn();
        }

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

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Log("", e.Exception);
            e.Handled = true;
            MessageBox.Show("Es ist ein unerwarteter Fehler aufgetreten!");
        }

        private void DB_Import_new(object sender, RoutedEventArgs e)
        {
            new Importer.Z21Import(db).ShowDialog();
        }

        public void DrawAllVehicles(IEnumerable<Vehicle> list)
        {
            VehicleGrid.Children.Clear();
            foreach (var item in list)
            {
                if (item is null) continue;
                Border border = new()
                {
                    Padding = new(2),
                    Margin = new(10),
                    BorderThickness = new(1),
                    BorderBrush = Brushes.Black
                };

                StackPanel sp = new()
                {
                    Height = 120,
                    Width = 250,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Background = Brushes.White,
                    ContextMenu = new()
                };

                try
                {
                    sp.Children.Add(new Image()
                    {
                        Source = LoadPhoto($"{Directory.GetCurrentDirectory()}\\Data\\VehicleImage\\{item?.ImageName}"),
                        Width = 250,
                        Height = 100
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Loading image '{item?.ImageName}' failed!:\n{ex}");
                }

                TextBlock tb = new();
                tb.Text = !string.IsNullOrWhiteSpace(item?.FullName) ? item?.FullName : (!string.IsNullOrWhiteSpace(item?.Name) ? item?.Name : $"Adresse: {item?.Address}");

                sp.Children.Add(tb);
                sp.ContextMenu.Items.Add(GetControlVehicleMenuItem(item));
                //sp.ContextMenu.Items.Add(GetEditVehicleMenuItem(item));
                border.Child = sp;
                VehicleGrid.Children.Add(border);
            }
        }

        private MenuItem GetControlVehicleMenuItem(Vehicle item)
        {
            MenuItem miControlLoko = new();
            switch (item.Type)
            {
                case VehicleType.Lokomotive:
                    miControlLoko.Header = "Lok steuern";
                    break;
                case VehicleType.Steuerwagen:
                    miControlLoko.Header = "Steuerwagen steuern";
                    break;
                case VehicleType.Wagen:
                    miControlLoko.Header = "Wagen steuern";
                    break;
                default:
                    miControlLoko.Header = "Fahrzeug steuern";
                    break;
            }
            miControlLoko.Click += ControlLoko_Click;
            miControlLoko.Tag = item;
            return miControlLoko;
        }

        private MenuItem GetEditVehicleMenuItem(Vehicle item)
        {
            MenuItem miEditLoko = new();
            switch (item.Type)
            {
                case VehicleType.Lokomotive:
                    miEditLoko.Header = "Lok bearbeiten";
                    break;
                case VehicleType.Steuerwagen:
                    miEditLoko.Header = "Steuerwagen bearbeiten";
                    break;
                case VehicleType.Wagen:
                    miEditLoko.Header = "Wagen bearbeiten";
                    break;
                default:
                    miEditLoko.Header = "Fahrzeug steuern";
                    break;
            }
            miEditLoko.Tag = item;
            miEditLoko.Click += EditLoko_Click;
            return miEditLoko;
        }

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

        public void RemoveUnneededImages()
        {
            Task.Run(() =>
            {
                try
                {
                    List<string> images = db.Vehicles.Select(e => e.ImageName).ToList();
                    string directory = $"{Directory.GetCurrentDirectory()}\\Data\\VehicleImage";
                    Directory.CreateDirectory(directory);
                    foreach (var item in Directory.GetFiles($"{directory}\\"))
                    {
                        if (!images.Where(e => e == Path.GetFileName(item)).Any())
                            File.Delete(item);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"Deleting file failed", ex);
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
                Logger.Log($"Beim öffnen ist ein unerwarteter Fehler aufgetreten", ex);
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
                    }
                }
                else
                    MessageBox.Show("Öffnen nicht möglich da Vehicle null ist!", "Error");
            }
            catch (Exception ex)
            {
                Logger.Log($"-", ex);
                MessageBox.Show($"Beim öffnen ist ein unerwarteter Fehler aufgetreten! Fehlermeldung: {ex?.Message}", "Error beim öffnen");
            }
        }

        private void Search() => DrawAllVehicles(db.Vehicles.Include(e => e.Category).ToList().Where(i => i.IsActive && ($"{i.Address} {i.ArticleNumber} {i.Category?.Name} {i.Owner} {i.Railway} {i.FullName} {i.Name} {i.Type}").ToLower().Contains(tbSearch.Text.ToLower().Trim())).OrderBy(e => e.Position));

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

        private void OpenVehicleManagement_Click(object sender, RoutedEventArgs e)
        {
            new VehicleManagement(db).Show();
        }
    }
}
