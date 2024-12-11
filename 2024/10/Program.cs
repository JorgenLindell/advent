using common;
using System.Collections.Generic;

var data = StreamUtils.GetLines();
//data = @"
//89010123
//78121874
//87430965
//96549874
//45678903
//32019012
//01329801
//10456732
//    ".Split("\r\n".ToCharArray(), StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
var m = new Matris<int>(data.Length, data[0].Length, cell => data[cell.r][cell.c] - '0');
for (int r = 0; r < m.Rows; r++)
    for (int c = 0; c < m.Columns; c++)
    {
        m.Set((r, c), data[r][c] - '0');
    }

var trailheads = m.Cells.Where(x => x.Value == 0);
var scoreSum = 0;
var ratingSum = 0;
foreach (var cell in trailheads)
{
    var paths = new List<List<(int r, int c)>>();
    Traverse(cell.Cell, 0, paths, new Stack<(int r, int c)>());
    ratingSum += paths.Count();
    scoreSum += paths.Select(p => p.First()).Distinct().Count();
}

Console.WriteLine(scoreSum);
Console.WriteLine(ratingSum);
return;
void Traverse((int, int) cell, int atHeight, List<List<(int r, int c)>> paths, Stack<(int r, int c)> path)
{
    path.Push(cell);
    if (atHeight > 8)
    {
        paths.Add(path.ToArray().ToList());
        path.Pop();
        return;
    }
    var nextCells = cell.GetAdjacentStraight(m).Where(x => m.Value(x) == atHeight + 1);
    foreach (var cell1 in nextCells)
    {
        Traverse(cell1, atHeight + 1, paths, path);
    }
    path.Pop();
}

