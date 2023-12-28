using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace common
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> Pivot<T>(this IEnumerable<IEnumerable<T>> source)
        {
            var enumerators = source.Select(e => e.GetEnumerator()).ToArray();
            try
            {
                while (enumerators.All(e => e.MoveNext()))
                {
                    yield return enumerators.Select(e => e.Current).ToArray();
                }
            }
            finally
            {
                Array.ForEach(enumerators, e => e.Dispose());
            }
        }
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
        public static int TranslateVirtualIndex(this IList list, int index)
        {
             var listCount = list.Count;
           return index < 0 ? listCount + index % listCount : index % listCount;
        }
        public static T GetVirtualIndex<T>(this List<T> list, int index)
        {
            return list[list.TranslateVirtualIndex(index)];
        }
        public static IEnumerable<char> RepeatChars(this string str)
        {
            while (true)
            {
                foreach (char c in str)
                {
                    if (c == 0)
                        yield break;
                    yield return c;
                }
            }
        }

        public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> range)
        {
            foreach (var item in range)
            {
                set.Add(item);
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

        public static int SimpleLength(this System.Range range)
        {
            return range.End.Value - range.Start.Value + 1;
        }
    }
}