using Helper;
using Microsoft.EntityFrameworkCore;
using Model;
using ModelTrainController;
using ModelTrainController.Z21;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// Interaction logic for TrainControl.xaml
    /// </summary>
    public partial class TrainControl : Window, INotifyPropertyChanged
    {
        public ModelTrainController.CentralStationClient controler = default!;
        private LokInfoData lokInfo = new();
        public Database db = default!;
        private int maxDccStep = 126;
        private bool direction = true;
        private int speedStep;
        public event PropertyChangedEventHandler? PropertyChanged;
        private TrackPower trackPower;
        private Vehicle vehicle = default!;
        private bool lastTrackPowerUpdateWasShort = false;
        readonly Dictionary<FunctionType, (JoystickOffset joyStick, int maxValue)> functionToJoyStickDictionary = new();

        private JoyStick.JoyStick? Joystick { get; }

        /// <summary>
        /// Used to determin if this window is in focus.
        /// </summary>
        public new bool IsActive { get; set; } = false;

        /// <summary>
        /// The <see cref="Vehicle"/> the application is trying to controll
        /// </summary>
        public Vehicle Vehicle
        {
            get => vehicle;
            set
            {
                vehicle = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(VehicleTypeGridLength));
                OnPropertyChanged(nameof(VehicleTypeVisbility));

            }
        }

        public List<(LokInfoData lokinfo, bool invert)> DoubleTractionVehicles { get; } = new();

        public GridLength VehicleTypeGridLength
        {
            get => (Vehicle?.Type ?? VehicleType.Lokomotive) == VehicleType.Lokomotive ? new GridLength(80) : new GridLength(0);
        }

        public Visibility VehicleTypeVisbility
        {
            get => (Vehicle?.Type ?? VehicleType.Lokomotive) == VehicleType.Lokomotive ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// True if the speed controll slider is currently used by the user. 
        /// </summary>
        public bool SliderInUser { get; set; }

        /// <summary>
        /// The date and time the user last used the speed controll slider.
        /// </summary>
        public DateTime SliderLastused { get; set; }

        /// <summary>
        /// The speedstep the vehicle should drive at.
        /// </summary>
        public int SpeedStep
        {
            get => speedStep; set
            {
                if (value >= 0 && value <= maxDccStep)
                {
                    speedStep = value == 1 ? (speedStep > 1 ? 0 : 2) : value;
                    OnPropertyChanged();
                    if (speedStep != (LokInfo.Speed))
                        SetLocoDrive(speedstep: speedStep);
                }
            }
        }

        /// <summary>
        /// The direction of the vehicle. 
        /// </summary>
        public bool Direction
        {
            get => direction; set
            {
                direction = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(GetDirectionString));
                SetLocoDrive(direction: value);
            }
        }

        /// <summary>
        /// Returns a string that describes the current direction of travel. 
        /// </summary>
        public string GetDirectionString { get => Direction ? "Vorwärts" : "Rückwärts"; }

        /// <summary>
        /// List of all buttons on a grid which controll vehicle <see cref="Function"/>s.
        /// </summary>
        private readonly List<Button> FunctionButtons = new();

        /// <summary>
        /// List of all togglebuttons on a grid which controll vehicle <see cref="Function"/>s.
        /// </summary>
        private readonly List<ToggleButton> FunctionToggleButtons = new();

        /// <summary>
        /// Data directly from the Z21. Not Used to controll the vehicle. 
        /// </summary>
        public LokInfoData LokInfo
        {
            get => lokInfo; set
            {
                lokInfo = value;
                if (!SliderInUser && (DateTime.Now - SliderLastused).TotalSeconds > 2)
                {
                    SpeedStep = value.Speed;
                }
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// This is the Max Acceleration Step.
        /// </summary>
        public int MaxDccStep
        {
            get => maxDccStep; set
            {
                maxDccStep = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Data from the Z21.
        /// </summary>
        public TrackPower TrackPower
        {
            get => trackPower; set
            {
                if (!lastTrackPowerUpdateWasShort)
                {
                    trackPower = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TrackPowerBoolean));
                    OnPropertyChanged(nameof(TrackPowerMessage));
                }
                lastTrackPowerUpdateWasShort = (value == TrackPower.Short);

            }
        }

        /// <summary>
        /// True if the TrackPower is on. False otherwhise. Used to set the trackpower.
        /// </summary>
        public bool TrackPowerBoolean
        {
            set
            {
                if (TrackPowerBoolean)
                    controler.SetTrackPowerOFF();
                else
                    controler.SetTrackPowerON();
            }
            get => TrackPower.ToBoolean();
        }

        /// <summary>
        /// Gets a string describing the current track power mode. (Needed for UI)
        /// </summary>
        public string TrackPowerMessage
        {
            get => TrackPower.GetString();
        }

        public TrainControl(ModelTrainController.CentralStationClient _controler, Vehicle _vehicle, Database _db)
        {
            try
            {
                if (_controler is null) throw new NullReferenceException($"Parameter {nameof(_controler)} ist null!");
                if (_vehicle is null) throw new NullReferenceException($"Paramter{nameof(_vehicle)}ist null!");
                if (_db is null) throw new NullReferenceException($"Paramter{nameof(_db)}ist null!");
                InitializeComponent();
                db = _db;
                this.DataContext = this;
                controler = _controler!;
                Vehicle = db.Vehicles.Include(e => e.Functions).FirstOrDefault(e => e.Id == _vehicle.Id)!;
                LokInfo = new((byte)Vehicle.Address);
                this.Title = $"{Vehicle.Address} - {(string.IsNullOrWhiteSpace(Vehicle.Name) ? Vehicle.Full_Name : Vehicle.Name)}";

                controler.OnGetLocoInfo += new EventHandler<GetLocoInfoEventArgs>(OnGetLocoInfoEventArgs);
                controler.OnTrackPowerOFF += new EventHandler(OnGetTrackPowerOffEventArgs);
                controler.OnTrackPowerON += new EventHandler(OnGetTrackPowerOnEventArgs);
                controler.OnTrackShortCircuit += new EventHandler(OnTrackShortCircuitEventArgs);
                controler.OnProgrammingMode += new EventHandler(OnProgrammingModeEventArgs);

                controler.GetLocoInfo(LokInfo.Adresse);
                controler.SetTrackPowerON();
                DrawAllFunctions();
                DrawAllVehicles(db.Vehicles);
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
                Logger.Log($"{DateTime.UtcNow}: '{(string.IsNullOrWhiteSpace(ex.Message) ? "Keine Exception Message" : ex.Message)}' - Inner Exception: {ex?.InnerException?.Message ?? "Keine Inner Exception"}", LoggerType.Error);
                MessageBox.Show($"Beim öffnen des Controllers ist ein Fehler aufgetreten: {(string.IsNullOrWhiteSpace(ex?.Message) ? "Keine Exception Message" : ex.Message)}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                sp.HorizontalAlignment = HorizontalAlignment.Left;
                sp.VerticalAlignment = VerticalAlignment.Top;
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
            foreach (var item in vehicles.Where(e => e.Id != Vehicle.Id))
            {
                CheckBox c = new()
                {
                    Content = item.Name,
                    Tag = item,
                    Margin = new Thickness(5)
                };
                c.Unchecked += VehicleCheckBox_Unchecked;
                c.Checked += VehicleCheckBox_Checked;
                SPVehilces.Children.Add(c);
            }
        }

        /// <summary>
        /// Method for event when user checks a checkbox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VehicleCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is not CheckBox || (sender as CheckBox)!.Tag is null) return;
            DoubleTractionVehicles.Add(new(new(((Vehicle)(sender as CheckBox)!.Tag).Address), false));
            controler.GetLocoInfo(new(((Vehicle)(sender as CheckBox)!.Tag).Address));
        }

        /// <summary>
        /// Method for event when user unchecks a checkbox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VehicleCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is not CheckBox || (sender as CheckBox)!.Tag is null) return;
            DoubleTractionVehicles.Remove(new(new(((Vehicle)(sender as CheckBox)!.Tag).Address), false));
        }

        /// <summary>
        /// Function used to Set the speed and direction of a Loco. 
        /// </summary>
        /// <param name="locoAdress"></param>
        /// <param name="speedstep"></param>
        /// <param name="direction"></param>
        /// <param name="inUse"></param>
        private void SetLocoDrive(int? speedstep = null, bool? direction = null, bool? inUse = null)
        {
            var dat = new LokInfoData()
            {
                Adresse = new LokAdresse(Vehicle.Address),
                InUse = inUse ?? LokInfo.InUse,
                DrivingDirection = direction ?? LokInfo.DrivingDirection,
                Speed = (byte)(speedstep ?? LokInfo.Speed)
            };
            controler.SetLocoDrive(dat);
            foreach (var item in DoubleTractionVehicles)
            {
                dat.Adresse = item.lokinfo.Adresse;
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

        #region Events
        public void OnGetLocoInfoEventArgs(Object? sender, GetLocoInfoEventArgs e)
        {
            if (e.Data.Adresse.Value == Vehicle.Address || DoubleTractionVehicles.Where(f => f.lokinfo.Adresse == e.Data.Adresse).Any())
            {
                if (e.Data.Adresse.Value == Vehicle.Address)
                {
                    LokInfo = e.Data;
                    Direction = e.Data.DrivingDirection;
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
                else
                    DoubleTractionVehicles.Where(f => f.lokinfo.Adresse == e.Data.Adresse).ToList().ForEach(i => i.lokinfo = e.Data);
            }
        }

        public void OnGetTrackPowerOnEventArgs(Object? sender, EventArgs e) => TrackPower = TrackPower.ON;

        public void OnGetTrackPowerOffEventArgs(Object? sender, EventArgs e) => TrackPower = TrackPower.OFF;

        public void OnTrackShortCircuitEventArgs(Object? sender, EventArgs e) => TrackPower = TrackPower.Short;

        public void OnProgrammingModeEventArgs(Object? sender, EventArgs e) => TrackPower = TrackPower.Programing;

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
                            SpeedStep = MaxDccStep - ((e.currentValue * MaxDccStep) / Function.Value.maxValue);
                            break;
                        case FunctionType.ChangeDirection:
                            if (e.currentValue == e.maxValue)
                                SetLocoDrive(direction: LokInfo.DrivingDirection);
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

        protected void OnPropertyChanged([CallerMemberName] string name = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tbSearch.Text))
                DrawAllVehicles(db.Vehicles.Include(e => e.Category).Where(i => (i.Address + i.Article_Number + i.Category.Name + i.Owner + i.Railway + i.Description + i.Full_Name + i.Name + i.Type).ToLower().Contains(tbSearch.Text.ToLower())).OrderBy(e => e.Position));
            else
                DrawAllVehicles(db.Vehicles.Include(e => e.Category).OrderBy(e => e.Position));
        }
        #endregion

        #region Click_Events
        void FunctionToggle_Click(Object sender, RoutedEventArgs e) => SetLocoFunction((sender as ToggleButton)!.IsChecked ?? false ? ToggleType.on : ToggleType.off, ((e.Source as ToggleButton)!.Tag as Function)!);

        void FunctionButtonDown_Click(Object sender, RoutedEventArgs e) => SetLocoFunction(ToggleType.on, ((e.Source as Button)?.Tag as Function)!);

        void FunctionButtonUp_Click(Object sender, RoutedEventArgs e) => SetLocoFunction(ToggleType.off, ((e.Source as Button)?.Tag as Function)!);
        #endregion

        #region Preview Events
        private void SliderSpeed_PreviewMouseDown(object sender, MouseButtonEventArgs e) => SliderInUser = true;

        private void SliderSpeed_PreviewMouseUp(object sender, MouseButtonEventArgs e) => SliderInUser = false;
        #endregion

        #region Window Events
        private void Mw_Closing(object sender, CancelEventArgs e)
        {
            SetLocoDrive(inUse: false);
            Joystick?.Dispose();
        }

        private void Mw_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            SpeedStep = e.Delta < 0 ? SpeedStep - 1 : SpeedStep + 1;
            e.Handled = true;
            SliderLastused = DateTime.Now;
        }

        private void Mw_Activated(object sender, EventArgs e) => IsActive = true;

        private void Mw_Deactivated(object sender, EventArgs e) => IsActive = false;
        #endregion

        private void BtnDirection_Click(object sender, RoutedEventArgs e)
        {
            Direction = !LokInfo.DrivingDirection;
        }
    }
}
