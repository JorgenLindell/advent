internal class Tile
{

    public static MonkeyMap? Map { get; set; }
    public GlobalPosition Pos { get; private set; }
    public Types Typ { get; }

    public Tile(Types typ, GlobalPosition pos, GlobalPosition endPos)
    {
        Pos = pos;
        Typ = typ;

        Map.AddEdge(pos, endPos);
        Map.AddEdge(endPos, pos);
    }
    public Tile(Types typ, GlobalPosition pos)
    {
        Pos = pos;
        Typ = typ;
    }

    public enum Types
    {
        None,
        Edge,
        Wall
    }
    public static Types EEdge => Types.Edge;
    public static Types EWall => Types.Wall;
    public static Types EFree => Types.None;

    public string Symbol => Typ == EEdge ? "E" : Typ == EWall ? "#" : Typ == EFree ? "." : "?";
    public (GlobalPosition StartSide, LocalPosition Pos) PosInSide => Map.PosInSide(Pos);
    public (string Name, SidePosition SidePos, GlobalPosition StartSide, LocalPosition Pos) Side => Map.Side(Pos);


    public Tile GetRealTile(GlobalPosition increment)
    {
        if (Typ != EEdge)
        {
            return this;
        }

        if (Map.EdgeConnections.ContainsKey(Pos))
        {
            var conn = Map.EdgeConnections[Pos];
            var position = (GlobalPosition) increment;
            var target = position.Y != 0 ? conn.Vertical : conn.Horizontal;

            var result = target + position;
            return Map.Value(result)!;
        }
        throw new NotImplementedException("Crazy no edgeconnection for edge");
    }
}