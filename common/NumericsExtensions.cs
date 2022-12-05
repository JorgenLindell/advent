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
}
