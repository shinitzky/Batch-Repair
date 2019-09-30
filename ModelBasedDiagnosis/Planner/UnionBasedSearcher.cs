using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    class UnionBasedSearcher :RepairActionSearcher
    {
        public bool RATrie;
        public UnionBasedSearcher()
        {
            K = 1;
        }
        public UnionBasedSearcher(int k)
        {
            K = k;
        }
        public UnionBasedSearcher(int k, bool trie)
        {
            RATrie = trie;
            K = k;
        }
        public override RepairActionsSet ComputePossibleAcions(SystemState state)
        {
            int k = K;
            if (state == null || state.Diagnoses == null || state.Diagnoses.Count == 0)
                return null;
            RepairActionsSet ans;
            if (RATrie)
                ans = new RepairActionsTrie();
            else
                ans = new RepairActionsHashSet();
           
            List<Diagnosis> diagnoses = state.Diagnoses.Diagnoses.ToList();
            double diagProb = 0;
            bool cropDiagnoses = false;
            if (diagnoses.Count > 2000)
                cropDiagnoses = true;
            if (diagnoses.Count < k) //need to correct this!!
                k = diagnoses.Count;

            int maxIndex = diagnoses.Count -1;
            SortedSet<int> currPermutation = new SortedSet<int>();
            for (int size = 1; size <= k; size++)
            {
                if (cropDiagnoses && diagProb > 0.999)
                    break;
                if (currPermutation == null)
                    currPermutation = new SortedSet<int>();
                for (int i = 0; i < size;i ++ ) //computing first permutation
                    currPermutation.Add(i);
                
                while(currPermutation!=null && currPermutation.Count!=0)
                {
                    if (cropDiagnoses && diagProb > 0.999)
                        break;
                    SortedSet<Comp> newAction = new SortedSet<Comp>(new Comp.CompComparer());
                    foreach (int index in currPermutation)
                    {
                        Diagnosis diag = diagnoses[index];
                        diagProb += diag.Probability / state.Diagnoses.SetProbability;
                        foreach (Comp c in diag.Comps)
                        {
                            if (!newAction.Contains(c))
                                newAction.Add(c);
                        }
                    }
                    ans.AddAction(newAction);
                    currPermutation = nextPermutation(currPermutation, maxIndex);
                }
            }
            return ans;
        }                
        private SortedSet<int> nextPermutation(SortedSet<int> prevPermutation, int maxIndex)
        {
            SortedSet<int> ans = new SortedSet<int>();
            int i = prevPermutation.First();
            prevPermutation.Remove(i);
            i++;
            if (i > maxIndex)
                return null;
            int j = 0;
            while (prevPermutation.Contains(i))//check if its a final state for the current k
            {
                ans.Add(j);
                j++;
                prevPermutation.Remove(i);
                i++;
                if(i > maxIndex)
                    return null;
            }
            ans.Add(i);
            foreach (int num in prevPermutation)
                ans.Add(num);
            return ans;
        }
        public override string Type()
        {
            string ans = "Union";
            if (RATrie)
                ans += "_trie";
            return ans;
        }

    }
}
