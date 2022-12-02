using System.Collections.Generic;
using System.Linq;

namespace _4
{
    internal class Board
    {
        private int BoardSize { get; }
        private readonly SortedDictionary<int, (int r, int c)> _numbers = new SortedDictionary<int, (int r, int c)>();
        private readonly Dictionary<int, int> _rowsMark = new Dictionary<int, int>();
        private readonly Dictionary<int, int> _columnsMark = new Dictionary<int, int>();
        public int Index { get; set; }
        public bool HasWin { get; private set; }
        public Board(List<List<int>> rows)
        {
            HasWin = false;
            BoardSize = rows.Count;
            for (int r = 0; r < BoardSize; r++)
            {
                _rowsMark[r] = 0;
                _columnsMark[r] = 0;
                for (int c = 0; c < BoardSize; c++)
                {
                    var key = rows[r][c];
                    _numbers.Add(key, (r, c));
                }
            }
        }

        public void Mark(int number)
        {
            if (!_numbers.ContainsKey(number))
                return;

            var (r, c) = _numbers[number];
            _numbers.Remove(number);

            _rowsMark[r] += 1;
            _columnsMark[c] += 1;

            if (_rowsMark[r] >= BoardSize || _columnsMark[c] >= BoardSize)
                HasWin = true;

        }
        public int SumFree()
        {
            return _numbers.Keys.Sum();
        }
    }
}