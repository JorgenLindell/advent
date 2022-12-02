using System.Collections.Generic;
using System.Linq;

namespace _14
{
    internal class LongString
    {
        public LinkedList<char> ListofChars = new LinkedList<char>();

        public LongString(string value)
        {
            foreach (var c in value)
            {
                ListofChars.AddLast(c);
            }
        }

        public static implicit operator LongString(string value)
        {
            return new LongString(value);
        }
        public static implicit operator string(LongString value)
        {
            return new string(value.ToChars());
        }

        public long Length => ListofChars.LongCount();

        public char[] ToChars() => ListofChars.ToArray();


    }
}