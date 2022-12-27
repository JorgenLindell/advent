internal class DictionaryWithDuplicates<TKey,TValue> : Dictionary<TKey,HashSet<TValue>> 
    where TKey : notnull

{
    public virtual bool IsEmpty(TKey pos)
    {
        return !ContainsKey(pos) || this[pos].Count == 0;
    }
    public virtual void Remove(TKey pos, TValue value)
    {
        if (ContainsKey(pos))
        {
            var list = this[pos];
            list.Remove(value);
            if (list.Count == 0)
                this.Remove(pos);
        }
    }
    public virtual void Value(TKey pos, TValue newValue)
    {
        if (!ContainsKey(pos))
        {
            var list = new HashSet<TValue> { newValue };
            this[pos] = list;
        }
        else
        {
            var list = this[pos];
            if (!list.Contains(newValue))
                list.Add(newValue);
        }
    }
    public virtual HashSet<TValue> Value(TKey pos)
    {
        if (!ContainsKey(pos))
        {
            var list = new HashSet<TValue>();
            return list;
        }

        return this[pos];
    }

}