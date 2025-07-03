using System;

namespace Extensions
{
  public static class StringExtensions
  {
    public static bool IsNullOrWhiteSpace(this string e)
    {
      return string.IsNullOrWhiteSpace(e);
    }

    public static bool IsNullOrWhiteSpace(this string e, out string value)
    {
      value = e;
      return string.IsNullOrEmpty(e);
    }

    public static int ToInt32(this string e)
    {
      return Convert.ToInt32(string.IsNullOrEmpty(e) ? "0" : e);
    }

    public static bool ToBoolean(this string e)
    {
      return Convert.ToBoolean(Convert.ToInt32(string.IsNullOrEmpty(e) ? "0" : e));
    }

    public static decimal ToDecimal(this string e)
    {
      return Convert.ToDecimal(string.IsNullOrEmpty(e) ? 0 : e);
    }

    public static long ToInt64(this string e)
    {
      return Convert.ToInt64(Convert.ToInt32(string.IsNullOrEmpty(e) ? "0" : e));
    }

    public static bool IsDecimal(this string e)
    {
      return decimal.TryParse(e, out _);
    }

    public static bool IsInt(this string e)
    {
      return int.TryParse(e, out _);
    }
  }
}