using SharpDX.DirectInput;
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
            get => new((Properties.Settings.Default.ControllerIP));
            set
            {
                Properties.Settings.Default.ControllerIP = value.Address;
                Properties.Settings.Default.Save();
            }
        }

        public static void SetJoyStickButtonForFunction(JoystickOffset j)
        {

        }
    }
}
