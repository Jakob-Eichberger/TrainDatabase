using Extensions;
using Helper;
using Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Model;
using Service;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Importer
{
    /// <summary>
    /// Interaction logic for Z21.xaml
    /// </summary>
    public partial class Z21Import : Window, INotifyPropertyChanged
    {
        private readonly Database db;

        public Z21Import(IServiceProvider provider)
        {
            DataContext = this;
            InitializeComponent();
            db = provider.GetService<Database>()!;
            LogService = provider.GetService<LogService>()!;
        }

        public LogService LogService { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Path { get; set; } = "";

        protected void OnPropertyChanged([CallerMemberName] string name = null!) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private async void BtnGo_Click(object sender, RoutedEventArgs e)
        {
            await ImportAsync();
            Close();
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

        /// <summary>
        /// Extracts data from the Roco .z21 file.
        /// </summary>
        /// <returns>Returns the location of the sql database file.</returns>
        private async Task<string> ExtractDataFromZ21File()
        {
            GetDirectories(out string vehicleDirectory, out string tempDirectory, out string zipFileLocation);

            if (Directory.Exists(tempDirectory))
                Directory.Delete(tempDirectory, true);

            if (Directory.Exists(vehicleDirectory))
                Directory.Delete(vehicleDirectory, true);
            Directory.CreateDirectory(vehicleDirectory);

            File.Copy(Path, zipFileLocation);
            ZipFile.ExtractToDirectory(zipFileLocation, tempDirectory);
            File.Delete(zipFileLocation);

            var firstLayer = Directory.GetDirectories(tempDirectory).FirstOrDefault() ?? throw new ApplicationException($"Kein Subdirectory in '{tempDirectory}' gefunden!");

            var secondLayer = Directory.GetDirectories(firstLayer).FirstOrDefault() ?? throw new ApplicationException($"Kein Subdirectory in '{firstLayer}' gefunden!");

            var files = Directory.GetFiles(secondLayer).ToList();

            var sqlLiteDB = files.FirstOrDefault(e => System.IO.Path.GetExtension(e) == ".sqlite") ?? throw new ApplicationException("Benötigte .sqlite Datei nicht gefunden!");

            await Task.Run(() =>
            {
                foreach (var image in files.Where(e => System.IO.Path.GetExtension(e) != ".sqlite").ToList())
                    File.Move(image, $"{vehicleDirectory}\\{System.IO.Path.GetFileName(image)}");
            });

            return sqlLiteDB;
        }

        private async Task FillDbFromDB(string sqlLiteLocation)
        {
            if (string.IsNullOrWhiteSpace(sqlLiteLocation)) throw new ApplicationException($"Paramter {nameof(sqlLiteLocation)} is null!");
            await Task.Run(() =>
            {
                using SqliteConnection connection = new($"Data Source={sqlLiteLocation}");
                connection.Open();

                ImportVehicles(connection);
                ImportFunctions(connection);

                connection.Dispose();
            });
        }

        private void GetDirectories(out string vehicleDirectory, out string tempDirectory, out string zipFileLocation)
        {
            vehicleDirectory = Configuration.VehicleImagesFileLocation;
            tempDirectory = $"{Directory.GetCurrentDirectory()}\\Temp";
            zipFileLocation = new StringBuilder(Path).Replace(".z21", ".zip").ToString();
        }

        private FunctionType GetFunctionType(string name)
        {
            Dictionary<string, FunctionType> dic = new();
            dic.Add("sound", FunctionType.Sound1);
            dic.Add("light", FunctionType.Light1);
            dic.Add("main_beam", FunctionType.MainBeam);
            dic.Add("main_beam2", FunctionType.LowBeam);

            if (dic.TryGetValue(name.ToLower(), out FunctionType func))
                return func;
            else if (Enum.TryParse<FunctionType>(name.Replace("_", ""), true, out var result))
                return result;
            else
                return FunctionType.None;
        }

        private async Task ImportAsync()
        {
            Pb.Visibility = Visibility.Visible;
            IsEnabled = false;
            try
            {
                InitializeDatabase();

                await FillDbFromDB(await ExtractDataFromZ21File());

                Close();
                MessageBox.Show("Import erfolgreich!", "Erfolg", MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                LogService.Log(Microsoft.Extensions.Logging.LogLevel.Error, ex);
                MessageBox.Show($"Es ist ein Fehler aufgetreten: {ex?.Message}");
            }
            finally
            {
                Pb.Visibility = Visibility.Collapsed;
                IsEnabled = true;
            }
        }

        private void ImportFunctions(SqliteConnection connection)
        {
            var command = connection.CreateCommand();
            command.CommandText = @"SELECT * FROM functions;";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                FunctionModel func = new()
                {
                    Id = reader.GetString(reader.GetOrdinal("id")).ToInt32(),
                    Vehicle = db.Vehicles.FirstOrDefault(e => e.Id == reader.GetString(reader.GetOrdinal("vehicle_id")).ToInt32()),
                    ButtonType = (ButtonType)reader.GetString(reader.GetOrdinal("button_type")).ToInt32(),
                    Name = ParseFunctionName(reader.GetString(reader.GetOrdinal("shortcut")).IsNullOrWhiteSpace() ? reader.GetString(reader.GetOrdinal("image_name")) : reader.GetString(reader.GetOrdinal("shortcut"))),
                    Time = (int)reader.GetString(reader.GetOrdinal("time")).ToDecimal(),
                    Position = reader.GetString(reader.GetOrdinal("position")).ToInt32(),
                    ImageName = reader.GetString(reader.GetOrdinal("image_name")),
                    Address = reader.GetString(reader.GetOrdinal("function")).ToInt32(),
                    ShowFunctionNumber = reader.GetString(reader.GetOrdinal("show_function_number")).ToBoolean(),
                    IsConfigured = reader.GetString(reader.GetOrdinal("is_configured")).ToBoolean(),
                    EnumType = GetFunctionType(reader.GetString(reader.GetOrdinal("image_name")))
                };

                if (func.Name != "Empty")
                    db.Functions.Add(func);
            }
            db.SaveChanges();
        }

        private static string ParseFunctionName(string name) => Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(Regex.Replace(name?.ToLower()?.Replace("_", " ") ?? "", "[0-9]", " ").Trim());

        private void ImportVehicles(SqliteConnection connection)
        {
            var command = connection.CreateCommand();
            command.CommandText = @"SELECT * FROM vehicles";
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                db.Add(new VehicleModel()
                {
                    Id = reader.GetString(reader.GetOrdinal("id")).ToInt32(),
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    ImageName = reader.GetString(reader.GetOrdinal("image_name")),
                    Type = (VehicleType)reader.GetString(reader.GetOrdinal("type")).ToInt32(),
                    MaxSpeed = reader.GetString(reader.GetOrdinal("max_speed")).ToInt64(),
                    Address = reader.GetString(reader.GetOrdinal("address")).ToInt64(),
                    IsActive = reader.GetString(reader.GetOrdinal("active")).ToBoolean(),
                    Position = reader.GetString(reader.GetOrdinal("position")).ToInt64(),
                    DriversCab = reader.GetString(reader.GetOrdinal("drivers_cab")),
                    FullName = reader.GetString(reader.GetOrdinal("full_name")),
                    SpeedDisplay = reader.GetString(reader.GetOrdinal("speed_display")).ToInt64(),
                    Railway = reader.GetString(reader.GetOrdinal("railway")),
                    BufferLenght = reader.GetString(reader.GetOrdinal("buffer_lenght")).ToInt64(),
                    ModelBufferLenght = reader.GetString(reader.GetOrdinal("model_buffer_lenght")).ToInt64(),
                    ServiceWeight = reader.GetString(reader.GetOrdinal("service_weight")).ToInt64(),
                    ModelWeight = reader.GetString(reader.GetOrdinal("model_weight")).ToInt64(),
                    Rmin = reader.GetString(reader.GetOrdinal("rmin")).ToInt64(),
                    ArticleNumber = reader.GetString(reader.GetOrdinal("article_number")),
                    DecoderType = reader.GetString(reader.GetOrdinal("decoder_type")),
                    Owner = reader.GetString(reader.GetOrdinal("owner")),
                    BuildYear = reader.GetString(reader.GetOrdinal("build_year")),
                    OwningSince = reader.GetString(reader.GetOrdinal("owning_since")),
                    InvertTraction = reader.GetString(reader.GetOrdinal("traction_direction")).ToBoolean(),
                    Description = reader.GetString(reader.GetOrdinal("description")),
                    Dummy = reader.GetString(reader.GetOrdinal("dummy")).ToBoolean(),
                    Ip = IPAddress.Parse(reader.GetString(reader.GetOrdinal("ip"))),
                    Video = reader.GetString(reader.GetOrdinal("video")).ToInt64(),
                    Crane = reader.GetString(reader.GetOrdinal("crane")).ToBoolean(),
                    DirectSteering = reader.GetString(reader.GetOrdinal("direct_steering")).ToInt64(),
                });
            }
            db.SaveChanges();
        }

        private void InitializeDatabase()
        {
            db.Database.EnsureCreated();
            db.Clear();
        }
    }
}
