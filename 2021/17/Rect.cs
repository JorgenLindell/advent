namespace _16;

public struct Rect
{
    public int x1;
    public int x2;
    public int y1;
    public int y2;

    public Rect(int x1, int x2, int y1, int y2)
    {
        this.x1 = Math.Min(x1, x2);
        this.x2 = Math.Max(x1, x2);
        this.y1 = Math.Max(y1, y2);
        this.y2 = Math.Min(y1, y2);
    }

    public int Height => y1 - y2;
    public int Width => x2 - x1;


    public int Top => y1;
    public int Bottom => y2;
    public int Left => x1;
    public int Right => x2;
    public bool InsideX((int x, int y) p) => p.x >= Left && p.x <= Right;
    public bool InsideX(Point p) => p.x >= Left && p.x <= Right;
    public bool InsideY((int x, int y) p) => p.y <= Top && p.y >= Bottom;
    public bool InsideY(Point p) => p.y <= Top && p.y >= Bottom;
    public bool Inside(Point point) => InsideX(point) && InsideY(point);

    public bool IntersectsWithLine(Line line)
    {
        var topLine = new Line(Left, Top, Right, Top);
        if (topLine.Intersect(line)) return true;
        var leftLine = new Line(Left, Top, Left, Bottom);
        if (leftLine.Intersect(line)) return true;
        var rightLine = new Line(Right, Top, Right, Bottom);
        if (rightLine.Intersect(line)) return true;
        var bottomLine = new Line(Left, Bottom, Right, Bottom);
        if (bottomLine.Intersect(line)) return true;
        return false;
    }

    public void ForEachPoint(Action<(int x, int y)> action)
    {
        for (int i = x1; i < x2 + 1; i++)
        {
            for (int j = y2; j < y1; j++)
            {
                action((x: i, y: j));
            }
        }
    }
}