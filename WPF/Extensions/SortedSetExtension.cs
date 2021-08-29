using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Application.Extensions
{
    public static class SortedSetExtension
    {
        public static int GetXValue(this SortedSet<FunctionPoint> function, double yValue)
        {
            var sortedSet = new SortedSet<FunctionPoint>(function.Select(e => new FunctionPoint(e.Y, e.X)));
            var xValue = (int)function.FirstOrDefault(e => e.Y == yValue).X;
            if (!function.Any(e => e.Y == yValue))
            {
                var leftPoint = sortedSet.GetViewBetween(new FunctionPoint(double.NegativeInfinity), new FunctionPoint(yValue)).Max;
                var rightPoint = sortedSet.GetViewBetween(new FunctionPoint(yValue), new FunctionPoint(double.PositiveInfinity)).Min;
                xValue = (int)Math.Round(FunctionPoint.Interplate(leftPoint, rightPoint, yValue), MidpointRounding.ToEven);
            }
            return xValue;
        }

        public static double GetYValue(this SortedSet<FunctionPoint> function, int xValue)
        {
            var yValue = function.FirstOrDefault(e => e.X == xValue).Y;
            if (!function.Any(e => e.X == xValue))
            {
                var leftPoint = function.GetViewBetween(new FunctionPoint(double.NegativeInfinity), new FunctionPoint(xValue)).Max;
                var rightPoint = function.GetViewBetween(new FunctionPoint(xValue), new FunctionPoint(double.PositiveInfinity)).Min;
                yValue = FunctionPoint.Interplate(leftPoint, rightPoint, xValue);
            }
            return yValue;
        }
    }
}
