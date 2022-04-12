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
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TrainDatabase;
using TrainDatabase.Z21Client;
using WPF_Application;

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
                ResizeTimer.Elapsed += ResizeTimer_Elapsed;
#if RELEASE
                if (MessageBoxResult.No == MessageBox.Show("Achtung! Es handelt sich bei der Software um eine Alpha version! Es können und werden Bugs auftreten, wenn Sie auf JA drücken, stimmen Sie zu, dass der Entwickler für keinerlei Schäden, die durch die Verwendung der Software entstehen könnten, haftbar ist!", "Haftungsausschluss", MessageBoxButton.YesNo, MessageBoxImage.Information))
                {
                    Application.Current.Shutdown();
                    return;
                }
#endif
                DataContext = this;
                InitializeComponent();

                if (Configuration.OpenDebugConsoleOnStart)
                    new LogWindow().Show();
                DrawVehiclesIfAnyExist();
                RemoveUnneededImages();
                db.CollectionChanged += (a, b) => Dispatcher.Invoke(() => Search());
                Client.LogOn();
            }
            catch (Exception e)
            {
                Close();
                Logger.LogError(e, $"Fehler beim start");
                MessageBox.Show($"{e}");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private Z21Client? Client { get; } = new Z21Client(Configuration.ClientIP, Configuration.ClientPort);

        private System.Timers.Timer ResizeTimer { get; } = new System.Timers.Timer() { Enabled = false, Interval = new TimeSpan(0, 0, 0, 1).TotalMilliseconds, AutoReset = false };

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
            if (e.ClickCount == 2 && sender is VehicleBorder border)
            {
                await Task.Delay(150);
                OpenNewTrainControlWindow(border.Vehicle);
            }
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.LogError(e.Exception, "An unhandled exception occoured.");
            e.Handled = true;
            MessageBox.Show("Es ist ein unerwarteter Fehler aufgetreten!");
        }

        private void DB_Import_new(object sender, RoutedEventArgs e) => new Importer.Z21Import(db).ShowDialog();

        private void DrawVehicles(IEnumerable<Vehicle> list)
        {
            VehicleGrid.Children.Clear();
            foreach (var item in list.OrderBy(e => e.Position))
            {
                StackPanel sp = new()
                {
                    Height = 120,
                    Width = 250,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Background = Brushes.White,
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

                sp.Children.Add(new TextBlock()
                {
                    Text = !string.IsNullOrWhiteSpace(item?.Name) ? item.Name : (!string.IsNullOrWhiteSpace(item?.FullName) ? item.FullName : $"Adresse: {item?.Address}")
                });

                VehicleBorder border = new()
                {
                    Padding = new(2),
                    Margin = new(10),
                    BorderThickness = new(1),
                    BorderBrush = Brushes.Black,
                    Vehicle = item,
                    Child = sp,
                    ContextMenu = new()
                };

                var mi = new VehicleMenuItem(item, "Fahrzeug steuern", (a) => OpenNewTrainControlWindow(a));
                border.ContextMenu.Items.Add(mi);

                border.MouseDown += Border_MouseDown;
                VehicleGrid.Children.Add(border);
            }
        }

        private void DrawVehiclesIfAnyExist()
        {
            db.Database.EnsureCreated();
            if (!db.Vehicles.Any() && MessageBoxResult.Yes == MessageBox.Show("Sie haben noch keine Daten in der Datenbank. Möchten Sie jetzt welche importieren?", "Datenbank importieren", MessageBoxButton.YesNo, MessageBoxImage.Question))
                new Importer.Z21Import(db).ShowDialog();
            Search();
        }

        private void MeasureLoko_Click(object sender, RoutedEventArgs e) => new Einmessen(db, Client).Show();

        private void Mw_Closing(object sender, CancelEventArgs e)
        {
            Client?.LogOFF();
            Application.Current.Shutdown();
        }

        private void mw_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (IsActive && !tbSearch.IsFocused)
                tbSearch.Focus();
        }

        private void mw_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!ResizeTimer.Enabled)
                ResizeTimer.Start();
            else
            {
                ResizeTimer.Stop();
                ResizeTimer.Start();
            }
        }

        private void OpenNewTrainControlWindow(Vehicle? vehicle)
        {
            if (Application.Current.Windows.OfType<TrainControl>().FirstOrDefault(e => e.Vehicle.Id == vehicle?.Id) is TrainControl trainControl)
            {
                trainControl.WindowState = WindowState.Normal;
                trainControl.Activate();
            }
            else
                new TrainControl(Client, vehicle, db).Show();
        }

        private void OpenVehicleManagement_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Windows.OfType<VehicleManagement>().FirstOrDefault() is VehicleManagement vehicleManagement)
            {
                vehicleManagement.WindowState = WindowState.Normal;
                vehicleManagement.Activate();
            }
            else
                new VehicleManagement(db).Show();
        }

        private void RemoveUnneededImages() => Task.Run(() =>
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

        private void ResizeTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) => Dispatcher.Invoke(() =>
        {
            //var menuHeight = RSearchbar.ActualHeight + RMenu.ActualHeight;
            //int hCount = (int)((Height - (menuHeight)) / 152);
            //Height = (hCount * 152) + menuHeight;

            int wCount = (int)((Width - 18 + 10) / 282);
            Width = (wCount * 282) + 18;
        });

        private void Search()
        {
            var vehicles = db.Vehicles.Include(e => e.Category).Where(e => e.IsActive).ToList();
            foreach (var item in tbSearch.Text.Split(" ", StringSplitOptions.RemoveEmptyEntries))
                vehicles = vehicles.Where(i => i.IsActive && $"{i.Name} {i.FullName} {i.Type} {i.Address} {i.Railway} {i.DecoderType} {i.Manufacturer} {i.ArticleNumber}".ToLower().Contains(item.ToLower().Trim())).ToList();
            DrawVehicles(vehicles);
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Windows.OfType<SettingsWindow>().FirstOrDefault() is SettingsWindow settings)
            {
                settings.WindowState = WindowState.Normal;
                settings.Activate();
            }
            else
                new SettingsWindow().Show();
        }

        private void TbSearch_TextChanged(object sender, TextChangedEventArgs e) => Search();
    }
}
