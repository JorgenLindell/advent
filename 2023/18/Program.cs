﻿using System.Diagnostics;
using System.Drawing;
using System.Text;
using common;

namespace _18;

internal class Program
{

    const StringSplitOptions Tidy = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;

    static readonly string xinput = @"
R 6 (#70c710)
D 5 (#0dc571)
L 2 (#5713f0)
D 2 (#d2c081)
R 2 (#59c680)
D 2 (#411b91)
L 5 (#8ceee2)
U 2 (#caa173)
L 1 (#1b58a2)
U 2 (#caa171)
R 2 (#7807d2)
U 3 (#a77fa3)
L 2 (#015232)
U 2 (#7a21e3)";

    static async Task Main(string[] args)
    {
        var strings = input.Split("\r\n", Tidy);
        var plan = strings
            .Select(l => l.Split(new[] { ' ', ')', '(' }, Tidy))
            .Select(s =>
                new PlanEntry(s[0], s[1].ToInt() ?? 0, s[2])).ToList();
        ExecutePlan(plan);
        var plan2 = plan.Select(p =>
        {
            var value = Convert.ToInt32(p.Color.Substring(1, 5), 16);
            var c = p.Color.Last();
            var dir = ('0' == c ? "R"
                     : '1' == c ? "D"
                     : '2' == c ? "L"
                     : '3' == c ? "U" : throw new InvalidCastException());
            Debug.WriteLine(dir + " " + value);
            return new PlanEntry(dir, value, "");
        }).ToList();
        ExecutePlan(plan2);

    }

    private static void ExecutePlan(List<PlanEntry> plan)
    {
        var current = VectorRc.Zero;
        var lengthTotal = plan.Sum(x => (long)x.Length);
        plan.ForEach(dig =>
        {
            dig.Start = current;
            current += (dig.Dir * dig.Length);
        });

        var points = plan.Select(x => x.Start).ToList();

        static long ShoelaceArea(List<VectorRc> v)
        {
            int n = v.Count;
            long a = 0;
            for (int i = 0; i < n - 1; i++)
            {
                a += (long)v[i].Row * (long)v[i + 1].Col - (long)v[i + 1].Row * (long)v[i].Col;
            }
            return Math.Abs(a + (long)v[n - 1].Row * (long)v[0].Col - (long)v[0].Row * (long)v[n - 1].Col) / 2L;
        }
        long polygonArea(List<VectorRc> v)
        {

            // Initialize area
            long area = 0;
            var n = v.Count;
            var X = v.Select(x =>(long) x.Row).ToArray();
            var Y = v.Select(x =>(long) x.Col).ToArray();
            // Calculate value of shoelace formula
            int j = n - 1;

            for (int i = 0; i < n; i++)
            {
                area +=  (long) (X[j] + X[i]) * (long) (Y[j] - Y[i]);

                // j is previous vertex to i
                j = i;
            }

            // Return absolute value
            return Math.Abs(area / 2);
        }


        Debug.WriteLine("digged:" + (ShoelaceArea(points)+(lengthTotal/2L)+1));
    }

    internal class PlanEntry
    {
        public int Length { get; }
        public VectorRc Dir { get; }
        public string Color { get; }
        public VectorRc Start { get; set; }

        public PlanEntry(string dir, int length, string color)
        {
            Length = length;
            Dir = dir == "R" ? VectorRc.Right :
                dir == "U" ? VectorRc.Up :
                dir == "D" ? VectorRc.Down :
                dir == "L" ? VectorRc.Left : throw new ArgumentOutOfRangeException(nameof(dir));
            Color = color;
        }
    }

    private static readonly string input = @"
L 4 (#6f81b2)
D 5 (#295ca1)
L 2 (#51f302)
D 6 (#295ca3)
L 11 (#604112)
U 4 (#711a63)
L 2 (#113be0)
U 7 (#6d3273)
L 7 (#2180d0)
U 2 (#51c613)
L 10 (#0e0770)
U 2 (#10e181)
L 3 (#485450)
U 10 (#83df31)
L 3 (#1ea290)
U 4 (#235d31)
L 2 (#09ca50)
U 6 (#06daa1)
L 6 (#703070)
U 2 (#3c56a3)
L 3 (#20af60)
U 4 (#152273)
L 6 (#880cb2)
U 5 (#451b63)
L 3 (#880cb0)
U 5 (#48c2b3)
R 4 (#397f70)
U 4 (#069351)
R 4 (#0fe520)
U 10 (#7f2111)
R 4 (#0fe522)
D 8 (#1d4c21)
R 2 (#4a1650)
D 6 (#3966e3)
R 5 (#00cba0)
U 7 (#212d93)
R 8 (#273670)
U 3 (#6c6c51)
R 7 (#1bae00)
U 9 (#1053e1)
R 5 (#4d2030)
U 2 (#7cc033)
R 6 (#1659d0)
U 4 (#447301)
R 5 (#611880)
U 11 (#25c2b1)
R 5 (#0ed680)
U 5 (#7b67d1)
R 2 (#0ed682)
U 7 (#092ab1)
R 5 (#4eb230)
U 3 (#559ba3)
L 5 (#103b62)
U 7 (#40fb43)
R 5 (#103b60)
U 9 (#583153)
R 4 (#4cc440)
D 6 (#4c2dd3)
R 7 (#02e430)
U 7 (#2545d3)
R 3 (#5edcf0)
U 3 (#79e7f3)
R 5 (#10ccc0)
D 10 (#6d8b11)
R 3 (#3f7ce0)
D 3 (#240201)
R 6 (#23cec0)
D 5 (#59ce81)
L 6 (#370b30)
D 4 (#104291)
L 9 (#320380)
U 4 (#458441)
L 6 (#9263f2)
D 6 (#190861)
L 3 (#9263f0)
D 6 (#444301)
R 5 (#289070)
D 3 (#096fd1)
R 9 (#79ee30)
D 6 (#22e183)
L 9 (#08b990)
D 6 (#895df3)
R 5 (#164c60)
D 4 (#143041)
R 4 (#4d7220)
D 5 (#7c1e61)
R 5 (#1ad4f0)
U 5 (#0fc2e1)
R 6 (#684712)
D 8 (#012521)
R 3 (#183810)
D 6 (#387cf1)
L 4 (#070d90)
D 6 (#1cdc61)
L 6 (#3345b0)
U 7 (#46fa61)
L 4 (#0c1060)
D 7 (#38a731)
L 5 (#88f420)
U 6 (#2aae11)
L 3 (#4aee92)
U 4 (#2f2bc1)
L 5 (#2dd012)
D 4 (#2f2bc3)
L 3 (#4f8b92)
D 6 (#0637f1)
L 8 (#3fa330)
D 9 (#433fa1)
L 6 (#4ecdf0)
D 8 (#1e3aa1)
L 6 (#847120)
D 7 (#4673c3)
R 4 (#014b10)
D 7 (#836133)
R 8 (#259bd0)
D 4 (#155de3)
R 7 (#685880)
D 4 (#0c3403)
R 6 (#45ca80)
U 6 (#5d3e63)
R 6 (#3314b2)
U 5 (#210823)
R 7 (#3c6db2)
U 4 (#65ddf3)
R 8 (#6f8260)
D 5 (#42dde3)
R 4 (#037972)
D 8 (#24cd91)
R 4 (#5c2e72)
U 3 (#24cd93)
R 4 (#4e7b22)
U 9 (#066b33)
R 2 (#040360)
U 2 (#3d4c13)
R 5 (#8465c0)
U 5 (#2c64a3)
R 2 (#14ad02)
U 6 (#356233)
L 5 (#4ca262)
U 6 (#4acd83)
R 5 (#202ce2)
U 10 (#4579b3)
R 3 (#3fe2f2)
D 5 (#205603)
R 4 (#3319b2)
D 4 (#692d23)
R 5 (#2c7bd2)
D 5 (#68d661)
L 5 (#3742b2)
D 5 (#0fe0c1)
R 5 (#452892)
D 8 (#78b723)
R 3 (#37a0d2)
U 14 (#3768e3)
R 4 (#270100)
D 2 (#38ff13)
R 6 (#3dcc72)
D 3 (#2e1723)
L 6 (#3dcc70)
D 6 (#2cdba3)
R 8 (#4cdfd0)
D 5 (#17ca31)
L 8 (#419790)
D 5 (#035321)
R 6 (#397212)
D 5 (#914b21)
R 7 (#397210)
U 3 (#14f231)
R 5 (#5a4a50)
U 2 (#34d9c3)
R 9 (#320cf2)
U 6 (#626173)
L 6 (#320cf0)
U 2 (#2a1f73)
L 8 (#3a33c0)
U 5 (#2ba8e1)
R 2 (#68d670)
U 8 (#528601)
R 5 (#0ae4b0)
U 2 (#15c2f1)
R 5 (#02a230)
U 10 (#6338d3)
R 6 (#078062)
D 4 (#72c223)
R 2 (#5846a2)
D 5 (#39cd03)
R 11 (#464952)
U 3 (#660903)
R 7 (#34bd02)
U 9 (#2a59c3)
R 7 (#751042)
U 8 (#562a93)
R 7 (#0a81a2)
U 4 (#4d15f3)
R 6 (#67cc92)
U 6 (#4d15f1)
L 3 (#723672)
U 6 (#0566d1)
L 6 (#6cf2f2)
U 2 (#582a11)
L 4 (#74a212)
U 11 (#103141)
R 6 (#287b52)
U 4 (#84f181)
R 7 (#2eb650)
D 8 (#903541)
R 8 (#2eb652)
D 3 (#103391)
R 3 (#157392)
D 3 (#0789b3)
R 7 (#1d07a2)
D 6 (#61e393)
R 4 (#7881d2)
D 2 (#3f5893)
R 7 (#286722)
D 6 (#381f83)
R 8 (#4a7da2)
D 2 (#463143)
R 4 (#0b4032)
D 7 (#7e9143)
L 6 (#355d82)
D 2 (#02aae3)
L 4 (#11bbe2)
U 4 (#67fa03)
L 10 (#11bbe0)
D 4 (#3e2733)
L 5 (#407c92)
D 2 (#04db23)
L 5 (#386842)
D 8 (#4d5491)
R 6 (#4e3752)
D 4 (#4d5493)
R 6 (#409f12)
U 7 (#873c01)
R 7 (#4d0672)
U 2 (#873c03)
R 3 (#238232)
D 5 (#2ec8d3)
L 6 (#1a84c0)
D 6 (#4e0e23)
R 6 (#207ac0)
D 3 (#234f73)
R 6 (#257662)
D 5 (#665b03)
L 9 (#257660)
D 3 (#1154a3)
L 3 (#1a2570)
D 7 (#08f133)
R 6 (#7489d0)
D 11 (#0d29d1)
R 4 (#3b6d72)
U 5 (#5ad2a1)
R 8 (#4f1132)
U 5 (#021471)
R 4 (#12f0e0)
U 2 (#1b0d21)
R 4 (#4a05e0)
U 8 (#4d8d53)
R 4 (#69d810)
U 3 (#4d8d51)
R 5 (#168f40)
U 3 (#3b30b1)
R 8 (#5dc4b2)
U 8 (#289f01)
L 10 (#7f9962)
U 7 (#04d161)
L 4 (#8a7ea0)
U 4 (#043f51)
L 3 (#7d8e00)
U 3 (#1542d3)
R 8 (#13a2b0)
U 9 (#50e003)
L 6 (#105580)
U 4 (#159251)
L 11 (#536490)
U 3 (#69cae1)
R 10 (#1d0b70)
U 2 (#27dc61)
R 3 (#2e41c0)
U 6 (#01a1a1)
R 4 (#8dfc90)
U 7 (#2c9351)
L 8 (#166b42)
U 3 (#6e2221)
L 4 (#5b1e42)
U 7 (#6e2223)
L 5 (#3232c2)
U 4 (#520c71)
R 9 (#2d1100)
U 9 (#45aaa1)
R 8 (#76ab40)
D 8 (#2ca591)
R 8 (#8dfc92)
D 7 (#21f501)
R 6 (#49ca60)
D 8 (#887f81)
R 9 (#48c220)
D 3 (#887f83)
L 9 (#1d4540)
D 7 (#64f8f3)
R 6 (#29f2f0)
D 3 (#3672b3)
R 4 (#5b6a30)
D 9 (#47a913)
R 3 (#5b6a32)
D 4 (#1c77c3)
R 4 (#29f2f2)
U 13 (#14fa23)
R 5 (#56ead0)
D 5 (#258123)
R 11 (#6e74d0)
D 5 (#258143)
R 2 (#1127e0)
D 4 (#536d63)
R 4 (#7f9cb2)
D 2 (#08c9d3)
R 12 (#11c220)
D 4 (#0d4613)
R 4 (#1117c0)
U 6 (#63d6e3)
R 7 (#079410)
D 6 (#140c73)
R 4 (#518310)
D 2 (#2f5813)
R 8 (#3d38b0)
D 8 (#33bca3)
R 4 (#2fc890)
D 3 (#199091)
R 4 (#3c9a40)
D 6 (#199093)
L 12 (#671c60)
D 6 (#6314b1)
L 3 (#0ddda0)
D 6 (#2625c1)
L 8 (#67eb80)
D 4 (#5f03a1)
L 4 (#402810)
D 6 (#6622d1)
L 6 (#3c8210)
D 5 (#1f1501)
L 4 (#386720)
U 4 (#5e9f81)
L 3 (#4658b0)
U 7 (#50aee1)
L 6 (#3b36b0)
U 4 (#2d3bf1)
L 6 (#485710)
D 7 (#5b0341)
L 2 (#826220)
D 5 (#299251)
L 7 (#6d7080)
D 4 (#20d6f1)
L 7 (#547000)
D 4 (#7ded11)
R 9 (#19bc70)
D 3 (#119381)
R 5 (#28b900)
D 6 (#2a6a61)
L 6 (#11d062)
D 6 (#6de691)
L 8 (#11d060)
U 4 (#20cb01)
L 3 (#44ffd0)
U 12 (#5555e3)
L 5 (#567c32)
U 2 (#24a913)
L 3 (#567c30)
D 5 (#3f1d03)
L 8 (#6cc840)
D 2 (#4f6121)
L 4 (#427f10)
D 5 (#662b11)
R 12 (#274300)
D 3 (#4d3db3)
L 5 (#8269d0)
D 3 (#0a29d3)
L 4 (#8269d2)
D 4 (#4cad33)
R 9 (#5897b0)
D 7 (#07e1b1)
R 3 (#3b3d00)
D 4 (#502e91)
R 8 (#2afc70)
D 5 (#4c0471)
L 4 (#0abae0)
D 2 (#7d3af1)
L 9 (#085c40)
D 3 (#294e31)
L 3 (#5af032)
D 6 (#15b561)
R 6 (#3d5552)
D 4 (#538691)
R 5 (#1ef0e2)
U 4 (#4ad471)
R 5 (#365c92)
D 3 (#0b4651)
R 3 (#165bf2)
D 6 (#7969a1)
R 5 (#258320)
U 6 (#3eac11)
R 4 (#7f1160)
U 6 (#3bbb51)
R 2 (#2d4ac0)
U 8 (#7a6763)
R 8 (#320fa0)
U 5 (#497d21)
R 3 (#4affb0)
U 11 (#60bfa1)
R 7 (#4affb2)
D 4 (#058041)
R 9 (#085c42)
D 6 (#7b9b61)
R 7 (#1b6382)
D 7 (#69d101)
R 8 (#08bdd2)
D 3 (#7923f1)
L 5 (#0c8052)
D 12 (#047191)
L 6 (#217442)
D 2 (#12c433)
L 8 (#6a7582)
D 4 (#5c3823)
L 8 (#5fd572)
D 2 (#1b1723)
L 6 (#5fd570)
D 4 (#5d5313)
L 3 (#3443a2)
D 6 (#3865d1)
L 8 (#5d30a2)
D 5 (#528a23)
R 9 (#64c9c2)
D 2 (#7b78e3)
R 6 (#31c952)
U 12 (#476ac3)
R 6 (#57aeb2)
D 7 (#537b03)
R 2 (#1447f2)
D 5 (#582521)
R 5 (#12d112)
D 7 (#4424c1)
L 9 (#5c3792)
D 7 (#53d2c1)
L 3 (#5c3790)
D 10 (#519051)
L 7 (#218392)
U 10 (#273bd1)
L 6 (#2d6cd2)
D 5 (#4ec1d3)
L 3 (#8492b2)
D 5 (#4ec1d1)
L 3 (#171a22)
D 3 (#0002d1)
L 8 (#634852)
U 2 (#0ee1d1)
L 6 (#3d2eb2)
U 3 (#04ea01)
R 4 (#607f20)
U 9 (#7e4251)
R 2 (#607f22)
U 4 (#0fa471)
R 3 (#2d0682)
U 12 (#4fbb41)
L 3 (#005e72)
U 6 (#4c5b61)
L 6 (#7f5b72)
U 10 (#33e641)
L 7 (#4240a2)
D 3 (#3c80b1)
L 4 (#24d372)
D 9 (#3298d1)
L 6 (#055d42)
D 4 (#41a9c3)
L 6 (#4ace92)
D 3 (#41a9c1)
L 5 (#49d532)
D 8 (#2d69e1)
L 5 (#11cb42)
D 9 (#6343c1)
L 5 (#5aec92)
U 8 (#727291)
L 7 (#575b02)
U 8 (#2d9493)
L 5 (#29b0e2)
U 5 (#773de3)
L 6 (#448ef2)
U 4 (#30b9c3)
L 6 (#453232)
U 4 (#48bc11)
L 2 (#3e79d2)
U 11 (#7ccff1)
L 3 (#5516d2)
D 7 (#3596c1)
L 8 (#0a44d2)
D 2 (#363c41)
L 6 (#83a172)
D 4 (#23f181)
L 11 (#8de640)
D 4 (#312f31)
L 6 (#7ace72)
D 4 (#2dc461)
L 6 (#1a77a2)
D 7 (#215151)
L 6 (#8a6f80)
D 3 (#41f301)
R 3 (#0c6ef0)
D 7 (#41f303)
R 9 (#4b0960)
D 6 (#5ab701)
R 12 (#46eee0)
D 5 (#0cf881)
R 5 (#4175a2)
D 8 (#08d7b1)
L 6 (#576f32)
U 4 (#5b2e81)
L 3 (#1609b2)
D 4 (#548b91)
L 7 (#653f72)
D 3 (#503443)
R 5 (#42aae2)
D 5 (#5f85d3)
R 5 (#0efc42)
D 3 (#023203)
R 7 (#4ce242)
D 3 (#914443)
R 5 (#379b32)
D 8 (#914441)
L 7 (#23df42)
D 3 (#5f0713)
L 10 (#181532)
D 3 (#0caa31)
L 5 (#25dc72)
D 7 (#1f7ef3)
L 6 (#43f6d2)
D 3 (#1f7ef1)
L 10 (#39ee62)
U 6 (#0caa33)
L 3 (#729962)
U 8 (#08e3b3)
L 9 (#4abe22)
U 6 (#11eca3)
L 9 (#251700)
U 4 (#677653)
L 3 (#75e930)
U 4 (#5a9583)
L 6 (#64e040)
U 7 (#0d2d03)
R 9 (#723e00)
U 9 (#44b093)
L 6 (#070fe0)
U 2 (#42d003)
L 12 (#5dfdd0)
U 6 (#16e5d3)
L 4 (#4a5ee0)
U 4 (#5c7403)
L 5 (#623452)
U 9 (#653f33)
L 7 (#0983c2)
U 10 (#2b6293)
R 5 (#1eba12)
U 12 (#3f8d01)
R 3 (#516e32)
D 9 (#5114c1)
R 8 (#257082)
D 3 (#3c6e33)
R 7 (#541502)
U 6 (#502401)
R 5 (#3d0322)
U 6 (#4eec41)
R 6 (#74a502)
U 8 (#3a4fc3)
L 3 (#29c2f2)
U 3 (#572513)
L 11 (#780f92)
U 4 (#0d9b73)
L 2 (#321ee2)
U 3 (#6d7933)
L 5 (#614562)
U 6 (#4bdbd3)
L 12 (#614560)
U 3 (#3facd3)
L 4 (#4c7bd0)
D 8 (#58d343)
L 4 (#7d48b0)
D 4 (#05c741)
L 4 (#2d8730)
U 4 (#6964f1)
L 4 (#68bdd0)
U 8 (#780801)
L 8 (#3bbd00)
U 4 (#0296e1)
L 6 (#0d0280)
U 3 (#3094e3)
L 8 (#2581a0)
U 8 (#6b7e33)
L 5 (#2581a2)
U 3 (#4db803)
L 7 (#30e580)
U 5 (#4f8f73)
L 2 (#6ec3c2)
U 8 (#340063)
L 4 (#1428b2)
U 3 (#4810c3)";

}