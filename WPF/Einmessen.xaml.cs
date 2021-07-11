using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using Model;
using ModelTrainController;
using ModelTrainController.Z21;
using OxyPlot;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WPF_Application.Extensions;
using WPF_Application.Helper;
using WPF_Application.Infrastructure;
using LineSeries = OxyPlot.Series.LineSeries;
using LinearAxis = OxyPlot.Axes.LinearAxis;
using Renci.SshNet;


namespace WPF_Application
{
    /// <summary>
    /// Interaction logic for Einmessen.xaml
    /// </summary>
    public partial class Einmessen : Window, INotifyPropertyChanged
    {
        public readonly Database _db;
        public event PropertyChangedEventHandler? PropertyChanged;
        public readonly CentralStationClient _controller;
        private readonly SshClient _sshClient = new("192.168.0.73", "pi", "raspberry");
        //private readonly SshClient _sshClient = new(Settings.Get("RaspberryPiHost"), Settings.Get("RaspberryPiUsername"), Settings.Get("RaspberryPiPassword"));
        private Vehicle _vehicle = default!;
        private LokInfoData _lokInfo = new();
        public List<DataPoint> PointsBackward { get; private set; } = new();
        public List<DataPoint> PointsForward { get; private set; } = new();
        private bool direction = false;
        private bool IsDisposed = false;

        public int DistanceBetweenSensors_Measurement
        {
            get
            {
                return Settings.GetInt(nameof(DistanceBetweenSensors_Measurement)) ?? 1;
            }
            set
            {
                Settings.Set(nameof(DistanceBetweenSensors_Measurement), value.ToString());
            }
        }

        decimal?[] TractionForward { get; set; } = new decimal?[128];

        decimal?[] TractionBackward { get; set; } = new decimal?[128];

        public int Start_Measurement { get { return Settings.GetInt(nameof(Start_Measurement)) ?? 2; } set { Settings.Set(nameof(Start_Measurement), value.ToString()); } }

        public int Step_Measurement
        {
            get => Settings.GetInt(nameof(Step_Measurement)) ?? 1;
            set { Settings.Set(nameof(Step_Measurement), value.ToString()); OnPropertyChanged(); }
        }

        public Vehicle Vehicle
        {
            get => _vehicle; set
            {
                _vehicle = value;
                OnPropertyChanged();
            }
        }

        public Einmessen(Database db, CentralStationClient controller)
        {
            this.DataContext = this;
            InitializeComponent();

            if (db is null) throw new ApplicationException($"Paramter '{nameof(db)}' darf nicht null sein!");
            if (controller is null) throw new ApplicationException($"Paramter '{nameof(controller)}' darf nicht null sein!");
            this._db = db;
            this._controller = controller;
            _controller.OnGetLocoInfo += OnGetLocoInfoEventArgs;

            FillPointsList();
            DrawTable();
            PlotSpeedData();
        }

        protected void OnPropertyChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        private void FillPointsList()
        {
            PointsBackward = new();
            PointsForward = new();
            for (int i = 2; i <= 127; i++)
            {
                if (TractionBackward[i] is not null)
                    PointsBackward.Add(new(i, (double)(TractionBackward[i] ?? 0)));
            }
            for (int i = 2; i <= 127; i++)
            {
                if (TractionForward[i] is not null)
                    PointsForward.Add(new(i, (double)(TractionForward[i] ?? 0)));
            }
        }

