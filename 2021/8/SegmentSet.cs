using System;
using System.Collections;
using System.Collections.Generic;

namespace _8
{
    public class SegmentSet : IEnumerable<char>
    {
        public static SegmentSet Empty = new SegmentSet();

        private static readonly Dictionary<char, byte> BitAdr = new Dictionary<char, byte>
        {
            ['a'] = 0b10000000,
            ['b'] = 0b01000000,
            ['c'] = 0b00100000,
            ['d'] = 0b00010000,
            ['e'] = 0b00001000,
            ['f'] = 0b00000100,
            ['g'] = 0b00000010
        };

        private readonly byte _bitValue;
        private readonly string _value;

        public SegmentSet(byte b)
        {
            _bitValue = b;
            _value = "";
            foreach (var bte in BitAdr) _value += (byte)(_bitValue & bte.Value) > 0 ? "" + bte.Key : "";
        }

        public SegmentSet(SegmentSet set)
          : this(set._bitValue)
        {
        }

        public SegmentSet(string value = "")
        {
            _value = value;
            _bitValue = 0;
            foreach (var c in value)
                if (BitAdr.ContainsKey(c))
                    _bitValue = (byte)(_bitValue | BitAdr[c]);
        }

        public int Length => _value.Length;

        public IEnumerator<char> GetEnumerator()
        {
            return _value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected bool Equals(SegmentSet other)
        {
            return _bitValue == other._bitValue;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SegmentSet)obj);
        }

        public override int GetHashCode()
        {
            return _bitValue.GetHashCode();
        }

        public override string ToString()
        {
            var res = "";
            using var enumerable = Convert.ToString(_bitValue, 2).PadLeft(8, '0').GetEnumerator();
            for (var p = 0; p < 7; p++)
            {
                enumerable.MoveNext();
                var bit = enumerable.Current;
                if (bit == '1')
                    res += (char)('a' + p);
                else
                    res += "-";
            }

            return res;
        }

        public static explicit operator byte(SegmentSet v)
        {
            return v._bitValue;
        }

        public static explicit operator SegmentSet(byte b)
        {
            return new SegmentSet(b);
        }

        public bool Contains(char t)
        {
            return (BitAdr[t] & _bitValue) != 0;
        }

        public bool SetEquals(SegmentSet set)
        {
            return _bitValue == set._bitValue;
        }

        public static bool operator ==(SegmentSet a, SegmentSet b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(null, b)) return false;
            return a._bitValue == b._bitValue;
        }

        public static bool operator !=(SegmentSet a, SegmentSet b)
        {
            return !(a == b);
        }

        public static SegmentSet operator ^(SegmentSet a, SegmentSet b)
        {
            return new SegmentSet((byte)(a._bitValue ^ b._bitValue));
        }

        public static SegmentSet operator |(SegmentSet a, SegmentSet b)
        {
            return new SegmentSet((byte)(a._bitValue | b._bitValue));
        }

        public static SegmentSet operator &(SegmentSet a, SegmentSet b)
        {
            return new SegmentSet((byte)(a._bitValue & b._bitValue));
        }

        public static SegmentSet operator ~(SegmentSet a)
        {
            return new SegmentSet((byte)~a._bitValue);
        }
    }
}