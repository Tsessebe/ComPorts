using System.Linq;

namespace ComPort.Scanner.Extensions
{
    internal static class StringExtensions
    {
        public static string GetVersionNumbers(this string input)
        {
            return new string(input.Where(c => char.IsDigit(c) | c == '.').ToArray());
        }
    }
}
