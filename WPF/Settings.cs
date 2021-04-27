using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Specialized;
using Model;
using WPF_Application.Extensions;
using WPF_Application.CentralStation;

namespace WPF_Application
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

        public static CentralStationType? CentralStation
        {
            get
            {
                return Enum.TryParse(typeof(CentralStationType), Get(nameof(CentralStation)), out var type) ? (CentralStationType)type! : null;
            }
            set
            {
                if (value is not null)
                    Set(nameof(CentralStation), Enum.GetName((CentralStationType)value)!);
                else
                    Set(nameof(CentralStation), null!);
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

        public static string Get(string key)
        {
            return ConfigurationManager.AppSettings[key] ?? "";
        }

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
                        var i = "";
                    }
                }
                catch
                {
                    var i = "";
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
