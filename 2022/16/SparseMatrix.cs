using common;
namespace common.SparseMatrix;
public class SparseMatrix<TValue>
{
    private readonly Dictionary<(long x, long y), Cell> _list = new();
    public Cell CellAt(long x, long y)
    {
        if (_list.ContainsKey((x, y)))
            return _list[(x, y)];
        return new Cell(x, y, this);
    }
    public TValue? Value(long x, long y)
    {
        return CellAt(x, y).Value;
    }
    public TValue? Value(long x, long y, TValue newValue)
    {
        return CellAt(x, y).Value = newValue;
    }

    internal void Drop(Cell cell)
    {
        _list.Remove(cell.Coordinate);
    }

    public void Add(Cell cell)
    {
        _list[cell.Coordinate] = cell;
    }


    public (long minX, long minY, long maxX, long maxY)? UsedRange()
    {
        if (_list.Count == 0) return null;
        (long x, long y) first = _list.Keys.First();
        var minX = first.x;
        var maxX = first.x;
        var minY = first.y;
        var maxY = first.y;

        _list.Keys.ForEach(
            (coord, i) =>
            {
                var (x, y) = coord;
                minX = Math.Min(minX, (long)x);
                maxX = Math.Max(maxX, (long)x);
                minY = Math.Min(minY, (long)y);
                maxY = Math.Max(maxY, (long)y);
            });
        return (minX, minY, maxX, maxY);
    }

    public bool IsEmpty(long x, long y)
    {
        return !_list.ContainsKey((y, x));
    }

    public class Cell
    {

        private readonly SparseMatrix<TValue?> _parent;
        private TValue? _value = default;

        public Cell(long x, long y, SparseMatrix<TValue> sparseMatrix)
        {
            Coordinate = (x, y);
            Value = default(TValue)!;
            _parent = sparseMatrix!;
        }

        public (long x, long y) Coordinate { get; set; }
        internal TValue? Value
        {
            get => _value;
            set
            {
                if (!Equals(_value, default(TValue)) && Equals(value, default(TValue)))
                    _parent.Drop(this!);

                if (Equals(_value, default(TValue)) && !Equals(value, default(TValue)))
                    _parent.Add(this!);

                _value = value;
            }
        }
    }
}