using common;

namespace _22;

public class House2
{
    public static readonly Cell RoomAU;
    public static readonly Cell RoomA2;
    public static readonly Cell RoomA3;
    public static readonly Cell RoomAL;
    public static readonly Cell RoomBU;
    public static readonly Cell RoomB2;
    public static readonly Cell RoomB3;
    public static readonly Cell RoomBL;
    public static readonly Cell RoomCU;
    public static readonly Cell RoomC2;
    public static readonly Cell RoomC3;
    public static readonly Cell RoomCL;
    public static readonly Cell RoomDU;
    public static readonly Cell RoomD2;
    public static readonly Cell RoomD3;
    public static readonly Cell RoomDL;
    public static readonly Cell[] Hall = new Cell[11];
    private static List<Player> Players;
    public static readonly Dictionary<string, (Cell[] home, int cost)> Types;
    private string boardTemplate = @"
#############
#01234567890#
  #A#B#C#D#
  #A#B#C#D#
  #A#B#C#D#
  #A#B#C#D#
  #########
";

    static House2()
    {
        RoomAU = new Cell("A", "AU");
        RoomA2 = new Cell("A", "A2");
        RoomA3 = new Cell("A", "A3");
        RoomAL = new Cell("A", "AL");
        RoomBU = new Cell("B", "BU");
        RoomB2 = new Cell("B", "B2");
        RoomB3 = new Cell("B", "B3");
        RoomBL = new Cell("B", "BL");
        RoomCU = new Cell("C", "CU");
        RoomC2 = new Cell("C", "C2");
        RoomC3 = new Cell("C", "C3");
        RoomCL = new Cell("C", "CL");
        RoomDU = new Cell("D", "DU");
        RoomD2 = new Cell("D", "D2");
        RoomD3 = new Cell("D", "D3");
        RoomDL = new Cell("D", "DL");
        for (var i = 0; i < 11; i++)
        {
            Hall[i] = new Cell("H", "H" + i);
            if (i > 0) Connect(Hall[i], Hall[i - 1]);
        }

        Connect(RoomAL, RoomA3); Connect(RoomA3, RoomA2); Connect(RoomA2, RoomAU); Connect(RoomAU, Hall[2]);
        Connect(RoomBL, RoomB3); Connect(RoomB3, RoomB2); Connect(RoomB2, RoomBU); Connect(RoomBU, Hall[4]);
        Connect(RoomCL, RoomC3); Connect(RoomC3, RoomC2); Connect(RoomC2, RoomCU); Connect(RoomCU, Hall[6]);
        Connect(RoomDL, RoomD3); Connect(RoomD3, RoomD2); Connect(RoomD2, RoomDU); Connect(RoomDU, Hall[8]);

        Hall[2].NoStop = true;
        Hall[4].NoStop = true;
        Hall[6].NoStop = true;
        Hall[8].NoStop = true;

        Types = new()
        {
            ["A"] = (new[] { RoomAL, RoomA3, RoomA2, RoomAU, }, 1),
            ["B"] = (new[] { RoomBL, RoomB3, RoomB2, RoomBU, }, 10),
            ["C"] = (new[] { RoomCL, RoomC3, RoomC2, RoomCU, }, 100),
            ["D"] = (new[] { RoomDL, RoomD3, RoomD2, RoomDU, }, 1000)
        };
    }

    public House2(
        int room1U, int room12, int room13, int room1L,
        int room2U, int room22, int room23, int room2L,
        int room3U, int room32, int room33, int room3L,
        int room4U, int room42, int room43, int room4L)
    {
        RoomAU.Occ = "" + (char)room1U;
        RoomA2.Occ = "" + (char)room12;
        RoomA3.Occ = "" + (char)room13;
        RoomAL.Occ = "" + (char)room1L;
        RoomBU.Occ = "" + (char)room2U;
        RoomB2.Occ = "" + (char)room22;
        RoomB3.Occ = "" + (char)room23;
        RoomBL.Occ = "" + (char)room2L;
        RoomCU.Occ = "" + (char)room3U;
        RoomC2.Occ = "" + (char)room32;
        RoomC3.Occ = "" + (char)room33;
        RoomCL.Occ = "" + (char)room3L;
        RoomDU.Occ = "" + (char)room4U;
        RoomD2.Occ = "" + (char)room42;
        RoomD3.Occ = "" + (char)room43;
        RoomDL.Occ = "" + (char)room4L;

        Players = new List<Player>
        {
            new(RoomA2),
            new(RoomA3),
            new(RoomAL),
            new(RoomAU),
            new(RoomB2),
            new(RoomB3),
            new(RoomBL),
            new(RoomBU),
            new(RoomC2),
            new(RoomC3),
            new(RoomCL),
            new(RoomCU),
            new(RoomD2),
            new(RoomD3),
            new(RoomDL),
            new(RoomDU),
        };
        Players.OrderByDescending(x => x.Id.Substring(0, 1)).ToList();
        House2.Instance = this;
    }

