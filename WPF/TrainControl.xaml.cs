using Helper;
using Microsoft.EntityFrameworkCore;
using Model;
using ModelTrainController;
using ModelTrainController.Z21;
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
using WPF_Application.Extensions;
using WPF_Application.Helper;
using WPF_Application.Infrastructure;
using WPF_Application.JoyStick;

namespace WPF_Application
{
    /// <summary>
    /// Interaction logic for VehicleController.xaml
    /// </summary>
    public partial class TrainControl : Window, INotifyPropertyChanged
    {

        public TrainControl(ModelTrainController.CentralStationClient _controler, Vehicle _vehicle, Database _db)
        {
            try
            {
                if (_controler is null) throw new NullReferenceException($"Parameter {nameof(_controler)} ist null!");
                if (_vehicle is null) throw new NullReferenceException($"Paramter{nameof(_vehicle)} ist null!");
                if (_db is null) throw new NullReferenceException($"Paramter{nameof(_db)} ist null!");
                db = _db;
                this.DataContext = this;
                InitializeComponent();
                controler = _controler;
                Vehicle = db.Vehicles.Include(e => e.Functions).FirstOrDefault(e => e.Id == _vehicle.Id)!;
                if (Vehicle is null) throw new NullReferenceException($"Vehilce with adress {_vehicle.Address} not found!");
                Adresse = new(Vehicle.Address);
                this.Title = $"{Vehicle.Address} - {(string.IsNullOrWhiteSpace(Vehicle.Name) ? Vehicle.Full_Name : Vehicle.Name)}";
                SlowestVehicleInTractionList = Vehicle.Address;
                controler.OnGetLocoInfo += Controler_OnGetLocoInfo;
                controler.OnTrackPowerOFF += Controler_OnTrackPowerOFF;
                controler.OnTrackPowerON += Controler_OnTrackPowerON;
                controler.OnTrackShortCircuit += Controler_OnTrackShortCircuit;
                controler.OnProgrammingMode += Controler_OnProgrammingMode;
                controler.GetLocoInfo(Adresse);
                DrawAllFunctions();
                DrawAllVehicles(db.Vehicles.Where(m => m.Id != Vehicle.Id));
                DoubleTractionVehicles.Add((Vehicle.Address, false, (GetLineSeries(Vehicle.TractionForward), GetLineSeries(Vehicle.TractionBackward))));

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
                Logger.Log(isError: true, exception: ex);
                MessageBox.Show($"Beim öffnen des Controllers ist ein Fehler aufgetreten: {(string.IsNullOrWhiteSpace(ex?.Message) ? "" : ex.Message)}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Controler_OnTrackPowerOFF(object? sender, EventArgs e) => TrackPower = TrackPower.OFF;

        private void Controler_OnTrackPowerON(object? sender, EventArgs e) => TrackPower = TrackPower.ON;

        private void Controler_OnTrackShortCircuit(object? sender, EventArgs e) => TrackPower = TrackPower.Short;

        private void Controler_OnProgrammingMode(object? sender, EventArgs e) => TrackPower = TrackPower.Programing;

        private void Controler_OnGetLocoInfo(object? sender, GetLocoInfoEventArgs e)
        {
            if (e.Data.Adresse.Value == Vehicle.Address)
            {
                LiveData = e.Data;
                DrivingDirection = e.Data.DrivingDirection;
                foreach (var (functionIndex, state) in e.Data.Functions)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var x = FunctionToggleButtons.FirstOrDefault(e => (e?.Tag as Function)?.FunctionIndex == functionIndex);
                        if (x is not null)
                            x.IsChecked = state;
                    }));
                }
            }
        }

        void FunctionToggle_Click(Object sender, RoutedEventArgs e) => SetLocoFunction(((sender as ToggleButton)!.IsChecked ?? false) ? ToggleType.on : ToggleType.off, ((e.Source as ToggleButton)!.Tag as Function)!);

