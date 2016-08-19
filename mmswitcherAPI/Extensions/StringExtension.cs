using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace mmswitcherAPI.Extensions
{
    public static class StringExtension
    {
        public static int ParseNumber(this string str)
        {
            var regex = new Regex(@"-?\d+");
            var match = regex.Match(str);

            return match.Success ? Int32.Parse(match.Value) : 0; 

        }

        public static int? TryParseNumber(this string str)
        {
            var regex = new Regex(@"-?\d+");
            var match = regex.Match(str);
            if (match.Success)
                return Int32.Parse(match.Value);
            return null;
        }

        public static int[] ParseNumbers(this string str)
        {
            return Regex.Matches(str, @"-?\d+").OfType<Match‌>().Select(m => Int32.Parse(m.Value)).ToArray();
        }

    }
}
