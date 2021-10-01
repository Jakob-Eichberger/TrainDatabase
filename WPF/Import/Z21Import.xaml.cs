using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.Win32;
using Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WPF_Application.Extensions;
using WPF_Application.Helper;
using WPF_Application.Infrastructure;

namespace Importer
{
    /// <summary>
    /// Interaction logic for Z21.xaml
    /// </summary>
    public partial class Z21Import : Window, INotifyPropertyChanged
    {

        private readonly Database db;

        public string Path { get; set; } = "";

        public event PropertyChangedEventHandler? PropertyChanged;

        public Z21Import(Database _db)
        {
            DataContext = this;
            InitializeComponent();
            db = _db;
        }

        private void BtnOpenFileDalog_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofp = new();
            ofp.DefaultExt = ".z21";
            ofp.Filter = "Z21 DB FIle (*.z21)|*.z21";
            ofp.ShowDialog();
            BtnImportNow.IsEnabled = !string.IsNullOrWhiteSpace(ofp.FileName);
            TbFileSelector.Text = ofp.FileName;
            Path = ofp.FileName;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private async void BtnGo_Click(object sender, RoutedEventArgs e)
        {
            await ImportAsync();
            this.Close();
        }

        private async Task ImportAsync()
        {
            db.Database.EnsureCreated();
            db.Vehicles.RemoveRange(db.Vehicles);
            db.Functions.RemoveRange(db.Functions);
            db.SaveChanges();
            try
            {
                Pb.Visibility = Visibility.Visible;
                IsEnabled = false;
                string vehicleDirectory = $"{Directory.GetCurrentDirectory()}\\Data\\VehicleImage";
                string tempDirectory = $"{Directory.GetCurrentDirectory()}\\Temp";
                string zipFileLocation = new StringBuilder(Path).Replace(".z21", ".zip").ToString();
             
                if (Directory.Exists(tempDirectory))
                    Directory.Delete(tempDirectory, true);
                if (Directory.Exists(vehicleDirectory))
                    Directory.Delete(vehicleDirectory, true);

                Directory.CreateDirectory(vehicleDirectory);
                File.Move(Path, zipFileLocation);
                ZipFile.ExtractToDirectory(zipFileLocation, tempDirectory);
                File.Delete(zipFileLocation);
                var firstLayer = Directory.GetDirectories(tempDirectory).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(firstLayer)) throw new ApplicationException($"Kein Subdirectory in '{tempDirectory}' gefunden!");
                var secondLayer = Directory.GetDirectories(firstLayer).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(secondLayer)) throw new ApplicationException($"Kein Subdirectory in '{firstLayer}' gefunden!");
                var files = Directory.GetFiles(secondLayer).ToList();

                var sqlLiteDB = files.FirstOrDefault(e => System.IO.Path.GetExtension(e) == ".sqlite");
                if (string.IsNullOrWhiteSpace(sqlLiteDB)) throw new ApplicationException("Benötigte Datei (.sqlite) nicht gefunden!");
                var images = files.Where(e => System.IO.Path.GetExtension(e) != ".sqlite").ToList();
                await Task.Run(() =>
                {
                    foreach (var image in images ?? new List<string>())
                        File.Move(image, $"{vehicleDirectory}\\{System.IO.Path.GetFileName(image)}");
                });
                await FillDbFromDB(sqlLiteDB);
                this.Close();
                MessageBox.Show("Import erfolgreich!", "Erfolg", MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                Logger.Log($"Es ist ein import Fehler aufgetreten", ex);
                MessageBox.Show($"Es ist ein Fehler aufgetreten: {ex?.Message}");
            }
            finally
            {
                Pb.Visibility = Visibility.Collapsed;
                IsEnabled = true;
            }
        }

