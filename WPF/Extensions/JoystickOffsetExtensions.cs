using SharpDX.DirectInput;
using System;

namespace WPF_Application.Extensions
{
    public static class JoystickOffsetExtensions
    {
        /// <summary>
        /// Gets the max value when a JoyStick Button is Pressed on a JoyStick. Stores the Data in App.Config.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static int GetMaxValue(this JoystickOffset e)
        {
            return Convert.ToInt32(Settings.Get($"{Enum.GetName(e)}_MaxValue")!.IsNullOrWhiteSpace(out string v) ? "0" : v);
            //return Convert.ToInt32(ConfigurationManager.AppSettings[]!.IsNullOrWhiteSpace(out string v) ? "0" : v);
        }

        /// <summary>
        /// Sets the max value when a JoyStick Button is Pressed on a Joystick. Stores the Data in App.Config.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="value"></param>
        public static void SetMaxValue(this JoystickOffset e, int value)
        {
            //var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //if (ConfigurationManager.AppSettings[$"{Enum.GetName(e)}_MaxValue"] is not null)
            //{
            //    config.AppSettings.Settings.Remove($"{Enum.GetName(e)}_MaxValue");
            //    config.Save(ConfigurationSaveMode.Modified);
            //    ConfigurationManager.RefreshSection("appSettings");
            //}
            //config.AppSettings.Settings.Add($"{Enum.GetName(e)}_MaxValue", value.ToString());
            //config.Save(ConfigurationSaveMode.Modified);
            //ConfigurationManager.RefreshSection("appSettings");
            Settings.Set($"{Enum.GetName(e)}_MaxValue", value.ToString());
        }

    }
}
