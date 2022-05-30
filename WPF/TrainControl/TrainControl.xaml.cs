﻿using Extensions;
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
    public partial class TrainControl : Window, INotifyPropertyChanged, IDisposable
    {
        public TrainControl(IServiceProvider serviceProvider, VehicleModel _vehicle)
        {
            try
            {
                ServiceProvider = serviceProvider;
                Db = ServiceProvider.GetService<Database>()!;
                VehicleService = ServiceProvider.GetService<VehicleService>()!;
                controller = ServiceProvider.GetService<Z21Client.Z21Client>()!;
                Vehicle = Db.Vehicles.Include(e => e.Functions).ToList().FirstOrDefault(e => e.Id == _vehicle.Id)!;
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

        public IServiceProvider ServiceProvider { get; } = default!;

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

        public void Dispose()
        {
            SetLocoDrive(inUse: false);
        }

        protected void OnPropertyChanged(string propertyName = null!) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private static LokInfoData GetLocoInfoData(int speedstep, bool direction, bool inUse, VehicleModel Vehicle) => new()
        {
            Adresse = new(Vehicle.Address),
            DrivingDirection = direction,
            InUse = inUse,
            Speed = (byte)speedstep
        };

        private static double GetSlowestVehicleSpeed(int speedstep, bool direction, MultiTractionItem traction)
        {
            if ((!traction.TractionForward.Any()) || (!traction.TractionForward.Any()))
                return double.NaN;

            if (!traction.Vehicle.InvertTraction)
                return direction ? traction.TractionForward.GetYValue(speedstep) : traction.TractionBackward.GetYValue(speedstep);
            else
                return direction ? traction.TractionBackward.GetYValue(speedstep) : traction.TractionForward.GetYValue(speedstep);
        }

        private static bool IsVehicleMeasured(MultiTractionItem item) => item.TractionForward.Any() && item.TractionBackward.Any();

        private void BtnDirection_Click(object sender, RoutedEventArgs e) => SwitchDirection();

        private void Controller_OnGetLocoInfo(object? sender, GetLocoInfoEventArgs e)
        {
            if (e.Data.Adresse.Value == Vehicle.Address)
            {
                LiveData = e.Data;
            }
        }

        private void Controller_OnStatusChanged(object? sender, StateEventArgs e) => TrackPower = e.TrackPower;

        private void Controller_TrackPowerChanged(object? sender, TrackPowerEventArgs e) => TrackPower = e.TrackPower;

        private async Task DeterminSlowestVehicleInList() => await Task.Run(() =>
        {
            var list = MultiTractionList.Where(e => e.Vehicle.Type == VehicleType.Lokomotive && e.TractionForward.Any() && e.TractionBackward.Any()).ToList();

            if (list.Any())
                SlowestVehicleInTractionList = list.Aggregate((cur, next) => (cur.TractionForward?.GetYValue(MaxDccSpeed) ?? int.MaxValue) < (next.TractionForward?.GetYValue(MaxDccSpeed) ?? int.MaxValue) ? cur : next).Vehicle ?? Vehicle;
            else
                SlowestVehicleInTractionList = Vehicle;
        });

        /// <summary>
        /// Functions draws every single Function of a vehicle for the user to click on. 
        /// </summary>
        private void DrawAllFunctions()
        {
            FunctionGrid.Children.Clear();
            foreach (var item in Vehicle.Functions.OrderBy(e => e.FunctionIndex))
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
                    c.Unchecked += async (sender, e) =>
                    {
                        if ((sender as CheckBox)?.Tag is VehicleModel vehicle)
                        {
                            VehicleService.RemoveTractionVehilce(vehicle, Vehicle);
                        }
                        await DeterminSlowestVehicleInList();
                    };
                    c.Checked += async (sender, e) =>
                    {
                        if (MultiTractionList.Count >= 140)
                        {
                            MessageBox.Show("Mehr als 140 Traktionsfahrzeuge sind nicht möglich!", "Max Limit reached.", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        if (sender is CheckBox c && c.Tag is VehicleModel v)
                        {
                            VehicleService.AddTractionVehilce(v, Vehicle);
                            await DeterminSlowestVehicleInList();
                        }
                    };
                    SPVehilces.Children.Add(c);
                }
            }
        }

        private bool GetDrivingDirection(VehicleModel vehicle, bool direction) => vehicle.Id != Vehicle.Id ? (vehicle.InvertTraction ? !direction : direction) : direction;

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

        /// <summary>
        /// Function used to Set the speed and direction of a Loco. 
        /// </summary>
        /// <param name="locoAdress"></param>
        /// <param name="speedstep"></param>
        /// <param name="drivingDirection"></param>
        /// <param name="inUse"></param>
        private async void SetLocoDrive(int? speedstep = null, bool? drivingDirection = null, bool inUse = true) => await Task.Run(() =>
        {
            if (speedstep is not null && speedstep != 0 && speedstep != Z21Client.Z21Client.maxDccStep && DateTime.Now - lastSpeedchange < new TimeSpan(0, 0, 0, 0, 500))
                return;
            else
                lastSpeedchange = DateTime.Now;

            bool direction = drivingDirection ??= LiveData.DrivingDirection;
            int speed = speedstep ?? Speed;
            List<LokInfoData> data = new();
            var slowestVehicle = MultiTractionList.FirstOrDefault(e => e.Vehicle.Equals(SlowestVehicleInTractionList));
            var yValue = GetSlowestVehicleSpeed(speed, direction, slowestVehicle);
            foreach (var item in MultiTractionList.Where(e => !e.Vehicle.Equals(SlowestVehicleInTractionList)))
            {
                if (IsVehicleMeasured(item))
                {
                    int dccSpeed;
                    if (!item.Vehicle.InvertTraction)
                        dccSpeed = direction ? item.TractionForward.GetXValue(yValue) : item.TractionBackward.GetXValue(yValue);
                    else
                        dccSpeed = direction ? item.TractionBackward.GetXValue(yValue) : item.TractionForward.GetXValue(yValue);

                    data.Add(GetLocoInfoData(dccSpeed, GetDrivingDirection(item.Vehicle, direction), inUse, item.Vehicle));
                }
                else
                    data.Add(GetLocoInfoData(speed, GetDrivingDirection(item.Vehicle, direction), inUse, item.Vehicle));
            }
            data.Add(GetLocoInfoData(speed, GetDrivingDirection(slowestVehicle.Vehicle, direction), inUse, slowestVehicle.Vehicle));
            controller.SetLocoDrive(data);
        });

        private void SliderSpeed_PreviewMouseDown(object sender, MouseButtonEventArgs e) => SliderInUser = true;

        private void SliderSpeed_PreviewMouseUp(object sender, MouseButtonEventArgs e) => SliderInUser = false;

        private void SwitchDirection() => SetLocoDrive(drivingDirection: !LiveData.DrivingDirection);

        private void TBRailPower_Click(object sender, RoutedEventArgs e)
        {
            if (TrackPower == TrackPower.ON)
            {
                controller.SetTrackPowerON();
            }
            else
            {
                controller.SetTrackPowerOFF();
            }
        }

        private void Tc_Activated(object sender, EventArgs e) => IsActive = true;

        private void Tc_Closing(object sender, CancelEventArgs e) => Dispose();

        private void Tc_Deactivated(object sender, EventArgs e) => IsActive = false;

        private void Tc_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter || e.Key == System.Windows.Input.Key.Space)
                e.Handled = true;
        }

        private void Tc_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Speed = e.Delta < 0 ? Speed - 1 : Speed + 1;
            e.Handled = true;
            SliderLastused = DateTime.Now;
        }

        private async void TrainControl_Loaded(object sender, RoutedEventArgs e)
        {
            Title = $"{Vehicle.Address} - {(string.IsNullOrWhiteSpace(Vehicle.Name) ? Vehicle.FullName : Vehicle.Name)}";

            SlowestVehicleInTractionList = Vehicle;

            controller.OnGetLocoInfo += Controller_OnGetLocoInfo;
            controller.TrackPowerChanged += Controller_TrackPowerChanged;
            controller.OnStatusChanged += Controller_OnStatusChanged;
            controller.ClientReachabilityChanged += (a, b) => Dispatcher.Invoke(() => { IsEnabled = controller.ClientReachable; controller.GetLocoInfo(new LokAdresse(Vehicle.Address)); });
            controller.GetStatus();
            controller.GetLocoInfo(new LokAdresse(Vehicle.Address));

            IsEnabled = controller.ClientReachable;

            DrawAllFunctions();
            SearchTractionVehicles();
            UpdateMultiTractionList();
            await DeterminSlowestVehicleInList();

            Db.ChangeTracker.StateChanged += async (a, b) =>
            {
                Vehicle = Db.Vehicles.Include(e => e.Functions).ToList().FirstOrDefault(e => e.Id == Vehicle.Id)!;
                Title = $"{Vehicle.Address} - {(string.IsNullOrWhiteSpace(Vehicle.Name) ? Vehicle.FullName : Vehicle.Name)}";
                DrawAllFunctions();
                SearchTractionVehicles();
                UpdateMultiTractionList();
                await DeterminSlowestVehicleInList();
            };
        }

        private void UpdateMultiTractionList()
        {
            MultiTractionList.Clear();
            MultiTractionList.AddRange(Vehicle.TractionVehicleIds.Select(e => new MultiTractionItem(Db.Vehicles.FirstOrDefault(f => f.Id == e)!)).Where(e => e.Vehicle is not null));
            MultiTractionList.Add(new(Vehicle));
        }

        public struct MultiTractionItem
        {
            public MultiTractionItem(VehicleModel vehicle)
            {
                Vehicle = vehicle;
                TractionForward = GetSortedSet(vehicle?.TractionForward!) ?? new();
                TractionBackward = GetSortedSet(vehicle?.TractionBackward!) ?? new();
            }

            public SortedSet<FunctionPoint> TractionBackward { get; private set; }

            public SortedSet<FunctionPoint> TractionForward { get; private set; }

            public VehicleModel Vehicle { get; }

            /// <summary>
            /// Converts the paramter <paramref name="tractionArray"/> to a <see cref="LineSeries"/> object.
            /// </summary>
            /// <param name="tractionArray"></param>
            /// <returns></returns>
            private static SortedSet<FunctionPoint>? GetSortedSet(decimal?[] tractionArray)
            {
                if (tractionArray is null || tractionArray[MaxDccSpeed] is null)
                    return null!;

                SortedSet<FunctionPoint>? function = new();

                for (int i = 0; i <= Z21Client.Z21Client.maxDccStep; i++)
                    if (tractionArray[i] is not null)
                        function.Add(new(i, (double)(tractionArray[i] ?? 0)));
                return function;
            }
        }
    }
}
