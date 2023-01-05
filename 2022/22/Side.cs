
using System.ComponentModel;
using System.Diagnostics;
using System.Numerics;
using System.Transactions;
using common.SparseMatrix;
using static System.Net.Mime.MediaTypeNames;

namespace _22;

internal class Side
{
    private static int _sequence = 0;
    public int Id { get; private set; }
    public string Name { get; set; }
    public static MonkeyMap Map { get; set; }
    public SidePosition SidePosition { get; }
    public GlobalPosition StartSide { get; }
    public Dictionary<Direction, SideConnection> Connections { get; } = new();
    public override string ToString()
    {
        return $"{Name} {SidePosition} {StartSide}";
    }

    private readonly Dictionary<Direction, List<(Direction Next, SidePosition FirstOffset, SidePosition NextOffset, int Turns)>>
        _transformationTests = new()
        {
            {
                Direction.N, new()
                {
                    (Direction.E, SidePosition.N, SidePosition.E, +1),
                    (Direction.W, SidePosition.N, SidePosition.W, -1)
                }
            },
            {
                Direction.S, new()
                {
                    (Direction.E, SidePosition.S, SidePosition.E, +1),
                    (Direction.W, SidePosition.S, SidePosition.W, -1)
                }
            },
            {
                Direction.E, new()
                {
                    (Direction.S, SidePosition.E, SidePosition.S, +1),
                    (Direction.N, SidePosition.E, SidePosition.N, -1)
                }
            },
            {
                Direction.W, new()
                {
                    (Direction.S, SidePosition.W, SidePosition.S, -1),
                    (Direction.N, SidePosition.W, SidePosition.N, +1)
                }
            },
        };


    public Side(SidePosition sideSidePos, GlobalPosition sideStartSide)
    {
        SidePosition = sideSidePos;
        Id = ++_sequence;
        StartSide = sideStartSide;
        Name = "" + ((char)((int)'A' + Id - 1)).ToString();
    }



    public void FigureOutConnections(SidePosition at, Direction going)
    {
        SidePosition goingTo = at + going;
        if (Map.Sides.IsEmpty(goingTo))
        {
            var foundConnection = false;
            // there is no tile, figure out where to go
            var test = _transformationTests[going];
            foreach (var subtest in test)
            {
                SidePosition posFirstOffset = goingTo + subtest.Next;
                if (!Map.Sides.IsEmpty(posFirstOffset))  // found an occupied side
                {
                    var otherSide = Map.Sides.Value(posFirstOffset)!;
                    if (!this.Connections.ContainsKey(going))
                    {
                        Map.Log($"a Connect {Name} {going} to {otherSide.Name}  t {going.Turn(subtest.Turns)} ");
                        this.Connections[going] = new SideConnection(subtest.Turns, otherSide);
                        if (otherSide.Connections.ContainsKey(subtest.Next.Invert()))
                            throw new Exception("Non consistent data in side connections");
                        Map.Log($"  Connect {otherSide.Name} {subtest.Next.Invert()} to {Name}  t {going.Invert()} ");
                        otherSide.Connections[subtest.Next.Invert()] = new SideConnection(-subtest.Turns, this);
                    }
                    foundConnection = true;
                }
                else // no immediate fold
                {
                    SidePosition pos = goingTo + going; // 2 steps
                    if (Map.Sides.IsEmpty(pos))
                    {
                        pos += subtest.Next; // other direction once
                        if (Map.Sides.IsEmpty(pos))
                        {
                            pos += subtest.Next; // other direction twice
                            if (Map.Sides.IsEmpty(pos)) // found an occupied side
                            {
                                pos -= going; // 2 steps
                                if (!Map.Sides.IsEmpty(pos)) // found an occupied side
                                {
                                    var otherSide = Map.Sides.Value(pos)!;
                                    var turns = subtest.Turns * 2;
                                    if (!this.Connections.ContainsKey(going))
                                    {
                                        Map.Log($"b Connect {Name} {going} to {otherSide.Name}  t {going.Turn(turns)} ");
                                        this.Connections[going] = new SideConnection(turns, otherSide);
                                        if (otherSide.Connections.ContainsKey(going.Turn(-turns).Invert()))
                                            throw new Exception("Non consistent data in side connections");
                                        Map.Log($"   Connect {otherSide.Name} {going.Turn(-turns).Invert()} to {Name}  t {going.Turn(-turns)} ");
                                        otherSide.Connections[going.Turn(-turns).Invert()] = new SideConnection(-turns, this);
                                    }
                                    foundConnection = true;
                                }
                            }
                        }
                    }
                }
            } // end of subtests

            if (!foundConnection)
            {

            }
        }
    }

