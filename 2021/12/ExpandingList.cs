using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace _12
{
    internal class ExpandingList<T> : IList<T>

    {

        public ExpandingList()
        {
        }

        public ExpandingList(IList<T> incoming)
        {
            _listImplementation = incoming as List<T> ?? incoming.ToList();
        }

        private readonly List<T> _listImplementation = new List<T>();

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _listImplementation.GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return _listImplementation.GetEnumerator();
        }


        bool ICollection<T>.Remove(T item)
        {
            return _listImplementation.Remove(item);
        }

        public int Count => _listImplementation.Count;
        public bool IsReadOnly => false;

        public void Add(T value)
        {
            _listImplementation.Add(value);
        }

        public void Clear()
        {
            _listImplementation.Clear();
        }

        public bool Contains(T value)
        {
            return _listImplementation.Contains(value);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _listImplementation.CopyTo(array, arrayIndex);
        }

        public int IndexOf(T value)
        {
            return _listImplementation.IndexOf(value);
        }

        public void Insert(int index, T value)
        {
            _listImplementation.Insert(index, value);
        }

        public void Remove(T value)
        {
            _listImplementation.Remove(value);
        }

        public void RemoveAt(int index)
        {
            _listImplementation.RemoveAt(index);
        }

        public T this[int index]
        {
            get
            {
                if (index < 0)
                    return default;
                if (index >= _listImplementation.Count)
                    return default;
                return _listImplementation[index];
            }
            set
            {
                while (index >= _listImplementation.Count)
                    _listImplementation.Add(default);
                _listImplementation[index] = value;
            }
        }

        public int FindOccurence(List<T> caves, Func<T, T, bool> equalityFunc)
        {
            for (int i = Count - 1 - caves.Count; i >= 0; i--)
            {
                var match = true;
                for (int j = 0; j < caves.Count; j++)
                {
                    if (!equalityFunc(this[j + i], caves[j]))
                    {
                        match = false;
                        break;
                    }
                }

                if (match) return i;
            }

            return -1;
        }
    }
}