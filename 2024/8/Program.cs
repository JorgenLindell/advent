using common;

var data = StreamUtils.GetLines();
//data = @"
//............
//........0...
//.....0......
//.......0....
//....0.......
//......A.....
//............
//............
//........A...
//.........A..
//............
//............
//    ".Split("\r\n".ToCharArray(), StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

VectorRc matrixMax = new VectorRc(data.Length, data[1].Length);
List<VectorRc> antinodes;
var antennas = new DictionaryWithDefault<char, List<VectorRc>>(x => new List<VectorRc>());
using (new Measure(Console.Out))
{
    char[,] matrix = new char[data.Length, data[1].Length];

    data.ForEach((x, ix) =>
    {
        x.ToCharArray()
            .ForEach((y, iy) =>
            {
                matrix[ix, iy] = y;
                if (y != '.')
                    antennas[y].Add(new VectorRc(ix, iy));
            });
    });
    antinodes = new List<VectorRc>();
    foreach (var antenna in antennas)
    {
        antinodes.AddRange(GetAntinodes(antenna, matrixMax));
    }
    antinodes = antinodes.Distinct().ToList();
    Console.WriteLine(antinodes.Count);
}

using (new Measure(Console.Out))
{

    antinodes = new List<VectorRc>();
    foreach (var antenna in antennas)
    {
        antinodes.AddRange(GetAllAntinodes(antenna, matrixMax));
    }

    antinodes = antinodes.Distinct().ToList();
    Console.WriteLine(antinodes.Count);
}

return;
List<VectorRc> GetAntinodes(KeyValuePair<char, List<VectorRc>> antennaPair, VectorRc matrixMax)
{
    var result = new List<VectorRc>();

    for (int a = 0; a < antennaPair.Value.Count; a++)
    {
        for (int b = a + 1; b < antennaPair.Value.Count; b++)
        {
            var aa = antennaPair.Value[a];
            var ab = antennaPair.Value[b];
            var c = ab - aa;
            var antinode = ab + c;
            if (antinode.Inside(0, 0, matrixMax.Col, matrixMax.Row))
                result.Add(antinode);
            antinode = aa - c;
            if (antinode.Inside(0, 0, matrixMax.Col, matrixMax.Row))
                result.Add(antinode);
        }
    }

    return result;
}
List<VectorRc> GetAllAntinodes(KeyValuePair<char, List<VectorRc>> antennaPair, VectorRc matrixMax)
{
    var result = new List<VectorRc>();

    for (int a = 0; a < antennaPair.Value.Count; a++)
    {
        for (int b = a + 1; b < antennaPair.Value.Count; b++)
        {
            var aa = antennaPair.Value[a];
            var ab = antennaPair.Value[b];
            var c = ab - aa;
            var start = aa;
            while (start.Inside(0, 0, matrixMax.Col, matrixMax.Row))
                start -= c;
            start += c;
            while (start.Inside(0, 0, matrixMax.Col, matrixMax.Row))
            {
                result.Add(start);
                start += c;
            }
        }
    }

    return result;
}

