using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Specialized;
using Importer;
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
                    return IPAddress.Parse(string.IsNullOrWhiteSpace(Settings.Get(nameof(ControllerIP))) ? "192.168.0.111" : Settings.Get(nameof(ControllerIP)));
                }
                catch (Exception)
                {
                    return IPAddress.Parse("192.168.0.111");
                }
            }
            set
            {
                Settings.Set(nameof(ControllerIP), value.ToString());
            }
        }


        private static void Set(string key, string value) => ConfigurationManager.AppSettings.Set(key, value.IsNullOrWhiteSpace(out string v) ? "" : v);


        private static string Get(string key) => ConfigurationManager.AppSettings.Get(key).IsNullOrWhiteSpace(out string v) ? "" : v;
    }
}
