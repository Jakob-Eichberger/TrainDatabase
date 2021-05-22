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
using WPF_Application.Services;
using LineSeries = OxyPlot.Series.LineSeries;
using LinearAxis = OxyPlot.Axes.LinearAxis;
using Renci.SshNet;

namespace WPF_Application
{
    /// <summary>
    /// Interaction logic for EditVehicleWindow.xaml
    /// </summary>
    public partial class EditVehicleWindow : Window, INotifyPropertyChanged
    {
        public readonly Database _db;
        private Vehicle _vehicle = default!;
        public readonly CentralStationClient _controller;
        public event PropertyChangedEventHandler? PropertyChanged;
        private readonly SshClient _sshClient = new("192.168.0.73", "pi", "raspberry");
        VehicleService VehicleService { get; }
        private bool direction = false;
        private bool IsDisposed = false;
        private LokInfoData _lokInfo = new();
        public List<DataPoint> PointsBackward { get; private set; } = new();
        public List<DataPoint> PointsForward { get; private set; } = new();

        public int DistanceBetweenSensors_Measurement { get { return Settings.GetInt(nameof(DistanceBetweenSensors_Measurement)) ?? 20; } set { Settings.Set(nameof(DistanceBetweenSensors_Measurement), value.ToString()); } }

        public int Start_Measurement { get { return Settings.GetInt(nameof(Start_Measurement)) ?? 2; } set { Settings.Set(nameof(Start_Measurement), value.ToString()); } }

        public int End_Measurement { get { return Settings.GetInt(nameof(End_Measurement)) ?? 127; } set { Settings.Set(nameof(End_Measurement), value.ToString()); } }

        public int Step_Measurement { get { return Settings.GetInt(nameof(Step_Measurement)) ?? 1; } set { Settings.Set(nameof(Step_Measurement), value.ToString()); } }

        decimal?[] TractionForward { get; set; } = new decimal?[128];

        decimal?[] TractionBackward { get; set; } = new decimal?[128];

        public Vehicle Vehicle
        {
            get => _vehicle; set
            {
                _vehicle = value;
                OnPropertyChanged();
            }
        }

        public EditVehicleWindow(Database _db, Vehicle _vehicle, CentralStationClient controller)
        {

            this.DataContext = this;
            InitializeComponent();
            if (_vehicle is null) throw new ApplicationException($"Paramter '{nameof(_vehicle)}' darf nicht null sein!");
            if (_db is null) throw new ApplicationException($"Paramter '{nameof(_db)}' darf nicht null sein!");
            this._db = _db;
            Vehicle = _db.Vehicles.Include(i => i.Functions).FirstOrDefault(e => e.Id == _vehicle.Id) ?? throw new ApplicationException($"Fahrzeg mit der Id'{_vehicle.Id}' wurde nicht in der Datenbank gefunden!");
            VehicleService = new(this._db);
            this.Title = Vehicle.Full_Name.IsNullOrWhiteSpace() ? Vehicle.Name : Vehicle.Full_Name;
            this._controller = controller;
            btnSaveVehicleAndClose.Click += SaveChanges_Click;
            _controller.OnGetLocoInfo += OnGetLocoInfoEventArgs;
            TractionForward = Vehicle.TractionForward;
            TractionBackward = Vehicle.TractionBackward;
            FillPointsList();
            DrawTable();
            PlotSpeedData();
            try
            {
                string path = $"{Directory.GetCurrentDirectory()}\\Data\\VehicleImage\\";
                path += string.IsNullOrWhiteSpace(_vehicle?.Image_Name) ? "default.png" : _vehicle?.Image_Name;
                BitmapImage bitmap = new();
                bitmap.BeginInit();
                bitmap.UriSource = new(path);
                bitmap.EndInit();
                ImageVehicle.Source = bitmap;
                ImageVehicle.Width = 250;
                ImageVehicle.Height = 100;
            }
            catch (Exception ex)
            {
                Logger.Log($"{DateTime.UtcNow}: Image for Lok with adress '{_vehicle?.Address}' not found. Message: {ex.Message}", LoggerType.Warning);
            }
        }