    public void MakeDirectConnections()
    {
        foreach (var direction in DirectionExtensions.Values())
        {
            var increment = SidePosition.Directions[(int)direction];
            var dir = increment.ToDirection()!.Value;
            if (!this.Connections.ContainsKey(dir))
            {
                var nextSidePos = SidePosition + increment;
                if (!Map.Sides.ContainsKey(nextSidePos))
                    continue;

                var inverseDirection = dir.Invert();
                var otherSide = Map.Sides[nextSidePos];
                if (otherSide == null)
                    throw new Exception("Null other side in MakeDirectConnections");
                Debug.WriteLine($"Direct connect {Name} {dir} to {otherSide.Name}");
                this.Connections[dir] = new SideConnection(0, otherSide);
                if (otherSide.Connections.ContainsKey(inverseDirection))
                    throw new Exception("Non consistent data in side connections");
                otherSide.Connections[inverseDirection] = new SideConnection(0, this);
            }
        }

        return;
    }

    public int CheckMissing()
    {
        var countConnected = 0;
        foreach (var direction in DirectionExtensions.Values())
        {
            var increment = GlobalPosition.Directions[(int)direction];
            var going = increment.ToDirection()!.Value;

            if (!this.Connections.ContainsKey(going))
            {
                Debug.WriteLine($"Side {Name} is missing {going}");

                /* here we might try go via 2 already connected sides on cube, ie:
                    go sideways via connection, turn 90, go via connection, turn 90, then connect wit accumulated number of rotations
                or figure out the pattern for that type of connections... 
                 */

                var result = SneakAroundCorner(going, -1);
                if (result == default)
                    result = SneakAroundCorner(going, 1);
                if (result != default)
                {
                    Debug.WriteLine($"Guessing for {result.Side.Name} going {result.Direction}");

                    var invertedResult = result.Direction.Invert();
                    var turns = invertedResult - going;
                    if (!this.Connections.ContainsKey(going))
                    {
                        Map.Log($"b Connect {Name} {going} to {result.Side.Name}  t {going.Turn(turns)} ");
                        this.Connections[going] = new SideConnection(turns, result.Side);
                        if (result.Side.Connections.ContainsKey(result.Direction))
                            throw new Exception("Non consistent data in side connections");
                        Map.Log($"   Connect {result.Side.Name} {result.Direction} to {Name}  t {going.Turn(-turns)} ");
                        result.Side.Connections[result.Direction] = new SideConnection(-turns, this);
                        countConnected++;
                    }

                }
                else
                {
                    Debug.WriteLine($"No Guess");
                }
            }
        }
        return countConnected;

        (Side Side, Direction Direction) SneakAroundCorner(Direction missingDirection, int steps)
        {

            var dir1 = missingDirection.Turn(steps);
            Debug.Write($"Looking for {Name} {dir1} ->");
            if (Connections.ContainsKey(dir1))
            {
                var conn1 = Connections[dir1];
                var nextdir1 = dir1.Turn(conn1.Turn);
                //  nextdir1 = nextdir1.Invert();
                Debug.Write($"{conn1.Side.Name} go:{dir1} enter:{nextdir1} ");
                nextdir1 = nextdir1.Turn(-steps);
                Debug.Write($" turn:{nextdir1}->");

                if (conn1.Side.Connections.ContainsKey(nextdir1))
                {
                    var conn2 = conn1.Side.Connections[nextdir1];
                    var nextdir2 = nextdir1.Turn(conn2.Turn);
                    Debug.Write($"{conn2.Side.Name} go:{nextdir1} enter:{nextdir2} ");
                    nextdir2 = nextdir2.Turn(-steps);
                    Debug.Write($" turn:{nextdir2}->");
                    if (!conn2.Side.Connections.ContainsKey(nextdir2))
                    {
                        Debug.WriteLine($"Found {conn2.Side.Name} has missing {nextdir2}");
                        return (Side: conn2.Side, Direction: nextdir2);
                    }
                }
                Debug.WriteLine($"Nope");
                return default;
            }
            Debug.WriteLine($"Nope");
            return default;
        }
    }

