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
using System.Drawing.Imaging;
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
    /// Interaction logic for EditVehicleWindow.xaml
    /// </summary>
    public partial class EditVehicleWindow : Window, INotifyPropertyChanged
    {
        public readonly Database _db;
        private Vehicle _vehicle = default!;
        public event PropertyChangedEventHandler? PropertyChanged;

        public int DistanceBetweenSensors_Measurement { get => Settings.GetInt(nameof(DistanceBetweenSensors_Measurement)) ?? 20; set { Settings.Set(nameof(DistanceBetweenSensors_Measurement), value.ToString()); } }

        public int Start_Measurement { get => Settings.GetInt(nameof(Start_Measurement)) ?? 2; set { Settings.Set(nameof(Start_Measurement), value.ToString()); } }

        public int End_Measurement { get => Settings.GetInt(nameof(End_Measurement)) ?? 127; set { Settings.Set(nameof(End_Measurement), value.ToString()); } }

        public int Step_Measurement { get => Settings.GetInt(nameof(Step_Measurement)) ?? 1; set { Settings.Set(nameof(Step_Measurement), value.ToString()); } }

        public Vehicle Vehicle
        {
            get => _vehicle; set
            {
                _vehicle = value;
                OnPropertyChanged();
            }
        }

        public EditVehicleWindow(Database _db, Vehicle _vehicle)
        {
            this.DataContext = this;
            InitializeComponent();
            if (_vehicle is null) throw new ApplicationException($"Paramter '{nameof(_vehicle)}' darf nicht null sein!");
            if (_db is null) throw new ApplicationException($"Paramter '{nameof(_db)}' darf nicht null sein!");
            this._db = _db;
            Vehicle = _db.Vehicles.Include(i => i.Functions).FirstOrDefault(e => e.Id == _vehicle.Id) ?? throw new ApplicationException($"Fahrzeg mit der Id'{_vehicle.Id}' wurde nicht in der Datenbank gefunden!");
            this.Title = Vehicle.Full_Name.IsNullOrWhiteSpace() ? Vehicle.Name : Vehicle.Full_Name;
            btnSaveVehicleAndClose.Click += SaveChanges_Click;
            DgFunctions.Items.SortDescriptions.Add(new SortDescription(nameof(Function.Position), ListSortDirection.Ascending));
            BtnDeleteVehicle.Visibility = Visibility.Visible;
            switch (Vehicle.Type)
            {
                case VehicleType.Lokomotive:
                    RbLokomotive.IsChecked = true;
                    break;
                case VehicleType.Steuerwagen:
                    RbSteuerwagen.IsChecked = true;
                    break;
                case VehicleType.Wagen:
                    RbWagen.IsChecked = true;
                    break;
            }

            LoadVehicleImage();
        }

        private void LoadVehicleImage()
        {
            try
            {
                string path = $"{Directory.GetCurrentDirectory()}\\Data\\VehicleImage\\";
                path += string.IsNullOrWhiteSpace(Vehicle?.Image_Name) ? "default.png" : Vehicle?.Image_Name;
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
                Logger.Log($"{DateTime.UtcNow}: Image for Lok with adress '{Vehicle?.Address}' not found.", ex);
            }
        }

        public EditVehicleWindow(Database _db)
        {
            this.DataContext = this;
            Vehicle = new();
            InitializeComponent();
            if (_db is null) throw new ApplicationException($"Paramter '{nameof(_db)}' darf nicht null sein!");
            this._db = _db;
            this.Title = "Neues Fahrzeug";
            btnSaveVehicleAndClose.Click += AddVehicle_Click;
        }

        protected void OnPropertyChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        private void ChangeImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new();
            ofd.Filter = "";
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            string sep = string.Empty;
            foreach (var c in codecs)
            {
                string codecName = c.CodecName.Substring(8).Replace("Codec", "Files").Trim();
                ofd.Filter = String.Format("{0}{1}{2} ({3})|{3}", ofd.Filter, sep, codecName, c.FilenameExtension);
                sep = "|";
            }
            ofd.Filter = String.Format("{0}{1}{2} ({3})|{3}", ofd.Filter, sep, "All Files", "*.*");
            ofd.DefaultExt = ".png"; 
            ofd.CheckPathExists = true;
            if (ofd.ShowDialog() ?? false)
            {
                string oldFileNameAndPath = ofd.FileName;
                string imageName = Guid.NewGuid() + ".png";
                string directory = $"{Directory.GetCurrentDirectory()}\\Data\\VehicleImage";
                Directory.CreateDirectory(directory);
                string path = $"{directory}\\{imageName}";
                File.Copy(oldFileNameAndPath, path);
                Vehicle.Image_Name = imageName;
                LoadVehicleImage();
            }
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            _db.Update(Vehicle);
            this.DialogResult = true;
            this.Close();
        }

        private void AddVehicle_Click(object sender, RoutedEventArgs e)
        {
            _db.Add(Vehicle);
            this.DialogResult = true;
            this.Close();
        }

        private void TypeRadioButton_Click(object sender, RoutedEventArgs e)
        {
            Vehicle.Type = (VehicleType)Enum.Parse(typeof(VehicleType), (sender as RadioButton)!.Tag!.ToString()!);
        }

        private void DgFunctions_Opening(object sender, ContextMenuEventArgs e)
        {
            MiEditFunction.Visibility = DgFunctions.SelectedCells.Any() ? Visibility.Visible : Visibility.Collapsed;
            MiRemoveFunction.Visibility = DgFunctions.SelectedCells.Any() ? Visibility.Visible : Visibility.Collapsed;
        }

        private void MiAddFunction_Click(object sender, RoutedEventArgs e)
        {
            if (new EditFunctionWindow(_db, Vehicle)?.ShowDialog() ?? false)
                Vehicle = _db.Vehicles.Include(m => m.Functions).FirstOrDefault(m => m.Id == Vehicle.Id) ?? throw new ApplicationException($"Fahrzeug mit Adresse {Vehicle.Id} nicht gefunden!");
        }

        private void MiEditFunction_Click(object sender, RoutedEventArgs e)
        {
            if (DgFunctions.SelectedCells.FirstOrDefault().Item is Function function)
            {
                if (new EditFunctionWindow(_db, function).ShowDialog() ?? false)
                {
                    Vehicle = _db.Vehicles.Include(m => m.Functions).FirstOrDefault(m => m.Id == Vehicle.Id) ?? throw new ApplicationException($"Fahrzeug mit Adresse {Vehicle.Id} nicht gefunden!");
                }
            }
        }

        private void MiRemoveFunction_Click(object sender, RoutedEventArgs e)
        {
            if (DgFunctions.SelectedCells.FirstOrDefault().Item is Function function)
            {
                if (MessageBoxResult.Yes == MessageBox.Show($"Sind Sie sicher, dass Sie die Funktion '{function.Name}' löschen möchten?", "Funktion löschen", MessageBoxButton.YesNo))
                {
                    _db.Remove(function);
                    Vehicle = _db.Vehicles.Include(m => m.Functions).FirstOrDefault(m => m.Id == Vehicle.Id) ?? throw new ApplicationException($"Fahrzeug mit Adresse {Vehicle.Id} nicht gefunden!");
                }
            }
        }

        private void DeleteVehicle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBoxResult.OK == MessageBox.Show($"Sind Sie sicher, dass Sie das Fahrzeug '{Vehicle.Name ?? Vehicle.Full_Name}' löschen möchten.", "Fahrzeug löschen", MessageBoxButton.OKCancel))
                {
                    _db.Remove(Vehicle);
                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Delete Vehicle failed", exception: ex);
            }
        }
    }
}