    public static House2 Instance { get; set; }

    public static void Connect(Cell c1, Cell c2)
    {
        c1.Exits.Add(c2);
        c2.Exits.Add(c1);
    }

    public override string ToString()
    {
        var halls = Hall.Select(h => h.Occ != "" ? h.Occ : ".").StringJoin("");
        return $" \n{halls}\n" +
               $" |{RoomAU.Occ}|{RoomBU.Occ}|{RoomCU.Occ}|{RoomDU.Occ}|\n" +
               $" |{RoomA2.Occ}|{RoomB2.Occ}|{RoomC2.Occ}|{RoomD2.Occ}|\n" +
               $" |{RoomA3.Occ}|{RoomB3.Occ}|{RoomC3.Occ}|{RoomD3.Occ}|\n" +
               $" |{RoomAL.Occ}|{RoomBL.Occ}|{RoomCL.Occ}|{RoomDL.Occ}|\n";
    }


    public static House2 LoadStream(TextReader stream)
    {
        stream.SkipUntil('#');
        var skip = stream.ReadLine();
        var corridor = stream.ReadLine();
        skip = stream.SkipOver("###");
        var room1U = stream.Read(); stream.Read();
        var room2U = stream.Read(); stream.Read();
        var room3U = stream.Read(); stream.Read();
        var room4U = stream.Read(); stream.ReadLine();
        skip = stream.SkipOver("#");
        var room12 = stream.Read(); stream.Read();
        var room22 = stream.Read(); stream.Read();
        var room32 = stream.Read(); stream.Read();
        var room42 = stream.Read(); stream.ReadLine();
        skip = stream.SkipOver("#");
        var room13 = stream.Read(); stream.Read();
        var room23 = stream.Read(); stream.Read();
        var room33 = stream.Read(); stream.Read();
        var room43 = stream.Read(); stream.ReadLine();
        skip = stream.SkipOver("#");
        var room1L = stream.Read(); stream.Read();
        var room2L = stream.Read(); stream.Read();
        var room3L = stream.Read(); stream.Read();
        var room4L = stream.Read(); stream.ReadLine();

        return new House2(
            room1U, room12, room13, room1L,
            room2U, room22, room23, room2L,
            room3U, room32, room33, room3L,
            room4U, room42, room43, room4L);
    }

