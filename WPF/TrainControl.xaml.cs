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
        private LokInfoData lokState = new();
        public Database db;
        private int maxDccStep = 126;
        private bool direction = true;
        private int speedStep;
        public event PropertyChangedEventHandler? PropertyChanged;
        private TrackPower trackPower;
        private Vehicle vehicle;
        private bool lastTrackPowerUpdateWasShort = false;
        readonly Dictionary<FunctionType, (JoystickOffset joyStick, int maxValue)> functionToJoyStickDictionary;

        private JoyStick.JoyStick? Joystick { get; }

        public new bool IsActive { get; set; } = false;

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

        public GridLength VehicleTypeGridLength
        {
            get => (Vehicle?.Type ?? VehicleType.Lokomotive) == VehicleType.Lokomotive ? new GridLength(80) : new GridLength(0);
        }

        public Visibility VehicleTypeVisbility
        {
            get => (Vehicle?.Type ?? VehicleType.Lokomotive) == VehicleType.Lokomotive ? Visibility.Visible : Visibility.Collapsed;
        }

        public bool SliderInUser { get; set; }

        public DateTime SliderLastused { get; set; }

        public int SpeedStep
        {
            get => speedStep; set
            {
                if (value >= 0 && value <= maxDccStep)
                {
                    speedStep = value == 1 ? (speedStep > 1 ? 0 : 2) : value;
                    OnPropertyChanged();
                    if (speedStep != (LokState.Fahrstufe))
                        SetLocoDrive(speedStep);
                }
            }
        }

        public bool Direction
        {
            get => direction; set
            {
                direction = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(GetDirectionString));
                SetLocoDrive(direction: value.GetDrivingDirection());
            }
        }

        public string GetDirectionString { get => Direction ? "Vorwärts" : "Rückwärts"; }

        private List<Button> FunctionButtons = new();

        private List<ToggleButton> FunctionToggleButtons = new();

        /// <summary>
        /// Data directly from the Z21. Not Used to controll the Loko. 
        /// </summary>
        public LokInfoData LokState
        {
            get => lokState; set
            {
                lokState = value;
                if (!SliderInUser && (DateTime.Now - SliderLastused).TotalSeconds > 2)
                {
                    SpeedStep = value.Fahrstufe;
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
                lastTrackPowerUpdateWasShort = (value == TrackPower.Short) ? true : false;

            }
        }

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
                lokState.Adresse = new((byte)Vehicle.Address);
                this.Title = $"{Vehicle.Address} - {(string.IsNullOrWhiteSpace(Vehicle.Name) ? Vehicle.Full_Name : Vehicle.Name)}";

                controler.OnGetLocoInfo += new EventHandler<GetLocoInfoEventArgs>(OnGetLocoInfoEventArgs);
                controler.OnTrackPowerOFF += new EventHandler(OnGetTrackPowerOffEventArgs);
                controler.OnTrackPowerON += new EventHandler(OnGetTrackPowerOnEventArgs);
                controler.OnTrackShortCircuit += new EventHandler(OnTrackShortCircuitEventArgs);
                controler.OnProgrammingMode += new EventHandler(OnProgrammingModeEventArgs);

                controler.GetLocoInfo(LokState.Adresse);
                controler.SetTrackPowerON();
                DrawAllFunctions();
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
                MessageBox.Show($"Beim öffnen des Controllers ist ein Fehler aufgetreten: {(string.IsNullOrWhiteSpace(ex.Message) ? "Keine Exception Message" : ex.Message)}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

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
                    var tb = new ToggleButton();
                    tb.Height = 50;
                    tb.Width = 90;
                    tb.Content = $"({item.FunctionIndex}) {item.Name}";
                    tb.Click += FunctionToggle_Click;
                    tb.Tag = item;
                    tb.Name = $"btn_Function_{item.Id}";
                    FunctionToggleButtons.Add(tb);
                    sp.Children.Add(tb);
                }
                else
                {
                    var btn = new Button();
                    btn.Height = 50;
                    btn.Width = 90;
                    btn.Content = $"({item.FunctionIndex}) {item.Name}";
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

        private void SetLocoDrive(int? speedstep = null, DrivingDirection? direction = null, bool? inUse = null) => controler.SetLocoDrive(new LokInfoData()
        {
            Adresse = new LokAdresse((int)vehicle.Address),
            Besetzt = inUse ?? LokState.Besetzt,
            drivingDirection = direction ?? LokState.drivingDirection,
            Fahrstufe = (byte)(speedstep ?? LokState.Fahrstufe)
        });

        private void SetLocoFunction(ToggleType type, Function function) => controler.SetLocoFunction(new LokAdresse((int)Vehicle.Address), function, type);

        #region Events
        public void OnGetLocoInfoEventArgs(Object? sender, GetLocoInfoEventArgs e)
        {
            if (e.Data.Adresse.Value == Vehicle.Address)
            {
                LokState = e.Data;
                if (Direction != e.Data.drivingDirection.ConvertToBool())
                {
                    Direction = e.Data.drivingDirection.ConvertToBool();
                }
                foreach (var item in e.Data.Functions)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        //var x = FunctionButtons.FirstOrDefault(e=> (e.Tag as Function).FunctionIndex == item.Item1);
                        var x = FunctionToggleButtons.FirstOrDefault(e => (e.Tag as Function).FunctionIndex == item.Item1);
                        if (x is not null)
                            x.IsChecked = item.Item2;
                    }));
                }
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

                if (this.IsActive)
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
                                SetLocoDrive(direction: LokState.drivingDirection.ConvertToBool() ? DrivingDirection.R : DrivingDirection.F);
                            break;
                        default:
                            var function = db.Functions.Where(e => e.VehicleId == vehicle.Id && e.EnumType == Function.Key).ToList();
                            if (function is not null)
                            {
                                foreach (var item in function)
                                {

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

        private void Mw_Closing(object sender, CancelEventArgs e)
        {
            SetLocoDrive(inUse: false);
            Joystick?.Dispose();
        }

        private void mw_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            SpeedStep = e.Delta < 0 ? SpeedStep - 1 : SpeedStep + 1;
            e.Handled = true;
            SliderLastused = DateTime.Now;
        }

        private void mw_Activated(object sender, EventArgs e) => IsActive = true;

        private void mw_Deactivated(object sender, EventArgs e) => IsActive = false;
    }
}
