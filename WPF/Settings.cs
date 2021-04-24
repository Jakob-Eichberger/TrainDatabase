using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Application
{
    public static class Settings
    {
        public static IPAddress ControlerIP
        {
            get => new(Properties.Settings.Default.ControllerIP);
            set
            {
#pragma warning disable CS0618 // Type or member is obsolete
                Properties.Settings.Default.ControllerIP = value.Address;
#pragma warning restore CS0618 // Type or member is obsolete
                Properties.Settings.Default.Save();
            }
        }


    }
}
