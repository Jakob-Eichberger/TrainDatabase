using Model;

namespace TrainDatabase.Extensions
{
    public static class VehicleTypeExtensions
    {
        public static bool IsLokomotive(this VehicleType e) => e == VehicleType.Lokomotive;

        public static bool IsSteuerwagen(this VehicleType e) => e == VehicleType.Steuerwagen;

        public static bool IsWagen(this VehicleType e) => e == VehicleType.Wagen;

    }
}
