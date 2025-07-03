using System;

namespace Service.Viewmodel
{
  public struct FunctionPoint : IComparable<FunctionPoint>
  {
    public double X, Y;

    public FunctionPoint(double x, double y = 0)
    {
      X = x;
      Y = y;
    }

    /// <summary>
    /// Gets the x value for a given <paramref name="y"/> value.
    /// </summary>
    /// <param name="start">Point where the line starts.</param>
    /// <param name="end">Point where the line ends.</param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static double GetXValue(FunctionPoint start, FunctionPoint end, double y)
    {
      double x1 = start.X;
      double y1 = start.Y;
      double x2 = end.X;
      double y2 = end.Y;
      return y / ((y2 - y1) / (x2 - x1)) - y + x1;
    }

    /// <summary>
    /// Gets the y value for a given <paramref name="x"/> value.
    /// </summary>
    /// <param name="start">Point where the line starts.</param>
    /// <param name="end">Point where the line ends.</param>
    /// <param name="x"></param>
    /// <returns></returns>
    public static double Interplate(FunctionPoint start, FunctionPoint end, double x)
    {
      return start.Y + (x - start.X) * ((end.Y - start.Y) / (end.X - start.X));
    }

    public int CompareTo(FunctionPoint other)
    {
      return X.CompareTo(other.X);
    }
  }
}