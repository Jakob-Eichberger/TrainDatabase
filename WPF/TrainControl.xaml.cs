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
        private LokInfoData lok_CurrentState = new();

        public event PropertyChangedEventHandler? PropertyChanged;

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
                lokInfoData = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Data directly from the Z21. Not Used to controll the Loko.
        /// </summary>
        public LokInfoData Lok_CurrentState
        {
            get => lok_CurrentState; set
            {
                lok_CurrentState = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// This is the speed the Locomotive should have. But it is not the speed the Locomotive is actually at. For that please us the <see cref="LokInfoData.fahrstufe"/> property.
        /// </summary>
        public int TargetSpeed { get; set; } = 0;

        /// <summary>
        /// This is the Breaking Mode the Loco is at. 
        /// </summary>

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
                Lok_FutureState.drivingDirection = DrivingDirection.R;
                Lok_FutureState.Besetzt = false;

                this.controler.OnGetLocoInfo += new EventHandler<GetLocoInfoEventArgs>(OnGetLocoInfoEventArgs);
                this.controler.GetLocoInfo(Lok_FutureState.Adresse);

            }
            catch (Exception ex)
            {
                Logger.Log($"{DateTime.UtcNow}: '{ex?.Message ?? "Keine Exception Message"}' - Inner Exception: {ex?.InnerException?.Message ?? "Keine Inner Exception"}", LoggerType.Error);
                MessageBox.Show($"Beim öffnen des Controlls ist ein Fehler aufgetreten: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void OnGetLocoInfoEventArgs(Object sender, GetLocoInfoEventArgs e)
        {
            if (e.Data.Adresse.Value == vehicle.Address)
            {
                Lok_CurrentState = e.Data;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void TargetSpeed_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Lok_FutureState.Fahrstufe = (byte)SliderTargetSpeed.Value;
            controler.SetLocoDrive(Lok_FutureState);
        }
    }
}
