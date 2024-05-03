using Helper;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Model;
using OxyPlot;
using Service.Controller;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WPF_Application.TimeCapture;
using LinearAxis = OxyPlot.Axes.LinearAxis;
using LineSeries = OxyPlot.Series.LineSeries;

namespace TrainDatabase
{
    public class ComPortsComboBox : ComboBox
    {
        public ComPortsComboBox()
        {
            SetupManagementEventWatcher();
            UpdateComPortList();
            SelectionChanged += (a, b) => Configuration.ArduinoComPort = SelectedComPort;
        }

        public static List<string> ComPorts => ArduinoSerialPort.GetPortNames().ToList();

        public string SelectedComPort => $"{SelectedItem}";

        private ManagementEventWatcher ManagementEventWatcher { get; } = new ManagementEventWatcher();

        private void ManagementEventWatcher_EventArrived(object sender, EventArrivedEventArgs e) => UpdateComPortList();

        private void SetupManagementEventWatcher()
        {
            ManagementEventWatcher.Query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent");
            ManagementEventWatcher.EventArrived += new EventArrivedEventHandler(ManagementEventWatcher_EventArrived);
            ManagementEventWatcher.Start();
        }

        private void UpdateComPortList() => Dispatcher.Invoke(() =>
        {
            var selectedItem = SelectedItem ?? Configuration.ArduinoComPort;
            Items.Clear();
            ComPorts.ForEach(e => Items.Add(e));
            if (selectedItem is not null)
                SelectedItem = selectedItem;
        });
    }

    /// <summary>
    /// Interaction logic for Einmessen.xaml
    /// </summary>
    public partial class Einmessen : Window, INotifyPropertyChanged
    {
        private VehicleModel? _vehicle = default!;
        public Einmessen(IServiceProvider serviceProvider)
        {
            this.DataContext = this;
            ServiceProvider = serviceProvider;
            this.Db = ServiceProvider.GetService<Database>()!;
           
            InitializeComponent();

            TiMultitraction.Content = new MultiTraction(ServiceProvider, Vehicle);

        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public IServiceProvider ServiceProvider { get; }

        public Database Db { get; } = default!;

        private VehicleModel? Vehicle
        {
            get => _vehicle; set
            {
                _vehicle = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));

        private void BtnStopSpeedMeasurement_Click(object sender, RoutedEventArgs e)
        {
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

        private void CmbAllVehicles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TcMeasure.IsEnabled = CmbAllVehicles.SelectedItem is not Model.VehicleModel;
            if (CmbAllVehicles.SelectedItem is not ComboBoxItem cbi || cbi.Tag is not VehicleModel vehicle) return;
            Vehicle = vehicle;
            TiMultitraction.Content = new MultiTraction(ServiceProvider, Vehicle);
        }

    }
}
