using Exceptions;
using Helper;
using Model;
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
        public Z21 z21;
        public Vehicle vehicle;

        private LokInfoData lokInfoData = new();
        private int acceleration = 0;
        private BreakingMode breaking = BreakingMode.FA;
        private Weight weight = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Represents the states the Loko will be in eventually.
        /// </summary>
        private LokInfoData TargetLokInfoData
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
            }
        }

        /// <summary>
        /// Data directly from the Z21. Represents the actually state of the Locomotive.
        /// </summary>
        public LokInfoData CurrentLokInfoData { get; set; } = new();

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
            }
        }

        public Weight Weight { get => weight; set => weight = value; }


        public TrainControl(Z21 _z21, Vehicle _vehicle)
        {
            try
            {
                DataContext = this;
                InitializeComponent();
                z21 = _z21;
                vehicle = _vehicle;
                this.Title = vehicle.FullName;
                TargetLokInfoData.Adresse = new((int)(vehicle?.Address ?? throw new Z21Exception(z21, $"Addresse '{vehicle?.Address.ToString() ?? ""}' ist keine valide Addresse!")));


                z21.OnGetLocoInfo += new EventHandler<GetLocoInfoEventArgs>(OnGetLocoInfoEventArgs);
                z21.GetLocoInfo(TargetLokInfoData.Adresse);

                PBSpeed.Maximum = 126;
                lblMaxSpeed.Content = 126;

                PBAcceleration.Maximum = 7000;
                SetLocoDrive();
            }
            catch (Exception ex)
            {
                Logger.Log($"{DateTime.UtcNow}: '{ex?.Message ?? "Keine Exception Message"}' - Inner Exception: {ex?.InnerException?.Message ?? "Keine Inner Exception"}", LoggerType.Error);
                MessageBox.Show($"Beim öffnen des Controlls ist ein Fehler aufgetreten: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private byte GetNextSpeedStep()
        {
            return 0;
        }


        private void SetLocoDrive()
        {
            Thread th = new(() =>
            {
                while (true)
                {
                    TargetLokInfoData.Fahrstufe = GetNextSpeedStep();
                    z21.GetLocoInfo(TargetLokInfoData.Adresse);
                    //if (CurrentLokInfoData.drivingDirection is not DrivingDirection.N)
                    //    z21.SetLocoDrive();
                    Thread.Sleep(1000);
                }
            });
            th.Start();
        }


        public void OnGetLocoInfoEventArgs(Object sender, GetLocoInfoEventArgs e)
        {
            if (e.Data.Adresse.Value == vehicle.Address)
            {
                CurrentLokInfoData = e.Data;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    PBSpeed.Value = e.Data.Fahrstufe - 1 < 0 ? 0 : e.Data.Fahrstufe - 1;
                    lblCurrentSpeed.Content = e.Data.Fahrstufe - 1 < 0 ? 0 : e.Data.Fahrstufe - 1;
                }));
            }
        }


        private void BtnDirection_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentLokInfoData.Fahrstufe == 0)
            {
                switch (TargetLokInfoData.drivingDirection)
                {
                    case DrivingDirection.R:
                        TargetLokInfoData.drivingDirection = DrivingDirection.N;
                        break;
                    case DrivingDirection.N:
                        TargetLokInfoData.drivingDirection = DrivingDirection.F;
                        break;
                    case DrivingDirection.F:
                        TargetLokInfoData.drivingDirection = DrivingDirection.R;
                        break;
                }
                btnDirection.Content = TargetLokInfoData.drivingDirection;
                //z21.SetLocoDrive(TargetLokInfoData);
            }
        }



        private void BtnSifa_Click(object sender, RoutedEventArgs e)
        {
            z21.SetTrackPowerOFF();
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.W)
            {
                Acceleration += 2;
                Acceleration = Acceleration >= 7000 ? 7000 : Acceleration;
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

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
