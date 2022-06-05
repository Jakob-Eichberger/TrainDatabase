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
using TrainDatabase.Z21Client.DTO;
using TrainDatabase.Z21Client.Enums;
using TrainDatabase.Z21Client.Events;

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
                Z21Client = ServiceProvider.GetService<Z21Client.Z21Client>()!;

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
                Logger.LogError(ex, "Fehler beim öffnen des Controllers.");
                MessageBox.Show($"Beim öffnen des Controllers ist ein Fehler aufgetreten: {(string.IsNullOrWhiteSpace(ex?.Message) ? "" : ex.Message)}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public Database Db { get; } = default!;

        public int MaxDccSpeed => TrainDatabase.Z21Client.Z21Client.maxDccStep;

        public IServiceProvider ServiceProvider { get; } = default!;

        public TrackPowerService TrackPowerService { get; } = default!;

        /// <summary>
        /// The <see cref="Vehicle"/> the application is trying to controll
        /// </summary>
        public VehicleModel Vehicle { get; private set; } = default!;

        public VehicleService VehicleService { get; } = default!;

        public GridLength VehicleTypeGridLength => (Vehicle?.Type ?? VehicleType.Lokomotive) == VehicleType.Lokomotive ? new GridLength(80) : new GridLength(0);

        public Visibility VehicleTypeVisbility => (Vehicle?.Type ?? VehicleType.Lokomotive) == VehicleType.Lokomotive ? Visibility.Visible : Visibility.Collapsed;

        public Viewmodel.Vehicle VehicleViewmodel { get; private set; } = default!;

        public Z21Client.Z21Client Z21Client { get; } = default!;

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

        private async void BtnDirection_Click(object sender, RoutedEventArgs e) => await VehicleViewmodel.SwitchDirection();

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

        private void DrawAllVehicles(IEnumerable<VehicleModel> vehicles)
        {
            var tractionVehicles = vehicles.Where(f => Vehicle.TractionVehicleIds.Any(e => e == f.Id)).OrderBy(e => e.Position).ToList();
            TIMultiTraction.Header = $"Mehrfachtraktion ({tractionVehicles.Count})";

            if (tractionVehicles.Any())
            {
                AddListToStackPanel(tractionVehicles);
                SPVehilces.Children.Add(new Separator());
            }
            AddListToStackPanel(vehicles.Where(f => !Vehicle.TractionVehicleIds.Any(e => e == f.Id)).OrderBy(e => e.Position));

            void AddListToStackPanel(IEnumerable<VehicleModel> vehicles)
            {
                foreach (var vehicle in vehicles.Where(e => e.Id != Vehicle.Id))
                {
                    CheckBox c = new()
                    {
                        Content = vehicle.Name,
                        Tag = vehicle,
                        Margin = new Thickness(5),
                        IsChecked = Vehicle.TractionVehicleIds.Any(e => e == vehicle.Id)
                    };
                    c.Unchecked += (sender, e) =>
                    {
                        if ((sender as CheckBox)?.Tag is VehicleModel vehicle)
                        {
                            VehicleService.RemoveTractionVehilce(vehicle, Vehicle);
                        }
                    };
                    c.Checked += (sender, e) =>
                    {
                        if (sender is CheckBox c && c.Tag is VehicleModel v)
                        {
                            VehicleService.AddTractionVehilce(v, Vehicle);
                        }
                    };
                    SPVehilces.Children.Add(c);
                }
            }
        }

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e) => SearchTractionVehicles();

        private void SearchTractionVehicles()
        {
            SPVehilces.Children.Clear();
            if (!string.IsNullOrWhiteSpace(tbSearch.Text))
            {
                DrawAllVehicles(Db.Vehicles.Where(i => (i.Address + i.ArticleNumber + i.Owner + i.Railway + i.Description + i.FullName + i.Name + i.Type).ToLower().Contains(tbSearch.Text.ToLower())).OrderBy(e => e.Position));
            }
            else
            {
                DrawAllVehicles(Db.Vehicles.OrderBy(e => e.Position));
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
            SearchTractionVehicles();

            Db.ChangeTracker.StateChanged += (a, b) =>
            {
                SetTitle();
                DrawAllFunctions();
                SearchTractionVehicles();
            };

            Z21Client.GetLocoInfo(new LokAdresse(Vehicle.Address));
        }
    }
}
