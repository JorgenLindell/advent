using _22;
using common.SparseMatrix;

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

    public string Symbol => Typ == EEdge ? "e" : Typ == EWall ? "#" : Typ == EFree ? "." : "?";
    public (Side? Side, GlobalPosition SideStart, SidePosition SidePosition, LocalPosition LocalPosition, long SideId) PosInSide => Map!.GlobalToSide(Pos);
    public Side? Side => Map!.GlobalToSide(Pos).Side;


    public Tile GetRealTile(GlobalPosition comingFrom, ref GlobalPosition increment)
    {
        if (Typ != EEdge)
        {
            return this;
        }
        if (MonkeyMap.UseCubeCoordinates)
        {
            var direction = increment.ToDirection();
            if (direction is null)
                throw new Exception("Cant translate increment to direction");

            //comingFrom is already incremented, but we need the old pos
            var startSide = Map.GlobalToSide(comingFrom);
            var edgeSide = startSide.Side!.Connections[direction.Value].Side;
            if (edgeSide.Id == startSide.SideId)
                throw new Exception("An edge cant go to same side");
            var localIncrement = new LocalPosition(increment);
            var translation = startSide.Side!.Translate(startSide.LocalPosition, localIncrement);
      
            var newGlobal = Map.SideToGlobal(translation.Connect.Side, translation.Local);
            increment = translation.Incr;
            var globalPosition = newGlobal+increment;
            return Map.Value(globalPosition)!;
        }

        if (Map!.EdgeConnections.ContainsKey(Pos))
        {
            var conn = Map.EdgeConnections[Pos];
            var target = increment.Y != 0 ? conn.Vertical : conn.Horizontal;

            var result = target + increment;
            return Map.Value(result)!;
        }
        throw new NotImplementedException("Crazy no edgeconnection for edge");
    }
}