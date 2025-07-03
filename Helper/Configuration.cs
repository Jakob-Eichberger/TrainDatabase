using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;

namespace Helper
{
  public static class Configuration
  {
    public static class ApplicationData
    {
      /// <summary>
      /// Path where application related data is stored. 
      /// </summary>
      public static DirectoryInfo ApplicationDataDirectory => new(
                                                                  Path.Combine(
                                                                               Environment.GetFolderPath(
                                                                                                         Environment
                                                                                                          .SpecialFolder
                                                                                                          .ApplicationData),
                                                                               "TrainDatabase"));

      /// <summary>
      /// Returns the location of the vehicle image folder.
      /// </summary>
      public static DirectoryInfo VehicleImages =>
        new(Path.Combine(ApplicationDataDirectory.FullName, "Data", "VehicleImage"));

      /// <summary>
      /// Returns the location of the log file directory.
      /// </summary>
      public static DirectoryInfo LogDirectory => new(Path.Combine(ApplicationDataDirectory.FullName, "Log"));

      /// <summary>
      /// Returns the location of the database file directory.
      /// </summary>
      public static FileInfo DatabaseFile =>
        new(Path.Combine(ApplicationDataDirectory.FullName, "Data", "Database.sqlite"));

      /// <summary>
      /// Returns a temporary path for this application. Every time this property is called the path is going to be different!
      /// </summary>
      public static DirectoryInfo TempPath =>
        new(Path.Combine(Path.GetTempPath(), "TrainDatabase", Guid.NewGuid().ToString()));
    }


    public static IPAddress ClientIP
    {
      get
      {
        try
        {
          return IPAddress.Parse(Get(nameof(ClientIP), "192.168.0.111") ?? IPAddress.Loopback.ToString());
        }
        catch (Exception)
        {
          return IPAddress.Parse("192.168.0.111");
        }
      }
      set => Set(nameof(ClientIP), value.ToString());
    }

    public static bool UsingJoyStick
    {
      get
      {
        try
        {
          return bool.Parse(Get(nameof(UsingJoyStick)));
        }
        catch
        {
          return false;
        }
      }
      set => Set(nameof(UsingJoyStick), value.ToString());
    }

    public static bool OpenDebugConsoleOnStart
    {
      get => GetBool(nameof(OpenDebugConsoleOnStart)) ?? false;
      set => Set(nameof(OpenDebugConsoleOnStart), value.ToString());
    }

    public static string ArduinoComPort
    {
      get => Get(nameof(ArduinoComPort)) ?? SerialPort.GetPortNames().FirstOrDefault() ?? "";
      set => Set(nameof(ArduinoComPort), value);
    }

    public static int ArduinoBaudrate
    {
      get => GetInt(nameof(ArduinoBaudrate)) ?? 9600;
      set => Set(nameof(ArduinoBaudrate), $"{value}");
    }

    /// <summary>
    /// Sets a value for a key.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void Set(string key, string value)
    {
      if (string.IsNullOrWhiteSpace(key))
      {
        return;
      }

      System.Configuration.Configuration? config =
        ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
      if (ConfigurationManager.AppSettings[key] is not null)
      {
        config.AppSettings.Settings.Remove(key);
        config.Save(ConfigurationSaveMode.Modified);
        ConfigurationManager.RefreshSection("appSettings");
      }

      if (value is not null)
      {
        config.AppSettings.Settings.Add(key, value);
        config.Save(ConfigurationSaveMode.Modified);
        ConfigurationManager.RefreshSection("appSettings");
      }
    }

    /// <summary>
    /// Tries to get the value for the key as a <see cref="string"/>. Does not return null.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string Get(string key, string @default = "")
    {
      return ConfigurationManager.AppSettings[key] ?? @default;
    }

    /// <summary>
    /// Tries to get the value for the key as a <see cref="bool"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static bool? GetBool(string key)
    {
      return bool.TryParse(Get(key), out bool result) ? result : null;
    }

    /// <summary>
    /// Tries to get the value for the key as a <see cref="int"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static int? GetInt(string key)
    {
      return int.TryParse(Get(key), out int result) ? result : null;
    }

    /// <summary>
    /// Tries to get the value for the key as a <see cref="long"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static long? GetLong(string key)
    {
      return long.TryParse(Get(key), out long result) ? result : null;
    }

    /// <summary>
    /// Tries to get the value for the key as a <see cref="decimal"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static decimal? GetDecimal(string key)
    {
      return decimal.TryParse(Get(key), out decimal result) ? result : null;
    }
  }
}