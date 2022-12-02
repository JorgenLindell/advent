using System.Net;
using System.Text;

namespace _16;

public class Trajectory
{
    public enum ShotResult
    {
        Hit,
        Short,
        Long,
        SpeedY,
        SpeedX,
        Speed,
        InsideX
    }

    private Rect lastTarget;

    public Point Missile;

    public string LastShotScreen()
    {
        var r = GetLastRect();
        var offsetX = -r.Left;
        var offsetY = -r.Bottom;
        var screen = new XYCoordSystem<char>(r.Width, r.Height, offsetX, offsetY);
        for (int x = r.Left; x < r.Right; x++)
        {
            for (int y = r.Bottom; y < r.Top; y++)
            {
                screen[x, y] = (char)'.';
            }
        }


        lastTarget.ForEachPoint(p =>
        {
            screen[p.x, p.y] = 'T';

        });
        LastShot.ForEach(p =>
        {
            screen[p.p.x, p.p.y] = '#';
        });
        screen[0, 0] = 'S';

        var sb = new StringBuilder();
        var height = screen.Matr.GetLength(1);
        for (int ci = 0; ci < height; ci++)
        {
            var width = screen.Matr.GetLength(0);
            for (int ri = 0; ri < width; ri++)
            {
                sb.Append($" {"" + screen.Matr[ri, ci]} ");
            }
            sb.AppendLine();

        }

        return sb.ToString();
    }

    public List<(Point p, int v)> LastShot { get; set; } = new();


    public ShotResult Shoot((int xSpeed, int ySpeed) speed, Rect target)
    {
        lastTarget = target;
        LastShot = new List<(Point p, int v)>();
        Missile = new Point(0, 0);
        LastShot.Add((Missile, speed.ySpeed));
        var result = ShotResult.Short;
        var lastResult = result;
        var startSpeed = speed.ySpeed;
        while (Missile.y > target.Bottom)
        {
            var pBefore = Missile;
            Missile = Move(Missile,ref speed);
            LastShot.Add((Missile, speed.ySpeed));
            if (target.Inside(Missile))
                return ShotResult.Hit;

            if (Missile.x < target.Left)
            {
                result = ShotResult.Short;
            }

            if (Missile.x > target.Right)
                result = ShotResult.Long;

            if (pBefore.x >= target.Left
                && Missile.x <= target.Right)
                result = ShotResult.InsideX;

            if (lastResult == ShotResult.Short
                && result == ShotResult.Long
                && pBefore.y < target.Top
                && Missile.y > target.Bottom)
                return ShotResult.SpeedX;

            if (lastResult == ShotResult.InsideX
                && result == ShotResult.InsideX
                && pBefore.y > target.Top
                && Missile.y < target.Bottom)
                return ShotResult.SpeedY;
            //   if (target.IntersectsWithLine(new Line(pBefore, Missile)))
            //
            //       return ShotResult.Speed;

            lastResult = result;
        }

        return result;

    }
    public static Point Move(Point missilePoint, ref (int xSpeed, int ySpeed) speed)
    {
        /* 
The probe's x position increases by its x velocity.
The probe's y position increases by its y velocity.
Due to drag, the probe's x velocity changes by 1 toward the value 0; that is,
        it decreases by 1 if it is greater than 0, 
        increases by 1 if it is less than 0, or does not change if it is already 0.
Due to gravity, the probe's y velocity decreases by 1.
        */
        missilePoint = new Point(missilePoint.x + speed.xSpeed, missilePoint.y + speed.ySpeed);
        speed.xSpeed += speed.xSpeed > 0 ? -1 : speed.xSpeed < 0 ? +1 : 0;
        speed.ySpeed -= 1;
        return missilePoint;
    }

    public Rect GetLastRect()
    {
        var lastShot = LastShot.ToList();
        lastShot.Add((new Point(lastTarget.Right, lastTarget.Bottom), 0));
        var minx = lastShot.Min(p => p.p.x) - 1;
        var maxx = lastShot.Max(p => p.p.x) + 1;
        var miny = lastShot.Min(p => p.p.y) - 1;
        var maxy = lastShot.Max(p => p.p.y) + 1;
        return new Rect(minx - 1, maxx + 1, miny - 1, maxy + 1);
    }
}