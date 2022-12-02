using System.Collections.Generic;

namespace _4
{
    internal class Game
    {
        public List<int> Numbers { get; set; } = new List<int>();
        public int Index { get; set; }
        public List<Board> Boards = new List<Board>();
        public void AddBoard(Board board)
        {
            board.Index = Boards.Count;
            Boards.Add(board);
        }

        public int DrawNumber()
        {
            return Numbers[Index++];
        }
        public int LastCall()
        {
            return Numbers[Index - 1];
        }
        internal List<Board> Play()
        {
            while (Index < Numbers.Count)
            {
                var x = DrawNumber();
                foreach (var board in Boards)
                {
                    board.Mark(x);
                }

                var winners = new List<Board>();
                foreach (var board in Boards)
                {
                    if (board.HasWin)
                        winners.Add(board);
                }

                if (winners.Count > 0)
                    return winners;

            }

            return new List<Board>();
        }
    }
}