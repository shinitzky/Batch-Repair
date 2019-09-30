using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    public class RepairActionsHashSet:RepairActionsSet
    {
        private HashSet<Action> actions;
        private int currIndex;
        public int MaxSize { get; set; }

        public RepairActionsHashSet()
        {
            actions = new HashSet<Action>();
            currIndex = -1;
            MaxSize = 100000; //defult value
        }

        public override RepairAction NextAction()
        {
            if (currIndex+1 < MaxSize && currIndex+1>=0 && currIndex<actions.Count-1)
            {
                currIndex = currIndex + 1;
                return new RepairAction(actions.ToArray()[currIndex].GetAction());
            }
            return null;
        }
       

        public override void AddAction(SortedSet<Comp> action)
        {
            if (action == null || action.Count == 0 || actions.Count==MaxSize)
                return;
            Action newAction = new Action(action.ToList());
            if (actions.Contains(newAction))
                return;
            actions.Add(newAction);
        }

        class Action
        {
            private List<Comp> action;
            private List<int> primes;
            public Action(List<Comp> action)
            {
                this.action = action;
                primes = new List<int>();
                primes.Add(3);
                primes.Add(7);
                primes.Add(13);
                primes.Add(17);
                primes.Add(23);
                primes.Add(29);
            }
            public List<Comp> GetAction()
            {
                return action;
            }
            public override int GetHashCode()
            {
                int ans = 0;
                if(action==null||action.Count==0)
                    return ans;
                int i = 0;
                foreach (Comp c in action)
                {
                    if(i >= primes.Count)
                        i = 0;
                    ans += c.Id * primes[i];
                    i++;
                }
                return ans;
            }

        }
    }
}
