using common;
using System.Collections.Generic;
using System.Linq;

namespace _9
{
    public class Basin
    {
        private (int r, int c) _lowPoint;
        private readonly Matris<byte> _matrix;
        private readonly Dictionary<(int r, int c), List<(int r, int c)>> _edgePoints;
        private readonly HashSet<(int r, int c)> _insidePoints;

        public Basin((int r, int c) lowPoint, Matris<byte> matrix)
        {
            _lowPoint = lowPoint;
            _matrix = matrix;
            _edgePoints = new Dictionary<(int r, int c), List<(int r, int c)>>();
            _insidePoints = new HashSet<(int r, int c)>();
            _edgePoints.Add(lowPoint, GetAdjecent(lowPoint).ToList());
            Traverse();
        }

        public int Size => _insidePoints.Count;

        private void Traverse()
        {
            var added = true;
            while (added)
            {
                added = false;
                foreach (var p in _edgePoints.ToList())
                {
                    var pp = p.Key;
                    foreach (var next in p.Value)
                    {
                        if (!_insidePoints.Contains(next) && !_edgePoints.ContainsKey(next))
                        {

                            if (_matrix.Value(next) < 9)
                            {
                                _edgePoints.Add(next, GetAdjecent(next).Where(x => !pp.Equals(x) || _matrix.Value(x) > 8).ToList());
                                added = true;
                            }
                        }
                    }
                    _insidePoints.Add(pp);
                    _edgePoints.Remove(pp);
                }
            }

        }

        IEnumerable<(int r, int c)> GetAdjecent((int r, int c) c)
        {
            yield return c.Up();
            yield return c.Down();
            yield return c.Left();
            yield return c.Right();
        }
    }
}