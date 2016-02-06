using System.Globalization;

namespace Quartz.Util
{
    public static class StringExtensions
    {
        public static string NullSafeTrim(this string s)
        {
            if (s == null)
            {
                return null;
            }

            return s.Trim();
        }

        public static string TrimEmptyToNull(this string s)
        {
            if (s == null)
            {
                return null;
            }

            s = s.Trim();

            if (s.Length == 0)
            {
                return null;
            }

            return s;
        }

        public static bool IsNullOrWhiteSpace(this string s)
        {
            return s == null || s.Trim().Length == 0;
        }

        public static string FormatInvariant(this string s, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, s, args);
        }
    }
}