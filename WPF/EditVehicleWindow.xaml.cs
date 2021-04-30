using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using Model;
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
        private Vehicle vehicle = default!;
        public event PropertyChangedEventHandler? PropertyChanged;
        VehicleService VehicleService { get; }

        public Vehicle Vehicle
        {
            get => vehicle; set
            {
                vehicle = value;
                OnPropertyChanged();
            }
        }

        public EditVehicleWindow(Database _db, Vehicle _vehicle)
        {
            this.DataContext = this;
            InitializeComponent();
            if (_vehicle is null) throw new ApplicationException($"Paramter '{nameof(_vehicle)}' darf nicht null sein!");
            if (_db is null) throw new ApplicationException($"Paramter '{nameof(_db)}' darf nicht null sein!");
            db = _db;
            Vehicle = db.Vehicles.Include(i => i.Functions).FirstOrDefault(e => e.Id == _vehicle.Id) ?? throw new ApplicationException($"Fahrzeg mit der Id'{_vehicle.Id}' wurde nicht in der Datenbank gefunden!");
            VehicleService = new(db);
            this.Title = Vehicle.Full_Name.IsNullOrWhiteSpace() ? Vehicle.Name : Vehicle.Full_Name;

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
    }
}
