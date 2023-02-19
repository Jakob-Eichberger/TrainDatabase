using Extensions.Exceptions;
using Helper;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    internal class Z21ImportService
    {
        public Z21ImportService(Database database)
        {
            Database = database;
        }

        private Database Database { get; set; }

        public async Task Import(FileInfo z21File) => await Task.Run(async () =>
        {
            if (z21File.Extension.ToLower() is not ".z21")
            {
                throw new NotSupportedException($"Importing a {z21File.Extension} file is not supported!");
            }

            Database.DetachAllEntities();
            await Database.Database.EnsureDeletedAsync();
            await Database.Database.EnsureCreatedAsync();

            var filePath = ExtractDataFromZ21File(z21File.FullName);
        });

        /// <summary>
        /// Extracts the database file and pictures from the Roco .z21 archive file.
        /// </summary>
        /// <returns>Returns the location of the sql database file.</returns>
        private async Task<FileInfo> ExtractDataFromZ21File(string z21Path) => await Task.Run(() =>
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
                File.Move(image, $"{Configuration.ApplicationData.VehicleImages.FullName}\\{System.IO.Path.GetFileName(image)}");
            }

            return new FileInfo(sqlLiteDB);
        });

    }
}
