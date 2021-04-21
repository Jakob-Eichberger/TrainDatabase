using Exceptions;
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
        private LokInfoData lok_FutureState = new();
        private LokInfoData lok_CurrentState = new();
        public Database db = new();
        private int maxAcceleration = 127;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Represents the State the Loko will be in. 
        /// </summary>
        private LokInfoData Lok_FutureState
        {
            get
            {
                return lok_FutureState;
            }
            set
            {
                lok_FutureState = value;
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
        /// This is the Max Acceleration Step.
        /// </summary>
        public int MaxAcceleration
        {
            get => maxAcceleration; set
            {
                maxAcceleration = value;
                OnPropertyChanged();
            }
        }

        public TrainControl(ModelTrainController.ModelTrainController controler, Vehicle _vehicle)
        {
            try
            {
                if (controler is null) throw new NullReferenceException($"Parameter {nameof(controler)} ist null!");
                if (_vehicle is null) throw new NullReferenceException($"Paramter{nameof(_vehicle)}ist null!");
                DataContext = this;
                InitializeComponent();
                if (controler is null | _vehicle is null) return;
                this.controler = controler!;
                vehicle = _vehicle!;
                this.Title = vehicle.FullName;
                Lok_FutureState.Adresse = new((int)(vehicle?.Address ?? throw new ControlerException(this.controler, $"Addresse '{vehicle?.Address.ToString() ?? ""}' ist keine valide Addresse!")));
                Lok_FutureState.drivingDirection = DrivingDirection.R;
                Lok_FutureState.Besetzt = false;

                this.controler.OnGetLocoInfo += new EventHandler<GetLocoInfoEventArgs>(OnGetLocoInfoEventArgs);
                this.controler.GetLocoInfo(Lok_FutureState.Adresse);

                for (int item = 0; item < 50; item++)
                {
                    vehicle.Functions.Add(new() { FunctionIndex = item, ImageName = $"Func {item}" });
                }
                DrawAllFunctions();
            }
            catch (Exception ex)
            {
                Close();
                Logger.Log($"{DateTime.UtcNow}: '{ex?.Message ?? "Keine Exception Message"}' - Inner Exception: {ex?.InnerException?.Message ?? "Keine Inner Exception"}", LoggerType.Error);
                MessageBox.Show($"Beim öffnen des Controlls ist ein Fehler aufgetreten: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void FunctionToggle_Click(Object sender, RoutedEventArgs e)
        {
            try
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
            catch
            {

            }
        }

        public void DrawAllFunctions()
        {
            FunctionGrid.Children.Clear();
            foreach (var item in vehicle?.Functions ?? new())
            {
                //if (item is null) return;
                Border border = new();
                border.Padding = new(2);
                border.Margin = new(10);
                border.BorderThickness = new(1);
                border.BorderBrush = Brushes.Black;

                StackPanel sp = new();


                var tb = new ToggleButton();
                tb.Height = 25;
                tb.Width = 50;
                tb.Content = item?.ImageName ?? "-";
                tb.Click += FunctionToggle_Click;
                tb.Tag = item;
                sp.Children.Add(tb);
                border.Child = sp;
                ////try
                ////{
                ////    string path = $"{Directory.GetCurrentDirectory()}\\Data\\VehicleImage\\";
                ////    path += string.IsNullOrWhiteSpace(item?.ImageName) ? "default.png" : item?.ImageName;
                ////    Image image = new();
                ////    BitmapImage bitmap = new();
                ////    bitmap.BeginInit();
                ////    bitmap.UriSource = new(path);
                ////    bitmap.EndInit();
                ////    image.Source = bitmap;
                ////    image.Width = 250;
                ////    image.Height = 100;
                ////    sp.Children.Add(image);
                ////}
                ////catch (Exception ex)
                ////{
                ////    Logger.Log($"{DateTime.UtcNow}: Image for Lok with adress '{item?.Address}' not found. Message: {ex.Message}", LoggerType.Warning);
                ////}
                //TextBlock x = new();
                //x.Text = !string.IsNullOrWhiteSpace(item?.FullName) ? item?.FullName : (!string.IsNullOrWhiteSpace(item?.Name) ? item?.Name : $"Adresse: {item?.Address}");
                //sp.Height = 25;
                //sp.Width = 50;
                //sp.Children.Add(x);
                sp.HorizontalAlignment = HorizontalAlignment.Left;
                sp.VerticalAlignment = VerticalAlignment.Top;

                //sp.ContextMenu = new();

                //MenuItem miControlLoko = new();
                //miControlLoko.Header = item.Type == VehilceType.Lokomotive ? "Lok steuern" : (item.Type == VehilceType.Steuerwagen ? "Steuerwagen steuern" : "Wagen steuern");
                ////miControlLoko.Click += ControlLoko_Click;
                //sp.ContextMenu.Items.Add(miControlLoko);
                //miControlLoko.Tag = item;
                //MenuItem miEditLoko = new();
                //miEditLoko.Header = item.Type == VehilceType.Lokomotive ? "Lok bearbeiten" : (item.Type == VehilceType.Steuerwagen ? "Steuerwagen bearbeiten" : "Wagen bearbeiten");
                ////miEditLoko.Click += EditLoko_Click;

                //sp.ContextMenu.Items.Add(miEditLoko);
                //border.Child = sp;
                FunctionGrid.Children.Add(border);
            }
        }

        public void OnGetLocoInfoEventArgs(Object? sender, GetLocoInfoEventArgs e)
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

        private void BtnDirection_Click(object sender, RoutedEventArgs e)
        {
            var temp = Lok_CurrentState.Fahrstufe;
            //Lok_FutureState.Fahrstufe = 0;
            Lok_FutureState.drivingDirection = DrivingDirection.F == Lok_CurrentState.drivingDirection ? DrivingDirection.R : DrivingDirection.F;
            BtnDirection.Content = DrivingDirection.F == Lok_CurrentState.drivingDirection ? "Vorwärts" : "Rückwärts";
            controler.SetLocoDrive(Lok_FutureState);
            //Lok_FutureState.Fahrstufe = temp;
            //controler.SetLocoDrive(Lok_FutureState);
        }
    }
}
