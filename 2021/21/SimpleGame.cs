namespace _21;
public class Board
{
    private int _pos;

    public Board(int start)
    {
        _pos = start;
    }

    public int Move(int roll3)
    {
        _pos += roll3;
        while (_pos > 10) _pos -= 10;
        return _pos;
    }
}

public class SimpleGame
{
    public void Play(int start1, int start2,int scoretoWin)
    {
        var dice = new DetDice(3);
        var player1 = new Player(1, dice, start1, scoretoWin);
        var player2 = new Player(2, dice, start2, scoretoWin);
        while (player1.Score < 1000 && player2.Score < scoretoWin)
        {
            if (!player1.Move())
                player2.Move();
        }

        var looserScore = player1.Score > player2.Score ? player2.Score : player1.Score;
        var game = dice.TotalNumberOfRolls * looserScore;
        Console.WriteLine("Game result= " + game);

    }
    public class DetDice
    {
        private readonly int _max;

        public DetDice(int max)
        {
            _max = max;
        }
        public int Next { get; private set; } = 1;
        public int TotalNumberOfRolls { get; private set; } = 0;

        public int Roll()
        {
            var retval = Next;
            if (++Next == _max+1) Next = 1;
            TotalNumberOfRolls++;
            return retval;
        }

        public int Roll3() => Roll() + Roll() + Roll();
    }
    public class Player
    {
        private readonly DetDice _detDice;
        private readonly int _scoretoWin;
        private readonly int _number;
        private Board Board { get; }
        public int Score { get; private set; }
        public int Moves { get; private set; }

        public Player(int number, DetDice detDice, int startpos, int scoretoWin)
        {
            _detDice = detDice;
            _scoretoWin = scoretoWin;
            _number = number;
            this.Board = new Board(startpos);
            Console.WriteLine($"Player {_number} starts at:{startpos} ");
        }


        public bool Move()
        {
            Moves++;
            var res = Board.Move( _detDice.Roll3());
            this.Score += res;
            if (Score >= _scoretoWin)
            {
                Console.WriteLine($"Player {_number} wins at score:{Score} after {Moves} moves.");
                return true;
            }
            return false;
        }

    }
}