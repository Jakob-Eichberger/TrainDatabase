using Extensions;
using SharpDX.DirectInput;
using System;

namespace TrainDatabase.Extensions
{
    public static class JoystickOffsetExtensions
    {
        /// <summary>
        /// Gets the max value when a JoyStick Button is Pressed on a JoyStick. Stores the Data in App.Config.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static int GetMaxValue(this JoystickOffset e) => Convert.ToInt32(Settings.Get($"{Enum.GetName(e)}_MaxValue")!.IsNullOrWhiteSpace(out string v) ? "0" : v);

        /// <summary>
        /// Sets the max value when a JoyStick Button is Pressed on a Joystick. Stores the Data in App.Config.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="value"></param>
        public static void SetMaxValue(this JoystickOffset e, int value) => Settings.Set($"{Enum.GetName(e)}_MaxValue", value.ToString());

    }
}
