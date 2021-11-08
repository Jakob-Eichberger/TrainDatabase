using Microsoft.EntityFrameworkCore;
using Model;
using OxyPlot;
using OxyPlot.Series;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using TrainDatabase.Z21Client;
using TrainDatabase.Z21Client.DTO;
using TrainDatabase.Z21Client.Enum;
using TrainDatabase.Z21Client.Events;
using TrainDatabase.Extensions;
using TrainDatabase.Infrastructure;
using TrainDatabase.JoyStick;
using Helper;

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

                DrawAllFunctions();
                DrawAllVehicles(db.Vehicles.ToList().Where(m => m.Id != Vehicle.Id));

                DoubleTractionVehicles.Add((Vehicle, (GetSortedSet(Vehicle.TractionForward), GetSortedSet(Vehicle.TractionBackward))));

                if (Vehicle.Type.IsLokomotive() && Settings.UsingJoyStick)
                {
                    Joystick = new(Guid.Empty);
                    Joystick.OnValueUpdate += new EventHandler<JoyStickUpdateEventArgs>(OnJoyStickValueUpdate);
                    functionToJoyStickDictionary = Settings.FunctionToJoyStickDictionary();
                }
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

        /// <summary>
        /// Functions draws every single Function of a vehicle for the user to click on. 
        /// </summary>
        public void DrawAllFunctions()
        {
            FunctionGrid.Children.Clear();
            foreach (var item in Vehicle.Functions.OrderBy(e => e.FunctionIndex))
            {
                Border border = new();
                border.Padding = new(2);
                border.Margin = new(10);
                border.BorderThickness = new(1);
                border.BorderBrush = Brushes.Black;

                StackPanel sp = new();

                if (item.ButtonType == 0)
                {
                    ToggleButton tb = new() { Height = 50, Width = 90, Content = $"{(item.ShowFunctionNumber ? $"({item.FunctionIndex}) " : "")}{item.Name}" };
                    tb.Click += FunctionToggle_Click;
                    tb.Tag = item;
                    tb.Name = $"btn_Function_{item.Id}";
                    tb.ToolTip = $"{item}";
                    FunctionToggleButtons.Add(tb);
                    sp.Children.Add(tb);
                }
                else
                {
                    Button btn = new() { Height = 50, Width = 90, Content = $"{(item.ShowFunctionNumber ? $"({item.FunctionIndex}) " : "")}{item.Name}" };
                    btn.PreviewMouseDown += FunctionButtonDown_Click;
                    btn.PreviewMouseUp += FunctionButtonUp_Click;
                    btn.Tag = item;
                    btn.Name = $"btn_Function_{item.Id}";
                    btn.ToolTip = $"{item}";
                    FunctionButtons.Add(btn);
                    sp.Children.Add(btn);
                }

                border.Child = sp;
                sp.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                sp.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                FunctionGrid.Children.Add(border);
            }
        }

        /// <summary>
        /// Draws every vehicle in a list as a checkbox
        /// </summary>
        /// <param name="vehicles"></param>
        public void DrawAllVehicles(IEnumerable<Vehicle> vehicles)
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
                c.Unchecked += VehicleCheckBox_Unchecked;
                c.Checked += VehicleCheckBox_Checked;
                SPVehilces.Children.Add(c);
            }
        }

        public void OnJoyStickValueUpdate(object? sender, JoyStickUpdateEventArgs e)
        {
            try
            {
                if (IsActive)
                {
                    var i = e.joyStickOffset;
                    var Function = functionToJoyStickDictionary.Where(f => f.Value.joyStick == e.joyStickOffset).FirstOrDefault();

                    switch (Function.Key)
                    {
                        case FunctionType.Drive:
                            SliderLastused = DateTime.Now;
                            Speed = Z21Client.Z21Client.maxDccStep - (e.currentValue * Z21Client.Z21Client.maxDccStep / Function.Value.maxValue);
                            break;
                        case FunctionType.ChangeDirection:
                            if (e.currentValue == e.maxValue)
                                SwitchDirection();
                            break;
                        default:
                            if (Function.Key == FunctionType.None) return;
                            var function = DoubleTractionVehicles.Where(e => e.Vehicle.Id == Vehicle.Id || e.Vehicle.Type == VehicleType.Steuerwagen).SelectMany(e => e.Vehicle.Functions).Where(e => e.EnumType == Function.Key).ToList();
                            if (function is null) return;
                            List<(ToggleType toggle, Function Func)> list = new();
                            foreach (var item in function)
                            {
                                switch (item.ButtonType)
                                {
                                    case ButtonType.Switch:
                                        if (e.currentValue == Function.Value.maxValue)
                                            list.Add((ToggleType.Toggle, item));
                                        break;
                                    case ButtonType.PushButton:
                                        if (e.currentValue == Function.Value.maxValue)
                                            list.Add((ToggleType.On, item));
                                        if (e.currentValue == 0)
                                            list.Add((ToggleType.Off, item));
                                        break;
                                    case ButtonType.Timer:
                                        break;
                                }
                            }
                            controller.SetLocoFunction(list);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{nameof(OnJoyStickValueUpdate)} failed! {ex}");
            }
        }

        public void RefreshSource()
        {
            Vehicle = db.Vehicles.Include(e => e.Functions).ToList().FirstOrDefault(e => e.Id == Vehicle.Id)!;
            DrawAllFunctions();
            DrawAllVehicles(db.Vehicles.ToList().Where(m => m.Id != Vehicle.Id));
        }

        protected void OnPropertyChanged(string propertyName = null!) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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
            var list = DoubleTractionVehicles.Where(e => e.Vehicle.Type == VehicleType.Lokomotive && e.Traction.Forwards is not null && e.Traction.Backwards is not null).ToList();

            if (list.Any())
                SlowestVehicleInTractionList = list.Aggregate((cur, next) => (cur.Traction.Forwards?.GetYValue(MaxDccSpeed) ?? int.MaxValue) < (next.Traction.Forwards?.GetYValue(MaxDccSpeed) ?? int.MaxValue) ? cur : next).Vehicle ?? Vehicle;
            else
                SlowestVehicleInTractionList = Vehicle;
        });

        void FunctionButtonDown_Click(Object sender, RoutedEventArgs e) => SetLocoFunction(ToggleType.On, ((e.Source as Button)?.Tag as Function)!);

        void FunctionButtonUp_Click(Object sender, RoutedEventArgs e) => SetLocoFunction(ToggleType.Off, ((e.Source as Button)?.Tag as Function)!);

        void FunctionToggle_Click(Object sender, RoutedEventArgs e) => SetLocoFunction(((sender as ToggleButton)!.IsChecked ?? false) ? ToggleType.On : ToggleType.Off, ((e.Source as ToggleButton)!.Tag as Function)!);
        private bool GetDrivingDirection(Vehicle vehicle, bool direction) => vehicle.Id != Vehicle.Id ? (vehicle.InvertTraction ? !direction : direction) : direction;

        private double GetSlowestVehicleSpeed(bool direction, int xValue)
        {
            var (Id, Traction) = DoubleTractionVehicles.FirstOrDefault(e => e.Vehicle == SlowestVehicleInTractionList);
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
            var slowestVehicle = DoubleTractionVehicles.FirstOrDefault<(Vehicle Vehicle, (SortedSet<FunctionPoint> Forwards, SortedSet<FunctionPoint> Backwards) Traction)>(e => e.Vehicle.Equals(SlowestVehicleInTractionList));
            var yValue = GetSlowestVehicleSpeed(speed, direction, slowestVehicle);
            foreach (var item in DoubleTractionVehicles.Where<(Vehicle Vehicle, (SortedSet<FunctionPoint> Forwards, SortedSet<FunctionPoint> Backwards) Traction)>(e => !e.Vehicle.Equals(SlowestVehicleInTractionList)))
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
                var functions = DoubleTractionVehicles.Where(e => e.Vehicle.Id == Vehicle.Id || e.Vehicle.Type == VehicleType.Steuerwagen).SelectMany(e => e.Vehicle.Functions).Where(e => e.EnumType == function.EnumType && e.ButtonType == function.ButtonType).ToList();

                List<(ToggleType toggle, Function Func)> list = new();

                foreach (var item in functions)
                    list.Add((type, item));

                controller.SetLocoFunction(list);
            }
            else
                controller.SetLocoFunction(new((int)vehicle.Address), function, type);
        }

        private void Tc_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter || e.Key == System.Windows.Input.Key.Space)
                e.Handled = true;
        }

        /// <summary>
        /// Method for event when user checks a checkbox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void VehicleCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (((sender as CheckBox)?.Tag ?? null) is null) return;
            if (DoubleTractionVehicles.Count >= 140)
            {
                MessageBox.Show("Mehr als 140 Traktionsfahrzeuge sind nicht möglich!", "Max Limit reached.", MessageBoxButton.OK);
                return;
            }
            var temp = (Vehicle)(sender as CheckBox)!.Tag;
            if (temp.Type == VehicleType.Lokomotive && (temp.TractionForward[Z21Client.Z21Client.maxDccStep] is null || temp.TractionForward[Z21Client.Z21Client.maxDccStep] is null))
                MessageBox.Show("Achtung! Fahrzeug ist nicht eingemessen!");
            DoubleTractionVehicles.Add((temp, (GetSortedSet(temp.TractionForward), GetSortedSet(temp.TractionBackward))));
            await DeterminSlowestVehicleInList();
        }

        /// <summary>
        /// Method for event when user unchecks a checkbox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void VehicleCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (((sender as CheckBox)?.Tag ?? null) is null)
                return;

            var temp = (Vehicle)(sender as CheckBox)!.Tag;

            DoubleTractionVehicles.RemoveAll(e => e.Vehicle.Id == temp.Id);

            await DeterminSlowestVehicleInList();
        }
        
        private void BtnDirection_Click(object sender, RoutedEventArgs e) => SwitchDirection();

        private void SwitchDirection() => SetLocoDrive(drivingDirection: !LiveData.DrivingDirection);

        private void Tc_Activated(object sender, EventArgs e) => IsActive = true;

        private void Tc_Closing(object sender, CancelEventArgs e) => Dispose();

        private void Tc_Deactivated(object sender, EventArgs e) => IsActive = false;

        private void Tc_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Speed = e.Delta < 0 ? Speed - 1 : Speed + 1;
            e.Handled = true;
            SliderLastused = DateTime.Now;
        }
   
        private void SliderSpeed_PreviewMouseDown(object sender, MouseButtonEventArgs e) => SliderInUser = true;

        private void SliderSpeed_PreviewMouseUp(object sender, MouseButtonEventArgs e) => SliderInUser = false;
    }
}
