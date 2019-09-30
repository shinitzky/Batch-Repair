using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    public class DiagnosisSet
    {
        public HashSet<Diagnosis> Diagnoses { get; private set; }
        public double SetProbability { get; set; }
        public int Count
        {
            get
            {
                if (Diagnoses == null)
                    return 0;
                else
                    return Diagnoses.Count;
            }
        }
        public List<Comp> Components { get; private set; }
       
        public DiagnosisSet()
        {
            Diagnoses = new HashSet<Diagnosis>();
            Components = new List<Comp>();
            SetProbability = 0;
        }

        public void AddDiagnosis(Diagnosis diagnosis)
        {                                                                                                  
            if (diagnosis != null && diagnosis.Comps != null && diagnosis.Comps.Count != 0 && !Diagnoses.Contains(diagnosis))
            {
                Diagnoses.Add(diagnosis);
                if (diagnosis.Probability == 0)
                    diagnosis.CalcAndSetProb();
                SetProbability += diagnosis.Probability;
                foreach(Comp c in diagnosis.Comps)
                {
                    if (!Components.Contains(c))
                        Components.Add(c);
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is DiagnosisSet)
            {
                DiagnosisSet diagSet = (DiagnosisSet)obj;
                if (diagSet == null || diagSet.Diagnoses == null)
                    return false;
                if (this.Diagnoses == null || this.Diagnoses.Count != diagSet.Diagnoses.Count)
                    return false;
                foreach (Diagnosis diag in this.Diagnoses)
                {
                    if (!diagSet.Diagnoses.Contains(diag))
                        return false;
                }
                return true;
            }
            else
                return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            int hash = 13;
            foreach (Diagnosis diag in Diagnoses)
            {
                hash = (hash * 7) + diag.GetHashCode();
            }
            return hash;
        }

        public void PrintDiagnoses()
        {
            Console.WriteLine("Diagnoses:");
            foreach (Diagnosis diag in Diagnoses)
            {
                diag.PrintDiagnosis();
            }
        }
    }
}
