using common;
using System.Collections.Generic;
using System.Linq;

namespace _8
{
    internal class LedDigit
    {
        static LedDigit()
        {
            Digits = new Dictionary<char, SegmentSet>
            {
                ['1'] = new SegmentSet("cf"),
                ['7'] = new SegmentSet("acf"),
                ['4'] = new SegmentSet("bcdf"),
                ['8'] = new SegmentSet("abcdefg"),
                ['2'] = new SegmentSet("acdeg"),
                ['3'] = new SegmentSet("acdfg"),
                ['5'] = new SegmentSet("abdfg"),
                ['0'] = new SegmentSet("abcefg"),
                ['6'] = new SegmentSet("abdefg"),
                ['9'] = new SegmentSet("abcdfg")
            };

            DigitsByDistinct = Digits
              .OrderBy(x => x.Key)
              .GroupBy(x => x.Value.Length)
              .GroupBy(x => x.Count())
              .ToDictionary(x => x.Key, x => x.SelectMany(x => x.Select(y => y)));

            DigitsByLength = Digits
              .OrderBy(x => x.Key)
              .GroupBy(x => x.Value.Length)
              .ToDictionary(x => x.Key, x => x);
        }

        public LedDigit(SegmentSet segments)
        {
            Set = new SegmentSet(segments);
        }

        public LedDigit(KeyValuePair<char, SegmentSet> segments)
          : this(segments.Value)
        {
        }

        public static Dictionary<char, SegmentSet> Digits { get; }
        public static Dictionary<int, IGrouping<int, KeyValuePair<char, SegmentSet>>> DigitsByLength { get; }
        public static Dictionary<int, IEnumerable<KeyValuePair<char, SegmentSet>>> DigitsByDistinct { get; }

        public SegmentSet Set { get; private set; }

        public char Value
        {
            get => Digits.FirstOrDefault(x => x.Value.SetEquals(Set)).Key;
            set => Set = new SegmentSet(Digits[value]);
        }

        public int Length => Set.Length;

        public LedDigit SubtractSegments(LedDigit other)
        {
            var resSet = Set & ~other.Set;
            return new LedDigit(new SegmentSet(resSet));
        }

        public LedDigit XorSegments(LedDigit other)
        {
            var resSet = Set ^ other.Set;
            return new LedDigit(new SegmentSet(resSet));
        }

        public string[] DisplayRows()
        {
            string Cell(char x)
            {
                if (x.In("bcef")) return Set.Contains(x) ? "|" : " ";
                if (x.In("adg")) return Set.Contains(x) ? "---" : "   ";
                return "".PadLeft(x, ' ');
            }
            string Pad(int fillr)
            {
                return "".PadLeft(fillr, ' ');
            }

            return new string[5]
            {
        $"{Pad(1)}{Pad(1)}{Cell('a')}{Pad(1)}",
        $"{Pad(1)}{Cell('b')}{Pad(3)}{Cell('c')}",
        $"{Pad(1)}{Pad(1)}{Cell('d')}{Pad(2)}",
        $"{Pad(1)}{Cell('e')}{Pad(3)}{Cell('f')}",
        $"{Pad(1)}{Pad(1)}{Cell('g')}{Pad(1)}"
            };
        }
    }
}