using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    class PowersetBasedSearcher:RepairActionSearcher
    {
        public PowersetBasedSearcher()
        {
            K = 1;
        }
        public PowersetBasedSearcher(int k)
        {
            K = k;
        }
         public override RepairActionsSet ComputePossibleAcions(SystemState state)
        {
            int k = K;
            if(state==null|| state.Diagnoses ==null || state.Diagnoses.Count==0)
                return null;
            List<List<Comp>> ans = new List<List<Comp>>();
            List<Comp> comps = new List<Comp>(); // hash set?
            foreach (Diagnosis diag in state.Diagnoses.Diagnoses)
            {
                foreach (Comp c in diag.Comps)
                {
                    if (!comps.Contains(c))
                    {
                        comps.Add(c);
                        List<Comp> list = new List<Comp>();
                        list.Add(c);
                        ans.Add(list);
                    }
                       
                }
            }
            while (k > 1)
            {
                List<List<Comp>> temp = new List<List<Comp>>();
                foreach (List<Comp> list in ans)
                {
                    foreach (Comp c in comps)
                    {
                        if (!list.Contains(c))
                        {
                            List<Comp> newList = new List<Comp>(list);
                            newList.Add(c);
                            temp.Add(newList);
                        }
                    }
                }
                ans.AddRange(temp);
                k--;
            }
            RepairActionsSet actions;
            if (k == 1)
                actions = new RepairActionsHashSet();
            else
                actions = new RepairActionsTrie();
            foreach (List<Comp> list in ans)
            {
                SortedSet<Comp> action = new SortedSet<Comp>(new Comp.CompComparer());
                foreach (Comp c in list)
                {
                    action.Add(c);
                }
                actions.AddAction(action);
            }
            return actions;
         }
         public override string Type()
         {
             return "Powerset";
         }

    }
}
