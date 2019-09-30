using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    public class Diagnosis
    {
        public HashSet<Comp> Comps { get; private set; }
        public double Probability { get; private set; } //Probability to be faulty

        public Diagnosis()
        {
            Comps = new HashSet<Comp>();
            Probability = 0;
        }
        public Diagnosis(HashSet<Comp> diagnosis)
        {
            if (diagnosis != null)
                Comps = new HashSet<Comp>(diagnosis);
            else
                Comps = new HashSet<Comp>();
            CalcAndSetProb();
        }
        public Diagnosis(List<Comp> diagnosis) :this(new HashSet<Comp>(diagnosis)){}
       
        public void AddCompsToDiagnosis(List<Comp> comps)
        {
            if (comps != null && comps.Count != 0)
            {
                foreach (Comp comp in comps)
                    AddCompToDiagnosis(comp);
            }
        }
        public void AddCompToDiagnosis(Comp comp)
        {
            if (comp != null && !Comps.Contains(comp))
                Comps.Add(comp);
        }
        public void CalcAndSetProb()
        {
            if (Comps == null || Comps.Count == 0)
            {
                Probability = 0;
                return;
            }
            double p = 1;
            foreach (Comp c in Comps)
            {
                p = p * c.P;
            }
            Probability = p;
        }
        
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj is Diagnosis)
            {
                Diagnosis diag = (Diagnosis)obj;
                if (diag.Comps == null || Comps == null)
                    return false;
                if(Comps.Count!=diag.Comps.Count)
                    return false;
                foreach (Comp c in Comps)
                {
                    if (!diag.Comps.Contains(c))
                        return false;
                }
                return true;
            }
            else
                return base.Equals(obj);
        }
        
        public override string ToString()
        {
            string diagString = "";
            foreach (Comp c in Comps)
            {
                diagString += c.Name + ",";
            }
            return diagString;
        }
        public void PrintDiagnosis()
        {
            Console.WriteLine(ToString());
        }
        public override int GetHashCode()
        {
            int hash = 13;
            foreach(Comp comp in Comps)
            {
                hash = (hash * 7) + comp.GetHashCode();
            }
            return hash;
        }
    }
}
