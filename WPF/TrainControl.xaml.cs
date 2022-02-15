using Extensions;
using Helper;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Model;
using OxyPlot.Series;
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
using TrainDatabase.Z21Client.Enum;
using TrainDatabase.Z21Client.Events;

namespace TrainDatabase
{
    /// <summary>
    /// Interaction logic for VehicleController.xaml
    /// </summary>
    public partial class TrainControl : Window, INotifyPropertyChanged, IDisposable
    {
        public TrainControl(Z21Client.Z21Client _controller, Vehicle _vehicle, Database _db)
        {
            try
            {
                if (_controller is null || _vehicle is null || _db is null)
                    throw new NullReferenceException($"Parameter {nameof(_controller)} ist null!");

                db = _db;
                controller = _controller;

                DataContext = this;
                InitializeComponent();
                Activate();

                Vehicle = db.Vehicles.Include(e => e.Functions).ToList().FirstOrDefault(e => e.Id == _vehicle.Id)!;

                if (Vehicle is null)
                    throw new NullReferenceException($"Vehilce with adress {_vehicle.Address} not found!");

                Adresse = new(Vehicle.Address);
                Title = $"{Vehicle.Address} - {(string.IsNullOrWhiteSpace(Vehicle.Name) ? Vehicle.FullName : Vehicle.Name)}";

                SlowestVehicleInTractionList = Vehicle;

                controller.LogOn();
                controller.OnGetLocoInfo += Controller_OnGetLocoInfo;
                controller.TrackPowerChanged += Controller_TrackPowerChanged;
                controller.OnStatusChanged += Controller_OnStatusChanged;
                controller.GetLocoInfo(new LokAdresse(Vehicle.Address));
                controller.GetStatus();

                DrawAllFunctions();
                DrawAllVehicles(db.Vehicles.ToList().Where(m => m.Id != Vehicle.Id));

                MultiTractionList.Add((Vehicle, (GetSortedSet(Vehicle.TractionForward), GetSortedSet(Vehicle.TractionBackward))));
            }
            catch (Exception ex)
            {
                Close();
                Logger.Log("Fehler beim öffnen des Controllers.", ex);
                MessageBox.Show($"Beim öffnen des Controllers ist ein Fehler aufgetreten: {(string.IsNullOrWhiteSpace(ex?.Message) ? "" : ex.Message)}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Dispose()
        {
            SetLocoDrive(inUse: false);
            Joystick?.Dispose();
        }

        protected void OnPropertyChanged(string propertyName = null!) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Returns a new instance of <typeparamref name="T"/>, which is configured to controll a <see cref="Function"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        private static T GetButton<T>(Function item) where T : ButtonBase, new() => new T()
        {
            Height = 50,
            Width = 90,
            Content = $"{(item.ShowFunctionNumber ? $"({item.FunctionIndex}) " : "")}{item.Name}",
            Tag = item,
            ToolTip = $"{item}",
            Margin = new(10),
            Padding = new(2),
            BorderBrush = Brushes.Black,
            BorderThickness = new(1)
        };

        private static LokInfoData GetLocoInfoData(int speedstep, bool direction, bool inUse, Vehicle Vehicle) => new()
        {
            Adresse = new(Vehicle.Address),
            DrivingDirection = direction,
            InUse = inUse,
            Speed = (byte)speedstep
        };

        private static double GetSlowestVehicleSpeed(int speedstep, bool direction, (Vehicle Vehicle, (SortedSet<FunctionPoint>? Forwards, SortedSet<FunctionPoint>? Backwards) Traction) Vehicle)
        {
            if (Vehicle.Traction.Forwards is null || Vehicle.Traction.Backwards is null)
                return double.NaN;

            if (!Vehicle.Vehicle.InvertTraction)
                return direction ? Vehicle.Traction.Forwards.GetYValue(speedstep) : Vehicle.Traction.Backwards.GetYValue(speedstep);
            else
                return direction ? Vehicle.Traction.Backwards.GetYValue(speedstep) : Vehicle.Traction.Forwards.GetYValue(speedstep);
        }

        /// <summary>
        /// Converts the paramter <paramref name="tractionArray"/> to a <see cref="LineSeries"/> object.
        /// </summary>
        /// <param name="tractionArray"></param>
        /// <returns></returns>
        private static SortedSet<FunctionPoint>? GetSortedSet(decimal?[] tractionArray)
        {
            if (tractionArray[MaxDccSpeed] is null)
                return null!;

            SortedSet<FunctionPoint>? function = new();

            for (int i = 0; i <= Z21Client.Z21Client.maxDccStep; i++)
                if (tractionArray[i] is not null)
                    function.Add(new(i, (double)(tractionArray[i] ?? 0)));
            return function;
        }

        private static bool IsVehicleMeasured((Vehicle Vehicle, (SortedSet<FunctionPoint> Forwards, SortedSet<FunctionPoint> Backwards) Traction) item) => item.Traction.Forwards is not null && item.Traction.Backwards is not null;

        private void BtnDirection_Click(object sender, RoutedEventArgs e) => SwitchDirection();

        private async void Controller_OnGetLocoInfo(object? sender, GetLocoInfoEventArgs e)
        {
            if (e.Data.Adresse.Value == Vehicle.Address)
            {
                LiveData = e.Data;
                foreach (var (functionIndex, state) in e.Data.Functions)
                {
                    await Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var x = FunctionToggleButtons.FirstOrDefault(e => (e?.Tag as Function)?.FunctionIndex == functionIndex);
                        if (x is not null)
                            x.IsChecked = state;
                    }));
                }
            }
        }

