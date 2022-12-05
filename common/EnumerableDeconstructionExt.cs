using System;
using System.Collections.Generic;
using System.Linq;

namespace common
{

    public static class EnumerableDeconstructionExt
    {

        public static void Deconstruct<T>(this IEnumerable<T>? seq, out T first, out IEnumerable<T> rest)
        {
            if (seq == null)
            {
                throw new ArgumentNullException(nameof(seq));
            }

            // ReSharper disable PossibleMultipleEnumeration
            first = seq.FirstOrDefault()!;

            rest = seq.Skip(1);
            // ReSharper restore PossibleMultipleEnumeration
        }


        public static void Deconstruct<T>(this IEnumerable<T> seq, out T first, out T second, out IEnumerable<T> rest)
            => (first, (second, rest)) = seq;

        public static void Deconstruct<T>(this IEnumerable<T> seq, out T first, out T second, out T third, out IEnumerable<T> rest)
            => (first, second, (third, rest)) = seq;

        public static void Deconstruct<T>(this IEnumerable<T> seq, out T first, out T second, out T third, out T fourth, out IEnumerable<T> rest)
            => (first, second, third, (fourth, rest)) = seq;

        public static void Deconstruct<T>(this IEnumerable<T> seq, out T first, out T second, out T third, out T fourth, out T fifth, out IEnumerable<T> rest)
            => (first, second, third, fourth, (fifth, rest)) = seq;
        public static void Deconstruct<T>(this IEnumerable<T> seq, out T first, out T second, out T third, out T fourth, out T fifth, out T sixth, out IEnumerable<T> rest)
            => (first, second, third, fourth, fifth, (sixth, rest)) = seq;
    }
}


