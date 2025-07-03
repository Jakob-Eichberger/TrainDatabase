using Extensions;
using Helper;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TrainDatabase.Extensions;

namespace TrainDatabase
{
  /// <summary>
  /// Interaction logic for VehicleManagement.xaml
  /// </summary>
  public partial class VehicleManagement : Window, INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler? PropertyChanged;

    public Database Db { get; }

    public ObservableCollection<FunctionModel> SelectedVehicleFunctions { get; set; } = new();

    public FunctionModel? SelectedFunction { get; set; } = default!;

    public VehicleModel? SelectedVehicle { get; set; } = default!;

    public bool VehicleSelected => SelectedVehicle is not null;

    public bool FunctionSelected => SelectedFunction is not null;

    public ObservableCollection<VehicleModel> Vehicles { get; private set; } = new();

    public IServiceProvider ServiceProvider { get; }

    public VehicleManagement(IServiceProvider serviceProvider)
    {
      Db = serviceProvider.GetService<Database>()!;
      ServiceProvider = serviceProvider;

      Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      InitializeComponent();
      _ = ReloadVehicles();
    }

    protected void OnPropertyChanged()
    {
      PropertyChanged?.Invoke(this, new(null));
    }

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new(propertyName));
    }

    private async void AddNewVehicle()
    {
      try
      {
        BtnNew.IsEnabled = false;
        await Db.AddAsync(
                          new VehicleModel()
                          { Position = Db.Vehicles.Any() ? Db.Vehicles.Max(e => e.Position) + 1 : 1 });
        await ReloadVehicles();
      }
      finally
      {
        BtnNew.IsEnabled = true;
      }
    }

    private async void DeleteSelectedVehicle()
    {
      try
      {
        BtnDeleteVehicle.IsEnabled = false;
        if (DgVehicles.SelectedItem is not VehicleModel vehicle)
        {
          return;
        }

        if (MessageBoxResult.No == MessageBox.Show(
                                                   $"Möchten Sie das Fahrzeug '{(vehicle.Name.IsNullOrWhiteSpace() ? vehicle.FullName : vehicle.Name)}' wirklich löschen?",
                                                   "Fahrzeug löschen", MessageBoxButton.YesNo))
        {
          return;
        }

        Db.RemoveRange(Db.Functions.Include(e => e.Vehicle).Where(e => e.Vehicle.Id == vehicle.Id));
        Db.Remove(vehicle);
        await ReloadVehicles();
      }
      finally
      {
        BtnDeleteVehicle.IsEnabled = true;
      }
    }

    private void BtnNewVehicle_Click(object sender, RoutedEventArgs e)
    {
      AddNewVehicle();
    }

    private void BtnDeleteVehicle_Click(object sender, RoutedEventArgs e)
    {
      DeleteSelectedVehicle();
    }

    private void Vehicle_Changed(object sender, RoutedEventArgs e)
    {
    }

    private async void BtnNewFunction_Click(object sender, RoutedEventArgs e)
    {
      if (SelectedVehicle is null)
      {
        return;
      }

      await Db.AddAsync(
                        new FunctionModel()
                        {
                          Vehicle = SelectedVehicle, IsActive = true, ShowFunctionNumber = true,
                          Position = SelectedVehicle.Functions.Any()
                                       ? SelectedVehicle.Functions.Max(e => e.Position)
                                       : 1 + 1
                        });
      await ReloadVehicles();
    }

    private async void BtnDeleteFunction_Click(object sender, RoutedEventArgs e)
    {
      if (SelectedVehicle is null)
      {
        return;
      }

      await Db.RemoveAsync(SelectedFunction);
      await ReloadVehicles();
    }

    private async void DgFunctions_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
    {
      await Db.SaveChangesAsync();
      OnPropertyChanged();
    }

    private async Task ReloadVehicles()
    {
      await Db.SaveChangesAsync();

      int vehicleTempId = SelectedVehicle?.Id ?? -1;

      Vehicles.Clear();

      foreach (VehicleModel item in Db.Vehicles.Include(e => e.Functions).OrderBy(e => e.Position))
      {
        Vehicles.Add(item);
      }

      SelectedVehicle = Vehicles.FirstOrDefault(e => e.Id == vehicleTempId);

      await ReloadSelectedVehicleFunctions();
    }

    private async void DgVehicles_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
    {
      await ReloadSelectedVehicleFunctions();
    }

    private async Task ReloadSelectedVehicleFunctions()
    {
      await Db.SaveChangesAsync();
      SelectedVehicleFunctions.Clear();

      foreach (FunctionModel function in SelectedVehicle?.Functions.OrderBy(e => e.Position).ToList() ??
                                         new List<FunctionModel>())
      {
        SelectedVehicleFunctions.Add(function);
      }

      OnPropertyChanged();
      Db.InvokeCollectionChanged();
    }

    private async void Vm_Closing(object sender, CancelEventArgs e)
    {
      await ReloadVehicles();
    }

    private void BtnEditFunction_Click(object sender, RoutedEventArgs e)
    {
      EditSelectedVehicle();
    }

    private void DgFunctions_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
      EditSelectedVehicle();
    }

    private async void EditSelectedVehicle()
    {
      if (SelectedFunction is null)
      {
        return;
      }

      new EditFunctionWindow(Db, SelectedFunction).ShowDialog();
      await ReloadVehicles();
    }

    private async void BtnEditVehicleFunction_Click(object sender, RoutedEventArgs e)
    {
      if (SelectedVehicle is null)
      {
        return;
      }

      IEnumerable<string> extensions = ImageCodecInfo.GetImageEncoders()
                                                     .SelectMany(codec => codec.FilenameExtension?.Split(';')!)
                                                     .Where(extension => !string.IsNullOrEmpty(extension))
                                                     .Distinct();

      string filter = "Image Files|" + string.Join(";", extensions);

      OpenFileDialog openFileDialog = new()
                                      {
                                        Filter = filter,
                                        CheckPathExists = true
                                      };

      if (openFileDialog.ShowDialog() ?? false)
      {
        string oldFileNameAndPath = openFileDialog.FileName;
        string imageName = Guid.NewGuid() + Path.GetExtension(openFileDialog.FileName);
        string directory = Configuration.ApplicationData.VehicleImages.FullName;
        Directory.CreateDirectory(directory);
        File.Copy(oldFileNameAndPath, $"{directory}\\{imageName}");
        SelectedVehicle.ImageName = imageName;
        await Db.SaveChangesAsync();
        Db.InvokeCollectionChanged();
        await ReloadVehicles();
      }
    }

    private void BtnMoveVehiclePositionDown_Click(object sender, RoutedEventArgs e)
    {
      if (SelectedVehicle is not null)
      {
        Db.Vehicles.Swap(SelectedVehicle, true);
      }

      _ = ReloadVehicles();
    }

    private void BtnMoveVehiclePositionUp_Click(object sender, RoutedEventArgs e)
    {
      if (SelectedVehicle is not null)
      {
        Db.Vehicles.Swap(SelectedVehicle, false);
      }

      _ = ReloadVehicles();
    }
  }
}