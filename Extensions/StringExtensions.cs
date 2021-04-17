using System;

namespace Extensions
{
    public static class StringExtensions
    {
        public static bool IsNullOrWhiteSpace(this string e) => string.IsNullOrWhiteSpace(e);
    }
}
