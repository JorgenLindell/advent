// See https://aka.ms/new-console-template for more information
//: row 70, column 7, seat ID 567.
//: row 14, column 7, seat ID 119.
//: row 102, column 4, seat ID 820.
string[] lines = System.IO.File.ReadAllLines(@"./input1.txt");

//Print("BFFFBBFRRR");
//Print("FFFBBBFRRR");
//Print("BBFFBBFRLL");
//Print("FBFBBFFRLR");
//Print("BBBBBBBRRR");

var usedSeats=lines.Select(line => SeatToBinary(line)).ToHashSet();
for (int i = 0; i < 1024; i++)
{
    if (!usedSeats.Contains(i) && usedSeats.Contains(i-1) && usedSeats.Contains(i + 1))
    {
        Console.WriteLine(i);
        break;
    }
}

Console.WriteLine(lines.Max(x => SeatToBinary(x)));

void Print(string s)
{
    var seat= SeatToBinary(s);
    Console.WriteLine($"seat:{seat} row:{Row(seat)} col:{Col(seat)}");
}
static int SeatToBinary(string seat)
{
    var bStr=seat.Replace("F", "0").Replace("B", "1").Replace("R", "1").Replace("L", "0");
    var temp = Convert.ToInt32(bStr, 2);
    return temp;
}
static int Row(int seat)
{
    return (seat & 0b1111111000) >> 3;
}
static int Col(int seat)
{
    return seat & 0b111;
}
