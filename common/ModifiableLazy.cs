using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace common
{

    public class ModifiableLazy<T>
    {
        private readonly object _syncroot = new();
        private StrongBox<T>? _value;

        public T Get(Func<T> valueFactory)
        {
            if (valueFactory == null)
            {
                throw new ArgumentNullException(nameof(valueFactory));
            }

            var box = Volatile.Read(ref _value);
            return box != null ? box.Value! : GetOrCreate();

            T GetOrCreate()
            {
                lock (_syncroot)
                {
                    box = Volatile.Read(ref _value);
                    if (box != null)
                    {
                        return box.Value!;
                    }

                    box = new StrongBox<T>(valueFactory());
                    Volatile.Write(ref _value, box);
                    return box.Value!;
                }
            }
        }

        public void Invalidate()
        {
            Volatile.Write(ref _value, null);
        }

        public void SetValue(T newValue)
        {
            lock (_syncroot)
            {
                Volatile.Write(ref _value, new StrongBox<T>(newValue));
            }
        }

        public bool TryGet(out T value)
        {
            var box = Volatile.Read(ref _value);
            if (box != null)
            {
                value = box.Value!;
                return true;
            }

            value = default(T)!;
            return false;
        }
    }
}