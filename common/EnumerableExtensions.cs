using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace common
{
    public static class EnumerableExtensions
    {
        public static bool In<T>(this T src, params T[] elems)
        {
            return elems.Contains(src);
        }
        public static void ForEach<T>(this IEnumerable<T> src, Action<T, int> action)
        {
            int i = 0;
            foreach (T value in src)
            {
                action(value, i);
                ++i;
            }
        }
        public static bool In<T>(this T src, IEnumerable<T> elems)
        {
            return elems.Contains(src);
        }

        public static List<List<T>> ToBatches<T>(this IEnumerable<T> enumerable, int batchSize)
        {
            if (batchSize < 1)
            {
                throw new ArgumentException("Batch size must be greater than 0.");
            }

            var original = enumerable as IList<T> ?? enumerable.ToList();
            var list = new List<List<T>>();

            for (var startIndex = 0; startIndex < original.Count; startIndex += batchSize)
            {
                list.Add(original.Skip(startIndex).Take(batchSize).ToList());
            }

            return list;
        }
        public static List<List<T>> SplitToLists<T>(this List<T> list, int listcount)
        {
            if (listcount <= 0)
                throw new ArgumentException("listCount must be greater than 0.");
            int chunkSize = (int)Math.Ceiling((double)list.Count / (double)listcount);
            return ToBatches(list, chunkSize);
        }

        public static Stack<T> CloneStack<T>(this Stack<T> original)
        {
            var arr = new T[original.Count];
            original.CopyTo(arr, 0);
            Array.Reverse(arr);
            return new Stack<T>(arr);
        }
        public static IEnumerable<int> Range(int a, int b)
        {
            if (a <= b)
            {
                for (int i = a; i <= b; i++)
                {
                    yield return i;
                }
            }
            else /* b < a */
            {
                for (int i = a; i >= b; i--)
                {
                    yield return i;
                }
            }
        }
    }

    public static class ZipExtension
    {
        public static IEnumerable<(T1, T2)> Zip<T1, T2>(this IEnumerable<T1> t1, IEnumerable<T2> t2)
        {
            using var t1E = t1.GetEnumerator();
            using var t2E = t2.GetEnumerator();
            while (t1E.MoveNext() && t2E.MoveNext())
                yield return (t1E.Current, t2E.Current);
        }
        public static IEnumerable<(T1, T2, T3)> Zip<T1, T2, T3>(this IEnumerable<T1> t1, IEnumerable<T2> t2, IEnumerable<T3> t3)
        {
            using var t1E = t1.GetEnumerator();
            using var t2E = t2.GetEnumerator();
            using var t3E = t3.GetEnumerator();
            while (t1E.MoveNext() && t2E.MoveNext() && t3E.MoveNext())
                yield return (t1E.Current, t2E.Current, t3E.Current);
        }
        public static IEnumerable<(T1, T2, T3, T4)> Zip<T1, T2, T3, T4>(this IEnumerable<T1> t1, IEnumerable<T2> t2, IEnumerable<T3> t3, IEnumerable<T4> t4)
        {
            using var t1E = t1.GetEnumerator();
            using var t2E = t2.GetEnumerator();
            using var t3E = t3.GetEnumerator();
            using var t4E = t4.GetEnumerator();
            while (t1E.MoveNext() && t2E.MoveNext() && t3E.MoveNext() && t4E.MoveNext())
                yield return (t1E.Current, t2E.Current, t3E.Current, t4E.Current);
        }
        public static IEnumerable<(T1, T2, T3, T4, T5)> Zip<T1, T2, T3, T4, T5>(this IEnumerable<T1> t1, IEnumerable<T2> t2,
            IEnumerable<T3> t3, IEnumerable<T4> t4, IEnumerable<T5> t5)
        {
            using var t1E = t1.GetEnumerator();
            using var t2E = t2.GetEnumerator();
            using var t3E = t3.GetEnumerator();
            using var t4E = t4.GetEnumerator();
            using var t5E  = t5.GetEnumerator(); ;
            while (t1E.MoveNext() && t2E.MoveNext() && t3E.MoveNext() && t4E.MoveNext())
            {
                yield return (t1E.Current, t2E.Current, t3E.Current, t4E.Current,t5E.Current);
            }
        }
    }

    public static class PermutationExtension
    {
        public static IEnumerable<IEnumerable<T>> GetPermutations<T>(this IEnumerable<T> enumerable)
        {
            var array = enumerable as T[] ?? enumerable.ToArray();

            var factorials = Enumerable.Range(0, array.Length + 1)
                .Select(Factorial)
                .ToArray();

            for (var i = 0L; i < factorials[array.Length]; i++)
            {
                var sequence = GenerateSequence(i, array.Length - 1, factorials);

                yield return GeneratePermutation(array, sequence);
            }
        }

        private static IEnumerable<T> GeneratePermutation<T>(T[] array, IReadOnlyList<int> sequence)
        {
            var clone = (T[])array.Clone();

            for (int i = 0; i < clone.Length - 1; i++)
            {
                Swap(ref clone[i], ref clone[i + sequence[i]]);
            }

            return clone;
        }

        private static int[] GenerateSequence(long number, int size, IReadOnlyList<long> factorials)
        {
            var sequence = new int[size];

            for (var j = 0; j < sequence.Length; j++)
            {
                var facto = factorials[sequence.Length - j];

                sequence[j] = (int)(number / facto);
                number = (int)(number % facto);
            }

            return sequence;
        }

        static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }

        private static long Factorial(int n)
        {
            long result = n;

            for (int i = 1; i < n; i++)
            {
                result = result * i;
            }

            return result;
        }
    }
}