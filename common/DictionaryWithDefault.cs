using System;
using System.Collections.Generic;

namespace common
{



    /// <summary>
    /// Implements a dictionary that takes a lambda in the constructor to create a default element when you try to address a non existing key.
    /// </summary>
    /// <remarks>
    ///     Useful to avoid testing .ContainsKey
    /// </remarks>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class DictionaryWithDefault<TKey, TValue> : Dictionary<TKey, TValue>
    {
        private readonly Func<TKey, TValue> _defaultValue = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="func"></param>
        public DictionaryWithDefault(Func<TKey, TValue> func)
        {
            this._defaultValue = func;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="func"></param>
        /// <param name="comparer"> </param>
        public DictionaryWithDefault(Func<TKey, TValue> func, IEqualityComparer<TKey> comparer) :
            base(comparer)
        {
            this._defaultValue = func;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="func"></param>
        /// <param name="other"></param>
        public DictionaryWithDefault(Func<TKey, TValue> func, IDictionary<TKey, TValue> other)
            : base(other)
        {
            this._defaultValue = func;
            foreach (var vp in other)
                this[vp.Key] = vp.Value;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="func"></param>
        /// <param name="other"></param>
        public DictionaryWithDefault(Func<TKey, TValue> func, IEnumerable<KeyValuePair<TKey, TValue>> other)
        {
            this._defaultValue = func;
            foreach (var vp in other)
                this[vp.Key] = vp.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="func"></param>
        /// <param name="other"></param>
        /// <param name="comparer"> </param>
        public DictionaryWithDefault(Func<TKey, TValue> func, IEnumerable<KeyValuePair<TKey, TValue>> other, IEqualityComparer<TKey> comparer)
            : base(comparer)
        {
            this._defaultValue = func;
            foreach (var vp in other)
                this[vp.Key] = vp.Value;
        }

        /// <summary>
        ///  Accessor. Inserts a constructed default object if trying to access a non existing element.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual new TValue this[TKey key]
        {
            get
            {
                if (this.ContainsKey(key))
                    return base[key];
                if (typeof(TValue).IsValueType)
                    return _defaultValue(key);
                base[key] = this._defaultValue(key);
                return base[key];
            }
            set { base[key] = value; }
        }


    }

    /// <summary>
    /// Extensions for DictionaryWithDefaultExtensions
    /// </summary>
    public static class DictionaryWithDefaultExtensions
    {
        private static class IdentityFunction<TElement>
        {
            public static Func<TElement, TElement> Instance
            {
                get { return x => x; }
            }
        }

        /// <summary>
        /// extension to convert IEnumerable to DictionaryWithDefault
        /// </summary>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <param name="defaultFunc"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <returns></returns>
        public static DictionaryWithDefault<TKey, TSource> ToDictionaryWithDefault<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, TSource> defaultFunc)
        {
            return ToDictionaryWithDefault<TSource, TKey, TSource>(source, keySelector, IdentityFunction<TSource>.Instance, null);
        }

        /// <summary>
        /// extension to convert IEnumerable to DictionaryWithDefault
        /// </summary>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <param name="defaultFunc"></param>
        /// <param name="comparer"> </param>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <returns></returns>
        public static DictionaryWithDefault<TKey, TSource> ToDictionaryWithDefault<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, TSource> defaultFunc, IEqualityComparer<TKey> comparer)
        {
            return ToDictionaryWithDefault<TSource, TKey, TSource>(source, keySelector, IdentityFunction<TSource>.Instance, defaultFunc, comparer);
        }

        /// <summary>
        /// extension to convert IEnumerable to DictionaryWithDefault
        /// </summary>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <param name="elementSelector"> </param>
        /// <param name="defaultFunc"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TElement"> </typeparam>
        /// <returns></returns>
        public static DictionaryWithDefault<TKey, TElement> ToDictionaryWithDefault<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, TElement> defaultFunc)
        {
            return ToDictionaryWithDefault<TSource, TKey, TElement>(source, keySelector, elementSelector, defaultFunc, null);
        }

        /// <summary>
        /// extension to convert IEnumerable to DictionaryWithDefault
        /// </summary>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <param name="elementSelector"> </param>
        /// <param name="defaultFunc"></param>
        /// <param name="comparer"> </param>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TElement"> </typeparam>
        /// <returns></returns>
        public static DictionaryWithDefault<TKey, TElement> ToDictionaryWithDefault<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, TElement> defaultFunc, IEqualityComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (keySelector == null) throw new ArgumentNullException("keySelector");
            if (elementSelector == null) throw new ArgumentNullException("elementSelector");
            if (defaultFunc == null)
            {
                defaultFunc = (key => default(TElement));
            }
            DictionaryWithDefault<TKey, TElement> d = new DictionaryWithDefault<TKey, TElement>(defaultFunc, comparer);
            foreach (TSource element in source)
                d[keySelector(element)] = elementSelector(element);
            return d;
        }

    }

    /// <summary>
    ///  dictionary of dictionary of value
    /// </summary>
    /// <typeparam name="TKey1"></typeparam>
    /// <typeparam name="TKey2"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class DictionaryWithDefault<TKey1, TKey2, TValue> : DictionaryWithDefault<TKey1, DictionaryWithDefault<TKey2, TValue>>
    {
        /// <summary>
        ///  dictionary of dictionary of dictionary of value
        /// </summary>
        /// <param name="func"></param>
        public DictionaryWithDefault(Func<TKey2, TValue> func)
            : base(k => new DictionaryWithDefault<TKey2, TValue>(func))
        {
        }

        public bool ContainsKey(TKey1 key1, TKey2 key2)
        {
            if (!this.ContainsKey(key1))
                return false;
            return this[key1].ContainsKey(key2);
        }
    }

    /// <summary>
    ///  dictionary of dictionary of dictionary of value
    /// </summary>
    /// <typeparam name="TKey1"></typeparam>
    /// <typeparam name="TKey2"></typeparam>
    /// <typeparam name="TKey3"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class DictionaryWithDefault<TKey1, TKey2, TKey3, TValue> : DictionaryWithDefault<TKey1, TKey2, DictionaryWithDefault<TKey3, TValue>>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="func"></param>
        public DictionaryWithDefault(Func<TKey3, TValue> func)
            : base(k => new DictionaryWithDefault<TKey3, TValue>(func))
        {
        }
        public bool ContainsKey(TKey1 key1, TKey2 key2, TKey3 key3)
        {
            if (!this.ContainsKey(key1, key2))
                return false;
            return this[key1][key2].ContainsKey(key3);
        }
    }

    /// <summary>
    ///  dictionary of dictionary of dictionary of dictionary of value
    /// </summary>
    /// <typeparam name="TKey1"></typeparam>
    /// <typeparam name="TKey2"></typeparam>
    /// <typeparam name="TKey3"></typeparam>
    /// <typeparam name="TKey4"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class DictionaryWithDefault<TKey1, TKey2, TKey3, TKey4, TValue> : DictionaryWithDefault<TKey1, TKey2, TKey3, DictionaryWithDefault<TKey4, TValue>>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="func"></param>
        public DictionaryWithDefault(Func<TKey4, TValue> func)
            : base(k => new DictionaryWithDefault<TKey4, TValue>(func))
        {
        }
        public bool ContainsKey(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4)
        {
            if (!this.ContainsKey(key1, key2, key3))
                return false;
            return this[key1][key2][key3].ContainsKey(key4);
        }
    }
}
