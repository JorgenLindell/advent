using System.Diagnostics;
using System.Threading.Tasks.Sources;
using common;

namespace _21;

public class QuantumGame
{
    public class Player
    {
        public int Id;
        public ulong Won;
    }
    public struct State : IEquatable<State>
    {
        public int Score;
        public int PosOnBoard;
        public int LastRollNumber;
        public bool Equals(State other) => Score == other.Score && PosOnBoard == other.PosOnBoard && LastRollNumber == other.LastRollNumber;
        public override bool Equals(object? obj) => obj is State other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Score, PosOnBoard, LastRollNumber);
    }

    public struct GameState : IEquatable<GameState>
    {
        public int PlayerId;
        public State Current;
        public State Other;

        public bool Equals(GameState other) => Current.Equals(other.Current) && Other.Equals(other.Other) && PlayerId.Equals(other.PlayerId);
        public override bool Equals(object? obj) => obj is GameState other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Current, Other, PlayerId);
    }


    public Player Player1 = new() { Id = 1 };
    public Player Player2 = new() { Id = 2 };
    public double GamePercentage;
    private double _gamePercentageReported;
    private readonly Stopwatch _stopWatch = new();
    private readonly Dictionary<(int rolled, GameState state), ulong> _cache = new();
    private bool _report = false;

    public void StartGame(int start1, int start2)
    {

        var stack = new Stack<int>();
        var state1 = new State
        {
            PosOnBoard = start1,
            LastRollNumber = 0,
            Score = 0
        };
        var state2 = new State
        {
            PosOnBoard = start2,
            LastRollNumber = 0,
            Score = 0
        };
        var initialState = new GameState() { PlayerId = 1, Current = state1, Other = state2 };
        GamePercentage = 0;
        _stopWatch.Start();
        for (int roll = 1; roll < 4; roll++)
        {
            stack.Push(roll);
            var i = Play(0, roll, initialState, Player1,
                (roll - 1) / 3d, 1 / 3d, stack);
            stack.Pop();
            GamePercentage = roll / 3d;
            Console.WriteLine($"Percent done:{GamePercentage}   Scores:{Player1.Won} {Player2.Won} ");
        }

        var testResults = new ulong[] { 444356092776315, 341960390180808 };
        var myResults = Player1.Won + Player2.Won;
        var tResults = testResults[0] + testResults[1];
        var numberPlayerDiff = Player1.Won - testResults[0];
        var numberDiff = myResults - tResults;
        if (Player1.Won == 444356092776315)
            Console.WriteLine("Test succeded");
    }


    public ulong Play(int depth, int rolled, GameState incomingState, Player currentPlayer,
        double percentDone, double percentage, Stack<int> stack)
    {
        if (_cache.ContainsKey((rolled, incomingState)))
        {
            if (incomingState.PlayerId != currentPlayer.Id)
            {

            }
            var w = _cache[(rolled, incomingState)];
            currentPlayer.Won += w;
            if (_report)
                Console.WriteLine($"({depth}) Player {currentPlayer.Id} win {w} by cache");
            return w;
        }

        var stepPercentage = percentage / 3;

        var (nextPlayer, nextState, wins) = PlayStep(depth, currentPlayer, rolled, incomingState);

        if (wins > 0)
            return wins;

        wins = 0;
        var anyLost = false;
        for (int roll = 1; roll < 4; roll++)
        {
            stack.Push(roll);
            var win = Play(depth + 1, roll, nextState, nextPlayer,
                percentDone + (stepPercentage * (roll - 1)), stepPercentage, stack);
            if (_report) Console.WriteLine($"Continue at depth {depth} with player {nextPlayer.Id} Scores:{nextState.Current.Score} {nextState.Other.Score} ");
            stack.Pop();

            if (win > 0)
            {
                wins += win;
                _cache[(roll, nextState)] = win;
            }
            else
            {
                anyLost = true;
            }

        }
        if (!anyLost)
        {
            _cache[(rolled, incomingState)] = wins;
        }


        GamePercentage = percentDone + percentage;
        if ((GamePercentage - _gamePercentageReported > 0.01d)
            /*|| (player1.Won + player2.Won) % 1000000 == 0*/)
        {
            _gamePercentageReported = GamePercentage;
            var expect = _stopWatch.Elapsed / GamePercentage;
            Console.WriteLine($"{_stopWatch.Elapsed:g} ({expect:g})( Percent done:{GamePercentage} at depth {depth}  Scores:{Player1.Won} {Player2.Won} ");
            Console.WriteLine($"Stack:{stack.ToArray().Select(i => "" + i).StringJoin()} at depth {depth}  ");
        }
        return 0;

    }

    private (Player currentPlayer, GameState nextGameState, ulong wins) PlayStep(int depth,
        Player player, int rolled, GameState currentGameState)
    {

        GameState nextGameState;
        if (currentGameState.Current.LastRollNumber < 2)
        {
            //current rolls on
            var posOnBoard = currentGameState.Current.PosOnBoard.AddOneBasedModular(rolled, 10);
            var nextCurrentState = new State
            {
                PosOnBoard = posOnBoard,
                LastRollNumber = currentGameState.Current.LastRollNumber + 1,
                Score = currentGameState.Current.Score
            };
            if (_report)
                Console.WriteLine(
                    $"({depth}) Player {player.Id} rolls {rolled} ({nextCurrentState.LastRollNumber}) and moves to {nextCurrentState.PosOnBoard}");
            nextGameState = new GameState
            {
                PlayerId = player.Id,
                Current = nextCurrentState,
                Other = currentGameState.Other
            };
            return (player, nextGameState, 0);
        }
        else
        {
            //register score and switch
            var posOnBoard = currentGameState.Current.PosOnBoard.AddOneBasedModular(rolled, 10);
            var nextState = new State
            {
                PosOnBoard = posOnBoard,
                Score = currentGameState.Current.Score + posOnBoard,
                LastRollNumber = 0,
            };

            if (nextState.Score >= 21)
            {
                if (_report)
                    Console.WriteLine(
                        $"Player {player.Id} rolls {rolled} and moves to {nextState.PosOnBoard} Score={nextState.Score} WINS at {depth}");
                _cache[(rolled, currentGameState)] = 1;
                player.Won += 1;

                nextGameState = new GameState
                {
                    PlayerId = player.Id,
                    Current = nextState,
                    Other = currentGameState.Other
                };

                return (player, nextGameState, 1);
            }

            if (_report)
                Console.WriteLine(
                    $"({depth}) Player {player.Id} rolls {rolled} and moves to {nextState.PosOnBoard} Score={nextState.Score}\n");

            player = player == Player1 ? Player2 : Player1;

            nextGameState = new GameState
            {
                PlayerId = player.Id,
                Current = currentGameState.Other,
                Other = nextState
            };
            return (player, nextGameState, 0);
        }
    }
}