    public static T RotateAround<T>(T pos, (double X, double Y) center, int connectTurn,
        out (double, double) actual)
        where T : PositionBase, new()
    {
        var p = new T
        {
            X = pos.X,
            Y = pos.Y
        };
        var angleInRadians = -connectTurn * 90 * Math.PI / 180;
        double s = Math.Sin(angleInRadians);
        double c = Math.Cos(angleInRadians);
        // translate point back to origin:
        double pX = pos.X - center.X;
        double pY = pos.Y - center.Y;
        // rotate point
        double xNew = (pX * c - pY * s);
        double yNew = (pX * s + pY * c);
        xNew += center.X;
        yNew += center.Y;

        // translate point back:
        actual = (xNew, yNew);
        p.X = (long)Math.Round(xNew);
        p.Y = (long)Math.Round(yNew);
        return p;
    }


    public (LocalPosition Local, GlobalPosition Incr, SideConnection Connect)
        Translate(LocalPosition startSideLocalPosition, LocalPosition localIncrement)
    {
        var direction = localIncrement.ToDirection();
        if (direction == null)
            throw new Exception("Increment is not a clean direction");

        // get connection in requested direction
        SideConnection connect = this.Connections[direction.Value];
        var sideLocalPosition = startSideLocalPosition;
        double middleOfSide = (Map.SideWidth & 1) == 1
            ? (Map.SideWidth + 1) / 2d  // 0..4 == length 5, midpoint=3
            : (Map.SideWidth - 1) / 2d; // 0..49 == length 50, midpoint=24.5

        (double x, double y) midLocal = (middleOfSide, -middleOfSide);

        var lp2 = RotateAround(sideLocalPosition!, midLocal, connect.Turn, out (double, double) actual);
        // increments are -1..+1, ie midpoint = 0
        var newIncrement = RotateAround(localIncrement, (0, 0), connect.Turn, out actual);
        var sw = Map.SideWidth;
        // transpose the relevant coordinate, the rotation was done at the place of the starting side,
        // it should end up from the perspective of the ending side
        var lpOut = lp2 + newIncrement.ToDirection() switch
        {
            Direction.W => (sw, 0),
            Direction.E => (-sw, 0),
            Direction.N => (0, -sw),
            Direction.S => (0, sw),
            _ => throw new ArgumentOutOfRangeException()
        };
        Debug.WriteLine($"Translating {Name}{sideLocalPosition + localIncrement} going {direction} "
            + $"=> {connect.Side.Name}{lpOut + newIncrement}  going {newIncrement.ToDirection()}  (turn:{connect.Turn})  raw:({lp2})");
        return (Local: lpOut, Incr: new GlobalPosition(newIncrement), Connect: connect);
    }

    public static void ResetSeq()
    {
        _sequence = 0;
    }

    public void PrintSides()
    { 
        // diagnostic printout to show connected sides
        var N = Connections[Direction.N].Side.Name;
        var E = Connections[Direction.E].Side.Name;
        var S = Connections[Direction.S].Side.Name;
        var W = Connections[Direction.W].Side.Name;

        Debug.WriteLine($"---");
        Debug.WriteLine($" {N} ");
        Debug.WriteLine($"{W}{Name}{E} ");
        Debug.WriteLine($" {S} ");
    }
}