using common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace _4
{
    class Program
    {

        static void Main(string[] args)
        {
            var game = LoadData("input.txt");
            //var game = LoadData();
            while (game.Boards.Count > 0)
            {
                var winner = game.Play();
                foreach (var board in winner)
                {
                    game.Boards.Remove(board);
                    Console.WriteLine($"Winner board {board.Index + 1} Score= {(board.SumFree() * game.LastCall())} after call number {game.Index}");
                }
            }
        }

        static Game LoadData(string file = "")
        {
            var game = new Game();
            TextReader reader;
            if (file == "")
            {
                reader = new StringReader(testData);
            }
            else
            {
                Console.WriteLine("Loading from file " + file);
                reader = File.OpenText(file);
            }

            var randomNumbers = reader.ReadLine();
            game.Numbers = randomNumbers?.Split(',').Select(x => int.Parse(x)).ToList();
            var boardWidth = 5;
            var someValue = false;
            while (true)
            {
                var rows = new List<List<int>>();
                for (int r = 0; r < boardWidth; r++)
                {
                    var row = new List<int>();
                    for (int c = 0; c < boardWidth; c++)
                    {
                        var next = reader.ReadInt();
                        if (next == null) break;
                        someValue = true;
                        row.Add(next.Value);
                    }

                    if (row.Count < boardWidth) break;
                    rows.Add(row);
                }

                if (rows.Count < boardWidth)
                {
                    break;
                }

                game.AddBoard(new Board(rows));
                someValue = false;
            }
            if (someValue)
            {
                Console.WriteLine("Formatting issue: some board could not be read.");
            }
            Console.WriteLine($"Loaded {game.Numbers?.Count} numbers and {game.Boards.Count} boards");

            return game;
        }


        private static string testData = @"7,4,9,5,11,17,23,2,0,14,21,24,10,16,13,6,15,25,12,22,18,20,8,19,3,26,1

22 13 17 11  0
 8  2 23  4 24
21  9 14 16  7
 6 10  3 18  5
 1 12 20 15 19

 3 15  0  2 22
 9 18 13 17  5
19  8  7 25 23
20 11 10 24  4
14 21 16 12  6

14 21 17 24  4
10 16 15  9 19
18  8 23 26 20
22 11 13  6  5
 2  0 12  3  7
";
    }

}
