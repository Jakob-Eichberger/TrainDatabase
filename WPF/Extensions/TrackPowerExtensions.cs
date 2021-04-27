using ModelTrainController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Application.Extensions
{
    public static class TrackPowerExtensions
    {
        public static bool ToBoolean(this TrackPower e)
        {
            switch (e)
            {
                case TrackPower.OFF:
                    return false;
                case TrackPower.ON:
                    return true;
                case TrackPower.Short:
                    return false;
                case TrackPower.Programing:
                    return false;
                default:
                    return false;
            }
        }

        public static string GetString(this TrackPower e)
        {
            string v = e switch
            {
                TrackPower.OFF => "Off",
                TrackPower.ON => "ON",
                TrackPower.Short => "Short",
                TrackPower.Programing => "Programing",
                _ => "-",
            };
            return v;
        }
    }
}
