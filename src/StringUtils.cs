using System;

namespace Wheatech
{
    internal static class StringUtils
    {
        internal static bool StringStartsWithIgnoreCase(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
            {
                return false;
            }
            if (s2.Length > s1.Length)
            {
                return false;
            }
            return (string.Compare(s1, 0, s2, 0, s2.Length, StringComparison.OrdinalIgnoreCase) == 0);
        }
    }
}
