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
        public event PropertyChangedEventHandler? PropertyChanged;
        private Vehicle _vehicle = default!;

        //private readonly SshClient _sshClient = new(Settings.Get("RaspberryPiHost"), Settings.Get("RaspberryPiUsername"), Settings.Get("RaspberryPiPassword"));

        public Database Db { get; } = default!;

        private CentralStationClient Controller { get; } = default!;

        private SshClient SshClient { get; } = new("192.168.0.73", "pi", "raspberry");

        private LokInfoData LokData { get; set; } = new();

        public List<DataPoint> PointsBackward { get; private set; } = new();

        public List<DataPoint> PointsForward { get; private set; } = new();

        private bool IsDisposed = false;

        /// <summary>
        /// Holds the distance between the two raspberry pi sensors in mm.
        /// </summary>
        public decimal DistanceBetweenSensorsInMM
        {
            get => Settings.GetDecimal(nameof(DistanceBetweenSensorsInMM)) ?? 1.0m;
            set => Settings.Set(nameof(DistanceBetweenSensorsInMM), value.ToString());
        }

        private decimal?[] TractionForward { get; set; } = new decimal?[CentralStationClient.maxDccStep + 1];

        private decimal?[] TractionBackward { get; set; } = new decimal?[CentralStationClient.maxDccStep + 1];

        public int Start_Measurement { get => Settings.GetInt(nameof(Start_Measurement)) ?? 2; set { Settings.Set(nameof(Start_Measurement), value.ToString()); } }

        public int Step_Measurement
        {
            get => Settings.GetInt(nameof(Step_Measurement)) ?? 1;
            set { Settings.Set(nameof(Step_Measurement), value.ToString()); OnPropertyChanged(); }
        }

        private Vehicle Vehicle
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
            if (db is null) throw new ApplicationException($"Paramter '{nameof(db)}' darf nicht null sein!");
            if (controller is null) throw new ApplicationException($"Paramter '{nameof(controller)}' darf nicht null sein!");
            this.Db = db;
            this.Controller = controller;
            InitializeComponent();
            Controller.OnGetLocoInfo += OnGetLocoInfoEventArgs;
        }

        protected void OnPropertyChanged() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));

        private async Task FillDataPointsList() => await Task.Run(() =>
        {
            PointsBackward = new();
            PointsForward = new();
            for (int i = 2; i <= CentralStationClient.maxDccStep; i++)
            {
                if (TractionBackward[i] is not null)
                    PointsBackward.Add(new(i, (double)Math.Round((TractionBackward[i] / 3.6m) ?? 0, 2)));
            }
            for (int i = 2; i <= CentralStationClient.maxDccStep; i++)
            {
                if (TractionForward[i] is not null)
                    PointsForward.Add(new(i, (double)Math.Round((TractionForward[i] / 3.6m) ?? 0, 2)));
            }
        });

        private void DrawSpeedMeasurementTable()
        {
            sp_Table.Children.Clear();
            Grid functionGrid = new();
            functionGrid.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            functionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            functionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            functionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            CreatSpeedTableRow($"Step", $"km/h (V)", $"km/h (R)");
            bool lastStep = false;
            for (int i = Start_Measurement; i <= CentralStationClient.maxDccStep; i += Step_Measurement)
            {
                CreatSpeedTableRow($"Step {i}", $"{(TractionForward[i] is null ? "-" : (double)Math.Round((TractionForward[i] / 3.6m) ?? 0, 2))} km/h", $"{(TractionBackward[i] is null ? "-" : (double)Math.Round((TractionBackward[i] / 3.6m) ?? 0, 2))}  km/h");
                if (!lastStep && i + Step_Measurement > CentralStationClient.maxDccStep) { i = (CentralStationClient.maxDccStep - Step_Measurement); lastStep = true; }
            }

            void CreatSpeedTableRow(string text1, string text2, string text3)
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
            }

            sp_Table.Children.Add(functionGrid);
        }

        private async Task GetSpeed(int speed, bool direction)
        {
            decimal result = await GetTimeBetweenSensors(speed, direction);
            decimal mps = Math.Round(((DistanceBetweenSensorsInMM / 1000.0m) / result) * 87.0m, 2);
            await SetTractionSpeed(speed, direction, mps);
            Log($"Result:\n\tStepStep:\t'{speed}'\n\tDirection:\t'{direction}'\n\tmeasured in seconds:\t'{result}'\n\tDistance:\t{DistanceBetweenSensorsInMM}\n\tSpeed in m/s:\t{mps}\n");
        }

        /// <summary>
        /// Starts the process of messuring the lokospeed.
        /// </summary>
        private async Task Run()
        {
            try
            {
                if (!SshClient.IsConnected)
                    SshClient.Connect();
                Log("Starting...");
                bool lastStep = false;
                bool direction = true;
                for (int speed = Start_Measurement; speed <= CentralStationClient.maxDccStep; speed += Step_Measurement)
                {
                    await GetSpeed(speed, direction);
                    await GetSpeed(speed, !direction);
                    if (!lastStep && speed + Step_Measurement > CentralStationClient.maxDccStep) { speed = (CentralStationClient.maxDccStep - Step_Measurement); lastStep = true; }
                }
                await ReturnHome();
                await SaveChanges();
            }
            catch (OperationCanceledException)
            {
                TractionForward = Vehicle.TractionForward;
                TractionBackward = Vehicle.TractionBackward;
                DrawSpeedMeasurementTable();
                await DrawSpeedDataPlot();
            }
            finally
            {
                await SetLocoDrive(0, true);
                await KillPythonScript();
                btnStart.IsEnabled = true;
                btnStop.IsEnabled = false;
                DrawSpeedMeasurementTable();
                await DrawSpeedDataPlot();
                TabTraktionSettings.IsEnabled = true;
            }

        }

        private async Task ReturnHome()
        {
            await SetLocoDrive(40, true);
            await ExecutePythonScriptAndGetResult();
            await SetLocoDrive(40, false);
            await ExecutePythonScriptAndGetResult(1);
            await SetLocoDrive(0, true);
        }

        private async Task SaveChanges()
        {
            TractionBackward[0] = 0;
            TractionBackward[1] = 0;
            TractionForward[0] = 0;
            TractionForward[1] = 0;
            var temp = Db.Vehicles.FirstOrDefault(e => e.Id == Vehicle.Id) ?? throw new Exception($"Fahrzeug mit Adresse '{Vehicle.Address}' nicht gefunden!");
            temp.TractionBackward = TractionBackward;
            temp.TractionForward = TractionForward;
            await Db.SaveChangesAsync();
            Vehicle = temp;
        }

        private async Task<decimal> GetTimeBetweenSensors(int speed, bool direction)
        {
            try
            {
                if (IsDisposed) throw new OperationCanceledException();
                await KillPythonScript();
                await SetLocoDrive(speed, direction);
                Log($"{DateTime.Now:hh-mm-ss} Python script running...");
                SshCommand result = await ExecutePythonScriptAndGetResult();
                if (result.ExitStatus != 0) throw new ApplicationException($"Script am Pi ist unerwarted abgestürtzt. ()");
                Log($"{DateTime.Now:hh-mm-ss} Python script complete...");
                if (!decimal.TryParse(result.Result, out decimal mps)) throw new ApplicationException($"Wert vom Raspberry Pi '{result.Result}' konte nicht als decimal geparsed werden.");
                await SetLocoDrive(0, direction);
                await Task.Delay((int)new TimeSpan(0, 0, 2).TotalMilliseconds);
                return mps;
            }
            finally
            {
                await KillPythonScript();
            }
        }

        private async Task<SshCommand> ExecutePythonScriptAndGetResult(int itterations = 5)
        {
            SshCommand result = default!;
            var task = Task.Run(() => { result = SshClient.RunCommand($"python /home/pi/Desktop/measure_speed.py {itterations}"); });
            await Task.Run(() => task.Wait(new TimeSpan(0, 2, 0)));
            return result;
        }

        private async Task KillPythonScript() => await Task.Run(() => SshClient.RunCommand("pkill -9 -f measure_speed.py"));

        private async Task SetLocoDrive(int speed, bool direction)
        {
            Log($"Speed: {speed} - Direction: {direction}");
            Controller.SetLocoDrive(new() { Adresse = new(Vehicle.Address), DrivingDirection = direction, InUse = true, Speed = speed });
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
            if (!(speed == LokData.Speed && LokData.DrivingDirection == direction))
            {
                Log("Speed check failed. Trying again. ...");
                await SetLocoDrive(speed, direction);
            }
        }

        private async Task SetTractionSpeed(int speedStep, bool direction, decimal speed)
        {
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
            DrawSpeedMeasurementTable();
            await DrawSpeedDataPlot();
            Log($"Loco drove {speed} m/s at dcc speed {speedStep} and direction {direction}.");
        }

        private void OnGetLocoInfoEventArgs(Object? sender, GetLocoInfoEventArgs e)
        {
            if (Vehicle is null) return;
            if (e.Data.Adresse.Value == Vehicle.Address)
            {
                LokData = e.Data;
            }
        }

        private async Task DrawSpeedDataPlot()
        {
            if (sp_Plot is null) return;
            await FillDataPointsList();
            PlotModel model = new();
            // Create the OxyPlot graph for Salt Split
            OxyPlot.Wpf.PlotView plot = new();
            plot.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            plot.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

            // Create new Line Series
            LineSeries linePoints_Forward = new()
            { StrokeThickness = 1, MarkerSize = 1, Title = "Vorwärts", Color = OxyPlot.OxyColors.Red };
            // Add each point to the new series
            linePoints_Forward.Points.Clear();
            linePoints_Forward.Points.AddRange(PointsForward);
            LineSeries linePoints_Backwards = new()
            { StrokeThickness = 1, MarkerSize = 1, Title = "Rückwärts", Color = OxyPlot.OxyColors.Blue };
            // Add each point to the new series
            linePoints_Backwards.Points.Clear();
            linePoints_Backwards.Points.AddRange(PointsBackward);

            model.Axes.Add(GetXAxis());

            model.Axes.Add(GetYAxis());

            model.Series.Add(linePoints_Backwards);
            model.Series.Add(linePoints_Forward);

            plot.Model = model;
            //plot.InvalidatePlot(true);
            sp_Plot.Children.Clear();
            sp_Plot.Children.Add(plot);
        }

        private LinearAxis GetXAxis()
        {
            OxyPlot.Axes.LinearAxis Xaxis = new();
            Xaxis.Maximum = CentralStationClient.maxDccStep;
            Xaxis.Minimum = Start_Measurement;
            Xaxis.Position = OxyPlot.Axes.AxisPosition.Bottom;
            Xaxis.Title = "Dcc Speed Step";
            return Xaxis;
        }

        private static LinearAxis GetYAxis()
        {
            OxyPlot.Axes.LinearAxis Yaxis = new();
            Yaxis.Minimum = 0;
            Yaxis.MinorStep = double.NaN;
            Yaxis.Title = "km/h";
            return Yaxis;
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
                if (Vehicle is null) { return; }
                Initialize();
                TractionForward = new decimal?[CentralStationClient.maxDccStep + 1];
                TractionBackward = new decimal?[CentralStationClient.maxDccStep + 1];
                DrawSpeedMeasurementTable();
                await DrawSpeedDataPlot();

                Controller.SetTrackPowerON();
                Controller.GetLocoInfo(new(Vehicle.Address));
                await Task.Run(() => { SshClient.Connect(); });
                PointsBackward = new();
                PointsForward = new();
                await Run();
            }
            catch (SshConnectionException) { MessageBox.Show($"Die ssh Verbindung zum Pi wurde terminiert!"); }
            catch (System.Net.Sockets.SocketException) { MessageBox.Show($"Verbindung zum Raspberry Pi konnte nicht hergestellt werden."); }
            catch (SshAuthenticationException) { MessageBox.Show($"Authentifizierung hat fehlgeschlagen."); }
            catch (Exception ex)
            {
                Logger.Log($"", ex, Environment.StackTrace);
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

        private async void IntegerUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            DrawSpeedMeasurementTable();
            await DrawSpeedDataPlot();
        }

        private static void Log(string message) => Console.WriteLine(message);

        private async void EinmessenWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DrawSpeedMeasurementTable();
            await DrawSpeedDataPlot();
        }

        private async void CmbAllVehicles_Loaded(object sender, RoutedEventArgs e)
        {
            CmbAllVehicles.Items.Clear();
            foreach (var vehicle in await Db.Vehicles.Where(e => e.Type == VehicleType.Lokomotive).OrderBy(e => e.Address).ToListAsync())
            {
                CmbAllVehicles.Items.Add(new ComboBoxItem() { Tag = vehicle, Content = $"({vehicle.Address:D3})  {(string.IsNullOrWhiteSpace(vehicle.Name) ? vehicle.Full_Name : vehicle.Name)}" });
            }
        }

        private async void CmbAllVehicles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TcMeasure.IsEnabled = CmbAllVehicles.SelectedItem is not Model.Vehicle;
            if (CmbAllVehicles.SelectedItem is not ComboBoxItem cbi || cbi.Tag is not Vehicle vehicle) return;
            Vehicle = vehicle;
            TractionForward = vehicle.TractionForward;
            TractionBackward = vehicle.TractionBackward;

            DrawSpeedMeasurementTable();
            await DrawSpeedDataPlot();
        }

        private async void EinmessenWindow_Closing(object sender, CancelEventArgs e)
        {
            SshClient.Disconnect();
            IsDisposed = true;
            await SetLocoDrive(0, false);
        }
    }
}
