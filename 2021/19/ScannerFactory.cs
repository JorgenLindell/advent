using System.Numerics;
using common;

namespace _19;

public class ScannerFactory
{
    public static List<Scanner> ParseScanners(TextReader stream)
    {
        var scanners = new List<Scanner>();
        var scanner = new Scanner();
        while (stream.Peek() != -1)
        {
            var line = stream.ReadLine();
            if (line?.StartsWith("--") ?? false)
            {

                var number = int.Parse(line.Split(' ')[2]);
                scanner = new Scanner(number);
                scanners.Add(scanner);
            }
            else if (!line?.IsNullOrWhiteSpace() ?? false)
            {
                var number = line.Split(",").Select(w => float.Parse(w)).ToArray();
                var coord = new Vector3(number[0], number[1], number[2]);
                scanner.AddBeacon(coord, Util3D.Rotations.Select(f => f(coord)).ToList());
            }
        }
        scanners.ForEach(s => s.CalculateDistances());
        return scanners;
    }
}