using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using System.Windows.Markup;
using static System.Net.Mime.MediaTypeNames;

namespace common;
public abstract class GridBase<T>
{
    protected T OutsideMarker;

    public GridBase(VectorRc size, T outsideMarker)
    {
        OutsideMarker = outsideMarker;
    }

    protected GridBase()
    {
    }

    public ICollection<T[]> Data { get; init; }
    public int Height { get => Data.Count; }
    public int Width { get; init; }

    public T GetVirtual(VectorRc next) => GetVirtual(next.Row, next.Col);

    public T GetVirtual(int row, int col)
    {
        var r = row < 0 ? Height + row % Height : row % Height;
        var c = col < 0 ? Width + col % Width : col % Width;
        return Get(r, c);
    }

    public T Get(int row, int col)
    {
        if (row < 0 || row >= Data.Count || col < 0 || col >= Data.ElementAt(row).Length)
        {
            return OutsideMarker;
        }
        return Data.ElementAt(row)[col];
    }
    public T Get(VectorRc coord)
    {
        return Get(coord.Row, coord.Col);
    }

    public T this[int row, int col]
    {
        get => Get(row, col);
        set
        {
            if (row < 0 || row >= Data.Count || col < 0 || col >= Data.ElementAt(row).Count())
                return;
            Data.ElementAt(row)[col] = value;
        }
    }

    public IEnumerable<T> this[int row]
    {
        get => Data.ElementAt(row);
        set
        {
            var values = value.ToArray();
            for (int i = 0; i < values.Length; i++)
            {
                Data.ElementAt(row)[i] = values[i];
            }
        }
    }

    public T this[VectorRc current]
    {
        get => this[current.Row, current.Col];
        set => this[current.Row, current.Col] = value;
    }
    public IEnumerable<(T Value, VectorRc Pos)> Cells
    {
        get
        {
            for (int r = 0; r < Data.Count; r++)
            {
                for (int c = 0; c < Data.ElementAt(r).Length; c++)
                {
                    yield return (Data.ElementAt(r)[c], new(r, c));
                }
            }
        }
    }
}

public class Grid : GridBase<char>
{

    public Grid(string input, char outsideChar)
    {
        var data = input.ReplaceLineEndings("\n").Split('\n', StringSplitOptions.RemoveEmptyEntries).ToImmutableArray();
        Data = data.Select(s => s.ToCharArray()).ToArray();
        this.OutsideMarker = outsideChar;
        Width = data.Max(row => row.Length);
    }

}


public class Grid<T> : GridBase<T>
{

    public Grid(VectorRc size, T outsideMarker) : base(size, outsideMarker)
    {
    }

}