    private static Dictionary<(Cell, Cell), List<Cell>> GenerateWays()
    {
        IEnumerable<int> Iter(int i1, int i2)
        {
            if (i1 > i2)
            {
                for (int i = i1 - 1; i >= i2; i--)
                {
                    yield return i;
                }
            }
            else
            {
                for (int i = i1+1; i < i2 + 1; i++)
                {
                    yield return i;
                }
            }
        }
        var hallIndex = Hall.Select((x, i) =>
            new { x, i }).ToDictionary(x => x.x, x => x.i);

        var d = new Dictionary<(Cell, Cell), List<Cell>>();
        for (int i = 0; i < Hall.Length; i++)
        {
            var startCell = Hall[i];
            foreach (var type in Types)
            {
                var hallCell = type.Value.home[3].Exits[1];
                if (hallCell.Typ != "H" || !type.Value.home[3].Name.EndsWith("U"))
                {

                }
                var i2 = hallIndex[hallCell];
                var indexes = Iter(i, i2).ToList();

                var targetList = d[(startCell, type.Value.home[3])] = new List<Cell>();
                indexes.ForEach(x => targetList.Add(Hall[x]));
                targetList.Add(type.Value.home[3]);

                targetList = d[(startCell, type.Value.home[2])] = new List<Cell>();
                indexes.ForEach(x => targetList.Add(Hall[x]));
                targetList.Add(type.Value.home[3]);
                targetList.Add(type.Value.home[2]);

                targetList = d[(startCell, type.Value.home[1])] = new List<Cell>();
                indexes.ForEach(x => targetList.Add(Hall[x]));
                targetList.Add(type.Value.home[3]);
                targetList.Add(type.Value.home[2]);
                targetList.Add(type.Value.home[1]);

                targetList = d[(startCell, type.Value.home[0])] = new List<Cell>();
                indexes.ForEach(x => targetList.Add(Hall[x]));
                targetList.Add(type.Value.home[3]);
                targetList.Add(type.Value.home[2]);
                targetList.Add(type.Value.home[1]);
                targetList.Add(type.Value.home[0]);
            }
        }

        return d;
    }
    public void StartPlay()
    {
        Console.WriteLine("");
        Console.WriteLine(this.ToString());
        Console.WriteLine("");
        ListAllCells();
        Console.WriteLine("");
        //Get all players possible moves
        //play one, then recurse until locked or game solved, save solutions
        Stack<(Player, Cell, int stepCost)> stack = new();
        List<(int cost, List<(Player, Cell, int stepCost)> l)> solutions = new();
        Ways = GenerateWays();
        this.BestCost = int.MaxValue;
        Play(null, stack, solutions, 0);
        solutions.Sort((x, y) => x.cost - y.cost);
        foreach (var valueTuple in solutions)
        {
            Console.WriteLine(valueTuple.cost);
        }
    }

    public static Dictionary<(Cell, Cell), List<Cell>> Ways;

    public int BestCost { get; set; }

    public void Play(Player? last, Stack<(Player, Cell, int stepCost)> stack,
        List<(int cost, List<(Player, Cell, int stepCost)> l)> solutions, int totCost)
    {
        if (totCost >= BestCost)
        {
            //shortcircuit bad paths
            return;
        }

        //Get all players possible moves
        //play one, then recurse until locked or game solved, save solutions
        if (Solved())
        {
            var l = stack.ToList();
            solutions.Add((totCost, l));
            Console.WriteLine("Found a solution: " + totCost);
            BestCost = totCost;
            return;
        }

        foreach (var player in Players)
        {
            var moves = player.PossibleMoves();
            //             Console.WriteLine($"{stack.Count } Player {player.Id} Moves:{moves.Count}");

            if (moves.Count <= 0) continue;

            if (stack.Count < 2)
            {
                Console.WriteLine($"{stack.Count } Player {player.Id} Moves:{moves.Count}");
            }

            foreach (var move in moves)
            {
                var moveCost = move.Value * player.Cost;
                var newTotcost = totCost + moveCost;

                var posWas = player.Pos;
                var cameFromWas = player.CameFrom;
                player.Pos.Occ = ".";
                player.CameFrom = player.Pos;
                player.Pos = move.Key;
                player.Pos.Occ = player.Kind;
                if (player.Id == "B2")
                {

                }
                // Console.WriteLine(this.ToString());
                stack.Push((player, move.Key, moveCost));
                Play(player, stack, solutions, newTotcost);
                if (Solved())
                {

                }
                stack.Pop();

                player.Pos.Occ = ".";
                player.Pos = posWas;
                player.CameFrom = cameFromWas;
                player.Pos.Occ = player.Kind;
            }
        }

        //Console.Write("-");
    }

    private bool Solved()
    {
        return
            RoomAU.Occ == "A" &&
            RoomBU.Occ == "B" &&
            RoomCU.Occ == "C" &&
            RoomDU.Occ == "D" &&
            RoomAL.Occ == "A" &&
            RoomBL.Occ == "B" &&
            RoomCL.Occ == "C" &&
            RoomDL.Occ == "D" &&
            RoomA3.Occ == "A" &&
            RoomB3.Occ == "B" &&
            RoomC3.Occ == "C" &&
            RoomD3.Occ == "D" &&
            RoomA2.Occ == "A" &&
            RoomB2.Occ == "B" &&
            RoomC2.Occ == "C" &&
            RoomD2.Occ == "D";
    }

    public class Cell
    {
        public readonly string Typ = "";

        public List<Cell> Exits = new();
        public string Occ = "";

        public Cell(string typ, string au, params Cell[] exits)
        {
            Typ = typ;
            Name = au;
            foreach (var exit in exits) Exits.Add(exit);
            AllCells.Add(this);
        }