        private void DrawTable()
        {
            sp_Table.Children.Clear();
            Grid functionGrid = new();
            functionGrid.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            functionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            functionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            functionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            //functionGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Action<string, string, string> CreatSpeedTableRow = (text1, text2, text3) =>
            {
                int row = functionGrid.RowDefinitions.Count;
                functionGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                Label step = new() { Content = text1, Margin = new Thickness(0, 0, 2, 1), HorizontalAlignment = System.Windows.HorizontalAlignment.Left };
                Grid.SetColumn(step, 0);
                Grid.SetRow(step, row);
                functionGrid.Children.Add(step);

                Label forward = new() { Content = text2, Margin = new Thickness(0, 0, 2, 1), HorizontalAlignment = System.Windows.HorizontalAlignment.Left };
                Grid.SetColumn(forward, 1);
                Grid.SetRow(forward, row);
                functionGrid.Children.Add(forward);

                Label backward = new() { Content = text3, Margin = new Thickness(0, 0, 2, 1), HorizontalAlignment = System.Windows.HorizontalAlignment.Left };
                Grid.SetColumn(backward, 2);
                Grid.SetRow(backward, row);
                functionGrid.Children.Add(backward);
            };

            CreatSpeedTableRow($"Step", $"M/S (V)", $"M/S (R)");
            bool lastStep = false;
            for (int i = Start_Measurement; i <= 127; i += Step_Measurement)
            {
                CreatSpeedTableRow($"Step {i}", $"{(TractionForward[i] is null ? "-" : TractionForward[i])} m/s", $"{(TractionBackward[i] is null ? "-" : TractionBackward[i])}  m/s");
                if (!lastStep && i + Step_Measurement > 127) { i = (127 - Step_Measurement); lastStep = true; }
            }


            sp_Table.Children.Add(functionGrid);
        }

        private async void GetSpeed(int speed, bool direction)
        {
            lblStatus.Content = $"{speed} - {direction}";
            decimal result = await GetTimeBetweenSensors(speed, direction);
            decimal mps = Math.Round(((DistanceBetweenSensors_Measurement / 100.0m) / result) * 87);
            SetTractionSpeed(speed, direction, mps);
        }

        /// <summary>
        /// Starts the process of messuring the lokospeed.
        /// </summary>
        private async void Run()
        {
            try
            {
                Log("Starting...");
                bool lastStep = false;
                bool direction = true;
                for (int speed = Start_Measurement; speed <= 127; speed += Step_Measurement)
                {
                    GetSpeed(speed, direction);
                    GetSpeed(speed, !direction);
                    if (!lastStep && speed + Step_Measurement > 127) { speed = (127 - Step_Measurement); lastStep = true; }
                }
                Vehicle.TractionForward = TractionForward;
                Vehicle.TractionBackward = TractionBackward;

                direction = false;
                for (int i = 0; i < 2; i++)
                {
                    direction = !direction;
                    await KillPythonScript();
                    await SetLocoDrive(40, direction);
                    await Task.Run(() => { _sshClient.RunCommand("python /home/pi/Desktop/GetTimeBetweenSensors.py"); });
                    await SetLocoDrive(0, direction);
                }
            }
            catch (OperationCanceledException)
            {
                TractionForward = Vehicle.TractionForward;
                TractionBackward = Vehicle.TractionBackward;
                FillPointsList();
                DrawTable();
                PlotSpeedData();
            }
            catch { throw; }
            finally
            {
                await SetLocoDrive(0, true);
                await KillPythonScript();
                _sshClient.Disconnect();
                btnStart.IsEnabled = true;
                btnStop.IsEnabled = false;
                DrawTable();
                PlotSpeedData();
                TabTraktionSettings.IsEnabled = true;
            }

        }

        private async Task<decimal> GetTimeBetweenSensors(int speed, bool direction)
        {
            try
            {
                if (IsDisposed) throw new OperationCanceledException();
                await KillPythonScript();
                await SetLocoDrive(speed, direction);
                SshCommand result = default!;
                await Task.Run(() => { result = _sshClient.RunCommand("python /home/pi/Desktop/GetTimeBetweenSensors.py"); });
                if (result.ExitStatus != 0) throw new ApplicationException("Script am Pi ist unerwarted abgestürtzt.");
                if (!decimal.TryParse(result.Result, out decimal mps)) throw new ApplicationException($"Wert vom Raspberry Pi '{result.Result}' konte nicht als decimal geparsed werden.");
                await Task.Delay((int)new TimeSpan(0, 0, (int)(14 - (speed * 0.1))).TotalMilliseconds);
                await SetLocoDrive(0, direction);
                await Task.Delay((int)new TimeSpan(0, 0, 2).TotalMilliseconds);
                return mps;
            }
            catch
            {
                throw;
            }
            finally
            {
                await KillPythonScript();
            }
        }

