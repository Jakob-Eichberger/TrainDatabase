using Model;
using Z21;

namespace Viewmodel
{
    public struct MultiTractionItem
    {
        public MultiTractionItem(VehicleModel vehicle)
        {
            Vehicle = vehicle;
            TractionForward = GetSortedSet(vehicle?.TractionForward!);
            TractionBackward = GetSortedSet(vehicle?.TractionBackward!);
        }

        public SortedSet<FunctionPoint> TractionBackward { get; private set; }

        public SortedSet<FunctionPoint> TractionForward { get; private set; }

        public VehicleModel Vehicle { get; }

        /// <summary>
        /// Converts the paramter <paramref name="tractionArray"/> to a <see cref="LineSeries"/> object.
        /// </summary>
        /// <param name="tractionArray"></param>
        /// <returns></returns>
        private static SortedSet<FunctionPoint> GetSortedSet(decimal?[] tractionArray)
        {
            if (tractionArray is null || tractionArray[Client.maxDccStep] is null)
                return new();

            SortedSet<FunctionPoint>? function = new();

            for (int i = 0; i <= Client.maxDccStep; i++)
                if (tractionArray[i] is not null)
                    function.Add(new(i, (double)(tractionArray[i] ?? 0)));
            return function;
        }
    }
}
