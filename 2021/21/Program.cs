using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using common;

namespace _21
{


    class Program
    {

        static void Main()
        {
            Stopwatch stopWatch = new();
            stopWatch.Start();
            var result = new Part2OtherWay().DoPart2((10, 4));
            stopWatch.Stop();
            Console.WriteLine("Result 1 " + result[0]);
            Console.WriteLine("Result 2 " + result[1]);
            Console.WriteLine($"Time: {stopWatch.Elapsed:g}");

        }
        private static IEnumerable<string> LoadStream(TextReader stream)
        {
            var inputLine = stream.ReadLine();
            while (stream.Peek() != -1 || !inputLine!.IsNullOrEmpty())
            {
                if (inputLine is { } && inputLine != "")
                {
                    yield return inputLine;
                }
                inputLine = stream.ReadLine();
            }
        }
    }
}