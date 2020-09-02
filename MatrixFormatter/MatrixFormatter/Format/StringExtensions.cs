using System.Text.RegularExpressions;

namespace MatrixFormatter.Format
{
    //From https://alexdunn.org/2017/05/16/xamarin-tip-binding-a-picker-to-an-enum/
    public static class StringExtensions
    {
        public static string SplitCamelCase(this string str)
        {
            return Regex.Replace(
                Regex.Replace(
                    str,
                    @"(\P{Ll})(\P{Ll}\p{Ll})",
                    "$1 $2"
                ),
                @"(\p{Ll})(\P{Ll})",
                "$1 $2"
            );
        }
    }
}