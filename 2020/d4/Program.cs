
using System.Text.RegularExpressions;

var fields = new FieldValidation[]
{
   new ( "byr", x => FieldValidation.Between(x,1920,2002)),
   new ( "iyr", x => FieldValidation.Between(x,2010,2020)),
   new ( "eyr", x => FieldValidation.Between(x,2020,2030)),
   new ( "hgt", x => FieldValidation.ValidHeight(x)),
   new ( "hcl", x => FieldValidation.ValidHair(x)),
   new ( "ecl", x => FieldValidation.ValidEye(x)),
   new ( "pid", x => FieldValidation.NineDigits(x)),
   new ( "cid", x => true)
};
string lines = System.IO.File.ReadAllText(@"./input1.txt");

/*
// testData
@"ecl:gry pid:860033327 eyr:2020 hcl:#fffffd
byr:1937 iyr:2017 cid:147 hgt:183cm

iyr:2013 ecl:amb cid:350 eyr:2023 pid:028048884
hcl:#cfa07d byr:1929

hcl:#ae17e1 iyr:2013
eyr:2024
ecl:brn pid:760753108 byr:1931
hgt:179cm

hcl:#cfa07d eyr:2025 pid:166559648
iyr:2011 ecl:brn hgt:59in";*/

var pp = lines.Split("\r\n\r\n");

var allPP = ExtractPassports(pp, fields);
var valid = allPP.Sum(p => ValidPP(p, fields));

Console.WriteLine(valid);


List<Dictionary<string, string>> ExtractPassports(string[] rawStrings, FieldValidation[] fields)
{

    var result = new List<Dictionary<string, string>>();
    foreach (var rawString in rawStrings)
    {
        var str = rawString.Replace("\r\n", " ");
        var pairs = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var ppdict = new Dictionary<string, string>();
        foreach (var pair in pairs)
        {
            string[] kvp = pair.Split(':');
            if (kvp.Length == 1)
                ppdict[kvp[0]] = "";
            else
                ppdict[kvp[0]] = kvp[1];
        }
        result.Add(ppdict);
    }

    return result;
}

int ValidPP(Dictionary<string, string> pp, FieldValidation[] fields)
{
    var missing = new List<string>();
    foreach (var field in fields)
    {
        if (!pp.ContainsKey(field.Name))
            missing.Add(field.Name);
        else
        {
            if (!field.Valid(pp[field.Name]))
                missing.Add(field.Name);
        }
    }

    if (missing.Count == 0 ||
        missing.Count == 1 && missing[0] == "cid")
    {
        return 1;
    }
    return 0;
}

public class FieldValidation
{
    public readonly string Name;
    public readonly Func<string, bool> Valid;

    public FieldValidation(string name, Func<string, bool> func)
    {
        Name = name;
        Valid = func;
    }

    public static bool Between(string x, int i1, int i2)
    {
        if (int.TryParse(x, out int xi))
        {
            if (xi >= i1 && xi <= i2) return true;
        }
        return false;
    }
    public static bool ValidHeight(string x)
    {
        if (x.EndsWith("cm"))
        {
            return Between(x.Substring(0, x.Length - 2), 150, 193);
        }

        if (x.EndsWith("in"))
        {
            return Between(x.Substring(0, x.Length - 2), 59, 76);

        }

        return false;
    }

    static Regex reHair = new Regex("^#[0-9a-f]{6}$");
    public static bool ValidHair(string s)
    {
        return reHair.IsMatch(s);
    }


    static string[] colors = "amb blu brn gry grn hzl oth".Split(' ');
    public static bool ValidEye(string s)
    {
        if (s.Length != 3) return false;
        if (colors.Contains(s)) return true;
        return false;
    }

    static Regex re9Digits = new Regex("^[0-9]{9}$", RegexOptions.Singleline);

    public static bool NineDigits(string s)
    {
        return re9Digits.IsMatch(s);
    }
};