        void FunctionButtonDown_Click(Object sender, RoutedEventArgs e) => SetLocoFunction(ToggleType.on, ((e.Source as Button)?.Tag as Function)!);

        void FunctionButtonUp_Click(Object sender, RoutedEventArgs e) => SetLocoFunction(ToggleType.off, ((e.Source as Button)?.Tag as Function)!);

        /// <summary>
        /// Method for event when user checks a checkbox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void VehicleCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (((sender as CheckBox)?.Tag ?? null) is null) return;
            var temp = (Vehicle)(sender as CheckBox)!.Tag;
            if (temp.Type == VehicleType.Lokomotive && (temp.TractionForward[127] is null || temp.TractionForward[127] is null))
            {
                MessageBox.Show("Actung! Fahrzeug ist nicht eingemessen!");
            }
            DoubleTractionVehicles.Add((temp.Address, temp.Traction_Direction, (GetLineSeries(temp.TractionForward), GetLineSeries(temp.TractionBackward))));
            await DeterminSlowestVehicleInList();
            controler.GetLocoInfo(new(((Vehicle)(sender as CheckBox)!.Tag).Address));
        }

        /// <summary>
        /// Method for event when user unchecks a checkbox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void VehicleCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (((sender as CheckBox)?.Tag ?? null) is null) return;
            var temp = (Vehicle)(sender as CheckBox)!.Tag;
            DoubleTractionVehicles.RemoveAll(e => e.Address == temp.Address);
            await DeterminSlowestVehicleInList();
        }

        protected void OnPropertyChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        /// <summary>
        /// Functions draws every single Function of a vehicle for the user to click on. 
        /// </summary>
        public void DrawAllFunctions()
        {
            FunctionGrid.Children.Clear();
            int i = 0;
            foreach (var item in Vehicle.Functions)
            {
                i++;
                Border border = new();
                border.Padding = new(2);
                border.Margin = new(10);
                border.BorderThickness = new(1);
                border.BorderBrush = Brushes.Black;

                StackPanel sp = new();

                if (item.ButtonType == 0)
                {
                    ToggleButton tb = new() { Height = 50, Width = 90, Content = $"({item.FunctionIndex}) {item.Name}" };
                    tb.Click += FunctionToggle_Click;
                    tb.Tag = item;
                    tb.Name = $"btn_Function_{item.Id}";
                    FunctionToggleButtons.Add(tb);
                    sp.Children.Add(tb);
                }
                else
                {
                    Button btn = new() { Height = 50, Width = 90, Content = $"({item.FunctionIndex}) {item.Name}" };
                    btn.PreviewMouseDown += FunctionButtonDown_Click;
                    btn.PreviewMouseUp += FunctionButtonUp_Click;
                    btn.Tag = item;
                    btn.Name = $"btn_Function_{item.Id}";
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

        /// <summary>
        /// Function used to Set the speed and direction of a Loco. 
        /// </summary>
        /// <param name="locoAdress"></param>
        /// <param name="speedstep"></param>
        /// <param name="direction"></param>
        /// <param name="inUse"></param>
        private async void SetLocoDrive(int? speedstep = null, bool? direction = null, bool inUse = true)
        {
            direction ??= DrivingDirection;
            foreach (var (Adresse, InvertTraction, Traction) in DoubleTractionVehicles)
            {
                var dat = new LokInfoData()
                {
                    Adresse = new(Adresse),
                    DrivingDirection = Adresse == Vehicle.Address ? (bool)direction : (bool)(InvertTraction ? direction : !direction),
                    InUse = inUse,
                    Speed = (byte)(speedstep ?? Speed)
                };
                controler.SetLocoDrive(dat);
            }
        }

        /// <summary>
        /// Function used to set a Function on/off or switch its state. 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="function"></param>
        /// <param name="locoAdress"></param>
        private void SetLocoFunction(ToggleType type, Function function, int? locoAdress = null)
        {
            controler.SetLocoFunction(new LokAdresse((int)(locoAdress is null ? (int)vehicle.Address : locoAdress)), function, type);
        }

        /// <summary>
        /// Converts the paramter <paramref name="tractionArray"/> to a <see cref="LineSeries"/> object.
        /// </summary>
        /// <param name="tractionArray"></param>
        /// <returns></returns>
        private LineSeries GetLineSeries(decimal?[] tractionArray)
        {
            LineSeries series = new();
            for (int i = 2; i <= CentralStationClient.maxDccStep; i++)
            {
                if (tractionArray[i] is not null)
                {
                    series.Points.Add(new(i, (double)(tractionArray[i] ?? 0)));
                }
            }
            return series;
        }

        private async Task DeterminSlowestVehicleInList() => await Task.Run(() =>
        {
            List<(long adress, decimal value)> list = new();
            foreach (var (Adress, TractionDirection, Traction) in DoubleTractionVehicles)
            {
                var v = db.Vehicles.FirstOrDefault(e => e.Address == Adress);
                if (v is null) continue;
                if (v.TractionForward[127] is not null)
                {
                    list.Add((v.Address, (decimal)v.TractionForward[127]!));
                }
            }
            SlowestVehicleInTractionList = list.Count > 1 ? list.First(e => e.value == list.Min(e => e.value)).adress : Vehicle.Address;
        });

        private void BtnDirection_Click(object sender, RoutedEventArgs e)
        {
            DrivingDirection = !DrivingDirection;
        }

        #region Window Events
        private void Mw_Closing(object sender, CancelEventArgs e)
        {
            SetLocoDrive(inUse: false);
            Joystick?.Dispose();
        }

        private void Mw_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Speed = e.Delta < 0 ? Speed - 1 : Speed + 1;
            e.Handled = true;
            SliderLastused = DateTime.Now;
        }

        private void Mw_Activated(object sender, EventArgs e) => IsActive = true;

        private void Mw_Deactivated(object sender, EventArgs e) => IsActive = false;
        #endregion
        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tbSearch.Text))
                DrawAllVehicles(db.Vehicles.Include(e => e.Category).Where(i => (i.Address + i.Article_Number + i.Category.Name + i.Owner + i.Railway + i.Description + i.Full_Name + i.Name + i.Type).ToLower().Contains(tbSearch.Text.ToLower())).OrderBy(e => e.Position));
            else
                DrawAllVehicles(db.Vehicles.Include(e => e.Category).OrderBy(e => e.Position));
        }

        #region Preview Events
        private void SliderSpeed_PreviewMouseDown(object sender, MouseButtonEventArgs e) => SliderInUser = true;

        private void SliderSpeed_PreviewMouseUp(object sender, MouseButtonEventArgs e) => SliderInUser = false;
        #endregion
        public void OnJoyStickValueUpdate(Object? sender, JoyStickUpdateEventArgs e)
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
                            Speed = CentralStationClient.maxDccStep - ((e.currentValue * CentralStationClient.maxDccStep) / Function.Value.maxValue);
                            break;
                        case FunctionType.ChangeDirection:
                            if (e.currentValue == e.maxValue)
                            {
                                DrivingDirection = !DrivingDirection;
                            }
                            break;
                        default:
                            if (Function.Key == FunctionType.None) return;
                            var function = Vehicle.Functions.Where(e => e.EnumType == Function.Key).ToList();
                            if (function is null) return;
                            foreach (var item in function)
                            {
                                switch (item.ButtonType)
                                {
                                    case ButtonType.Switch:
                                        if (e.currentValue == Function.Value.maxValue)
                                            SetLocoFunction(ToggleType.@switch, item);
                                        break;
                                    case ButtonType.PushButton:
                                        if (e.currentValue == Function.Value.maxValue)
                                            SetLocoFunction(ToggleType.on, item);
                                        if (e.currentValue == 0)
                                            SetLocoFunction(ToggleType.off, item);
                                        break;
                                    case ButtonType.Timer:
                                        break;
                                }
                            }
                            break;
                    }


                }
            }
            catch
            {
                //Do nothing. 
            }
        }

    }

}