        private void Controller_OnStatusChanged(object? sender, StateEventArgs e) => TrackPower = e.TrackPower;

        private void Controller_TrackPowerChanged(object? sender, TrackPowerEventArgs e) => TrackPower = e.TrackPower;

        private async Task DeterminSlowestVehicleInList() => await Task.Run(() =>
                {
                    var list = MultiTractionList.Where(e => e.Vehicle.Type == VehicleType.Lokomotive && e.Traction.Forwards is not null && e.Traction.Backwards is not null).ToList();

                    if (list.Any())
                        SlowestVehicleInTractionList = list.Aggregate((cur, next) => (cur.Traction.Forwards?.GetYValue(MaxDccSpeed) ?? int.MaxValue) < (next.Traction.Forwards?.GetYValue(MaxDccSpeed) ?? int.MaxValue) ? cur : next).Vehicle ?? Vehicle;
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
                        ToggleButton tb = GetButton<ToggleButton>(item);
                        tb.Click += (sender, e) => SetLocoFunction(((sender as ToggleButton)!.IsChecked ?? false) ? ToggleType.On : ToggleType.Off, ((e.Source as ToggleButton)!.Tag as Function)!);
                        FunctionToggleButtons.Add(tb);
                        FunctionGrid.Children.Add(tb);
                        break;
                    default:
                        Button btn = GetButton<Button>(item);
                        btn.PreviewMouseDown += (sender, e) => SetLocoFunction(ToggleType.On, ((e.Source as Button)?.Tag as Function)!);
                        btn.PreviewMouseUp += (sender, e) => SetLocoFunction(ToggleType.Off, ((e.Source as Button)?.Tag as Function)!); ;
                        FunctionButtons.Add(btn);
                        FunctionGrid.Children.Add(btn);
                        break;
                }
            }
        }

        /// <summary>
        /// Draws every vehicle in a list as a checkbox
        /// </summary>
        /// <param name="vehicles"></param>
        private void DrawAllVehicles(IEnumerable<Vehicle> vehicles)
        {
            SPVehilces.Children.Clear();
            foreach (var vehicle in vehicles.Where(e => e.Id != Vehicle.Id))
            {
                CheckBox c = new()
                {
                    Content = vehicle.Name,
                    Tag = vehicle,
                    Margin = new Thickness(5)
                };
                c.Unchecked += async (sender, e) =>
                {
                    MultiTractionList.RemoveAll(e => e.Vehicle.Id == (((sender as CheckBox)?.Tag as Vehicle)?.Id ?? -1));
                    await DeterminSlowestVehicleInList();
                };
                c.Checked += async (sender, e) =>
                 {
                     if (MultiTractionList.Count >= 140)
                     {
                         MessageBox.Show("Mehr als 140 Traktionsfahrzeuge sind nicht möglich!", "Max Limit reached.", MessageBoxButton.OK, MessageBoxImage.Error);
                         return;
                     }
                     if (sender is CheckBox c && c.Tag is Vehicle v)
                     {
                         MultiTractionList.Add((v, (GetSortedSet(v.TractionForward), GetSortedSet(v.TractionBackward))));
                         await DeterminSlowestVehicleInList();
                     }
                 };
                SPVehilces.Children.Add(c);
            }
        }

