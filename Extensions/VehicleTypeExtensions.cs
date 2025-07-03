using Model;

namespace Extensions
{
  public static class VehicleTypeExtensions
  {
    public static bool IsLokomotive(this VehicleType e)
    {
      return e == VehicleType.Lokomotive;
    }

    public static bool IsSteuerwagen(this VehicleType e)
    {
      return e == VehicleType.Steuerwagen;
    }

    public static bool IsWagen(this VehicleType e)
    {
      return e == VehicleType.Wagen;
    }
  }
}