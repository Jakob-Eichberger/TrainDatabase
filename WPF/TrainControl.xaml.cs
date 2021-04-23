using Exceptions;
using Extensions;
using Helper;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Model;
using ModelTrainController.Z21;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WPF_Application
{

    public enum Weight { light = 0, medium = 1, heavy = 2 }


    /// <summary>
    /// Interaction logic for TrainControl.xaml
    /// </summary>
    public partial class TrainControl : Window, INotifyPropertyChanged
    {
        public ModelTrainController.ModelTrainController controler = default!;
        public Vehicle vehicle;
        private LokInfoData lokState = new();
        public Database db = new();
        private int maxDccStep = 126;
        private bool direction = true;
        private int speedStep;
        public event PropertyChangedEventHandler? PropertyChanged;

        public bool SliderInUser { get; set; }

        public int SpeedStep
        {
            get => speedStep; set
            {
                if (value != 1)
                {
                    speedStep = value;
                    OnPropertyChanged();
                    if (speedStep != (LokState.Fahrstufe))
                        SetSpeed(value);
                }
            }
        }

        private void SetSpeed(int speedstep)
        {
            LokInfoData info = LokState;
            if (info is null) return;
            info.Fahrstufe = (byte)speedStep;
            controler.SetLocoDrive(info);
        }

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
                if (!SliderInUser)
                {
                    SpeedStep = value.Fahrstufe;
                }
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// This is the speed the Locomotive should have. But it is not the speed the Locomotive is actually at. For that please us the <see cref="LokInfoData.fahrstufe"/> property.
        /// </summary>
        public int TargetSpeed { get; set; } = 0;

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

        public TrainControl(ModelTrainController.ModelTrainController _controler, Vehicle _vehicle, Database db)
        {
            try
            {
                if (_controler is null) throw new NullReferenceException($"Parameter {nameof(_controler)} ist null!");
                if (_vehicle is null) throw new NullReferenceException($"Paramter{nameof(_vehicle)}ist null!");
                InitializeComponent();
                this.DataContext = this;
                controler = _controler!;
                vehicle = _vehicle!;
                lokState.Adresse = new((byte)vehicle.Address);
                this.Title = $"{vehicle.Address} - {(string.IsNullOrWhiteSpace(vehicle.Name) ? vehicle.Full_Name : vehicle.Name)}";
                controler.OnGetLocoInfo += new EventHandler<GetLocoInfoEventArgs>(OnGetLocoInfoEventArgs);
                controler.GetLocoInfo(LokState.Adresse);
                DrawAllFunctions();
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
            foreach (var item in (db.Functions.Where(e => e.VehicleId == vehicle.Id).OrderBy(e => e.Position).ToList() ?? new()))
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
                    tb.Content = string.IsNullOrWhiteSpace(item.Shortcut) ? item.ImageName : item.Shortcut;
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
                    btn.Content = string.IsNullOrWhiteSpace(item.Shortcut) ? item.ImageName : item.Shortcut;
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

        public void OnGetLocoInfoEventArgs(Object? sender, GetLocoInfoEventArgs e)
        {
            if (e.Data.Adresse.Value == vehicle.Address)
            {
                LokState = e.Data;
                if (Direction != e.Data.drivingDirection.ConvertToBool())
                {
                    Direction = e.Data.drivingDirection.ConvertToBool();
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        void FunctionToggle_Click(Object sender, RoutedEventArgs e)
        {

            var func = (e.Source as ToggleButton)?.Tag as Function;
            if ((sender as ToggleButton).IsChecked ?? false)
            {
                controler.SetLocoFunction(new LokAdresse((int)vehicle.Address), func, ToggleType.on);
            }
            else
            {
                controler.SetLocoFunction(new LokAdresse((int)vehicle.Address), func, ToggleType.off);
            }
        }

        void FunctionButtonDown_Click(Object sender, RoutedEventArgs e)
        {
            var func = (e.Source as Button)?.Tag as Function;
            controler.SetLocoFunction(new LokAdresse((int)vehicle.Address), func, ToggleType.on);
        }

        void FunctionButtonUp_Click(Object sender, RoutedEventArgs e)
        {
            var func = (e.Source as Button)?.Tag as Function;
            controler.SetLocoFunction(new LokAdresse((int)vehicle.Address), func, ToggleType.off);
        }

        private void BtnDirection_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Mw_Closing(object sender, CancelEventArgs e)
        {
            LokState.Besetzt = false;
            controler.SetLocoDrive(LokState);
        }

        private void SliderSpeed_PreviewMouseDown(object sender, MouseButtonEventArgs e) => SliderInUser = true;

        private void SliderSpeed_PreviewMouseUp(object sender, MouseButtonEventArgs e) => SliderInUser = false;
    }
}
