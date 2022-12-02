using System.Diagnostics;

namespace _16;

public class Program
{
    private static readonly string[] _testData =
    {
            "target area: x=20..30, y=-10..-5",
            "target area: x=88..125, y=-157..-103"
        };

    public static Rect ParseRect(string inp)
    {
        var split = inp.Split(":=,.".ToCharArray(), StringSplitOptions.TrimEntries);
        var x1 = int.Parse(split[2]);
        var x2 = int.Parse(split[4]);
        var y1 = int.Parse(split[6]);
        var y2 = int.Parse(split[8]);
        return new Rect(x1, x2, y1, y2);
    }

    private static void Main()
    {
        int FindZeroXSpeed((int x, int y) speed)
        {
            var endX = -1;
            var reached = 0;
            while (speed.x > 0)
            {
                reached += speed.x;
                endX++;
                speed.x += speed.x > 0 ? -1 : 0;
            }

            return reached;
        }

        var target = ParseRect(_testData[1]);

        var startSpeeds = (x: 0, y: 0);
        var minXSpeed = -1;
        var maxXSpeed = int.MaxValue;

        ++startSpeeds.x;
        var endX = -1;
        while (endX < target.Left)
        {
            startSpeeds.x++;
            var speed = startSpeeds;
            endX = FindZeroXSpeed(speed);
        }

        minXSpeed = startSpeeds.x;
        endX = -1;
        while (endX < target.Right)
        {
            startSpeeds.x++;
            var speed = startSpeeds;
            endX = FindZeroXSpeed(speed);
        }
        maxXSpeed = target.Right;

        var maxYSpeed = -target.Bottom;

        var minYSpeed = -1000;
        startSpeeds.y = 2;
        startSpeeds.x = minXSpeed+ 1;
        while (minYSpeed == -1000 && startSpeeds.y < maxYSpeed)
        {
            var missile = new Point(0, 0);
            startSpeeds.y++;
            var speeds = startSpeeds;
            while (missile.y >= target.Bottom)
            {
                missile = Trajectory.Move(missile, ref speeds);
                Debug.WriteLine(missile.ToString());
                if (target.InsideX(missile) && target.InsideY(missile))
                {
                    minYSpeed = startSpeeds.y;
                    break;
                }
            }
        }

        minYSpeed = target.Bottom;

        var tr = new Trajectory();
        var goodSpeeds = new List<(int x, int y)>();
        var nogoodSpeeds = new List<(int x, int y)>();
        var screenshots = new Dictionary<(int x, int y), string>();
        for (int xSpeed = minXSpeed-1; xSpeed < maxXSpeed+1 ; xSpeed++)
        {
            for (int ySpeed = minYSpeed-1; ySpeed < maxYSpeed+1 ; ySpeed++)
            {
                var speeds = (xSpeed, ySpeed);
                var res = tr.Shoot(speeds, target);
                if (res == Trajectory.ShotResult.Hit)
                {
                    goodSpeeds.Add(speeds);
                    //var screen = tr.LastShotScreen();
                    //screenshots.Add(speeds,"");
                }
                else
                {
                    nogoodSpeeds.Add(speeds);
                    //var screen = tr.LastShotScreen();
                    //screenshots.Add(speeds,"");
                }
            }
        }

        Console.WriteLine("Missing Speeds");
        foreach (var speed in nogoodSpeeds)
        {
            Console.Write($"({speed.x},{speed.y}) ");
        }

        Console.WriteLine("\n");
        Console.WriteLine("Good Speeds");
        foreach (var speed in goodSpeeds)
        {
            Console.Write($"({speed.x},{speed.y}) ");
        }
        Console.WriteLine("\n");

        Console.WriteLine("Number of good speeds: "+goodSpeeds.Count);


    }
}
