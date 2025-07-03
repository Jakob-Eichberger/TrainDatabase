using Helper;
using Model;
using SharpDX.DirectInput;
using System;

namespace TrainDatabase.Extensions
{
  public static class FunctionTypeExtensions
  {
    public static void SetJoyStick(this FunctionType e, JoystickOffset? joystick)
    {
      Configuration.Set(Enum.GetName(e)!, joystick is null ? null! : Enum.GetName((JoystickOffset)joystick)!);
    }
  }
}