using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper
{
    public class JoyStickUpdateEventArgs : EventArgs
    {
        public readonly int maxValue;
        public readonly int currentValue;
        public readonly JoystickOffset joyStickOffset;

        public JoyStickUpdateEventArgs(JoystickOffset _joyStickOffset, int _currentValue, int _maxValue) : base()
        {
            currentValue = _currentValue;
            maxValue = _maxValue;
            joyStickOffset = _joyStickOffset;
        }
    }
}
