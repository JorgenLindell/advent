using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using common;

//https://adventofcode.com/2022/day/7

namespace _7;

internal class Program
{
    private static string _testData =
        @"$ cd /
$ ls
dir a
14848514 b.txt
8504156 c.dat
dir d
$ cd a
$ ls
dir e
29116 f
2557 g
62596 h.lst
$ cd e
$ ls
584 i
$ cd ..
$ cd ..
$ cd d
$ ls
4060174 j
8033020 d.log
5626152 d.ext
7214296 k"
.Replace("\r\n", "\n");



    private static void Main(string[] args)
    {
        FirstPart(GetDataStream());
        SecondPart(GetDataStream());
    }

    private static TextReader GetDataStream()
    {
        var stream = StreamUtils.GetInputStream("input.txt");
        //var stream = StreamUtils.GetInputStream(testData: _testData);
        return stream;
    }
    private static void SecondPart(TextReader stream)
    {
        var system = LoadSystem(stream);
        var maxSize = 70000000;
        var needed = 30000000;
        var inUse = system.Root.CalculatedSize;
        var unused = maxSize - inUse;
        var needToDelete = needed-unused;
        var valueTuples = system.Root.AllDirectories().Select(x => (x.CalculatedSize, x)).ToList();

        var bigEnough = valueTuples
            .Where(x => x.CalculatedSize >= needToDelete).ToList();
        
        var list = bigEnough
            .OrderBy(x => x.CalculatedSize)
            .ToList();

        Debug.WriteLine($"In use: {inUse}");
        Debug.WriteLine($"unused: {unused}");
        Debug.WriteLine($"needToDelete: {needToDelete}");
        Debug.WriteLine($"result2 : Need {needToDelete}   {list.First().CalculatedSize} {list.First().x.FullPath}");
    }

    private static void FirstPart(TextReader stream)
    {
        var system = LoadSystem(stream);

        var sum = system.Root.AllDirectories()
            .Where(x => x.CalculatedSize < 100000).Sum(x => x.CalculatedSize);
        Debug.WriteLine($"result1 :" + sum);
    }

    private static FileSystem LoadSystem(TextReader stream)
    {
        var lines = new List<string>();
        var system = new FileSystem();
        while (stream.ReadLine() is { } line)
            lines.Add(line);

        var command = lines[0];
        var lineIndex = 0;
        while (lineIndex < lines.Count)
        {
            if (command[0] == '$')
            {
                var output = new List<string>();

                var line = lines[++lineIndex];
                while (line[0] != '$')
                {
                    output.Add(line);
                    ++lineIndex;
                    if (lineIndex >= lines.Count) break;
                    line = lines[lineIndex];
                }

                ;

                system.Parse(command, output);
                if (lineIndex >= lines.Count) break;
                command = line;
            }
        }

        return system;
    }
}