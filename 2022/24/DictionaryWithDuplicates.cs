public static class DictionaryWithDuplicatesExtensions
{
    public static DictionaryWithDuplicates<TKey, TValue> ToDictionaryWithDuplicates<TInputValue, TKey, TValue>
        (this IEnumerable<TInputValue> list, Func<TInputValue, TKey> keyFunc, Func<TInputValue, TValue> valueFunc)
        where TKey : notnull
    {

        var newDict = new DictionaryWithDuplicates<TKey, TValue>();
        foreach (var input in list)
        {
            newDict.Value(keyFunc(input), valueFunc(input));
        }
        return newDict;
    }
    public static DictionaryWithDuplicates<TKey, TValue> ToDictionaryWithDuplicates<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, HashSet<TValue>>> other)
        where TKey : notnull
    {

        var newDict = new DictionaryWithDuplicates<TKey, TValue>();
        foreach (var input in other)
        {
            newDict[input.Key]=input.Value;
        }
        return newDict;
    }
}
public class DictionaryWithDuplicates<TKey,TValue> : Dictionary<TKey,HashSet<TValue>> 
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
    public virtual HashSet<TValue> Value(TKey pos, TValue newValue)
    {
        HashSet<TValue> list;
        if (!ContainsKey(pos))
        {
            list = new HashSet<TValue> { newValue };
            this[pos] = list;
        }
        else
        {
            list = this[pos];
            if (!list.Contains(newValue))
                list.Add(newValue);
        }

        return list;
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

    public new IEnumerable<TValue> Values
    {
        get
        {
            foreach (var element in this)
            {
                foreach (var value in element.Value)
                {
                    yield return value;
                }
            }
        }
    }

}