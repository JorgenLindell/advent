using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace common
{
    public static class StringExtensions
    {
        public static string Repeat(this string input, int count)
        {
            if (string.IsNullOrEmpty(input) || count <= 1)
                return input;

            var builder = new StringBuilder(input.Length * count);

            for (var i = 0; i < count; i++) builder.Append(input);

            return builder.ToString();
        }
        public static string Map(this string s, string s1, string s2)
        {
            var retVal = "";
            foreach (var t in s)
            {
                var c = t;
                for (var j = 0; j < s1.Length; j++)
                {
                    if (t != s1[j]) continue;
                    c = s2[j];
                    break;
                }
                retVal += c;
            }
            return retVal;
        }
        public static IEnumerable<string> Pairs(this string full)
        {
            for (int i = 1; i < full.Length; i++)
            {
                yield return full.Substring(i - 1, 2);
            }
        }

        public static string? StringJoin(this IEnumerable<string>? strEnumerable, string delimiter = ", ")
        {
            return strEnumerable is null ? null : string.Join(delimiter, strEnumerable);
        }
        public static string? StringJoin(this IEnumerable<char>? strEnumerable, string delimiter = ", ")
        {
            return strEnumerable is null ? null : string.Join(delimiter, strEnumerable);
        }

        public static bool IsNullOrEmpty(this string s) => string.IsNullOrEmpty(s);
        public static bool IsNullOrWhiteSpace(this string s) => string.IsNullOrWhiteSpace(s);

        public static bool IsRegexMatch(this string s, string pattern)
        {
            return Regex.IsMatch(s, pattern);
        }

        private static readonly Dictionary<char, string> HToB = new Dictionary<char, string>
        {
            { '0', "0000" },
            { '1', "0001" },
            { '2', "0010" },
            { '3', "0011" },
            { '4', "0100" },
            { '5', "0101" },
            { '6', "0110" },
            { '7', "0111" },
            { '8', "1000" },
            { '9', "1001" },
            { 'a', "1010" },
            { 'b', "1011" },
            { 'c', "1100" },
            { 'd', "1101" },
            { 'e', "1110" },
            { 'f', "1111" }
        };

        public static long? ToLong(this string inp)
        {
            if (long.TryParse(inp, out long outLong))
                return outLong;
            return null;
        }
        public static int? ToInt(this string inp)
        {
            if (int.TryParse(inp, out int outInt))
                return outInt;
            return null;
        }
        public static string ToBin(this string inp)
        {
            return string.Join("", inp.ToLowerInvariant().Select(x => HToB[x]));
        }
        public static long BinToInt64(this string inp, int start = 0, int len = 0)
        {
            if (len == 0)
            {
                if (inp.Length > 63) throw new ArgumentException("binary string too long for int64");
                return Convert.ToInt64(inp, 2);
            }
            if (len < 64)
            {
                var substring = inp.Substring(start, len);
                return substring.BinToInt64();
            }
            throw new ArgumentException("binary string too long for int64");
        }


    }

}