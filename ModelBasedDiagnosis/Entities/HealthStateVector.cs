using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    public class HealthStateVector
    {
        public List<Comp> Components { get; private set; }
        public List<double> CurrentHealthState { get; private set; }

        public int Count
        {
            get
            {
                if (CurrentHealthState != null)
                    return CurrentHealthState.Count;
                else
                    return 0;
            }
        }

        public int DiagnosesCounter { get; private set; }

        public HealthStateVector(List<Comp> components)
        {
            if (components != null)
                Components = components;
            else
                Components = new List<Comp>();
            CurrentHealthState = new List<double>();
            DiagnosesCounter = 0;
        }

        public HealthStateVector(List<Comp> components, DiagnosisSet diagnoses)
        {
            if (components != null)
                Components = components;
            else
                Components = new List<Comp>();
            CurrentHealthState = new List<double>();
            DiagnosesCounter = 0;
            CalcHealthState(diagnoses);
        }

        public void CalcHealthState(DiagnosisSet diagnoses)
        {
            if (diagnoses == null || diagnoses.Count == 0)
                return;
            List<double> compHS = new List<double>();
            for (int i = 0; i < Components.Count; i++)
            {
                compHS.Add(0);
            }
            foreach (Diagnosis diag in diagnoses.Diagnoses)
            {
                foreach (Comp comp in diag.Comps)
                {
                    int i = Components.IndexOf(comp);
                    if (i >= 0 && i < compHS.Count)
                        compHS[i] += (diag.Probability / diagnoses.SetProbability);
                }
            }
            CurrentHealthState = compHS;
            DiagnosesCounter = diagnoses.Count;
        }

        public double GetCompHealthState(Comp component)
        {
            if (component == null)
                return 0;
            int i = Components.IndexOf(component);
            if (i >= 0 && i < Count)
                return CurrentHealthState[i];
            return 0;
        }
    }
}