        private async Task FillDbFromDB(string sqlLiteLocation)
        {
            if (string.IsNullOrWhiteSpace(sqlLiteLocation)) throw new ApplicationException($"Paramter {nameof(sqlLiteLocation)} is null!");
            await Task.Run(() =>
            {
                using SqliteConnection connection = new($"Data Source={sqlLiteLocation}");
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"SELECT id, name, image_name, type, max_speed, address, active, position, drivers_cab ,full_name, speed_display, railway,buffer_lenght,model_buffer_lenght,service_weight,model_weight,rmin,article_number,decoder_type,owner,build_year,owning_since,traction_direction,description,dummy,ip,video,crane,direct_steering FROM vehicles";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        db.Add(new Vehicle()
                        {
                            Id = reader.GetString(0).ToInt32(),
                            Name = reader.GetString(1),
                            ImageName = reader.GetString(2),
                            Type = (VehicleType)reader.GetString(3).ToInt32(),
                            MaxSpeed = reader.GetString(4).ToInt64(),
                            Address = reader.GetString(5).ToInt64(),
                            IsActive = reader.GetString(6).ToBoolean(),
                            Position = reader.GetString(7).ToInt64(),
                            DriversCab = reader.GetString(8),
                            FullName = reader.GetString(9),
                            SpeedDisplay = reader.GetString(10).ToInt64(),
                            Railway = reader.GetString(11),
                            BufferLenght = reader.GetString(12).ToInt64(),
                            ModelBufferLenght = reader.GetString(13).ToInt64(),
                            ServiceWeight = reader.GetString(14).ToInt64(),
                            ModelWeight = reader.GetString(15).ToInt64(),
                            Rmin = reader.GetString(16).ToInt64(),
                            ArticleNumber = reader.GetString(17),
                            DecoderType = reader.GetString(18),
                            Owner = reader.GetString(19),
                            BuildYear = reader.GetString(20),
                            OwningSince = reader.GetString(21),
                            InvertTraction = reader.GetString(22).ToBoolean(),
                            Description = reader.GetString(23),
                            Dummy = reader.GetString(24).ToBoolean(),
                            Ip = IPAddress.Parse(reader.GetString(25)),
                            Video = reader.GetString(26).ToInt64(),
                            Crane = reader.GetString(27).ToBoolean(),
                            DirectSteering = reader.GetString(28).ToInt64(),
                        });
                    }
                    db.SaveChanges();
                }
                command.CommandText = @"SELECT id, vehicle_id, button_type, shortcut, time, position, image_name, function, show_function_number, is_configured  FROM functions;";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        db.Functions.Add(new()
                        {
                            Id = reader.GetString(0).ToInt32(),
                            Vehicle = db.Vehicles.FirstOrDefault(e => e.Id == reader.GetString(1).ToInt32()),
                            ButtonType = (ButtonType)reader.GetString(2).ToInt32(),
                            Name = reader.GetString(3).IsNullOrWhiteSpace() ? reader.GetString(6) : reader.GetString(3),
                            Time = reader.GetString(4).ToDecimal(),
                            Position = reader.GetString(5).ToInt32(),
                            ImageName = reader.GetString(6),
                            FunctionIndex = reader.GetString(7).ToInt32(),
                            ShowFunctionNumber = reader.GetString(8).ToBoolean(),
                            IsConfigured = reader.GetString(9).ToBoolean(),
                            EnumType = GetFunctionType(reader.GetString(6))
                        });
                    }
                    db.SaveChanges();
                }
                connection.Dispose();
            });
        }
        private FunctionType GetFunctionType(string name)
        {
            Dictionary<string, FunctionType> dic = new();
            dic.Add("sound", FunctionType.Sound);
            dic.Add("sound2", FunctionType.Sound);
            dic.Add("main_beam", FunctionType.HighBeam);
            dic.Add("light", FunctionType.LowBeam);
            dic.Add("main_beam2", FunctionType.LowBeam);
            dic.Add("horn_high", FunctionType.HornHigh);
            dic.Add("horn_low", FunctionType.HornLow);
            dic.Add("hump_gear", FunctionType.HumpGear);
            dic.Add("curve_sound", FunctionType.CurveSound);
            dic.Add("compressor", FunctionType.Compressor);
            dic.Add("cabin", FunctionType.DriversCabinLight1);
            dic.Add("cabin_light", FunctionType.DriversCabinLight2);
            dic.Add("mute", FunctionType.Mute);
            if (dic.TryGetValue(name, out FunctionType func))
                return func;
            else
                return FunctionType.None;
        }

    }
}
