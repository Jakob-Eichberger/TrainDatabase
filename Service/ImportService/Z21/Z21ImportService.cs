using Dapper;
using Extensions;
using Extensions.Exceptions;
using Helper;
using Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Model;
using Serilog;
using Service.ImportService.Z21.TDO;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.ImportService.Z21
{
    public class Z21ImportService
    {
        public Z21ImportService(Database database)
        {
            Database = database;
        }

        private Database Database { get; set; }

        /// <summary>
        /// Import the Data from a z21 file into the internal database.
        /// </summary>
        /// <param name="z21File"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public async Task ImportAsync(FileInfo z21File) => await Task.Run(async () =>
        {
            if (z21File.Extension.ToLower() is not ".z21")
            {
                throw new NotSupportedException($"Importing a {z21File.Extension} file is not supported!");
            }

            Database.DeleteAll();

            var tempPath = Configuration.ApplicationData.TempPath.FullName;
            var filePath = await ExtractPhotosAndSqlFileFromZ21File(z21File.FullName, tempPath);
            using var con = new SqliteConnection($"Data Source={filePath}");
            await MapVehiclesAsync(con);
            await MapFunctionsAsync(con, true);
            await con.CloseAsync();
        });

        private async Task MapFunctionsAsync(SqliteConnection con, bool removeEmptyFunctions)
        {
            var f = con.Query<FunctionDTO>("Select * from functions").ToList();
            var functions = f.Select(e => new FunctionModel()
            {
                Id = (int)e.id,
                Vehicle = Database.Vehicles.FirstOrDefault(v => v.Id == (int)e.vehicle_id),
                ButtonType = (ButtonType)(int)e.button_type,
                Name = e.shortcut.IsNullOrWhiteSpace() ? e.image_name : e.shortcut,
                Time = (int)decimal.Parse(e.time),
                Position = (int)e.position,
                ImageName = e.image_name,
                Address = (int)e.function,
                ShowFunctionNumber = e.show_function_number == 1,
                IsConfigured = e.is_configured == 1,

            }).Where(e => e.Vehicle is not null).ToList();

            functions = functions.Where(e => removeEmptyFunctions && e.Name is not "Empty").ToList();

            await Database.AddRangeAsync(functions);
            await Database.SaveChangesAsync();
            Database.InvokeCollectionChanged();
        }

        private async Task MapVehiclesAsync(SqliteConnection con)
        {
            var v = con.Query<VehicleDTO>("Select * from vehicles").ToList();
            var vehicles = v.Select(e => new VehicleModel()
            {
                Id = (int)e.id,
                Name = e.name,
                ImageName = e.image_name,
                Type = (VehicleType)(int)e.type,
                MaxSpeed = e.max_speed,
                Speedstep = e.speed_display,
                Address = e.address,
                IsActive = e.active == 1,
                Position = e.position,
                FullName = e.full_name,
                Railway = e.railway,
                InvertTraction = e.traction_direction == 1,
                Description = e.description,
                Dummy = e.dummy == 1,
            }).ToList();
            await Database.AddRangeAsync(vehicles);
            await Database.SaveChangesAsync();
            await Database.Vehicles.Where(e => string.IsNullOrWhiteSpace(e.Name)).ForEachAsync(e => Log.Warning($"Imported Vehicle with Adresse {e.Address} has no display name!"));
        }

        /// <summary>
        /// Extracts the database file and pictures from the Roco .z21 archive file.
        /// </summary>
        /// <returns>Returns the location of the sql database file.</returns>
        private async Task<FileInfo> ExtractPhotosAndSqlFileFromZ21File(string z21Path, string tempPath) => await Task.Run(() =>
        {
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
            Directory.CreateDirectory(tempPath);


            if (Directory.Exists(Configuration.ApplicationData.VehicleImages.FullName))
            {
                Directory.Delete(Configuration.ApplicationData.VehicleImages.FullName, true);
            }
            Directory.CreateDirectory(Configuration.ApplicationData.VehicleImages.FullName);

            //Copy the z21File to the temp location
            var z21PathNew = Path.Combine(tempPath, Path.GetFileName(z21Path));
            File.Copy(z21Path, z21PathNew);
            z21Path = z21PathNew;

            //Rename the .z21 file to .zip.
            var zipFileLocation = new StringBuilder(z21Path).Replace(".z21", ".zip").ToString();
            File.Copy(z21Path, zipFileLocation);

            //Extract the zip file and delte the zip and z21 file.
            ZipFile.ExtractToDirectory(zipFileLocation, tempPath);
            File.Delete(zipFileLocation);
            File.Delete(z21Path);

            var firstLayer = Directory.GetDirectories(tempPath).FirstOrDefault() ?? throw new MissingDirectoryException(tempPath);

            var secondLayer = Directory.GetDirectories(firstLayer).FirstOrDefault() ?? throw new MissingDirectoryException(firstLayer);

            var files = Directory.GetFiles(secondLayer).ToList();

            var sqlLiteDB = files.FirstOrDefault(e => Path.GetExtension(e).ToLower() == ".sqlite") ?? throw new FileNotFoundException("Failed to find the required .sql file!");

            foreach (var image in files.Where(e => Path.GetExtension(e) != ".sqlite").ToList())
            {
                File.Move(image, $"{Configuration.ApplicationData.VehicleImages.FullName}\\{Path.GetFileName(image)}");
            }

            return new FileInfo(sqlLiteDB);
        });
    }
}
