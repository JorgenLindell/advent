using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace common
{
    public static class NumericsExtensions
    {
        public static Vector3 Transform(this Vector3 v, Quaternion q)
        {
            return Vector3.Transform(v, q);
        }
        public static int AddOneBasedModular(this int number, int add, int modulo) => ((number + add - 1) % modulo) + 1;

    }

    /// <summary>
    /// Decode result of CompareTo
    /// </summary>
    public static class CompareToResultExtensions
    {
        public static bool GreaterOrEqual(this int r) => r >= 0;
        public static bool SmallerOrEqual(this int r) => r <= 0;
        public static bool Smaller(this int r) => r < 0;
        public static bool Greater(this int r) => r > 0;
    }

    public static class BasesConversions
    {

        public static long BaseToLong(this string number, int radix, string? digits = null)
        {
            const string Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var digitsArr = (digits ?? Digits);

            if (radix < 2 || radix > digitsArr.Length)
                throw new ArgumentException("The radix must be >= 2 and <= " +
                                            digitsArr.Length.ToString());

            if (String.IsNullOrEmpty(number))
                return 0;

            // Make sure the arbitrary numeral system number is in upper case
            number = number.ToUpperInvariant();

            long result = 0;
            long multiplier = 1;
            for (int i = number.Length - 1; i >= 0; i--)
            {
                char c = number[i];
                if (i == 0 && c == '-')
                {
                    // This is the negative sign symbol
                    result = -result;
                    break;
                }

                int digit = digitsArr.IndexOf(c);
                if (digit == -1)
                    throw new ArgumentException(
                        "Invalid character in the arbitrary numeral system number",
                        "number");

                result += digit * multiplier;
                multiplier *= radix;
            }

            return result;
        }
        public static string LongToBase(this long decimalNumber, int radix,string? digits=null)
        {
            const int BitsInLong = 64;
            const string Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var digitsArr = (digits ?? Digits);

            if (radix < 2 || radix > digitsArr.Length)
                throw new ArgumentException("The radix must be >= 2 and <= " +
                                            digitsArr.Length.ToString());

            if (decimalNumber == 0)
                return "0";

            int index = BitsInLong - 1;
            long currentNumber = Math.Abs(decimalNumber);
            char[] charArray = new char[BitsInLong];

            while (currentNumber != 0)
            {
                int remainder = (int)(currentNumber % radix);
                charArray[index--] = digitsArr[remainder];
                currentNumber = currentNumber / radix;
            }

            string result = new String(charArray, index + 1, BitsInLong - index - 1);
            if (decimalNumber < 0)
            {
                result = "-" + result;
            }

            return result;
        }
    }
}