        private async Task KillPythonScript() => await Task.Run(() => { _sshClient.RunCommand("pkill -9 -f GetTimeBetweenSensors.py"); });

        private async Task SetLocoDrive(int speed, bool direction)
        {
            Log($"Speed: {speed} - Direction: {direction}");
            _controller.SetLocoDrive(new() { Adresse = new(Vehicle.Address), DrivingDirection = direction, InUse = true, Speed = speed });
            await CheckSpeed(speed, direction);
        }

        /// <summary>
        /// Checks if the speed reported by the controller is the same as the one set by the  <see cref="SetLocoDrive(int, bool)"/> function.
        /// If it is the same nothing happens, if it differs the <see cref="SetLocoDrive(int, bool)"/> function gets called again with the same parameters. Needed to midigate the package loss problem.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="direction"></param>
        /// <returns>Returns a <see cref="Task"/> that can be awaited.</returns>
        /// <remarks>The Function waits 2 seconds before it checks if the speed matches.</remarks>
        private async Task CheckSpeed(int speed, bool direction)
        {
            await Task.Delay(new TimeSpan(0, 0, 0, 1, 500));
            if (!(speed == _lokInfo.Speed && _lokInfo.DrivingDirection == direction))
            {
                Log("Speed check failed. Trying again. ...");
                await SetLocoDrive(speed, direction);
            }
        }

        private void SetTractionSpeed(int speedStep, bool direction, decimal speed)
        {
            if (speedStep < 2 || 127 < speedStep) throw new ApplicationException($"SpeedStep von '{speedStep}' nicht zulässig.");
            if (speed < 0) throw new ApplicationException($"Geschwindigkeit '{speed}' nicht zulässig!");
            if (direction)
            {
                TractionForward[speedStep] = speed;
                PointsForward.Add(new(speedStep, (double)speed));
            }
            else
            {
                TractionBackward[speedStep] = speed;
                PointsBackward.Add(new(speedStep, (double)speed));
            }
            DrawTable();
            PlotSpeedData();
            Log($"Loco drove {speed} m/s at dcc speed {speedStep} and direction {direction}.");
        }

        private void OnGetLocoInfoEventArgs(Object? sender, GetLocoInfoEventArgs e)
        {
            if (Vehicle is null) return;
            if (e.Data.Adresse.Value == Vehicle.Address)
            {
                _lokInfo = e.Data;
            }
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IsDisposed = true;
            await SetLocoDrive(0, false);
        }

        private void PlotSpeedData()
        {
            if (sp_Plot is null) return;
            PlotModel model = new();
            // Create the OxyPlot graph for Salt Split
            OxyPlot.Wpf.PlotView plot = new();
            plot.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            plot.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

            // Create new Line Series
            LineSeries linePoints_Forward = new LineSeries()
            { StrokeThickness = 1, MarkerSize = 1, Title = "Vorwärts", Color = OxyPlot.OxyColors.Red };
            // Add each point to the new series
            linePoints_Forward.Points.Clear();
            linePoints_Forward.Points.AddRange(PointsForward);
            LineSeries linePoints_Backward = new LineSeries()
            { StrokeThickness = 1, MarkerSize = 1, Title = "Rückwärts", Color = OxyPlot.OxyColors.Blue };
            // Add each point to the new series
            linePoints_Backward.Points.Clear();
            linePoints_Backward.Points.AddRange(PointsBackward);
            // Add Chart Title
            //model.Title = "Model Title";

            // Define X-Axis
            OxyPlot.Axes.LinearAxis Xaxis = new();
            Xaxis.Maximum = 127;
            Xaxis.Minimum = Start_Measurement;
            Xaxis.Position = OxyPlot.Axes.AxisPosition.Bottom;
            Xaxis.Title = "Dcc Speed Step";
            model.Axes.Add(Xaxis);

            //Define Y-Axis
            OxyPlot.Axes.LinearAxis Yaxis = new();
            Yaxis.MajorStep = 15;
            Yaxis.Maximum = 100;
            Yaxis.MaximumPadding = 0;
            Yaxis.Minimum = 0;
            Yaxis.MinimumPadding = 0;
            Yaxis.MinorStep = double.NaN;
            Yaxis.Title = "m/s";
            model.Axes.Add(Yaxis);

            model.Series.Add(linePoints_Backward);
            model.Series.Add(linePoints_Forward);

            plot.Model = model;
            //plot.InvalidatePlot(true);
            sp_Plot.Children.Clear();
            sp_Plot.Children.Add(plot);
        }

