using Model;
using SharpDX.DirectInput;
using System;

namespace WPF_Application.Extensions
{
    public static class FunctionTypeExtensions
    {
        public static void SetJoyStick(this FunctionType e, JoystickOffset? joystick)
        {
            Settings.Set(Enum.GetName(e)!, joystick is null ? null! : Enum.GetName((JoystickOffset)joystick)!);
        }
    }
}
