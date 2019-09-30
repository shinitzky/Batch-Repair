using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    abstract class DiagnosesSearcher :IDiagnoser
    {
        public IFunction function;
        protected Trie<string> trie;
        protected Observation observation;
        protected List<int> closed;
        protected TimeSpan x;
        protected HealthStateVector hSVector;
        //protected List<double> hSVector;
        //protected int diagnosesCounter;
        public bool FoundMinCard;
        public TimeSpan FirstMinCard;
        public Agenda agenda;
        public enum Agenda { findAllDiagnoses, minCard, HealthState };

        public DiagnosesSearcher(IFunction function, TimeSpan timeSpan): this(function)
        {
            x = timeSpan;
        }
        public DiagnosesSearcher(IFunction function)
        {
            this.function = function;
            closed = new List<int>();
            x = new TimeSpan(0, 1, 0); 
        }
        public DiagnosesSearcher() :this(null)
        {}
        public void addToGoodCompList(int compID)
        {
            closed.Add(compID);
        }
        public void resetGoodCompList()
        {
            closed.Clear();
        }
        public abstract DiagnosisSet FindDiagnoses(Observation observation);
        public abstract bool Stop();
        protected double CalcHSVectorDistance(List<double> oldHSVector)
        {
            double ans = 0;
            if (hSVector == null || hSVector.Count == 0 || oldHSVector == null || hSVector.Count != oldHSVector.Count)
                return -1;
            for (int i = 0; i < oldHSVector.Count; i++)
            {
                double j = Math.Pow((hSVector.CurrentHealthState[i] - oldHSVector[i]), 2);
                ans += j;
            }
            ans = Math.Sqrt(ans);
            return ans;
        }
        public bool IsConsistent(Observation observation,Diagnosis diagnosis)
        {
            observation.TheModel.SetValue(observation.InputValues);
            foreach (Gate g in diagnosis.Comps)
            {
                function.Operate(g);
            }
            bool[] modelOutput = observation.TheModel.GetValue();
            for (int i = 0; i < observation.OutputValues.Length; i++)
            {
                if (modelOutput[i] != observation.OutputValues[i])
                    return false;
            }
            return true;
        } 
        protected bool isDamaged()
        {
            bool[] modelOutput = observation.TheModel.GetValue();
            for (int i = 0; i < observation.OutputValues.Length; i++)
            {
                if (modelOutput[i] != observation.OutputValues[i])
                    return false;
            }
            return true;
        }
        protected bool isInTrie(List<Gate> list)//for CreateSet
        {
            if (list == null || list.Count == 0)
                return false;
            bool ans = true;
            if (list.Count == 1)
            {
                Gate g = list.First();
                foreach (char c in g.Id + "")
                {
                    if (!trie.Matcher.NextMatch(c))
                    {
                        ans = false;
                        break;
                    }
                }
                if (ans)
                    ans = trie.Matcher.IsExactMatch();
                while (trie.Matcher.LastMatch() != 0 && trie.Matcher.LastMatch() != 32)
                {
                    trie.Matcher.BackMatch();
                }
                return ans;
            }
            List<Gate> temp = new List<Gate>(list);
            foreach (Gate g in list)
            {
                ans = true;
                temp.RemoveAt(0);
                foreach (char c in g.Id + "")
                {
                    if (!trie.Matcher.NextMatch(c))
                    {
                        ans = false;
                        break;//
                    }
                }
                if (ans)
                {
                    if (trie.Matcher.NextMatch(' '))
                    {
                        ans = isInTrie(temp);
                        trie.Matcher.BackMatch();
                    }
                }
                //backtrack
                while (trie.Matcher.LastMatch() != 0 && trie.Matcher.LastMatch() != 32)
                {
                    trie.Matcher.BackMatch();
                }
                if (ans)
                    break;

            }
            return ans;

        }
        
    }
}
