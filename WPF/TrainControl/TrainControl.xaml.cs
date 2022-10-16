using Extensions;
using Helper;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Model;
using OxyPlot.Series;
using Service;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using TrainDatabase.Extensions;
using TrainDatabase.JoyStick;
using WPF_Application.TrainControl;
using Z21.Model;

namespace TrainDatabase
{
    /// <summary>
    /// Interaction logic for VehicleController.xaml
    /// </summary>
    public partial class TrainControl : Window, INotifyPropertyChanged
    {
        public TrainControl(IServiceProvider serviceProvider, VehicleModel _vehicle)
        {
            try
            {
                ServiceProvider = serviceProvider;
                Db = ServiceProvider.GetService<Database>()!;
                VehicleService = ServiceProvider.GetService<VehicleService>()!;
                LogService = ServiceProvider.GetService<LogEventBus>()!;
                Z21Client = ServiceProvider.GetService<Z21.Client>()!;

                TrackPowerService = ServiceProvider.GetService<TrackPowerService>()!;
                TrackPowerService.StateChanged += (a, b) => Dispatcher.Invoke(() => OnPropertyChanged());

                Vehicle = Db.Vehicles.Include(e => e.Functions).ToList().FirstOrDefault(e => e.Id == _vehicle.Id)!;

                VehicleViewmodel = new(serviceProvider, Vehicle);
                VehicleViewmodel.StateChanged += (a, b) => Dispatcher.Invoke(() => OnPropertyChanged());

                DataContext = this;
                InitializeComponent();
                Activate();
            }
            catch (Exception ex)
            {
                Close();
                LogService.Log(Microsoft.Extensions.Logging.LogLevel.Error, ex);
                MessageBox.Show($"Beim öffnen des Controllers ist ein Fehler aufgetreten: {(string.IsNullOrWhiteSpace(ex?.Message) ? "" : ex.Message)}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public Database Db { get; } = default!;

        public int MaxDccSpeed => Z21.Client.maxDccStep;

        public IServiceProvider ServiceProvider { get; } = default!;

        public TrackPowerService TrackPowerService { get; } = default!;

        /// <summary>
        /// The <see cref="Vehicle"/> the application is trying to controll
        /// </summary>
        public VehicleModel Vehicle { get; private set; } = default!;

        public VehicleService VehicleService { get; } = default!;
        public LogEventBus LogService { get; private set; }

        public GridLength VehicleTypeGridLength => (Vehicle?.Type ?? VehicleType.Lokomotive) == VehicleType.Lokomotive ? new GridLength(80) : new GridLength(0);

        public Visibility VehicleTypeVisbility => (Vehicle?.Type ?? VehicleType.Lokomotive) == VehicleType.Lokomotive ? Visibility.Visible : Visibility.Collapsed;

        public Viewmodel.Vehicle VehicleViewmodel { get; private set; } = default!;

        public Z21.Client Z21Client { get; } = default!;

        /// <summary>
        /// Creates a single instance of the <see cref="TrainControl"/> window.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="client"></param>
        /// <param name="db"></param>
        public static void CreatTrainControlWindow(IServiceProvider serviceProvider, VehicleModel vehicle)
        {
            if (Application.Current.Windows.OfType<TrainControl>().FirstOrDefault(e => e.Vehicle.Id == vehicle?.Id) is TrainControl trainControl)
            {
                trainControl.WindowState = WindowState.Normal;
                trainControl.Activate();
            }
            else
                new TrainControl(serviceProvider, vehicle).Show();
        }

        protected void OnPropertyChanged(string propertyName = null!) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void BtnDirection_Click(object sender, RoutedEventArgs e) => VehicleViewmodel.SwitchDirection();

        /// <summary>
        /// Functions draws every single Function of a vehicle for the user to click on. 
        /// </summary>
        private void DrawAllFunctions()
        {
            FunctionGrid.Children.Clear();
            foreach (var item in Vehicle.Functions.OrderBy(e => e.Address))
            {
                switch (item.ButtonType)
                {
                    case ButtonType.Switch:
                        FunctionGrid.Children.Add(new WPF_Application.TrainControl.FunctionButton.SwitchButton(ServiceProvider, item));
                        break;
                    case ButtonType.PushButton:
                        FunctionGrid.Children.Add(new WPF_Application.TrainControl.FunctionButton.PushButton(ServiceProvider, item));
                        break;
                    case ButtonType.Timer:
                        FunctionGrid.Children.Add(new WPF_Application.TrainControl.FunctionButton.TimerButton(ServiceProvider, item));
                        break;
                }
            }
        }

        private void SetTitle() => Title = $"{Vehicle.Address} - {(string.IsNullOrWhiteSpace(Vehicle.Name) ? Vehicle.FullName : Vehicle.Name)}";

        private void TBRailPower_Click(object sender, RoutedEventArgs e) => TrackPowerService.SetTrackPower(!TrackPowerService.TrackPowerOn);

        private void Tc_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter || e.Key == System.Windows.Input.Key.Space)
                e.Handled = true;
        }

        private void Tc_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            VehicleViewmodel.Speed = e.Delta < 0 ? VehicleViewmodel.Speed - 1 : VehicleViewmodel.Speed + 1;
            e.Handled = true;
            VehicleViewmodel.LastUserInteraction = DateTime.Now;
        }

        private void TrainControl_Loaded(object sender, RoutedEventArgs e)
        {
            Z21Client.ClientReachabilityChanged += (a, b) => Dispatcher.Invoke(() => { IsEnabled = Z21Client.ClientReachable; Z21Client.GetLocoInfo(new LokAdresse(Vehicle.Address)); });

            Z21Client.GetStatus();
            IsEnabled = Z21Client.ClientReachable;

            SetTitle();
            DrawAllFunctions();
            UpdateTiMultiTractionHeader();
            TIMultiTraction.Content = new MultitractionSelectorControl(ServiceProvider, Vehicle);

            Db.ChangeTracker.StateChanged += (a, b) =>
            {
                SetTitle();
                DrawAllFunctions();
                UpdateTiMultiTractionHeader();
            };

            Z21Client.GetLocoInfo(new LokAdresse(Vehicle.Address));
        }

        private void UpdateTiMultiTractionHeader() => TIMultiTraction.Header = $"Mehrfachtraktion ({Vehicle.TractionVehicleIds.Count})";
    }
}
