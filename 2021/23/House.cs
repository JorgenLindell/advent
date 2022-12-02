using common;

namespace _22;

public class House
{
    public static readonly Cell RoomAU;
    public static readonly Cell RoomAL;
    public static readonly Cell RoomBU;
    public static readonly Cell RoomBL;
    public static readonly Cell RoomCU;
    public static readonly Cell RoomCL;
    public static readonly Cell RoomDU;
    public static readonly Cell RoomDL;
    public static readonly Cell[] Hall = new Cell[11];
    private readonly List<Player> Players;

    private string boardTemplate = @"
#############
#01234567890#
  #A#B#C#D#
  #A#B#C#D#
  #########
";

    static House()
    {
        RoomAU = new Cell("A", "AU");
        RoomAL = new Cell("A", "AL");
        RoomBU = new Cell("B", "BU");
        RoomBL = new Cell("B", "BL");
        RoomCU = new Cell("C", "CU");
        RoomCL = new Cell("C", "CL");
        RoomDU = new Cell("D", "DU");
        RoomDL = new Cell("D", "DL");
        for (var i = 0; i < 11; i++)
        {
            Hall[i] = new Cell("H", "H" + i);
            if (i > 0) Connect(Hall[i], Hall[i - 1]);
        }

        Connect(RoomAL, RoomAU);
        Connect(RoomBL, RoomBU);
        Connect(RoomCL, RoomCU);
        Connect(RoomDL, RoomDU);
        Connect(RoomAU, Hall[2]);
        Connect(RoomBU, Hall[4]);
        Connect(RoomCU, Hall[6]);
        Connect(RoomDU, Hall[8]);
        Hall[2].NoStop = true;
        Hall[4].NoStop = true;
        Hall[6].NoStop = true;
        Hall[8].NoStop = true;


    }

    public House(int room1U, int room1L, int room2U, int room2L, int room3U, int room3L, int room4U, int room4L)
    {
        RoomAU.Occ = "" + (char)room1U;
        RoomAL.Occ = "" + (char)room1L;
        RoomBU.Occ = "" + (char)room2U;
        RoomBL.Occ = "" + (char)room2L;
        RoomCU.Occ = "" + (char)room3U;
        RoomCL.Occ = "" + (char)room3L;
        RoomDU.Occ = "" + (char)room4U;
        RoomDL.Occ = "" + (char)room4L;

        Players = new List<Player>
        {
            new(RoomAU),
            new(RoomAL),
            new(RoomBU),
            new(RoomBL),
            new(RoomCU),
            new(RoomCL),
            new(RoomDU),
            new(RoomDL)
        };
    }

    public static void Connect(Cell c1, Cell c2)
    {
        c1.Exits.Add(c2);
        c2.Exits.Add(c1);
    }

    public override string ToString()
    {
        var halls = Hall.Select(h => h.Occ != "" ? h.Occ : ".").StringJoin("");
        return $"{halls}\n" +
               $" |{RoomAU.Occ}|{RoomBU.Occ}|{RoomCU.Occ}|{RoomDU.Occ}|\n" +
               $" |{RoomAL.Occ}|{RoomBL.Occ}|{RoomCL.Occ}|{RoomDL.Occ}|\n";
    }

    public static House LoadStream(TextReader stream)
    {
        stream.SkipUntil('#');
        var skip = stream.ReadLine();
        var corridor = stream.ReadLine();
        skip = stream.SkipOver("###");
        var room1U = stream.Read();
        stream.Read();
        var room2U = stream.Read();
        stream.Read();
        var room3U = stream.Read();
        stream.Read();
        var room4U = stream.Read();
        stream.ReadLine();
        skip = stream.SkipOver("#");
        var room1L = stream.Read();
        stream.Read();
        var room2L = stream.Read();
        stream.Read();
        var room3L = stream.Read();
        stream.Read();
        var room4L = stream.Read();

        return new House(room1U, room1L, room2U, room2L, room3U, room3L, room4U, room4L);
    }

    public void StartPlay()
    {
        Console.WriteLine("");
        Console.WriteLine(this.ToString());
        //Get all players possible moves
        //play one, then recurse until locked or game solved, save solutions
        Stack<(Player, Cell, int stepCost)> stack = new();
        List<(int cost, List<(Player, Cell, int stepCost)> l)> solutions = new();
        this.BestCost = int.MaxValue;
        Players.Reverse();

        Play(stack, solutions, 0);
        solutions.Sort((x, y) => x.cost - y.cost);
        foreach (var valueTuple in solutions)
        {
            Console.WriteLine(valueTuple.cost);
        }
    }

