using Dapper;
using Extensions;
using Extensions.Exceptions;
using Helper;
using Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Model;
using Serilog;
using Serilog.Core;
using Service.ImportService.Z21.TDO;
using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
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
        public async Task Import(FileInfo z21File) => await Task.Run(async () =>
        {
            if (z21File.Extension.ToLower() is not ".z21")
            {
                throw new NotSupportedException($"Importing a {z21File.Extension} file is not supported!");
            }

            Database.DetachAllEntities();
            await Database.Database.EnsureDeletedAsync();
            await Database.Database.EnsureCreatedAsync();

            var filePath = await ExtractPhotosAndSqlFileFromZ21File(z21File.FullName);
            var con = new SqliteConnection($"Data Source={filePath}");
            MapVehicles(con);
            MapFunctions(con, true);
        });

        private void MapFunctions(SqliteConnection con, bool removeEmptyFunctions)
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

            Database.AddRange(functions);
            Database.SaveChanges();
            Database.InvokeCollectionChanged();
        }

        private void MapVehicles(SqliteConnection con)
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
            Database.AddRange(vehicles);
            Database.SaveChanges();
            Database.Vehicles.Where(e => string.IsNullOrWhiteSpace(e.Name)).ForEachAsync(e => Log.Warning($"Imported Vehicle with Adresse {e.Address} has no display name!"));
        }

        /// <summary>
        /// Extracts the database file and pictures from the Roco .z21 archive file.
        /// </summary>
        /// <returns>Returns the location of the sql database file.</returns>
        private async Task<FileInfo> ExtractPhotosAndSqlFileFromZ21File(string z21Path) => await Task.Run(() =>
        {
            var zipFileLocation = new StringBuilder(z21Path).Replace(".z21", ".zip").ToString();

            if (Directory.Exists(Configuration.ApplicationData.TempPath.FullName))
            {
                Directory.Delete(Configuration.ApplicationData.TempPath.FullName, true);
            }

            if (Directory.Exists(Configuration.ApplicationData.VehicleImages.FullName))
            {
                Directory.Delete(Configuration.ApplicationData.VehicleImages.FullName, true);
            }
            Directory.CreateDirectory(Configuration.ApplicationData.VehicleImages.FullName);

            File.Copy(z21Path, zipFileLocation);
            ZipFile.ExtractToDirectory(zipFileLocation, Configuration.ApplicationData.TempPath.FullName);
            File.Delete(zipFileLocation);

            var firstLayer = Directory.GetDirectories(Configuration.ApplicationData.TempPath.FullName).FirstOrDefault() ?? throw new MissingDirectoryException(Configuration.ApplicationData.TempPath);

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
