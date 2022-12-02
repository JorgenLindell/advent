string[] lines =  System.IO.File.ReadAllLines(@"./input1.txt");
// new string[]{
//    "..##.......",
//"#...#...#..",
//".#....#..#.",
//"..#.#...#.#",
//".#...##..#.",
//"..#.##.....",
//".#.#.#....#",
//".#........#",
//"#.##...#...",
//"#...##....#",
//".#..#...#.#"
//};




long t1=TreeCount(ci: 1, ri: 1, strings: lines);
long t2 = TreeCount(ci: 3, ri: 1, strings: lines);
long t3 = TreeCount(ci: 5, ri: 1, strings: lines);
long t4 = TreeCount(ci: 7, ri: 1, strings: lines);
long t5 = TreeCount(ci: 1, ri: 2, strings: lines);

Console.WriteLine("total="+(t1*t2*t3*t4*t5));
int TreeCount(int ci, int ri, string[] strings)
{
    var c = -ci;
    var r = -ri;
    var trees = 0;

    while (true)
    {
        r += ri;
        c += ci;
        if (r >= strings.Length)
            break;
        if (c >= strings[r].Length)
            c -= strings[r].Length;
        if (strings[r][c] == '#')
            trees++;
    }

    Console.WriteLine(" ci=" + ci + "ri=" + ri +  " trees " + trees);
    return trees;
}



