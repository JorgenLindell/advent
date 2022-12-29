using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace common.SparseMatrix;

public class SparseMatrix<TKey, TValue> : SparseMatrix<TKey, TValue, TKey>
    where TKey : notnull
{
}
public class SparseMatrix<TKey, TValue,TKeyBase> : IEnumerable<(TKey, TValue)>
    where TKey : TKeyBase
    where TKeyBase : notnull
{

    private readonly Dictionary<TKeyBase, Cell> _list = new();
    public IEnumerable<TValue> Values => _list.Values.Select(c => c.Value).Where(x => x != null).Cast<TValue>();
    public IEnumerable<TKey> Keys => _list.Keys.Cast<TKey>();

    public Cell CellAt(TKey key)
    {
        if (_list.ContainsKey(key))
            return _list[key];
        return new Cell(key, this);
    }
    public TValue? Value(TKey key)
    {
        return CellAt(key).Value;
    }
    public virtual TValue? Value(TKey key, TValue newValue)
    {
        return CellAt(key).Value = newValue;
    }

    internal void Drop(Cell cell)
    {
        _list.Remove(cell.Coordinate);
    }

    public void Add(Cell cell)
    {
        _list[cell.Coordinate] = cell;
    }

    public bool IsEmpty(TKeyBase key)
    {
        return !_list.ContainsKey(key);
    }

    public (TKey Min, TKey Max) MinMax(Func<TKeyBase,(TKey Min, TKey Max), (TKey Min, TKey Max)> func)
    {
        var first =(TKey) _list.Keys.First();
        (TKey Min, TKey Max) acc= (first,first);
        _list.Keys.ForEach((x,i)=>acc = func(x, acc));
        return acc;
    }
    public class Cell
    {

        private readonly SparseMatrix<TKey,TValue, TKeyBase> _parent;
        private TValue? _value = default;

        public Cell(TKeyBase key, SparseMatrix<TKey, TValue, TKeyBase> sparseMatrix)
        {
            Coordinate = key;
            Value = default(TValue)!;
            _parent = sparseMatrix!;
        }

        public TKeyBase Coordinate { get; set; }
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

    public IEnumerator<(TKey, TValue)> GetEnumerator()
    {
        return _list.Select(x => ((TKey)x.Key, x.Value.Value!)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}