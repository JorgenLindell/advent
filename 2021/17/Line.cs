namespace _16;

public struct Line
{
    public Point Start;
    public Point End;

    public Line(Point p1, Point p2)
    {
        Start = p1;
        End = p2;
    }

    public Line(int x1, int y1, int x2, int y2)
    {
        Start = new Point(x1, y1);
        End = new Point(x2, y2);
    }

    public bool Intersect(Line b)
    {
        var a = this;
        if (a.Start.x == a.End.x) return !(b.Start.x == b.End.x && a.Start.x != b.Start.x);
        if (b.Start.x == b.End.x) return true;

        // Both lines are not parallel to the y-axis
        var m1 = (a.Start.y - a.End.y) / (a.Start.x - a.End.x);
        var m2 = (b.Start.y - b.End.y) / (b.Start.x - b.End.x);
        return m1 != m2;
    }
}