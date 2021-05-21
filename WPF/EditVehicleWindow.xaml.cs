using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using Model;
using ModelTrainController;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WPF_Application.Extensions;
using WPF_Application.Helper;
using WPF_Application.Infrastructure;
using WPF_Application.Services;

namespace WPF_Application
{
    /// <summary>
    /// Interaction logic for EditVehicleWindow.xaml
    /// </summary>
    public partial class EditVehicleWindow : Window, INotifyPropertyChanged
    {
        public readonly Database db;
        private Vehicle _vehicle = default!;
        public readonly CentralStationClient _controller;
        public event PropertyChangedEventHandler? PropertyChanged;
        VehicleService VehicleService { get; }

        public int DistanceBetweenSensors_Measurement { get { return Settings.GetInt(nameof(DistanceBetweenSensors_Measurement)) ?? 20; } set { Settings.Set(nameof(DistanceBetweenSensors_Measurement), value.ToString()); } }

        public int Start_Measurement { get { return Settings.GetInt(nameof(Start_Measurement)) ?? 2; } set { Settings.Set(nameof(Start_Measurement), value.ToString()); } }

        public int End_Measurement { get { return Settings.GetInt(nameof(End_Measurement)) ?? 127; } set { Settings.Set(nameof(End_Measurement), value.ToString()); } }

        public int Step_Measurement { get { return Settings.GetInt(nameof(Step_Measurement)) ?? 1; } set { Settings.Set(nameof(Step_Measurement), value.ToString()); } }

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
            db = _db;
            Vehicle = db.Vehicles.Include(i => i.Functions).FirstOrDefault(e => e.Id == _vehicle.Id) ?? throw new ApplicationException($"Fahrzeg mit der Id'{_vehicle.Id}' wurde nicht in der Datenbank gefunden!");
            VehicleService = new(db);
            this.Title = Vehicle.Full_Name.IsNullOrWhiteSpace() ? Vehicle.Name : Vehicle.Full_Name;
            DrawTable();
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
            DrawAllFunctions();
            btnSaveVehicleAndClose.Click += SaveChanges_Click;
            this._controller = controller;
        }

        public EditVehicleWindow(Database _db, CentralStationClient controller)
        {
            this.DataContext = this;
            Vehicle = new();
            InitializeComponent();
            if (_db is null) throw new ApplicationException($"Paramter '{nameof(_db)}' darf nicht null sein!");
            db = _db;
            VehicleService = new(db);
            this.Title = "Neues Fahrzeug";
            btnSaveVehicleAndClose.Click += AddVehicle_Click;
            this._controller = controller;
            DrawTable();
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

        private void DrawTable()
        {
            SPTable.Children.Clear();
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

                Label forward = new() { Content = $"{(_vehicle.TractionForward[i] is null ? "-" : _vehicle.TractionForward[i])} m/s", Margin = new Thickness(0, 0, 2, 1), HorizontalAlignment = System.Windows.HorizontalAlignment.Left };
                Grid.SetColumn(forward, 1);
                Grid.SetRow(forward, row);
                functionGrid.Children.Add(forward);

                Label backward = new() { Content = $"{(_vehicle.TractionBackward[i] is null ? "-" : _vehicle.TractionBackward[i])}  m/s", Margin = new Thickness(0, 0, 2, 1), HorizontalAlignment = System.Windows.HorizontalAlignment.Left };
                Grid.SetColumn(backward, 2);
                Grid.SetRow(backward, row);
                functionGrid.Children.Add(backward);

            }
            SPTable.Children.Add(functionGrid);
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
            EditFunctionWindow w = new(db, vehicle);
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

        private void btnLokEinmessen_Click(object sender, RoutedEventArgs e)
        {
            SpeedMeasurement messen = new(_vehicle, _controller, db);
            if (messen.ShowDialog() ?? false)
            {

            }
        }

        /// <summary>
        /// Exports the current traction table to a csv. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLokEinmessen_export_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
