using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public enum FunctionType
    {
        None = 0,
        Drive = 1,
        ChangeDirection = 2,
        EmergencyStop = 3,
        Sound = 4,
        Highbeam = 5,
        Lowbeam = 6,
        HornHigh = 7,
        HornLow = 8,
        Humpgear = 9,
        CurveSound = 10,
        Compressor = 11,
        Cabin = 12,
        CabinLight = 13,
        Mute = 14
    }
}
