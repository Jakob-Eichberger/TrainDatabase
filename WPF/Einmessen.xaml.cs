using Helper;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Model;
using OxyPlot;
using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TrainDatabase.Z21Client.DTO;
using TrainDatabase.Z21Client.Events;
using LinearAxis = OxyPlot.Axes.LinearAxis;
using LineSeries = OxyPlot.Series.LineSeries;

namespace TrainDatabase
{
    /// <summary>
    /// Interaction logic for Einmessen.xaml
    /// </summary>
    public partial class Einmessen : Window, INotifyPropertyChanged
    {
        private VehicleModel _vehicle = default!;
        private bool IsDisposed = false;
        public Einmessen(Database db, Z21Client.Z21Client controller)
        {
            this.DataContext = this;
            if (db is null) throw new ApplicationException($"Paramter '{nameof(db)}' darf nicht null sein!");
            if (controller is null) throw new ApplicationException($"Paramter '{nameof(controller)}' darf nicht null sein!");
            this.Db = db;
            this.Controller = controller;
            InitializeComponent();
            SetupManagementEventWatcher();
            Controller.OnGetLocoInfo += OnGetLocoInfoEventArgs;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public static List<string> ComPorts => ArduinoSerialPort.GetPortNames().ToList();
        public Database Db { get; } = default!;

        /// <summary>
        /// Holds the distance between the two raspberry pi sensors in mm.
        /// </summary>
        public decimal DistanceBetweenSensorsInMM
        {
            get => Configuration.GetDecimal(nameof(DistanceBetweenSensorsInMM)) ?? 200.0m;
            set => Configuration.Set(nameof(DistanceBetweenSensorsInMM), value.ToString());
        }

        public ManagementEventWatcher ManagementEventWatcher { get; } = new ManagementEventWatcher();
        public List<DataPoint> PointsBackward { get; private set; } = new();
        public List<DataPoint> PointsForward { get; private set; } = new();
        public int Start_Measurement { get => Configuration.GetInt(nameof(Start_Measurement)) ?? 2; set { Configuration.Set(nameof(Start_Measurement), value.ToString()); } }
        public int Step_Measurement
        {
            get => Configuration.GetInt(nameof(Step_Measurement)) ?? 1;
            set { Configuration.Set(nameof(Step_Measurement), value.ToString()); OnPropertyChanged(); }
        }

        private Z21Client.Z21Client Controller { get; } = default!;

        private LokInfoData LokData { get; set; } = new();
        private decimal?[] TractionBackward { get; set; } = new decimal?[Z21Client.Z21Client.maxDccStep + 1];
        private decimal?[] TractionForward { get; set; } = new decimal?[Z21Client.Z21Client.maxDccStep + 1];
        private VehicleModel Vehicle
        {
            get => _vehicle; set
            {
                _vehicle = value;
                OnPropertyChanged();
            }
        }
        protected void OnPropertyChanged() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));

        private static LinearAxis GetYAxis()
        {
            OxyPlot.Axes.LinearAxis Yaxis = new();
            Yaxis.Minimum = 0;
            Yaxis.MinorStep = double.NaN;
            Yaxis.Title = "km/h";
            return Yaxis;
        }

        private static void Log(string message) => Logger.LogInformation(message);

