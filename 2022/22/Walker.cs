using _22;
using common.SparseMatrix;

internal class Walker
{
    private readonly MonkeyMap _map;
    public List<(GlobalPosition,Direction)> Track = new();
    public Walker(GlobalPosition startPosition, Direction direction, MonkeyMap map)
    {
        _map = map;
        Pos = startPosition;
        Direction = direction;
    }

    public GlobalPosition Pos { get; set; }
    public Direction Direction { get; private set; }

    public void Execute(Instruction instr)
    {
        var increment = GlobalPosition.Directions[(int)Direction];
        for (int i = 0; i < instr.Steps; i++)
        {
            var next = Pos + increment;
            var isEmpty = _map.IsEmpty(next);
            if (!isEmpty)
            {
                var prevIncrement = increment;
                increment = prevIncrement;
                var tile = _map.Value(next);
                if (tile!.Typ == Tile.Types.Edge)
                    tile = tile.GetRealTile(Pos,ref increment);
                if (tile.Typ == Tile.Types.None)
                {
                    Direction = increment.ToDirection()!.Value;
                    Pos = tile.Pos;
                    Track.Add((Pos,Direction));
                }
                else
                {
                    //in case blocked after edge
                    break;
                }
            }
            if (isEmpty)
            {
                // now we are outside map...
                throw new Exception("Crazy");
            }
        }

        int intOffset = (int)Direction;
        intOffset += ((int)instr.Turn);
        intOffset += Position.Directions.Length * 10;
        intOffset %= Position.Directions.Length;
        Direction = (Direction)(intOffset);
    }

}