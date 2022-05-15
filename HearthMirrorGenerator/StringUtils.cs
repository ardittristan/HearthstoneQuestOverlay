using System.Text.RegularExpressions;

namespace HearthMirrorGenerator
{
    internal static class StringUtils
    {
        public static string RegexReplace(this string str, string pattern, string replacement, RegexOptions options) =>
            Regex.Replace(str, pattern, replacement, options);

        public static string RegexReplace(this string str, string pattern, string replacement) =>
            Regex.Replace(str, pattern, replacement);
    }
}