        public string Name { get; set; }

        public bool IsFree => Occ == "" || Occ == ".";
        public bool NoStop { get; set; }
        public bool Correct => Occ == Typ;
        public bool InHall => Typ == "H";

        public override string ToString()
        {
            var occ = IsFree ? "(.)" : $"({Occ})";
            return $"{Name}{occ}";
        }
    }


    public class Player
    {

        public Cell? CameFrom = null;

        public int Cost;
        public readonly Cell HomeL;
        public readonly Cell Home2;
        public readonly Cell Home3;
        public readonly Cell HomeU;
        public readonly string Kind;
        public Cell Pos;

        public Player(Cell room)
        {
            Pos = room;
            Kind = room.Occ;
            Typ = Types[Kind];
            HomeU = Typ.home[3];
            Home2 = Typ.home[2];
            Home3 = Typ.home[1];
            HomeL = Typ.home[0];
            Cost = Typ.cost;
            Id = room.Name;
        }

        public (Cell[] home, int cost) Typ { get; set; }
        public string Id { get; }
        public bool IsHome => Pos.Typ == Kind;
        public bool HasMoved => Pos.Name != Id;
        public bool IsInHall => Pos.Typ == "H";
        public bool AtStart => Pos.Name == Id;

        public Cell? HomeRoomsFree(Cell pos, out bool allHome, out bool lockedInRightPlace)
        {
            Cell? target = null;
            allHome = true;
            lockedInRightPlace = false;
            foreach (var c in Typ.home)
            {
                if (c.Occ != Kind)
                {
                    allHome = false;
                    if (c.IsFree)
                    {
                        target ??= c;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (pos == c)
                {
                    lockedInRightPlace = allHome;
                }
            }

            return target;
        }
        public Dictionary<Cell, int> PossibleMoves()
        {
            var dest = new Dictionary<Cell, int>();
            var homeTarget = HomeRoomsFree(Pos, out var allHome, out var lockedAtHome);
            var canGoHome = homeTarget != null;
            if (IsHome && allHome)
            {
                //stay
            }
            else
            {
                if (AtStart)
                {
                    //at start
                    //to hall or home
                    if (lockedAtHome)
                        return dest;

                    GenerateAvailable(Pos, null, dest, 1, homeTarget);

                    if (dest.Count == 0)
                        return dest;

                    // not home if not emptied
                    // just outside not allowed

                    dest.Keys.ToList()
                        .Where(c => c.NoStop || c.Typ != "H" && c != homeTarget)
                        .ForEach((c, _) => dest.Remove(c));

                }
                else if (IsInHall)
                {
                    //in hall
                    //only move home
                    if (canGoHome) // Om target == null finns inte plats i home
                    {
                        var way = Ways[(Pos, homeTarget)];
                        var wayOk = way.All(x => x.IsFree);

                        if (wayOk)
                        {
                            dest[homeTarget] = way.Count;
                            return dest;
                        }
                        else
                        {
                            return dest;
                            GenerateAvailable(Pos, CameFrom, dest, 1, homeTarget);
                        }

                        if (dest.Count == 0) return dest;

                        // Hall=>Hall not allowed
                        dest.Keys.ToList().Where(c => c.Typ == "H" || c != homeTarget)
                            .ForEach((c, _) => dest.Remove(c));
                    }
                }
            }


            return dest;
        }


        private void GenerateAvailable(Cell pos, Cell? cameFrom, Dictionary<Cell, int> dest, int steps, Cell? targetCell)
        {
            foreach (var posExit in pos.Exits)
            {
                if (posExit.IsFree && !dest.ContainsKey(posExit))
                {
                    if (targetCell != null
                        && IsInHall
                        && !posExit.InHall
                        && posExit.Typ != targetCell.Typ)
                        continue;

                    dest[posExit] = steps;


                    GenerateAvailable(posExit, pos, dest, steps + 1, targetCell);
                }
            }
        }
    }

    public static List<Cell> AllCells { get; set; } = new List<Cell>();

    public static void ListAllCells()
    {
        AllCells.ForEach(x =>
        {
            Console.Write(x.Name + " exits;");
            x.Exits.ForEach(((e, _) =>
            {
                Console.Write($" {e.Name} ");

            }));
            Console.Write($"\n");

        });
    }
}