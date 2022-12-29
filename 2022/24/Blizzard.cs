internal class Blizzard
{
    private static int _counter;

    public Blizzard(int x, int y, Offset direction, char c)
    {
        Direction = direction;
        Step = Position.Offsets[(int)direction];

        StartPos = new Position(x, y);
        Pos = new Position(x, y);
        Symbol = c;
        Name = "Blz_" + _counter++;
    }

    public static Matrix? Matrix { get; set; }
    public Offset Direction { get; }
    public Position Pos { get; set; }
    public Position StartPos { get; set; }
    public char Symbol { get; set; }
    public string Name { get; }
    public Position<Position> Step { get; set; }

    public Position PositionAtTime(int time)
    {
        var positionAtTime = new Position
        {
            X = (StartPos.X + Step.X * time) % Matrix!.Size.X,
            Y = (StartPos.Y + Step.Y * time) % Matrix!.Size.Y
        };
        if (positionAtTime.X < 0) positionAtTime.X += Matrix!.Size.X;
        if (positionAtTime.Y < 0) positionAtTime.Y += Matrix!.Size.Y;
        return positionAtTime;
    }

    public Position PositionAtStart(int time, Position pos)
    {
        var positionAtTime = new Position
        {
            X = (pos.X - Step.X * time) % Matrix.Size.X,
            Y = (pos.Y - Step.Y * time) % Matrix.Size.Y
        };

        if (positionAtTime.X < 0) positionAtTime.X += Matrix.Size.X;
        if (positionAtTime.Y < 0) positionAtTime.Y += Matrix.Size.Y;
        return positionAtTime;
    }

    public override string ToString()
    {
        return $"{Name} {Pos} {Direction}";
    }
}