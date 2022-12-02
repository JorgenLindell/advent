namespace _18;

public interface ITreeNode<T>
{
    T Parent { get; set; }
    bool IsLeaf { get; }
    bool IsRoot { get; }
    int Level { get; }
    T GetRootNode();
    string IndexString();
    int OrderOfChild(T child);
    IEnumerable<T> AllChildren();
}

public abstract class Tree<T> : ITreeNode<T> where T : class, ITreeNode<T>
{
    protected Tree()
    {
        ChildNodes = new List<T>();
    }

    public List<T> ChildNodes { get; }
    protected virtual T MySelf => (T)(object)this;
    public T Parent { get; set; } = null!;
    public bool IsLeaf => ChildNodes.Count == 0;
    public bool IsRoot => Parent == null!;
    public T GetRootNode() => IsRoot ? MySelf : Parent.GetRootNode();
    public int Level => IsRoot ? 0 : Parent.Level + 1;
    public string IndexString() => IsRoot ? "0" : $"{Parent.IndexString()}.{Parent.OrderOfChild(MySelf)}";
    public int OrderOfChild(T child) => ChildNodes.FindIndex(x => x == child);

    public List<T> GetLeafNodes() => ChildNodes.Where(x => x.IsLeaf).ToList();
    public List<T> GetNonLeafNodes() => ChildNodes.Where(x => !x.IsLeaf).ToList();

    public void AddChild(T child)
    {
        child.Parent = MySelf;
        ChildNodes.Add(child);
    }

    public void AddChildren(IEnumerable<T> children)
    {
        foreach (var child in children)
            AddChild(child);
    }

    public IEnumerable<T> AllChildren()
    {
        yield return MySelf;
        foreach (var childNode in ChildNodes)
        {
            foreach (var allChild in childNode.AllChildren())
            {
                yield return allChild;
            }
        }
    }
}