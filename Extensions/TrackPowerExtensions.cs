using TrainDatabase.Z21Client.Enum;

namespace Extensions
{
    public static class TrackPowerExtensions
    {
        public static bool ToBoolean(this TrackPower e) => e == TrackPower.ON;
    }
}
