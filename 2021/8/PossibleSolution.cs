using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace _8
{
    public class PossibleSolution
    {
        public ImmutableDictionary<SegmentSet, char> Defined => _defined.ToImmutableDictionary();

        private readonly Dictionary<char, SegmentSet> _possible = new Dictionary<char, SegmentSet>
        {
            ['a'] = new SegmentSet("abcdefg"),
            ['b'] = new SegmentSet("abcdefg"),
            ['c'] = new SegmentSet("abcdefg"),
            ['d'] = new SegmentSet("abcdefg"),
            ['e'] = new SegmentSet("abcdefg"),
            ['f'] = new SegmentSet("abcdefg"),
            ['g'] = new SegmentSet("abcdefg")
        };

        private readonly Dictionary<SegmentSet, char> _defined = new Dictionary<SegmentSet, char>();

        public void Define(char x, SegmentSet seg)
        {
            var decodedSegments = LedDigit.Digits[x];
            var impossibleSegments = LedDigit.Digits['8'] & ~decodedSegments;

            foreach (var segment in decodedSegments)
                _possible[segment] = _possible[segment] & seg;

            foreach (var segment in impossibleSegments)
                _possible[segment] = _possible[segment] & ~seg;

            _defined[seg] = x;
        }

        public bool TestDigit(SegmentSet digitSet, SegmentSet signalSet)
        {
            var digitToTest = digitSet.ToString().Where(x => x != '-');
            var failed = false;
            foreach (var digitSeg in digitToTest)
            {
                var allowInSegment = _possible[digitSeg];
                if ((allowInSegment & signalSet) == SegmentSet.Empty)
                {
                    failed = true;
                    break;
                }
            }

            return !failed;
        }
    }
}