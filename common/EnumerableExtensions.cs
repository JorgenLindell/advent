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
}