        public EditVehicleWindow(Database _db, CentralStationClient controller)
        {
            this.DataContext = this;
            Vehicle = new();
            InitializeComponent();
            if (_db is null) throw new ApplicationException($"Paramter '{nameof(_db)}' darf nicht null sein!");
            this._db = _db;
            VehicleService = new(this._db);
            this.Title = "Neues Fahrzeug";
            this._controller = controller;
            btnSaveVehicleAndClose.Click += AddVehicle_Click;
            _controller.OnGetLocoInfo += OnGetLocoInfoEventArgs;
            DrawTable();
            PlotSpeedData();
        }

        private void DrawAllFunctions()
        {
            SPFunctions.Children.Clear();
            Grid functionGrid = new();
            functionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            functionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            functionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            functionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            foreach (var item in Vehicle.Functions)
            {
                Label enumLabel = new() { Content = item.Name, Margin = new Thickness(0, 0, 20, 5) };
                enumLabel.ContextMenu = new();
                MenuItem miEditLoko = new();
                miEditLoko.Header = "Funktion bearbeiten.";
                miEditLoko.Tag = item;
                miEditLoko.Click += EditFunction_Click;
                enumLabel.ContextMenu.Items.Add(miEditLoko);
                Grid.SetColumn(enumLabel, 0);
                Grid.SetRow(enumLabel, functionGrid.RowDefinitions.Count);
                SPFunctions.Children.Add(enumLabel);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void EditFunction_Click(Object sender, RoutedEventArgs e)
        {
            var menu = ((MenuItem)e.Source);
            Function? vehicle = (menu.Tag as Function);
            if (vehicle is null) return;
            EditFunctionWindow w = new(_db, vehicle);
            if (w.ShowDialog() ?? false)
            {
                DrawAllFunctions();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Hi :)");
        }

        private void ChangeImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new();
            ofd.CheckPathExists = true;
            ofd.Filter = "png (*.png)|*.png";
            if (ofd.ShowDialog() ?? false)
            {
                string oldFileNameAndPath = ofd.FileName;
                string imageName = Guid.NewGuid() + ".png";
                string path = $"{Directory.GetCurrentDirectory()}\\Data\\VehicleImage\\{imageName}";
                File.Copy(oldFileNameAndPath, path);
                Vehicle.Image_Name = imageName;
            }
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            VehicleService.Update(Vehicle);
            this.DialogResult = true;
            this.Close();
        }

        private void AddVehicle_Click(object sender, RoutedEventArgs e)
        {
            VehicleService.Add(Vehicle);
            this.DialogResult = true;
            this.Close();
        }

        #region SpeedMeasurments

        private void FillPointsList()
        {
            PointsBackward = new();
            PointsForward = new();
            for (int i = 0; i < 127; i++)
            {
                if (TractionBackward[i] is not null)
                    PointsBackward.Add(new(i, (double)(TractionBackward[i] ?? 0)));
            }
            for (int i = 0; i < 127; i++)
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
            int row = functionGrid.RowDefinitions.Count();
            functionGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Label stepColumnName = new() { Content = $"Step", Margin = new Thickness(0, 0, 2, 5), HorizontalAlignment = System.Windows.HorizontalAlignment.Left };
            Grid.SetColumn(stepColumnName, 0);
            Grid.SetRow(stepColumnName, row);
            functionGrid.Children.Add(stepColumnName);

            Label forwardColumnName = new() { Content = $"M/S (V)", Margin = new Thickness(0, 0, 2, 5), HorizontalAlignment = System.Windows.HorizontalAlignment.Left };
            Grid.SetColumn(forwardColumnName, 1);
            Grid.SetRow(forwardColumnName, row);
            functionGrid.Children.Add(forwardColumnName);

            Label backwardColumnName = new() { Content = $"M/S (R)", Margin = new Thickness(0, 0, 2, 5), HorizontalAlignment = System.Windows.HorizontalAlignment.Left };
            Grid.SetColumn(backwardColumnName, 2);
            Grid.SetRow(backwardColumnName, row);
            functionGrid.Children.Add(backwardColumnName);

            for (int i = Start_Measurement; i <= End_Measurement; i += Step_Measurement)
            {

                row = functionGrid.RowDefinitions.Count();
                functionGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                Label step = new() { Content = $"Step {i}", Margin = new Thickness(0, 0, 2, 1), HorizontalAlignment = System.Windows.HorizontalAlignment.Left };
                Grid.SetColumn(step, 0);
                Grid.SetRow(step, row);
                functionGrid.Children.Add(step);

                Label forward = new() { Content = $"{(TractionForward[i] is null ? "-" : TractionForward[i])} m/s", Margin = new Thickness(0, 0, 2, 1), HorizontalAlignment = System.Windows.HorizontalAlignment.Left };
                Grid.SetColumn(forward, 1);
                Grid.SetRow(forward, row);
                functionGrid.Children.Add(forward);

                Label backward = new() { Content = $"{(TractionBackward[i] is null ? "-" : TractionBackward[i])}  m/s", Margin = new Thickness(0, 0, 2, 1), HorizontalAlignment = System.Windows.HorizontalAlignment.Left };
                Grid.SetColumn(backward, 2);
                Grid.SetRow(backward, row);
                functionGrid.Children.Add(backward);

            }
            sp_Table.Children.Add(functionGrid);
        }

        /// <summary>
        /// Gets the direction of travell. Always gets the inverted value of the currently used direction.
        /// </summary>
        /// <returns></returns>
        private bool GetDirection()
        {
            direction = !direction;
            return direction;
        }

        /// <summary>
        /// Starts the process of messuring the lokospeed.
        /// </summary>
        private async void Start()
        {
            try
            {
                for (int i = Start_Measurement; i <= End_Measurement; i += Step_Measurement)
                {
                    await GetNextResult(i);
                    await GetNextResult(i);
                }
                Vehicle.TractionForward = TractionForward;
                Vehicle.TractionBackward = TractionBackward;
                _db.Update<Vehicle>(Vehicle);
                _db.SaveChanges();
                await ReturnHome();
                await ReturnHome();
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
                _sshClient.RunCommand("pkill -9 -f GetTimeBetweenSensors.py");
                _sshClient.Disconnect();
                btnStart.IsEnabled = true;
                btnStop.IsEnabled = false;
                DrawTable();
                PlotSpeedData();
                TabTraktionSettings.IsEnabled = true;
            }

            Log($"\nDONE!");
        }

        private async Task GetNextResult(int speed)
        {
            var direction = GetDirection();
            if (IsDisposed) throw new OperationCanceledException();
            await Task.Run(() => { _sshClient.RunCommand("pkill -9 -f GetTimeBetweenSensors.py"); });
            await SetLocoDrive(speed, direction);
            Log("\nWarte auf Messung....");
            SshCommand result = null!;
            await Task.Run(() => { result = _sshClient.RunCommand("python /home/pi/Desktop/GetTimeBetweenSensors.py"); });

            if (result.ExitStatus != 0)
                throw new ApplicationException("Script am Pi ist unerwarted abgestürtzt.");
            if (!decimal.TryParse(result.Result, out decimal mps))
                throw new ApplicationException($"Wert vom Raspberry Pi '{result.Result}' konte nicht als decimal geparsed werden.");
            SetTractionSpeed(speed, direction, Math.Round(((DistanceBetweenSensors_Measurement / 100.0m) / mps) * 87, 2));
            if (speed < 10)
                await Task.Delay((int)new TimeSpan(0, 0, 10).TotalMilliseconds);
            else if (speed < 40)
                await Task.Delay((int)new TimeSpan(0, 0, 6).TotalMilliseconds);
            else if (speed < 90)
                await Task.Delay((int)new TimeSpan(0, 0, 2).TotalMilliseconds);
            else
                await Task.Delay((int)new TimeSpan(0, 0, 1).TotalMilliseconds);

            await SetLocoDrive(0, direction);
            await Task.Delay((int)new TimeSpan(0, 0, 2).TotalMilliseconds);
        }

        private async Task SetLocoDrive(int speed, bool direction)
        {
            Log($"\nNeue Geschwindigkeit ({speed}, {(direction ? "Vorwärts" : "Rückwärts")}) -  Checking ....  ");
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
            await Task.Delay(1500);
            if (!(speed == _lokInfo.Speed && _lokInfo.DrivingDirection == direction))
            {
                Log($"failed ");
                await SetLocoDrive(speed, direction);
            }
            else { Log($"OK"); }
        }

        private void SetTractionSpeed(int speedStep, bool direction, decimal speed)
        {
            if (speedStep < 2 || 127 < speedStep) throw new ApplicationException($"SpeedStep von '{speedStep}' nicht zulässig.");
            if (speed < 0) throw new ApplicationException($"Geschwindigkeit '{speed}' nicht zulässig!");
            if (direction)
            {
                Log($"{speed} m/s(v)");
                TractionForward[speedStep] = speed;
                PointsForward.Add(new(speedStep, (double)speed));
            }
            else
            {
                Log($"{speed} m/s(r)");
                TractionBackward[speedStep] = speed;
                PointsBackward.Add(new(speedStep, (double)speed));
            }
            DrawTable();
            PlotSpeedData();
        }

        private async Task ReturnHome()
        {
            var direction = GetDirection();
            await SetLocoDrive(40, direction);
            await Task.Run(() => { _sshClient.RunCommand("pkill -9 -f GetTimeBetweenSensors.py"); });
            await Task.Run(() => { _sshClient.RunCommand("python /home/pi/Desktop/GetTimeBetweenSensors.py"); });
            await SetLocoDrive(0, direction);
        }

        private void OnGetLocoInfoEventArgs(Object? sender, GetLocoInfoEventArgs e)
        {
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
            Xaxis.Maximum = End_Measurement;
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

        /// <summary>
        /// Writes a message to the log <see cref="tbLog"/> <see cref="TextBox"/> for the user to see.
        /// </summary>
        /// <param name="message"></param>
        /// <remarks>Does not insert linebreaks.</remarks>
        private void Log(string message)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                tbLog.Text += $"{message}";
                svLog.ScrollToBottom();
            }));
        }

        private void btnStartSpeedMeasurement_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TabTraktionSettings.IsEnabled = false;
                IsDisposed = false;
                btnStart.IsEnabled = false;
                btnStop.IsEnabled = true;
                TractionForward = new decimal?[128];
                TractionBackward = new decimal?[128];
                DrawTable();
                _controller.SetTrackPowerON();
                _controller.GetLocoInfo(new(Vehicle.Address));
                //_sshClient = new(Settings.Get("RaspberryPiHost"), Settings.Get("RaspberryPiUsername"), Settings.Get("RaspberryPiPassword"));
                _sshClient.Connect();
                PointsBackward = new();
                PointsForward = new();
                PlotSpeedData();
                Start();
            }
            catch (SshConnectionException ex) { MessageBox.Show($"Connection zum Raspberry Pi konnte nicht hergestellt werden: {ex.Message}"); }
            catch (SshAuthenticationException ex) { MessageBox.Show($"Authentifizierung hat fehlgeschlagen: {ex.Message}"); }
            catch (Exception ex)
            {
                Logger.Log($"Error in Function {nameof(btnStartSpeedMeasurement_Click)}: {ex.Message}", LoggerType.Error);
                MessageBox.Show($"Es ist ein Fehler aufgetreten: {ex.Message}");
            }
            finally
            {
                btnStart.IsEnabled = true;
            }
        }

        private void btnStopSpeedMeasurement_Click(object sender, RoutedEventArgs e)
        {
            IsDisposed = true;
            btnStop.IsEnabled = false;
            Log("\nStopping...\n");
        }

        #endregion
    }
}
