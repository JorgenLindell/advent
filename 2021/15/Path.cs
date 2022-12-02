using common;

namespace _15;
public interface ICell
{
    (int r, int c) Pos { get; set; }
}

public class Path<T>
where T:ICell
{
    private readonly Func<T, double> _resultFunc;
    public double Result { get; internal set; }
    public Stack<ICell> Cells { get; } = new Stack<ICell>();
    public HashSet<int> VisitedCells { get; } = new HashSet<int>();
    public bool Visited(T x) => VisitedCells.Contains(AsInt(x.Pos));


    private int AsInt((int r, int c) xPos)
    {
        return (xPos.r << 8) | xPos.c;
    }

    public Path(Func<T, double> resultFunc)
    {
        _resultFunc = resultFunc;
    }

    public Path(Path<T> other)
    {
        _resultFunc = other._resultFunc;
        Result = other.Result;
        Cells = other.Cells.CloneStack();
        VisitedCells = other.VisitedCells.ToHashSet();
    }
    public Path(IEnumerable<ICell> otherReversed, double result)
    {
        Cells = new Stack<ICell>();
        foreach (var cell in otherReversed)
        {
            Cells.Push(cell);
        }
        Result = result;
    }

    public void Push(ICell cell, double result)
    {
        Cells.Push(cell);
        VisitedCells.Add(AsInt(cell.Pos));
        Result += result;
    }
    public void Push(ICell cell)
    {
        Cells.Push(cell);
        VisitedCells.Add(AsInt(cell.Pos));
        Result += _resultFunc((T)cell);
    }
    public void Pop()
    {
        T cell = (T)Cells.Pop();
        VisitedCells.Remove(AsInt(cell.Pos));
        Result -= _resultFunc(cell);
    }

    public double CalcResult(ICell cell)
    {
        return _resultFunc((T)cell);
    }

    public override string ToString()
    {
        var cells = "";
        foreach (var cell in Cells.Reverse())
        {
            cells += $" ({cell.Pos.r},{cell.Pos.c}),";
        }
        return $" { Result:##0.0}:{cells} ";
    }

    public void AdjustResult()
    {
        Result -= _resultFunc((T)Cells.ToArray().Last());
    }
}



