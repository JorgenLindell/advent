using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace common
{
    public static class StreamExtensions
    {
        private static Regex whitespace = new Regex("\\s", RegexOptions.Compiled);
        public static int? ReadInt(this TextReader stream)
        {
            var chars = "";
            while (stream.Peek() != -1
                   && (stream.Peek() < '0' || stream.Peek() > '9')
                   && !stream.Peek().In('-', '+'))
            {
                stream.Read();
            }
            while (stream.Peek() != -1
                    && (stream.Peek() >= '0' && stream.Peek() <= '9'
                   || stream.Peek().In('-', '+')))
            {
                chars += (char)stream.Read();
            }

            if (stream.Peek() == -1 && chars.Length == 0)
                return null;

            return int.Parse(chars);
        }
        public static string SkipUntil(this TextReader stream, char target)
        {
            var chars = "";
            while (stream.Peek() != -1
                   && target != (char)stream.Peek())
            {
                chars += (char)stream.Read();
                stream.Read();
            }
            return chars;
        }
        public static string SkipUntil(this TextReader stream, char[] pattern)
        {
            var chars = "";
            var peek = stream.Peek();
            while (peek != -1 && !((char)peek).In(pattern))
            {
                chars += (char)stream.Read();
                stream.Read();
            }
            return chars;
        }
        public static string SkipOver(this TextReader stream, string skipString)
        {
            while (stream.Peek() != -1
                   && !((char)stream.Peek()).In(skipString))
            {
                stream.Read();
            }

            var chars = "";
            while (stream.Peek() != -1
                   && skipString.StartsWith((chars + (char)stream.Peek()))
                   && chars.Length < skipString.Length)
            {
                chars += (char)stream.Read();
            }
            return chars;
        }
        public static string ReadWord(this TextReader stream)
        {
            var chars = "";
            while (stream.Peek() != -1
                   && whitespace.IsMatch("" + (char)stream.Peek()))
            {
                stream.Read();
            }
            while (stream.Peek() != -1
                   && !whitespace.IsMatch("" + (char)stream.Peek()))
            {
                chars += (char)stream.Read();
            }

            return chars;
        }
        public static string ReadUntil(this TextReader stream, Regex limitRegEx)
        {
            var chars = "";

            while (stream.Peek() != -1
                   && !limitRegEx.IsMatch("" + (char)stream.Peek()))
            {
                chars += (char)stream.Read();
            }

            return chars;
        }
        public static string ReadWhile(this TextReader stream, Regex limitRegEx)
        {
            var chars = "";

            while (stream.Peek() != -1
                   && limitRegEx.IsMatch("" + (char)stream.Peek()))
            {
                chars += (char)stream.Read();
            }

            return chars;
        }
        public static string ReadWhileMatch(this TextReader stream, Regex limitRegEx)
        {
            var chars = "";

            while (stream.Peek() != -1
                   && limitRegEx.IsMatch(chars + (char)stream.Peek())
                   && chars.Length + 1 == limitRegEx.Match(chars + (char)stream.Peek()).Length)
            {
                chars += (char)stream.Read();
            }

            return chars;
        }
        public static void Reset(this StringReader reader)
        {
            reader.GetType()
                .GetField("_pos", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(reader, 0);
        }
    }
}