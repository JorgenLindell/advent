using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace common
{
    public class BTreeNode<T, TImplementation> : IEnumerable<TImplementation>
        where TImplementation : BTreeNode<T, TImplementation>, new()
    {
        private bool _reportFlag = false; // only used in Root

        public bool ReportFlag
        {
            get => Root._reportFlag;
            set => Root._reportFlag = value;
        }

        public BTreeNode()
        {
            Value = default!;
        }

        public BTreeNode((T, T) p)
        {
            Value = default!;
            NodeLeft = new TImplementation() { Value = CloneValue(p.Item1), Parent = (TImplementation?)this };
            NodeRight = new TImplementation() { Value = CloneValue(p.Item2), Parent = (TImplementation?)this };
        }

        public BTreeNode(T i1, T i2)
        {
            Value = default!;
            NodeLeft = new TImplementation() { Value = CloneValue(i1), Parent = (TImplementation?)this };
            NodeRight = new TImplementation() { Value = CloneValue(i2), Parent = (TImplementation?)this };
        }

        public BTreeNode(TImplementation i1, T i2)
        {
            Value = default!;
            NodeLeft = i1.Clone((TImplementation)this);
            NodeLeft.Parent = (TImplementation?)this;
            NodeRight = new TImplementation() { Value = CloneValue(i2), Parent = (TImplementation?)this };
        }

        public BTreeNode(T i1, TImplementation i2)
        {
            Value = default!;
            NodeLeft = new TImplementation() { Value = CloneValue(i1), Parent = (TImplementation?)this };
            NodeRight = i2.Clone((TImplementation)this);
            NodeRight.Parent = (TImplementation?)this;
        }

        public BTreeNode(T i)
        {
            Value = CloneValue(i);
            NodeLeft = null;
            NodeRight = null;
        }

        public BTreeNode(TImplementation n1, TImplementation n2)
        {
            Value = default!;
            NodeLeft = n1.Clone((TImplementation)this);
            NodeRight = n2.Clone((TImplementation)this);
        }

        public T Value { get; set; }
        public TImplementation? Parent { get; set; }
        public TImplementation? NodeLeft { get; protected set; }
        public TImplementation? NodeRight { get; protected set; }
        public TImplementation Root => (TImplementation)(IsRoot ? this : Parent!.Root);
        public bool IsValueNode => NodeLeft == null || NodeRight == null;
        public int NumberOfParents => IsRoot ? 0 : Parent!.NumberOfParents + 1;
        public bool IsRoot => Parent == null!;
        public int LevelsBelow => Math.Max(NodeLeft?.LevelsBelow + 1 ?? 0, NodeRight?.LevelsBelow + 1 ?? 0);

        public int PosInLine { get; private set; }

        public IEnumerator<TImplementation> GetEnumerator()
        {
            return GetNodes();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual TImplementation Clone()
        {
            var clone = new TImplementation();
            clone.Value = CloneValue(Value);
            clone.NodeLeft = NodeLeft?.Clone(clone);
            clone.NodeRight = NodeRight?.Clone(clone);
            return clone;
        }

        public virtual T CloneValue(T value)
        {
            return value;
        }


        public TImplementation Clone(TImplementation parent)
        {
            var clone = new TImplementation
            {
                Parent = parent,
                Value = Value
            };
            clone.NodeLeft = NodeLeft?.Clone(clone);
            clone.NodeRight = NodeRight?.Clone(clone);
            return clone;
        }

        public static implicit operator BTreeNode<T, TImplementation>((T, T) p)
        {
            return new BTreeNode<T, TImplementation>(p);
        }

        public static implicit operator BTreeNode<T, TImplementation>(T p)
        {
            return new BTreeNode<T, TImplementation>(p);
        }


        public override string ToString()
        {
            if (this == Root)
            {
                var sb = new StringBuilder();
                ToSb(sb);
                return sb.ToString();
            }

            var res = "";
            if (IsValueNode)
                res += Value?.ToString();
            else
                res += $"[{NodeLeft},{NodeRight}]";
            return res;
        }

        public void ToSb(StringBuilder sb)
        {
            PosInLine = sb.Length;
            if (IsValueNode)
            {
                sb.Append(Value);
            }
            else
            {
                sb.Append("[");
                NodeLeft?.ToSb(sb);
                sb.Append(",");
                NodeRight?.ToSb(sb);
                sb.Append("]");
            }
        }

        public virtual string Represent(TImplementation x)
        {
            var l = "" + (x.NodeLeft?.IsValueNode ?? false ? x.NodeLeft?.Value?.ToString() : "n");
            var r = "" + (x.NodeRight?.IsValueNode ?? false ? x.NodeRight.Value?.ToString() : "n");
            return $" {l} {r} ";
        }


        public virtual string ToTree()
        {
            var levels = LevelsBelow;
            var levelsAboveMe = NumberOfParents;
            var nodesPerLevel = new Dictionary<int, Dictionary<TImplementation, string>>();
            for (var i = 0; i < levels; i++) nodesPerLevel[i] = new Dictionary<TImplementation, string>();

            var nodes = this.Where(n =>
            {
                if (!n.IsValueNode)
                {
                    nodesPerLevel[n.NumberOfParents - levelsAboveMe].Add(n, $" {Represent(n)}");
                    return true;
                }

                return false;
            }).ToList();

            var maxWidth = Math.Pow(2, nodesPerLevel.Count);
            //TODO: Implement
            return "";
        }

        public TSource ReplaceMe<TSource>(TSource bTreeNode, T i)
            where TSource : TImplementation, new()
        {
            if (bTreeNode == NodeLeft)
            {
                NodeLeft = new TSource
                {
                    Value = i,
                    Parent = (TImplementation)this
                };
                return (TSource)NodeLeft;
            }

            if (bTreeNode == NodeRight)
            {
                NodeRight = new TSource
                {
                    Value = i,
                    Parent = (TImplementation)this
                };
                return (TSource)NodeRight;
            }

            throw new Exception("ReplaceMe called with wrong node");
        }

        public TSource? GetValueNodeToTheLeft<TSource>(TSource comingFrom)
            where TSource : TImplementation
        {
            // used in exploding, an exploding pair is always two regular numbers,
            // no need to check own left number
            TSource? found = null;
            if (comingFrom != this)
            {
                if (comingFrom == NodeRight) found = NodeLeft?.FindValuePreferRight<TSource>();

                found ??= Parent?.GetValueNodeToTheLeft((TSource)this);
            }
            else
            {
                found ??= Parent?.GetValueNodeToTheLeft((TSource)this);
            }

            return found;
        }

        public TSource? FindValuePreferRight<TSource>()
            where TSource : TImplementation
        {
            if (IsValueNode) return (TSource)this;
            var found = NodeRight?.FindValuePreferRight<TSource>();
            return found;
        }

        public TSource? GetValueNodeToTheRight<TSource>(TSource comingFrom)
            where TSource : TImplementation
        {
            // used in exploding, an exploding pair is always two regular numbers,
            // no need to check own left number
            TSource? found = null;
            if (comingFrom != this)
            {
                if (comingFrom == NodeLeft) found = NodeRight?.FindValuePreferLeft<TSource>();
                found ??= Parent?.GetValueNodeToTheRight((TSource)this);
            }
            else
            {
                found ??= Parent?.GetValueNodeToTheRight((TSource)this);
            }

            return found;
        }

        public TSource? FindValuePreferLeft<TSource>()
            where TSource : TImplementation
        {
            if (IsValueNode) return (TSource)this;
            var found = NodeLeft?.FindValuePreferLeft<TSource>();
            return found;
        }

        public IEnumerator<TImplementation> GetNodes()
        {
            foreach (var bTreeNode in NodeLeft ?? Array.Empty<TImplementation>().AsEnumerable()) yield return bTreeNode;
            yield return (TImplementation)this;
            foreach (var bTreeNode in NodeRight ?? Array.Empty<TImplementation>().AsEnumerable()) yield return bTreeNode;
        }
    }
}