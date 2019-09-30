using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    public class RepairActionsTrie:RepairActionsSet
    {
        private MyTrieNode root;
        private MyTrieNode currRootNode;
        private SortedSet<MyTrieNode> currAction;
        private int currRootIndex;

        public RepairActionsTrie()
        {
            root = new MyTrieNode(new OneInputComponent(0, Gate.Type.buffer));
        }

        public override RepairAction NextAction()
        {
            List<Comp> ans = new List<Comp>();
            if(currAction==null)
            {
                currRootIndex = 0;
                currAction = new SortedSet<MyTrieNode>(new MyTrieNode.MyTrieNodeComparer());
                currRootNode = root.Children.First();
                currAction.Add(currRootNode);
                addFromCurrent(currRootNode);
            }
            else
            {
                MyTrieNode currNodeTerminator = currAction.Last();
                if (currNodeTerminator.IsLeaf)
                {
                    currAction.Remove(currNodeTerminator);
                    if (currAction.Count == 0)
                    {
                        currRootIndex = currRootIndex + 1;
                        if (currRootIndex == root.Children.Count)
                            return null; // no actions left
                        currRootNode = root.Children.ElementAt(currRootIndex);
                        currAction.Add(currRootNode);
                        addFromCurrent(currRootNode);
                    }
                    else
                    {
                        MyTrieNode currParent = currAction.Last();
                        MyTrieNode curr = nextChild(currParent, currNodeTerminator);
                        while (curr == null)
                        {
                            currAction.Remove(currParent);
                            if (currAction.Count == 0)
                            {
                                currRootIndex = currRootIndex + 1;
                                if (currRootIndex == root.Children.Count)
                                    return null; // no actions left
                                currRootNode = root.Children.ElementAt(currRootIndex);
                                currAction.Add(currRootNode);
                                curr = currRootNode;
                                break;
                            }
                            currNodeTerminator = currParent;
                            currParent = currAction.Last();
                            curr = nextChild(currParent, currNodeTerminator);
                        }
                        currAction.Add(curr);
                        addFromCurrent(curr);
                    }
                }
                else
                {
                    currNodeTerminator = currNodeTerminator.Children.First();
                    currAction.Add(currNodeTerminator);
                    addFromCurrent(currNodeTerminator);
                }
            }
            
            if (currAction == null || currAction.Count == 0)
                return null;
            foreach (MyTrieNode node in currAction)
            {
                ans.Add(node.value);
            }
            return new RepairAction(ans);
        }
        private void addFromCurrent(MyTrieNode curr)
        {
            while (!curr.IsTerminateSet)
            {
                curr = curr.Children.First();
                currAction.Add(curr);
            }
        }
        private MyTrieNode nextChild(MyTrieNode parent, MyTrieNode prevChild)
        {
            if (parent == null || prevChild == null)
                return null;
            if (parent.Children.Last().value.Equals(prevChild.value))
                return null;
            bool isNext = false;
            foreach (MyTrieNode child in parent.Children)
            {
                if (isNext)
                    return child;
                if (child.value.Equals(prevChild.value))
                {
                    isNext = true;
                }
            }
            return null;
        }

        public override void AddAction(SortedSet<Comp> action)
        {
            root.AddSet(action);
        }

        public void PrintTrie()
        {
            root.PrintNode(0);
        }

    }
}
