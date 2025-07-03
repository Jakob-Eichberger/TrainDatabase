using System;
using System.Collections.Generic;
using System.Linq;
using Service.Viewmodel;

namespace Service.Extension
{
  public static class SortedSetExtension
  {
    public static int GetXValue(this SortedSet<FunctionPoint> function, double yValue)
    {
      SortedSet<FunctionPoint> sortedSet = new(function.Select(e => new FunctionPoint(e.Y, e.X)));
      int xValue = (int)function.FirstOrDefault(e => e.Y == yValue).X;
      if (!function.Any(e => e.Y == yValue))
      {
        FunctionPoint leftPoint = sortedSet.GetViewBetween(new(double.NegativeInfinity), new(yValue)).Max;
        FunctionPoint rightPoint = sortedSet.GetViewBetween(new(yValue), new(double.PositiveInfinity)).Min;
        xValue = (int)Math.Round(FunctionPoint.Interplate(leftPoint, rightPoint, yValue), MidpointRounding.ToEven);
      }

      return xValue;
    }

    public static double GetYValue(this SortedSet<FunctionPoint> function, int xValue)
    {
      double yValue = function.FirstOrDefault(e => e.X == xValue).Y;
      if (!function.Any(e => e.X == xValue))
      {
        FunctionPoint leftPoint = function.GetViewBetween(new(double.NegativeInfinity), new(xValue)).Max;
        FunctionPoint rightPoint = function.GetViewBetween(new(xValue), new(double.PositiveInfinity)).Min;
        yValue = FunctionPoint.Interplate(leftPoint, rightPoint, xValue);
      }

      return yValue;
    }
  }
}