    public int BestCost { get; set; }

    public void Play(Stack<(Player, Cell, int stepCost)> stack,
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
            if (moves.Count > 0)
            {
                //         Console.WriteLine($"{stack.Count } Player {player.Id} Moves:{moves.Count}");
                if (stack.Count < 1)
                {
                    Console.WriteLine($"{stack.Count } Player {player.Id} Moves:{moves.Count}");
                }
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
                player.Pos.Occ = player.Name;
               
                stack.Push((player, move.Key, moveCost));
                Play(stack, solutions, newTotcost);
                stack.Pop();
                
                player.Pos.Occ = ".";
                player.Pos = posWas;
                player.CameFrom = cameFromWas;
                player.Pos.Occ = player.Name;
            }
        }
    }

    private bool Solved()
    {
        return
            RoomAU.Occ == "A" &&
            RoomAL.Occ == "A" &&
            RoomBU.Occ == "B" &&
            RoomBL.Occ == "B" &&
            RoomCU.Occ == "C" &&
            RoomCL.Occ == "C" &&
            RoomDU.Occ == "D" &&
            RoomDL.Occ == "D";
    }

    public class Cell
    {
        public readonly string Typ = "";

        public HashSet<Cell> Exits = new();
        public string Occ = "";

        public Cell(string typ, string au, params Cell[] exits)
        {
            Typ = typ;
            Name = au;
            foreach (var exit in exits) Exits.Add(exit);
        }

        public string Name { get; set; }

        public bool IsFree => Occ == "" || Occ == ".";
        public bool NoStop { get; set; }
        public override string ToString()
        {
            var occ = IsFree ? "(.)" : $"({Occ})";
            return $"{Name}{occ}";
        }
    }

    public class Player
    {
        private static readonly Dictionary<string, ((Cell u, Cell l) home, int cost)> Types = new()
        {
            ["A"] = ((RoomAU, RoomAL), 1),
            ["B"] = ((RoomBU, RoomBL), 10),
            ["C"] = ((RoomCU, RoomCL), 100),
            ["D"] = ((RoomDU, RoomDL), 1000)
        };

        public Cell? CameFrom = null;

        public int Cost;
        public readonly Cell HomeL;
        public readonly Cell HomeU;
        public readonly string Name;
        public Cell Pos;

        public Player(Cell room)
        {
            Pos = room;
            Name = room.Occ;
            var type = Types[Name];
            HomeU = type.home.u;
            HomeL = type.home.l;
            Cost = type.cost;
            Id = room.Name;
        }

        public string Id { get; }


        public Dictionary<Cell, int> PossibleMoves()
        {
            var dest = new Dictionary<Cell, int>();
            var done = Pos == HomeL || (Pos == HomeU && HomeL.Occ == Name);
            if (!done)
            {
                if (Pos.Typ != "H" && CameFrom == null)
                {
                    //at start
                    //to hall or home

                    GenerateAvailable(Pos, null, dest, 1);
                    if ((dest.ContainsKey(HomeU) && HomeL.Occ != Name) || Pos == HomeL)
                        dest.Remove(HomeU); //dont go to top home unless the lower cell is filled correctly

                    // just outside not allowed
                    dest.Keys.ToList().Where(c => c.NoStop).ForEach((c, _) => dest.Remove(c));
                }
                else if (Pos.Typ == "H")
                {
                    //in hall
                    //only move home
                    if (HomeU.IsFree && (HomeL.IsFree || HomeL.Occ == Name))
                    {
                        GenerateAvailable(Pos, CameFrom, dest, 1);

                        // Hall=>Hall not allowed
                        dest.Keys.ToList().Where(c => c.Typ == "H").ForEach((c, _) => dest.Remove(c));
                    }
                }
                else
                {
                    //no moves available
                }
            }

            return dest;
        }

        private void GenerateAvailable(Cell pos, Cell? cameFrom, Dictionary<Cell, int> dest, int steps)
        {
            if (pos == HomeL) return;
            if (pos == HomeU && HomeL.Occ == Name) return;

            foreach (var posExit in pos.Exits)
            {
                if (posExit.IsFree && posExit != cameFrom && !dest.ContainsKey(posExit))
                {
                    dest[posExit] = steps;
                    GenerateAvailable(posExit, pos, dest, steps + 1);
                }
            }
        }
    }
}