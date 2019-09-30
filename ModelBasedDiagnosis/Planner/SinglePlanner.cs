using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    class SinglePlanner
    {
        public enum Type { random, highestProb, bestDiag }
        private Type type;
        public List<int> fixedComp;
        private Random rnd;
        public double Overhead { get; set; }
        public SinglePlanner()
        {
            rnd = new Random();
            fixedComp = new List<int>();
            Overhead = 10; //defult value
        }
        public SinglePlanner(Type type): this()
        {
            this.type = type;
        }
        public Comp Plan(SystemState systemState)
        {
            Comp ans = null;
            switch (type)
            {
                case Type.random:
                    ans = Random(systemState.Diagnoses);
                    break;
                case Type.highestProb:
                    ans = HighstProb(systemState);
                    break;
                case Type.bestDiag:
                    ans = BestDiagnosis(systemState.Diagnoses);
                    break;
            }
            if (ans != null)
                fixedComp.Add(ans.Id);
            return ans;
        }
        private Comp Random(DiagnosisSet diagnoses)
        {
            int index = rnd.Next(0, diagnoses.Count - 1);
            Diagnosis d = diagnoses.Diagnoses.ToList()[index];
            index = rnd.Next(0, d.Comps.Count - 1);
            return d.Comps.ToList()[index];
        }
        private Comp HighstProb(SystemState systemState)
        {
            if (systemState.HealthState == null || systemState.HealthState.Count == 0)
                return null;
            double max = systemState.HealthState.CurrentHealthState.Max();
            int index = systemState.HealthState.CurrentHealthState.IndexOf(max);
            return systemState.HealthState.Components[index];
        }
        private Comp BestDiagnosis(DiagnosisSet diagnoses)
        {
            Diagnosis bestDiag = null;
            double bestDiagProb = 0;
            foreach(Diagnosis diag in diagnoses.Diagnoses)
            {
                if (bestDiag ==null || diag.Probability > bestDiagProb)
                {
                    bestDiag = diag;
                    bestDiagProb = diag.Probability;
                }
            }
            int j = rnd.Next(bestDiag.Comps.Count - 1);
            return bestDiag.Comps.ToList()[j];
        }
    }
}
