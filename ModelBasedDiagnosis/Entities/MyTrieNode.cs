using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    public class MyTrieNode
    {
        public Comp value;
        public SortedSet<MyTrieNode> Children;
        public MyTrieNode Parent; //parent 
        public bool IsLeaf
        {
            get
            {
                if (Children.Count == 0)
                {
                    return true;
                } 
                else
                    return false;
            }
        }
        public bool IsTerminateSet;
        public MyTrieNode(Comp value)
        {
            this.value = value;
            IsTerminateSet = false;
            Children = new SortedSet<MyTrieNode>(new MyTrieNode.MyTrieNodeComparer());
        }

        public class MyTrieNodeComparer : IComparer<MyTrieNode>
        {
            public int Compare(MyTrieNode x, MyTrieNode y) //by order
            {
                if (x == null || y == null)
                    return 0;
                Comp.CompComparer comparer = new Comp.CompComparer();
                return comparer.Compare(x.value, y.value);
            }
        }

        public MyTrieNode AddChild(Comp comp)
        {
            if (comp == null)
                return null;
            foreach (MyTrieNode child in Children)
            {
                if (child.value.Equals(comp))
                    return child;  
            }
            MyTrieNode newChild = new MyTrieNode(comp);
            Children.Add(newChild);
            newChild.Parent = this;
            return newChild;
        }
        public void AddSet(SortedSet<Comp> comps) 
        {
            if (comps == null || comps.Count == 0)
                return;
            Comp first = comps.First();
            comps.Remove(first);
            MyTrieNode newChild = AddChild(first);
            if (newChild == null)
                return;
            if (comps.Count > 0)
            {
                newChild.AddSet(comps);
            }
            else
                newChild.IsTerminateSet = true;
        }

        public void PrintNode(int depth)
        {
            string toPrint = depth +". "+ value.Id;
            if (IsTerminateSet)
                toPrint += " Terminator";
            if (IsLeaf)
                toPrint += " Leaf";
            Console.WriteLine(toPrint);
            foreach (MyTrieNode child in Children)
            {
                child.PrintNode(depth+1);
            }
        }
    }
}
