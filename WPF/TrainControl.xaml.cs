using Extensions;
using Helper;
using Infrastructure;
using Model;
using ModelTrainController;
using ModelTrainController.Z21;
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

namespace WPF_Application
{
    /// <summary>
    /// Interaction logic for TrainControl.xaml
    /// </summary>
    public partial class TrainControl : Window, INotifyPropertyChanged
    {
        public ModelTrainController.ModelTrainController controler = default!;
        private LokInfoData lokState = new();
        public Database db = new();
        private int maxDccStep = 126;
        private bool direction = true;
        private int speedStep;
        public event PropertyChangedEventHandler? PropertyChanged;
        private TrackPower trackPower;
        private Vehicle vehicle;
        private bool lastTrackPowerUpdateWasShort = false;
        private Helper.JoyStick? Joystick { get; }

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
                        SetLoco(value);
                }
            }
        }

        private void SetLoco(int? speedstep = null, DrivingDirection? direction = null, bool? inUse = null) => controler.SetLocoDrive(new LokInfoData()
        {
            Adresse = new LokAdresse((int)vehicle.Address),
            Besetzt = inUse ?? LokState.Besetzt,
            drivingDirection = direction ?? LokState.drivingDirection
        });

        public bool Direction
        {
            get => direction; set
            {
                direction = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(GetDirectionString));
                var temp = lokState;
                temp.drivingDirection = value.GetDrivingDirection();
                controler.SetLocoDrive(temp);
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

        public TrainControl(ModelTrainController.ModelTrainController _controler, Vehicle _vehicle, Database db)
        {
            try
            {
                if (_controler is null) throw new NullReferenceException($"Parameter {nameof(_controler)} ist null!");
                if (_vehicle is null) throw new NullReferenceException($"Paramter{nameof(_vehicle)}ist null!");
                InitializeComponent();
                this.DataContext = this;
                controler = _controler!;
                Vehicle = _vehicle!;
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
                if (Vehicle.Type.IsLokomotive())
                    Joystick = new(Guid.Empty);
                if (Joystick is not null)
                {
                    Joystick.OnValueUpdate += new EventHandler<JoyStickUpdateEventArgs>(OnJoyStickValueUpdate);
                    Joystick.Acquire();
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
            foreach (var item in (db.Functions.Where(e => e.VehicleId == Vehicle.Id).OrderBy(e => e.FunctionIndex).ToList() ?? new()))
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
                    tb.Content = $"({item.FunctionIndex}) {(string.IsNullOrWhiteSpace(item.Shortcut) ? item.ImageName : item.Shortcut)}";
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
                    btn.Content = $"({item.FunctionIndex}) {(string.IsNullOrWhiteSpace(item.Shortcut) ? item.ImageName : item.Shortcut)}";
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
                    SliderLastused = DateTime.Now;
                    SpeedStep = MaxDccStep - ((e.currentValue * MaxDccStep) / e.maxValue);
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
        void FunctionToggle_Click(Object sender, RoutedEventArgs e)
        {

            var func = (e.Source as ToggleButton)?.Tag as Function;
            if ((sender as ToggleButton).IsChecked ?? false)
            {
                controler.SetLocoFunction(new LokAdresse((int)Vehicle.Address), func, ToggleType.on);
            }
            else
            {
                controler.SetLocoFunction(new LokAdresse((int)Vehicle.Address), func, ToggleType.off);
            }
        }

        void FunctionButtonDown_Click(Object sender, RoutedEventArgs e)
        {
            var func = (e.Source as Button)?.Tag as Function;
            controler.SetLocoFunction(new LokAdresse((int)Vehicle.Address), func, ToggleType.on);
        }

        void FunctionButtonUp_Click(Object sender, RoutedEventArgs e)
        {
            var func = (e.Source as Button)?.Tag as Function;
            controler.SetLocoFunction(new LokAdresse((int)Vehicle.Address), func, ToggleType.off);
        }

        private void BtnDirection_Click(object sender, RoutedEventArgs e)
        {
        }
        #endregion

        #region Preview Events
        private void SliderSpeed_PreviewMouseDown(object sender, MouseButtonEventArgs e) => SliderInUser = true;

        private void SliderSpeed_PreviewMouseUp(object sender, MouseButtonEventArgs e) => SliderInUser = false;
        #endregion

        private void Mw_Closing(object sender, CancelEventArgs e)
        {
            LokState.Besetzt = false;
            controler.SetLocoDrive(LokState);
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
