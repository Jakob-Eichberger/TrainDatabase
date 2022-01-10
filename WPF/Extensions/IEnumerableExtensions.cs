using Model;
using System.Collections.Generic;
using System.Linq;

namespace TrainDatabase.Extensions
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Swaps the position of two vehicles. Uses the <see cref="Vehicle.Position"/> property.
        /// </summary>
        /// <param name="moveUp">True if the position of the <paramref name="vehicle"/> should be moved up.</param>
        /// <param name="vehicle"></param>
        public static void Swap(this IEnumerable<Vehicle> vehicles, Vehicle vehicle, bool moveUp)
        {
            var pos = vehicle.Position;
            Vehicle vehicle2 = default!;

            if (moveUp)
                vehicle2 = vehicles.OrderBy(e => e.Position).SkipWhile(e => e.Position <= pos).FirstOrDefault()!;
            else
                vehicle2 = vehicles.OrderByDescending(e => e.Position).SkipWhile(e => e.Position >= pos).FirstOrDefault()!;

            if (vehicle2 is object)
            {
                var temp = vehicle2.Position;
                vehicle2.Position = vehicle.Position;
                vehicle.Position = temp;
            }
        }

        /// <summary>
        /// Swaps the position of two functions. Uses the <see cref="Function.Position"/> property.
        /// </summary>
        /// <param name="functions"></param>
        /// <param name="Id2"></param>
        /// <param name="id2"></param>
        public static void Swap(this IEnumerable<Function> functions, int id1, bool moveUp)
        {

        }
    }
}
