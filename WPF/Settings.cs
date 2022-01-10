using Model;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using TrainDatabase.Z21Client.Enum;
using TrainDatabase.Extensions;
using Extensions;
using System.IO.Ports;
using System.Linq;

namespace TrainDatabase
{
    public static class Settings
    {
        public static IPAddress ControllerIP
        {
            get
            {
                try
                {
                    return IPAddress.Parse(string.IsNullOrWhiteSpace(Get(nameof(ControllerIP))) ? "192.168.0.111" : Get(nameof(ControllerIP)));
                }
                catch (Exception)
                {
                    return IPAddress.Parse("192.168.0.111");
                }
            }
            set
            {
                Set(nameof(ControllerIP), value.ToString());
            }
        }

        public static int ControllerPort
        {
            get
            {
                return int.TryParse(Get(nameof(ControllerPort)), out int port) ? port : 21105;
            }
            set
            {
                if (value > 0 && value < 65535)
                    Set(nameof(ControllerPort), value.ToString());
                else
                    throw new ApplicationException($"'{value}' ist kein valider Port!");
            }
        }

        public static bool UsingJoyStick
        {
            get
            {
                try
                {
                    return bool.Parse(Get(nameof(UsingJoyStick)).IsNullOrWhiteSpace(out string v) ? "false" : v);
                }
                catch
                {
                    return false;
                }
            }
            set
            {
                Set(nameof(UsingJoyStick), value.ToString());
            }
        }

        public static bool OpenDebugConsoleOnStart
        {
            get => GetBool(nameof(OpenDebugConsoleOnStart)) ?? false;
            set => Set(nameof(OpenDebugConsoleOnStart), value.ToString());
        }

        public static string ArduinoComPort
        {
            get => Get(nameof(ArduinoComPort)) ?? SerialPort.GetPortNames().FirstOrDefault() ?? "";
            set => Set(nameof(ArduinoComPort), value?.ToString());
        }

        public static int? ArduinoBaudrate
        {
            get => GetInt(nameof(ArduinoBaudrate)) ?? 9600;
            set => Set(nameof(ArduinoBaudrate), value?.ToString());
        }

        /// <summary>
        /// Sets a value for a key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Set(string key, string value)
        {
            if (key.IsNullOrWhiteSpace()) return;
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
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
        public static string? Get(string key) => ConfigurationManager.AppSettings[key];

        /// <summary>
        /// Tries to get the value for the key as a <see cref="bool"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool? GetBool(string key) => bool.TryParse(Get(key), out bool result) ? result : null;

        /// <summary>
        /// Tries to get the value for the key as a <see cref="int"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int? GetInt(string key) => int.TryParse(Get(key), out int result) ? result : null;

        /// <summary>
        /// Tries to get the value for the key as a <see cref="long"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static long? GetLong(string key) => long.TryParse(Get(key), out long result) ? result : null;

        /// <summary>
        /// Tries to get the value for the key as a <see cref="decimal"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static decimal? GetDecimal(string key) => decimal.TryParse(Get(key), out decimal result) ? result : null;

        public static Dictionary<FunctionType, (JoystickOffset joyStick, int maxValue)> FunctionToJoyStickDictionary()
        {
            Dictionary<FunctionType, (JoystickOffset joyStick, int maxValue)> l = new();
            foreach (var item in Enum.GetValues(typeof(FunctionType)))
            {
                try
                {
                    //Set(Enum.GetName((FunctionType)item), "");
                    if (Enum.TryParse(Get(Enum.GetName((FunctionType)item)!), out JoystickOffset joyStickUpdate))
                    {
                        l.Add((FunctionType)item, (joyStick: joyStickUpdate, maxValue: joyStickUpdate.GetMaxValue()));
                    }
                    else
                    {
                    }
                }
                catch
                {
                    //Just do nothing;
                }
            }
            return l;
        }

        public static Dictionary<JoystickOffset, int> GetJoyStickMaxValue()
        {
            Dictionary<JoystickOffset, int> l = new();
            foreach (var item in Enum.GetValues(typeof(JoystickOffset)))
            {
                l.Add((JoystickOffset)item, ((JoystickOffset)item).GetMaxValue());
            }
            return l;
        }


    }
}
