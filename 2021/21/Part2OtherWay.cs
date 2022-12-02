using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using common;

namespace _21
{
    internal class Part2OtherWay
    {

        struct PlayerState
        {
            public int Pos;
            public int Score;
        };

        readonly ulong[] _wins = new ulong[2];
        readonly int[] _waysToRollValue = new int[10];

        void Play(PlayerState[] incomingStates, int player = 0, ulong waysToReachThisState = 1)
        {
            for (int rolledSum = 3; rolledSum <= 9; rolledSum++) //possible outcomes of 3 rolls
            {
                var newWaysToGetHere = waysToReachThisState
                                       * (ulong)_waysToRollValue[rolledSum];

                var nextStates = new[] { incomingStates[0], incomingStates[1] };

                nextStates[player].Pos = nextStates[player].Pos.AddOneBasedModular(rolledSum, 10);
                nextStates[player].Score += nextStates[player].Pos;

                if (nextStates[player].Score >= 21)
                    _wins[player] += newWaysToGetHere;
                else
                    Play(nextStates, 1 - player, newWaysToGetHere);
            }
        }

        public ulong[] DoPart2((int, int) pos)
        {
            for (int i = 1; i <= 3; i++)
                for (int j = 1; j <= 3; j++)
                    for (int k = 1; k <= 3; k++)
                        _waysToRollValue[i + j + k]++;

            PlayerState[] psa = {
                new() { Pos = pos.Item1, Score = 0 },
                new() { Pos = pos.Item2, Score = 0 } };
            Play(psa);
            return _wins;
        }
    }

}
