using Exceptions;
using Helper;
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
        public ModelTrainController.ModelTrainController controler;
        public Vehicle vehicle;

        private LokInfoData lokInfoData = new();
        private int acceleration = 0;
        private BreakingMode breaking = BreakingMode.FA;
        private Weight weight = 0;
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Represents the State the Loko will be in. 
        /// </summary>
        private LokInfoData Lok_FutureState
        {
            get
            {
                return lokInfoData;
            }
            set
            {
                if (value is not null && value.Fahrstufe == 0)
                {
                    btnDirection.IsEnabled = true;
                }
                else
                {
                    btnDirection.IsEnabled = false;
                }
                lokInfoData = value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Data directly from the Z21. Not Used to controll the Loko.
        /// </summary>
        public LokInfoData Lok_CurrentState { get; set; } = new();

        public Dictionary<int, string> MyProperty { get; set; }

        /// <summary>
        /// This is the speed the Locomotive should have. But it is not the speed the Locomotive is actually at. For that please us the <see cref="LokInfoData.fahrstufe"/> property.
        /// </summary>
        public int TargetSpeed { get; set; } = 0;

        /// <summary>
        /// This is the ammount the Loco should accelerate at.
        /// </summary>
        public int Acceleration
        {
            get => acceleration;
            set
            {
                if (Breaking == BreakingMode.FA)
                {
                    acceleration = value;
                }
                else
                {
                    acceleration = 0;
                }
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// This is the Breaking Mode the Loco is at. 
        /// </summary>
        public BreakingMode Breaking
        {
            get => breaking;
            set
            {
                if (value > 0) Acceleration = 0;
                if (value == BreakingMode.VB)
                {
                    TargetSpeed = 0;
                }
                breaking = value;
                OnPropertyChanged();
            }
        }

        public Weight Weight { get => weight; set => weight = value; }

        public int MaxAcceleration { get; set; } = 126;

        public TrainControl(ModelTrainController.ModelTrainController controler, Vehicle _vehicle)
        {
            try
            {

                DataContext = this;
                InitializeComponent();
                if (controler is null | _vehicle is null) return;
                this.controler = controler;
                vehicle = _vehicle;
                this.Title = vehicle.FullName;
                Lok_FutureState.Adresse = new((int)(vehicle?.Address ?? throw new ControlerException(this.controler, $"Addresse '{vehicle?.Address.ToString() ?? ""}' ist keine valide Addresse!")));
                this.controler.OnGetLocoInfo += new EventHandler<GetLocoInfoEventArgs>(OnGetLocoInfoEventArgs);
                this.controler.GetLocoInfo(Lok_FutureState.Adresse);

                PBSpeed.Maximum = 126;
                lblMaxSpeed.Content = 126;

                SetLocoDrive();
            }
            catch (Exception ex)
            {
                Logger.Log($"{DateTime.UtcNow}: '{ex?.Message ?? "Keine Exception Message"}' - Inner Exception: {ex?.InnerException?.Message ?? "Keine Inner Exception"}", LoggerType.Error);
                MessageBox.Show($"Beim öffnen des Controlls ist ein Fehler aufgetreten: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GetNextSpeedStep()
        {
            var x = Lok_CurrentState.Fahrstufe - Lok_FutureState.Fahrstufe;
            return 0;
        }


        private void SetLocoDrive()
        {
            Thread th = new(() =>
            {
                while (true)
                {
                    Lok_FutureState.Fahrstufe = (byte)GetNextSpeedStep();
                    controler.GetLocoInfo(Lok_FutureState.Adresse);
                    //if (CurrentLokInfoData.drivingDirection is not DrivingDirection.N)
                    //z21.SetLocoDrive();
                    Thread.Sleep(1000);
                }
            });
            th.Start();
        }


        public void OnGetLocoInfoEventArgs(Object sender, GetLocoInfoEventArgs e)
        {
            if (e.Data.Adresse.Value == vehicle.Address)
            {
                Lok_CurrentState = e.Data;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    PBSpeed.Value = e.Data.Fahrstufe - 1 < 0 ? 0 : e.Data.Fahrstufe - 1;
                    lblCurrentSpeed.Content = e.Data.Fahrstufe - 1 < 0 ? 0 : e.Data.Fahrstufe - 1;
                }));
            }
        }


        private void BtnDirection_Click(object sender, RoutedEventArgs e)
        {
            if (Lok_CurrentState.Fahrstufe == 0)
            {
                switch (Lok_FutureState.drivingDirection)
                {
                    case DrivingDirection.R:
                        Lok_FutureState.drivingDirection = DrivingDirection.N;
                        break;
                    case DrivingDirection.N:
                        Lok_FutureState.drivingDirection = DrivingDirection.F;
                        break;
                    case DrivingDirection.F:
                        Lok_FutureState.drivingDirection = DrivingDirection.R;
                        break;
                }
                //z21.SetLocoDrive(TargetLokInfoData);
            }
        }


        private void BtnSifa_Click(object sender, RoutedEventArgs e)
        {
            controler.SetTrackPowerOFF();
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.W)
            {
                Acceleration += 2;
                Acceleration = Acceleration >= MaxAcceleration ? MaxAcceleration : Acceleration;
            }
            if (e.Key == Key.S)
            {
                Acceleration -= 2;
                Acceleration = Acceleration <= 0 ? 0 : Acceleration;
            }
            if (e.Key == Key.Up)
            {

            }
            if (e.Key == Key.Down)
            {

            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
