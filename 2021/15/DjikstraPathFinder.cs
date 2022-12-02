using System.Security.Cryptography.X509Certificates;
using common;

namespace _15
{
    internal class DjikstraPathFinder
    {
        protected bool _cancel;
        protected RunningAverage _averageQuitted = new RunningAverage();
        private int _stopAtPath;
        private double _bestSoFar;

        public Matris<DjikstraNode> NodeMatrix { get; }
        protected (int r, int c) Start => StartCell.Pos;
        protected (int r, int c) End => EndCell.Pos;
        protected List<Path<DjikstraNode>> _resultpaths = new List<Path<DjikstraNode>>();
        public DjikstraNode StartCell { get; private set; }
        public DjikstraNode EndCell { get; private set; }

        public DjikstraPathFinder(Matris<DjikstraNode> matrix,
            (int r, int c) start, (int r, int c) end)
        {
            NodeMatrix = matrix;
            StartCell = NodeMatrix.Value(start);
            EndCell = NodeMatrix.Value(end);
            _cancel = false;
            _stopAtPath = int.MaxValue;

        }

        public List<Path<DjikstraNode>> Run()
        {
            DjikstraNode cell;
            if (true)
                using (new Measure())
                    Bfs();
            else
                using (new Measure())
                    Dfs();
            cell = NodeMatrix.Value(End);
            var totRes = EndCell.SumValue;
            var pathBackwards = new List<DjikstraNode?>();
            while (cell != null)
            {
                pathBackwards.Add(cell);
                cell = cell.CameFrom;
            }

            _resultpaths = new List<Path<DjikstraNode>>
            {
                new(pathBackwards.Cast<DjikstraNode>().Reverse(),totRes)
            };
            Console.WriteLine(NodeMatrix.ToString((cell,c) => $" {c.SumValue,5}"));

            return _resultpaths;

        }

        private void Bfs()
        {

            foreach (var node in NodeMatrix.AllCells)
            {
                NodeMatrix.Update(node.Cell, n =>
                {
                    n.CameFrom = null;
                    n.SumValue = int.MaxValue;
                    return n;
                });
            }

            var start = NodeMatrix.Value((0, 0));
            start.SumValue = 0;
            start.CameFrom = null;
            var stack = new Stack<DjikstraNode>();
            stack.Push(start);
            var maxStack = 0;
            while (stack.Count > 0)
            {
                maxStack = Math.Max(maxStack, stack.Count);
                var cell = stack.Pop();
                StepBfs(cell);
            }


            void StepBfs(DjikstraNode cell)
            {
                foreach (var nextCell in cell.NextCells)
                {
                    var nextCellNewSum = cell.SumValue + nextCell.Value;
                    if (nextCellNewSum < nextCell.SumValue)
                    {
                        nextCell.CameFrom = cell;
                        nextCell.SumValue = nextCellNewSum;
                        stack.Push(nextCell);
                    }
                }
            }
        }

        private void Dfs(bool justOnce = false)
        {
            var maxcost = StepsToEnd(Start) * 9;
            foreach (var node in NodeMatrix.AllCells)
            {
                NodeMatrix.Update(node.Cell, n =>
                {
                    n.CameFrom = null;
                    n.SumValue = maxcost; //worst case
                    // always try go straight first:
                    n.NextCells = n.NextCells.OrderBy(x => DistanceToEnd(x.Pos)).ToList();
                    return n;
                });
            }

            var start = NodeMatrix.Value((0, 0));
            start.SumValue = 0;
            start.CameFrom = null;

            StepDfs(start);

            void StepDfs(DjikstraNode? cell)
            {
                if (cell.SumValue > maxcost)
                {
                    return;
                }
                if (cell.Pos == End && maxcost > cell.SumValue)
                {
                    maxcost = cell.SumValue;
                }

                if (justOnce && EndCell.SumValue < int.MaxValue)
                    return;

                foreach (var nextCell in cell.NextCells)
                {
                    var nextCellSumValue = cell.SumValue + nextCell.Value;
                    if (nextCellSumValue < nextCell.SumValue)
                    {
                        nextCell.CameFrom = cell;
                        nextCell.SumValue = nextCellSumValue;
                        StepDfs(nextCell);
                    }
                }
            }
        }



        private void RegisterPath(Path<DjikstraNode> path)
        {

            if (path.Result < _bestSoFar)
            {
                _bestSoFar = path.Result;
            }
            _resultpaths.Add(new Path<DjikstraNode>(path));
            if (_resultpaths.Count >= _stopAtPath) _cancel = true;
        }
        private static IEnumerable<DjikstraNode> GetNextCells(List<DjikstraNode> cellNextCells, Func<DjikstraNode, bool> whereFunc, Func<DjikstraNode, double> orderFunc)
        {
            (DjikstraNode, double)[] cells = new (DjikstraNode, double)[9];
            int i = 0;
            foreach (var c in cellNextCells)
            {
                if (whereFunc(c))
                {
                    cells[i++] = (c, orderFunc(c));
                }
            }

            switch (i)
            {
                case 5:
                    Bubble(0, i);
                    return new[] { cells[0].Item1, cells[1].Item1, cells[2].Item1, cells[3].Item1 };
                case 4:
                    Bubble(0, i);
                    return new[] { cells[0].Item1, cells[1].Item1, cells[2].Item1 };
                case 3 when cells[0].Item2 > cells[1].Item2:
                    return new[] { cells[1].Item1, cells[0].Item1 };
                case 3:
                    return new[] { cells[0].Item1, cells[1].Item1 };
                case 2:
                    return new[] { cells[0].Item1 };
                default:
                    return new DjikstraNode[] { };
            }

            void Bubble(int start, int end)
            {
                for (int n = start; n < end; n++)
                {
                    for (int j = start; j < end; j++)
                    {
                        var k = j + 1;
                        if (cells[j].Item2 > cells[k].Item2)
                        {
                            (cells[j], cells[k]) = (cells[k], cells[j]);
                        }
                    }
                }
            }
        }

        private void QuitPath(Path<DjikstraNode> path)
        {
            _averageQuitted.Add(path.Cells.Count);
            // _quittedPaths.Add(new Path(path));
        }
        protected double Distance((int r, int c) start, (int r, int c) end)
        {
            var b = Math.Abs(start.c - end.c);
            var h = Math.Abs(start.r - end.r);
            return Math.Sqrt(b * b + h * h);
        }
        protected int Steps((int r, int c) start, (int r, int c) end)
        {
            var b = Math.Abs(start.c - end.c);
            var h = Math.Abs(start.r - end.r);
            return b + h;
        }
        protected double DistanceToEnd((int r, int c) start)
        {
            return Distance(start, End);
        }
        protected int StepsToEnd((int r, int c) start)
        {
            return Steps(start, End);
        }
    }


}