        private bool GetDrivingDirection(Vehicle vehicle, bool direction) => vehicle.Id != Vehicle.Id ? (vehicle.InvertTraction ? !direction : direction) : direction;

        private double GetSlowestVehicleSpeed(bool direction, int xValue)
        {
            var (Id, Traction) = MultiTractionList.FirstOrDefault(e => e.Vehicle == SlowestVehicleInTractionList);
            return direction ? Traction.Forwards.GetYValue(xValue) : Traction.Forwards.GetYValue(xValue);
        }

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tbSearch.Text))
                DrawAllVehicles(db.Vehicles.Include(e => e.Category).Where(i => (i.Address + i.ArticleNumber + i.Category.Name + i.Owner + i.Railway + i.Description + i.FullName + i.Name + i.Type).ToLower().Contains(tbSearch.Text.ToLower())).OrderBy(e => e.Position));
            else
                DrawAllVehicles(db.Vehicles.Include(e => e.Category).OrderBy(e => e.Position));
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
            if (speedstep is not null && speedstep != 0 && speedstep != Z21Client.Z21Client.maxDccStep && DateTime.Now - lastSpeedchange < new TimeSpan(100))
                return;
            else
                lastSpeedchange = DateTime.Now;

            bool direction = drivingDirection ??= LiveData.DrivingDirection;
            int speed = speedstep ?? Speed;
            List<LokInfoData> data = new();
            var slowestVehicle = MultiTractionList.FirstOrDefault<(Vehicle Vehicle, (SortedSet<FunctionPoint> Forwards, SortedSet<FunctionPoint> Backwards) Traction)>(e => e.Vehicle.Equals(SlowestVehicleInTractionList));
            var yValue = GetSlowestVehicleSpeed(speed, direction, slowestVehicle);
            foreach (var item in MultiTractionList.Where<(Vehicle Vehicle, (SortedSet<FunctionPoint> Forwards, SortedSet<FunctionPoint> Backwards) Traction)>(e => !e.Vehicle.Equals(SlowestVehicleInTractionList)))
            {
                if (IsVehicleMeasured(item))
                {
                    int dccSpeed;
                    if (!item.Vehicle.InvertTraction)
                        dccSpeed = direction ? item.Traction.Forwards.GetXValue(yValue) : item.Traction.Backwards.GetXValue(yValue);
                    else
                        dccSpeed = direction ? item.Traction.Backwards.GetXValue(yValue) : item.Traction.Forwards.GetXValue(yValue);

                    data.Add(GetLocoInfoData(dccSpeed, GetDrivingDirection(item.Vehicle, direction), inUse, item.Vehicle));
                }
                else
                    data.Add(GetLocoInfoData(speed, GetDrivingDirection(item.Vehicle, direction), inUse, item.Vehicle));
            }
            data.Add(GetLocoInfoData(speed, GetDrivingDirection(slowestVehicle.Vehicle, direction), inUse, slowestVehicle.Vehicle));
            controller.SetLocoDrive(data);
        });

        /// <summary>
        /// Function used to set a Function on/off or switch its state. 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="function"></param>
        /// <param name="locoAdress"></param>
        private void SetLocoFunction(ToggleType type, Function function)
        {

            if (function.EnumType != FunctionType.None)
            {
                var functions = MultiTractionList.Where(e => e.Vehicle.Id == Vehicle.Id || e.Vehicle.Type == VehicleType.Steuerwagen).SelectMany(e => e.Vehicle.Functions).Where(e => e.EnumType == function.EnumType && e.ButtonType == function.ButtonType).ToList();

                List<(ToggleType toggle, Function Func)> list = new();

                foreach (var item in functions)
                    list.Add((type, item));

                controller.SetLocoFunction(list);
            }
            else
                controller.SetLocoFunction(new((int)vehicle.Address), function, type);
        }

        private void SliderSpeed_PreviewMouseDown(object sender, MouseButtonEventArgs e) => SliderInUser = true;

        private void SliderSpeed_PreviewMouseUp(object sender, MouseButtonEventArgs e) => SliderInUser = false;

        private void SwitchDirection() => SetLocoDrive(drivingDirection: !LiveData.DrivingDirection);

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
    }
}
