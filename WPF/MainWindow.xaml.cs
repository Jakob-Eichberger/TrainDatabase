using Helper;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Model;
using Service;
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
using WPF_Application;
using Z21;

namespace Wpf_Application
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private static readonly Mutex mutex = new(true, "{8F6F0AC4-B9A1-45fd-A8CF-72F04E6BDE8F}");

        public MainWindow(IServiceProvider serviceProvider)
        {
            try
            {
                DataContext = this;
                InitializeComponent();

                ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
                Db = ServiceProvider.GetService<Database>()!;
                Client = ServiceProvider.GetService<Client>()!;
                LogService = ServiceProvider.GetService<LogService>()!;

                if (!mutex.WaitOne(TimeSpan.Zero, true))
                {
                    MessageBox.Show("Achtung mehr als eine Instanz der Software kann nicht geöffnet werden!");
                    Application.Current.Shutdown();
                    return;
                }
                Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
                ResizeTimer.Elapsed += ResizeTimer_Elapsed;
            }
            catch (Exception e)
            {
                Close();
                LogService.Log(Microsoft.Extensions.Logging.LogLevel.Error, e);
                MessageBox.Show($"{e}");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public IServiceProvider ServiceProvider { get; } = default!;

        private Client Client { get; } = default!;

        private LogService LogService { get; } = default!;

        private Database Db { get; } = default!;

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
                TrainControl.CreatTrainControlWindow(ServiceProvider, border.Vehicle);
            }
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogService.Log(Microsoft.Extensions.Logging.LogLevel.Error, e.Exception);
            e.Handled = true;
            MessageBox.Show("Es ist ein unerwarteter Fehler aufgetreten!");
        }

        private void MiImportNewDatabase(object sender, RoutedEventArgs e) => new Importer.Z21Import(ServiceProvider).ShowDialog();

        private void DrawVehicles(IEnumerable<VehicleModel> list)
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

                var path = $"{Configuration.VehicleImagesFileLocation}\\{item?.ImageName}";
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

                var mi = new VehicleMenuItem(item, "Fahrzeug steuern", (a) => TrainControl.CreatTrainControlWindow(ServiceProvider, a));
                border.ContextMenu.Items.Add(mi);

                border.MouseDown += Border_MouseDown;
                VehicleGrid.Children.Add(border);
            }
        }

        private void DrawVehiclesIfAnyExist()
        {
            if (!Db.Vehicles.Any() && MessageBoxResult.Yes == MessageBox.Show("Sie haben noch keine Daten in der Datenbank. Möchten Sie jetzt welche importieren?", "Datenbank importieren", MessageBoxButton.YesNo, MessageBoxImage.Question))
                new Importer.Z21Import(ServiceProvider).ShowDialog();
            Search();
        }

        private void MeasureLoko_Click(object sender, RoutedEventArgs e) => new Einmessen(ServiceProvider).Show();

        private void Mw_Closing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Mw_Loaded(object sender, RoutedEventArgs e)
        {
#if RELEASE
                if (MessageBoxResult.No == MessageBox.Show("Achtung! Es handelt sich bei der Software um eine Alpha version! Es können und werden Bugs auftreten, wenn Sie auf JA drücken, stimmen Sie zu, dass der Entwickler für keinerlei Schäden, die durch die Verwendung der Software entstehen könnten, haftbar ist!", "Haftungsausschluss", MessageBoxButton.YesNo, MessageBoxImage.Information))
                {
                    Application.Current.Shutdown();
                    return;
                }
#endif
            DrawVehiclesIfAnyExist();
            RemoveUnneededImages();
            Db.CollectionChanged += (a, b) => Dispatcher.Invoke(() => Search());
        }

        private void Mw_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (IsActive && !tbSearch.IsFocused)
                tbSearch.Focus();
        }

        private void Mw_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!ResizeTimer.Enabled)
                ResizeTimer.Start();
            else
            {
                ResizeTimer.Stop();
                ResizeTimer.Start();
            }
        }

        private void OpenVehicleManagement_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Windows.OfType<VehicleManagement>().FirstOrDefault() is VehicleManagement vehicleManagement)
            {
                vehicleManagement.WindowState = WindowState.Normal;
                vehicleManagement.Activate();
            }
            else
                new VehicleManagement(ServiceProvider).Show();
        }

        private void RemoveUnneededImages() => Task.Run(() =>
        {
            try
            {
                var images = Db.Vehicles.Select(e => e.ImageName).ToList();
                string directory = Configuration.VehicleImagesFileLocation;
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
            var vehicles = Db.Vehicles.Where(e => e.IsActive).ToList();
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

        private void MiDeleteDatabase(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageBox.Show("Sicher dass die Datenbank gelöscht werden soll?", "Datenbank löschen", MessageBoxButton.YesNo, MessageBoxImage.Warning))
            {
                Db.Clear();
            }
        }
    }
}
