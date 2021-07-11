using ModelTrainController;

namespace WPF_Application.Extensions
{
    public static class TrackPowerExtensions
    {
        public static bool ToBoolean(this TrackPower e) => e == TrackPower.ON;
    }
}
