using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using common;


//https://adventofcode.com/2022/day/14
internal class Program
{
    private static string _testData =
        @"498,4 -> 498,6 -> 496,6
503,4 -> 502,4 -> 502,9 -> 494,9"
            .Replace("\r\n", "\n");

    private static void Main(string[] args)
    {
        FirstPart(GetDataStream());
        SecondPart(GetDataStream());
    }

    private static TextReader GetDataStream()
    {
        var stream = StreamUtils.GetInputStream("input.txt");
        //var stream = StreamUtils.GetInputStream(testData: _testData);
        return stream;
    }

    private static void SecondPart(TextReader stream)
    {
        var matrix = LoadMatrix(stream);

        var maxrow = matrix.MaxRow();
        Draw(matrix, (-500, maxrow + 2), (1500, maxrow + 2), Cell.CellType.Wall);
        maxrow = matrix.MaxRow();
        var numberOfSand = 0;
        while (true)
        {
            var dropPoint = (r: 0, c: 500);
            var stopped = DropSand1(matrix, dropPoint, maxrow);
            if (stopped.r > maxrow) break;
            numberOfSand++;
            if (stopped.r ==0) break;
        }
        Debug.WriteLine("Sand=" + numberOfSand);

    }


    private static void FirstPart(TextReader stream)
    {
        var matrix = LoadMatrix(stream);

        var maxrow = matrix.MaxRow();
        var numberOfSand = 0;
        while (true)
        {
            var dropPoint = (r: 0, c: 500);
            var stopped = DropSand1(matrix, dropPoint, maxrow);
            if (stopped.r > maxrow) break;
            numberOfSand++;
        }
        Debug.WriteLine("Sand=" + numberOfSand);
    }

    private static SparseMatrix LoadMatrix(TextReader stream)
    {
        var matrix = new SparseMatrix();
        while (stream.ReadLine() is { } inpLine)
        {
            var points = inpLine.Split("->".ToCharArray(),
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var coord = points[0].Split(',');
            var start = (c: coord[0].ToInt()!.Value, r: coord[1].ToInt()!.Value);
            for (int i = 1; i < points.Length; i++)
            {
                coord = points[i].Split(',');
                var end = (c: coord[0].ToInt()!.Value, r: coord[1].ToInt()!.Value);
                Draw(matrix, start, end, Cell.CellType.Wall);
                start = end;
            }
        }

        return matrix;
    }

    private static (int c, int r) DropSand1(SparseMatrix matrix, (int r, int c) dropPoint, int maxrow)
    {
        var r = dropPoint.r;
        var c = dropPoint.c;
        while (r <= maxrow)
        {
            if (matrix.IsEmpty(r + 1, c))
            {
                r++;
            }
            else if (matrix.IsEmpty(r + 1, c - 1))
            {
                r++;
                c--;
            }
            else if (matrix.IsEmpty(r + 1, c + 1))
            {
                r++;
                c++;
            }
            else
            {
                matrix.Cell(r, c).Value = Cell.CellType.Sand;
                return (c: c, r: r);
            }
        }
        return (c: c, r: r);
    }

    private static void Draw(
        SparseMatrix matrix,
        (int c, int r) start,
        (int c, int r) end,
        Cell.CellType content)
    {
        if (start.r == end.r)
        {
            int i = Math.Min(start.c, end.c);
            int lim = Math.Max(start.c, end.c);
            for (; i <= lim; ++i)
                matrix.Cell(start.r, i).Value = content;
        }
        else if (start.c == end.c)
        {
            int i = Math.Min(start.r, end.r);
            int lim = Math.Max(start.r, end.r);
            for (; i <= lim; ++i)
                matrix.Cell(i, start.c).Value = content;
        }
        else
        {
            throw new InvalidDataException("Diagonal line specified");
        }

    }
}

public class SparseMatrix
{
    private Dictionary<(int r, int c), Cell> _list = new();
    public Cell Cell(int r, int c)
    {
        if (_list.ContainsKey((r, c)))
            return _list[(r, c)];
        return new Cell(r, c, this);
    }

    public void Drop(Cell cell)
    {
        _list.Remove(cell.Coordinate);
    }

    public void Add(Cell cell)
    {
        _list[cell.Coordinate] = cell;
    }


    public int MaxRow()
    {
        return _list.Keys.Max(x => x.r);
    }

    public bool IsEmpty(int r, int c)
    {
        return !_list.ContainsKey((r, c));
    }
}

public class Cell
{
    public enum CellType { Empty, Wall, Sand }

    private readonly SparseMatrix _parent;
    private CellType _value = CellType.Empty;

    public Cell(int r, int c, SparseMatrix sparseMatrix)
    {
        Coordinate = (r, c);
        Value = CellType.Empty;
        _parent = sparseMatrix;
    }

    public (int r, int c) Coordinate { get; set; }

    public CellType Value
    {
        get => _value;
        set
        {
            if (_value != CellType.Empty && value == CellType.Empty)
                _parent.Drop(this);

            if (_value == CellType.Empty && value != CellType.Empty)
            {
                _parent.Add(this);

            }
            _value = value;
        }
    }
}