using System.Collections.Generic;
using System.Linq;

namespace _12
{
    public class Cave
    {
        public readonly string Name;
        public readonly bool Small;
        public List<Cave> Linked = new List<Cave>();
        public bool Big => !Small;
        public bool IsIsolated => IsStart || (!IsEnd && Small && Linked.All(x => (x.Small && !x.IsEnd)));
        public bool IsEnd => Name == "end";
        public bool IsStart => Name == "start";

        public Cave(string name)
        {
            Name = name;
            if (name == name.ToLower())
                Small = true;
        }

        public void AddLink(Cave other)
        {
            if (!Linked.Contains(other))
            {
                Linked.Add(other);
                other.AddLink(this);
            }
        }
    }
}