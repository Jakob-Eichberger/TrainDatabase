﻿using Microsoft.EntityFrameworkCore;
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
    /// Interaction logic for EditVehicleWindow.xaml
    /// </summary>
    public partial class EditVehicleWindow : Window, INotifyPropertyChanged
    {
        public readonly Database _db;
        private Vehicle _vehicle = default!;
        public event PropertyChangedEventHandler? PropertyChanged;


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
            DrawAllFunctions();

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
                Logger.Log($"{DateTime.UtcNow}: Image for Lok with adress '{_vehicle?.Address}' not found. Message: {ex.Message}", ex, Environment.StackTrace);
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
            BtnDeleteVehicle.Visibility = Visibility.Visible;
            btnSaveVehicleAndClose.Click += AddVehicle_Click;
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

                MenuItem miRemoveFunction = new();
                miRemoveFunction.Header = "Funktion entfernen";
                miRemoveFunction.Tag = item;
                miRemoveFunction.Click += RemoveFunction_Click;
                enumLabel.ContextMenu.Items.Add(miRemoveFunction);
                Grid.SetColumn(enumLabel, 0);
                Grid.SetRow(enumLabel, functionGrid.RowDefinitions.Count);
                SPFunctions.Children.Add(enumLabel);
            }
        }

        protected void OnPropertyChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
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

        private void AddFunction_Click(object sender, RoutedEventArgs e)
        {
            if (new EditFunctionWindow(_db, Vehicle)?.ShowDialog() ?? false)
            {
                Vehicle = _db.Vehicles.Include(m => m.Functions).FirstOrDefault(m => m.Id == Vehicle.Id) ?? throw new ApplicationException($"Fahrzeug mit Adresse {Vehicle.Id} nicht gefunden!");
                DrawAllFunctions();
            }
        }

        private void EditFunction_Click(object sender, RoutedEventArgs e)
        {
            var menu = ((MenuItem)e.Source);
            Function? function = (menu.Tag as Function);
            if (function is null) return;
            if (new EditFunctionWindow(_db, function).ShowDialog() ?? false)
            {
                Vehicle = _db.Vehicles.Include(m => m.Functions).FirstOrDefault(m => m.Id == Vehicle.Id) ?? throw new ApplicationException($"Fahrzeug mit Adresse {Vehicle.Id} nicht gefunden!");
                DrawAllFunctions();
            }
        }

        private void RemoveFunction_Click(object sender, RoutedEventArgs e)
        {
            var menu = ((MenuItem)e.Source);
            Function? function = (menu.Tag as Function);
            if (function is null) return;
            if (MessageBoxResult.Yes == MessageBox.Show($"Sind Sie sicher, dass Sie die Funktion '{function.Name}' löschen möchten?", "Funktion löschen", MessageBoxButton.YesNo))
            {
                _db.Remove(function);
                Vehicle = _db.Vehicles.Include(m => m.Functions).FirstOrDefault(m => m.Id == Vehicle.Id) ?? throw new ApplicationException($"Fahrzeug mit Adresse {Vehicle.Id} nicht gefunden!");
                DrawAllFunctions();
            }
        }

        private void TypeRadioButton_Click(object sender, RoutedEventArgs e)
        {
            Vehicle.Type = (VehicleType)Enum.Parse(typeof(VehicleType), (sender as RadioButton)!.Tag!.ToString()!);
        }

        private void DeleteVehicle_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageBox.Show($"Möchten Sie das Fahrzeug '{Vehicle.Name}' wirklich löschen?", "Fahrzeug löschen", MessageBoxButton.YesNo))
            {

            }
        }
    }
}