        private void Initialize()
        {
            IsDisposed = false;
            TabTraktionSettings.IsEnabled = false;
            btnStart.IsEnabled = false;
            btnStop.IsEnabled = true;
            Log("Initialize");
        }

        private async void Deinitialize()
        {
            await KillPythonScript();
            TabTraktionSettings.IsEnabled = true;
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
        }

        private async void BtnStartSpeedMeasurement_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Vehicle is null) { MessageBox.Show($"Fahrzeug mit Adresse {IUVehicleAdress.Value} wurde nicht gefunden!"); return; }
                Initialize();
                TractionForward = new decimal?[128];
                TractionBackward = new decimal?[128];
                DrawTable();

                _controller.SetTrackPowerON();
                _controller.GetLocoInfo(new(Vehicle.Address));
                await Task.Run(() => { _sshClient.Connect(); });
                PointsBackward = new();
                PointsForward = new();
                PlotSpeedData();
                Run();
            }
            catch (SshConnectionException) { MessageBox.Show($"Die ssh Verbindung zum Pi wurde terminiert!"); }
            catch (System.Net.Sockets.SocketException) { MessageBox.Show($"Verbindung zum Raspberry Pi konnte nicht hergestellt werden."); }
            catch (SshAuthenticationException) { MessageBox.Show($"Authentifizierung hat fehlgeschlagen."); }
            catch (Exception ex)
            {
                Logger.Log($"", true, ex);
                MessageBox.Show($"Es ist ein Fehler aufgetreten: {ex.Message}");
            }
            finally
            {
                Deinitialize();
            }
        }

        private async void BtnStopSpeedMeasurement_Click(object sender, RoutedEventArgs e)
        {
            IsDisposed = true;
            await KillPythonScript();
            this.Close();
        }

        private void IntegerUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            FillPointsList();
            DrawTable();
            PlotSpeedData();
        }

        private async void BtnCV6Go_Click(object sender, RoutedEventArgs e)
        {
            //await SetLocoDrive(50, false);
            //while (true)
            //{
            //    var direction = GetDirection();
            //    await SetLocoDrive(0, direction);
            //}
        }

        private void IUVehicleAdress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {

                Vehicle = _db?.Vehicles?.FirstOrDefault(e => e.Address == IUVehicleAdress.Value && e.Type == VehicleType.Lokomotive) ?? throw new ApplicationException();
                TractionForward = Vehicle.TractionForward;
                TractionBackward = Vehicle.TractionBackward;
            }
            catch (ApplicationException)
            {
                Vehicle = null!;
                TractionForward = new decimal?[128];
                TractionBackward = new decimal?[128];
            }
            finally
            {
                PlotSpeedData();
                DrawTable();
                FillPointsList();
            }
        }

        private void Log(string message)
        {
            Dispatcher.BeginInvoke(new Action(() => { lblLog.Text += message + "\n"; }));
        }
    }
}
