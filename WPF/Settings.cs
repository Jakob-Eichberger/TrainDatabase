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
using Extensions;

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

        public static void Set(string key, string value) => ConfigurationManager.AppSettings.Set(key, value.IsNullOrWhiteSpace(out string v) ? "" : v);

        public static string Get(string key) => ConfigurationManager.AppSettings.Get(key).IsNullOrWhiteSpace(out string v) ? "" : v;

        public static Dictionary<FunctionType, (JoystickOffset joyStick, int maxValue)> GetJoyStickFunctionDictionary()
        {
            Dictionary<FunctionType, (JoystickOffset joyStick, int maxValue)> l = new();
            foreach (var item in Enum.GetValues(typeof(FunctionType)))
            {
                try
                {
                    if (Enum.TryParse(Get(Enum.GetName((FunctionType)item)!), out JoystickOffset joyStickUpdate))
                    {
                        l.Add((FunctionType)item, (joyStick: joyStickUpdate, maxValue: joyStickUpdate.GetMaxValue()));
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
