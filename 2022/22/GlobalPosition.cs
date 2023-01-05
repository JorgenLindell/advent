using common.SparseMatrix;

namespace _22
{
    // The MonkeyMap handles 3 different coordinate types
    // Global for the cells of the map
    // Side for the sides of the cube
    // Local for cells within a side


    public class LocalPosition : Position<LocalPosition>
    {
        public LocalPosition(long x, long y) : base(x, y)
        {
        }

        public LocalPosition(PositionBase pos) : base(pos)
        {
        }
        public LocalPosition((long x, long y) pos) : base(pos.x, pos.y)
        {
        }

        public LocalPosition()
        {
        }

    }

    public class GlobalPosition : Position<GlobalPosition>
    {

        public GlobalPosition(long x, long y) : base(x, y)
        {
        }

        public GlobalPosition()
        {
        }
        public GlobalPosition(PositionBase pos) : base(pos)
        {
        }


    }
    public class SidePosition : Position<SidePosition>
    {
        public SidePosition(long x, long y) : base(x, y)
        {
        }
        public SidePosition(Position<SidePosition> pos) : this(pos.X, pos.Y)
        {
        }

        public SidePosition()
        {
        }

        public static implicit operator SidePosition((long x, long y) input)
        {
            return new SidePosition(input.x, input.y);
        }
    }
}