        private async void BtnStartSpeedMeasurement_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Vehicle is null) { return; }
                Initialize();
                TractionForward = new decimal?[Z21Client.Z21Client.maxDccStep + 1];
                TractionBackward = new decimal?[Z21Client.Z21Client.maxDccStep + 1];
                DrawSpeedMeasurementTable();
                await DrawSpeedDataPlot();

                Controller.SetTrackPowerON();
                Controller.GetLocoInfo(new(Vehicle.Address));
                PointsBackward = new();
                PointsForward = new();
                await Run();
            }
            catch (SshConnectionException) { MessageBox.Show($"Die ssh Verbindung zum Pi wurde terminiert!"); }
            catch (System.Net.Sockets.SocketException) { MessageBox.Show($"Verbindung zum Raspberry Pi konnte nicht hergestellt werden."); }
            catch (SshAuthenticationException) { MessageBox.Show($"Authentifizierung hat fehlgeschlagen."); }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"");
                MessageBox.Show($"Es ist ein Fehler aufgetreten: {ex.Message}");
            }
            finally
            {
                Deinitialize();
            }
        }

        private void BtnStopSpeedMeasurement_Click(object sender, RoutedEventArgs e)
        {
            IsDisposed = true;
            this.Close();
        }


        private async void CmbAllVehicles_Loaded(object sender, RoutedEventArgs e)
        {
            CmbAllVehicles.Items.Clear();
            foreach (var vehicle in await Db.Vehicles.Where(e => e.Type == VehicleType.Lokomotive).OrderBy(e => e.Address).ToListAsync())
            {
                CmbAllVehicles.Items.Add(new ComboBoxItem() { Tag = vehicle, Content = $"({vehicle.Address:D3})  {(string.IsNullOrWhiteSpace(vehicle.Name) ? vehicle.FullName : vehicle.Name)}" });
            }
        }

        private async void CmbAllVehicles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TcMeasure.IsEnabled = CmbAllVehicles.SelectedItem is not Model.VehicleModel;
            if (CmbAllVehicles.SelectedItem is not ComboBoxItem cbi || cbi.Tag is not VehicleModel vehicle) return;
            Vehicle = vehicle;
            TractionForward = vehicle.TractionForward;
            TractionBackward = vehicle.TractionBackward;

            DrawSpeedMeasurementTable();
            await DrawSpeedDataPlot();
        }

        private async void Deinitialize()
        {
            TabTraktionSettings.IsEnabled = true;
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
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
            for (int i = Start_Measurement; i <= Z21Client.Z21Client.maxDccStep; i += Step_Measurement)
            {
                CreatSpeedTableRow($"Step {i}", $"{(TractionForward[i] is null ? "-" : (double)Math.Round((TractionForward[i] / 3.6m) ?? 0, 2))} km/h", $"{(TractionBackward[i] is null ? "-" : (double)Math.Round((TractionBackward[i] / 3.6m) ?? 0, 2))}  km/h");
                if (!lastStep && i + Step_Measurement > Z21Client.Z21Client.maxDccStep) { i = (Z21Client.Z21Client.maxDccStep - Step_Measurement); lastStep = true; }
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

        private void EinmessenWindow_Closing(object sender, CancelEventArgs e)
        {
            IsDisposed = true;
            SetLocoDrive(0, false);
        }

        private async void EinmessenWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DrawSpeedMeasurementTable();
            await DrawSpeedDataPlot();
        }

        private async Task<SshCommand> ExecutePythonScriptAndGetResult(int itterations = 5)
        {
            return null;
        }

        private async Task FillDataPointsList() => await Task.Run(() =>
        {
            PointsBackward = new();
            PointsForward = new();
            for (int i = 2; i <= Z21Client.Z21Client.maxDccStep; i++)
            {
                if (TractionBackward[i] is not null)
                    PointsBackward.Add(new(i, (double)Math.Round((TractionBackward[i] / 3.6m) ?? 0, 2)));
            }
            for (int i = 2; i <= Z21Client.Z21Client.maxDccStep; i++)
            {
                if (TractionForward[i] is not null)
                    PointsForward.Add(new(i, (double)Math.Round((TractionForward[i] / 3.6m) ?? 0, 2)));
            }
        });

        private async Task GetSpeed(int speed, bool direction)
        {
            decimal time = await GetTimeBetweenSensors(speed, direction) / 1000.0m;
            decimal mps = Math.Round(((DistanceBetweenSensorsInMM / 1000.0m) / time) * 87.0m, 2);
            await SetTractionSpeed(speed, direction, mps);
            Log($"Result:\n\tStepStep:\t'{speed}'\n\tDirection:\t'{direction}'\n\tmeasured in seconds:\t'{time}'\n\tDistance:\t{DistanceBetweenSensorsInMM}\n\tSpeed in m/s:\t{mps}\n");
        }

        private async Task<decimal> GetTimeBetweenSensors(int speed, bool direction)
        {
            try
            {
                if (IsDisposed)
                    throw new OperationCanceledException();

                using ArduinoSerialPort port = new(Configuration.ArduinoComPort, Configuration.ArduinoBaudrate ?? 9600);
                SetLocoDrive(speed, direction);
                var data = await port.WaitForValue(int.Parse($"{new TimeSpan(0, 5, 0).TotalMilliseconds}"));
                if (decimal.TryParse(data.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                    return result;
                else
                    throw new ApplicationException($"Serial bus data ('{data}') coul not be parsed as decimal!");
            }
            finally
            {
                await Task.Delay((int)new TimeSpan(0, 0, 2).TotalMilliseconds);
                SetLocoDrive(0, direction);
            }
        }

        private LinearAxis GetXAxis()
        {
            OxyPlot.Axes.LinearAxis Xaxis = new();
            Xaxis.Maximum = Z21Client.Z21Client.maxDccStep;
            Xaxis.Minimum = Start_Measurement;
            Xaxis.Position = OxyPlot.Axes.AxisPosition.Bottom;
            Xaxis.Title = "Dcc Speed Step";
            return Xaxis;
        }

        private void Initialize()
        {
            IsDisposed = false;
            TabTraktionSettings.IsEnabled = false;
            btnStart.IsEnabled = false;
            btnStop.IsEnabled = true;
            Log("Initialize");
        }

        private async void IntegerUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            DrawSpeedMeasurementTable();
            await DrawSpeedDataPlot();
        }

        private void ManagementEventWatcher_EventArrived(object sender, EventArrivedEventArgs e) => OnPropertyChanged();

        private void OnGetLocoInfoEventArgs(Object? sender, GetLocoInfoEventArgs e)
        {
            if (e?.Data?.Adresse?.Value == Vehicle?.Address)
                LokData = e.Data;
        }

        private async Task ReturnHome()
        {
            using ArduinoSerialPort port = new(Configuration.ArduinoComPort);
            SetLocoDrive(40, true);
            await port.WaitForValue(int.Parse($"{new TimeSpan(0, 5, 0).TotalMilliseconds}"));
            SetLocoDrive(40, false);
            await port.WaitForValue(int.Parse($"{new TimeSpan(0, 5, 0).TotalMilliseconds}"));
            SetLocoDrive(0, true);
        }

        /// <summary>
        /// Starts the process of messuring the lokospeed.
        /// </summary>
        private async Task Run()
        {
            try
            {
                Log("Starting...");
                bool lastStep = false;
                bool direction = true;
                for (int speed = Start_Measurement; speed <= Z21Client.Z21Client.maxDccStep; speed += Step_Measurement)
                {
                    await GetSpeed(speed, direction);
                    await GetSpeed(speed, !direction);
                    if (!lastStep && speed + Step_Measurement > Z21Client.Z21Client.maxDccStep) { speed = (Z21Client.Z21Client.maxDccStep - Step_Measurement); lastStep = true; }
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
                SetLocoDrive(0, true);
                btnStart.IsEnabled = true;
                btnStop.IsEnabled = false;
                DrawSpeedMeasurementTable();
                await DrawSpeedDataPlot();
                TabTraktionSettings.IsEnabled = true;
            }

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

        private void SetLocoDrive(int speed, bool direction) => Controller.SetLocoDrive(new LokInfoData() { Adresse = new(Vehicle.Address), DrivingDirection = direction, InUse = true, Speed = speed });

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

        private void SetupManagementEventWatcher()
        {
            ManagementEventWatcher.Query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent");
            ManagementEventWatcher.EventArrived += new EventArrivedEventHandler(ManagementEventWatcher_EventArrived);
            ManagementEventWatcher.Start();
        }
    }
}
