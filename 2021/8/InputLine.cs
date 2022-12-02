using common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace _8
{
    public class InputLine
    {
        private readonly PossibleSolution _sol = new PossibleSolution();

        private readonly List<SegmentSet> _digits = new List<SegmentSet>();
        private List<SegmentSet> _signals = new List<SegmentSet>();
        public IEnumerable<SegmentSet> Signals => _signals;

        public Dictionary<int, IGrouping<int, SegmentSet>> SignalsByLength => GetSignalsByLength(Signals);
        public Dictionary<int, IGrouping<int, SegmentSet>> UndefinedSignalsByLength => GetSignalsByLength(UndefinedSignals);
        public IEnumerable<SegmentSet> UndefinedSignals => Signals.Where(x => !_sol.Defined.ContainsKey(x)).ToList();

        public IEnumerable<(char digit, SegmentSet set)> UndefinedDigits =>
          LedDigit.Digits
            .Where(d => !_sol.Defined.Values.Contains(d.Key))
            .Select(x => (x.Key, x.Value))
            .ToList();

        public Dictionary<int, List<(char digit, SegmentSet set)>> UndefinedDigitsByLength =>
          UndefinedDigits.GroupBy(x => x.set.Length)
            .ToDictionary(x => x.Key, s => s.Select(y => y).ToList());

        public IEnumerable<SegmentSet> All
        {
            get
            {
                foreach (var signal in _signals) yield return signal;

                foreach (var digit in _digits) yield return digit;
            }
        }

        public IEnumerable<SegmentSet> Wires
        {
            get
            {
                foreach (var digit in _digits) yield return digit;
            }
        }

        public int DisplayValue { get; private set; } = -1;

        private static Dictionary<int, IGrouping<int, SegmentSet>> GetSignalsByLength(IEnumerable<SegmentSet> enumerable)
        {
            return enumerable
              .GroupBy(s => s.Length)
              .ToDictionary(g => g.Key, g => g);
        }

        public static InputLine Create(TextReader stream)
        {
            if (stream.Peek() == -1)
                return null;

            var line = new InputLine();
            for (var i = 0; i < 10; i++)
            {
                var word = stream.ReadWord();
                line._signals.Add(new SegmentSet(word));
            }

            if (stream.Peek() == -1)
                return null;

            line._signals = line._signals.OrderBy(x => x.Length).ToList();

            var x = stream.SkipUntil('|');
            for (var i = 0; i < 4; i++)
            {
                var word = stream.ReadWord();
                if (word == "")
                    return null;
                line._digits.Add(new SegmentSet(word));
            }

            x = stream.SkipUntil('\n');

            return line;
        }


        public List<char> SolveLine()
        {

            // first define distinct patterns (1,4,7,8)
            var uniqueDigits = LedDigit.DigitsByDistinct[1].ToList();

            foreach (var segmentSet in UndefinedSignals)
            {
                var aUnique = uniqueDigits.FirstOrDefault(x => x.Value.Length == segmentSet.Length);
                if (aUnique.Key > 0)
                {
                    Console.WriteLine("Found a " + aUnique.Key + " " + segmentSet);
                    _sol.Define(aUnique.Key, segmentSet);
                }
            }

            //Then define patterns that can be deduced by combining patterns
            foreach (var s1 in Signals)
                foreach (var s2 in Signals)
                    switch (s1.Length)
                    {
                        case 4 when s2.Length == 5 && AndXor(s1, s2) == (2, 5):
                            _sol.Define('2', s2);
                            Console.WriteLine("Found 2 by combination " + s2);
                            break;
                        case 6 when s2.Length == 2 && AndXor(s1, s2) == (5, 6):
                            _sol.Define('6', s1);
                            Console.WriteLine("Found 6 by combination " + s1);
                            break;
                        case 2 when s2.Length == 5 && (s2 & ~s1).Length == 3:
                            _sol.Define('3', s2);
                            Console.WriteLine("Found 3 by combination " + s2);
                            break;
                    }


            foreach (var signalSet in UndefinedSignals)
            {
                var canBe = new List<char>();
                if (UndefinedDigitsByLength.ContainsKey(signalSet.Length))
                {
                    var undefinedDigits = UndefinedDigitsByLength[signalSet.Length];
                    Console.WriteLine("UndefinedDigits:" + undefinedDigits.Select(x => x.digit).StringJoin());

                    foreach (var undefinedDigit in undefinedDigits)
                    {
                        var testRes = _sol.TestDigit(undefinedDigit.set, signalSet);
                        if (testRes) canBe.Add(undefinedDigit.digit);
                    }
                }
                else
                {
                    Console.WriteLine("Failed on no undefined digit has length " + signalSet.Length);
                }

                if (canBe.Count == 1)
                {
                    var foundChar = canBe.First();
                    Console.WriteLine("Found that " + signalSet + " is " + foundChar);
                    _sol.Define(foundChar, signalSet);
                }
                else
                {
                    Console.WriteLine("Found that " + signalSet + " can be any of " + canBe.StringJoin());
                }
            }

            if (_sol.Defined.Count == 10)
            {
                var wiresDecoded = Wires
                  .SelectMany(wire => _sol.Defined, (wire, def) => new { wire, def })
                  .Where(t => t.def.Key == t.wire)
                  .Select(t => t.def.Value)
                  .ToList();

                DisplayValue = int.Parse(wiresDecoded.StringJoin("") ?? throw new InvalidDataException("Decoded number is not defined"));

                return wiresDecoded;
            }

            return new List<char>();
        }

        public static (int, int) AndXor(SegmentSet set1, SegmentSet set2)
        {
            return ((set1 & ~set2).Length, (set1 ^ set2).Length);
        }
    }
}