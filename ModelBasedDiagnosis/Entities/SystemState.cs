using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    public class SystemState
    {
        private DiagnosisSet m_diagnoses;
        public DiagnosisSet Diagnoses
        {
            get
            {
                return m_diagnoses;
            }
            set
            {
                m_diagnoses = value;
                if (HealthState != null)
                    HealthState.CalcHealthState(m_diagnoses);
            }
        }
        
        public HealthStateVector HealthState { get; private set; }

        public SystemState(List<Comp> components)
        {
            if (components != null && components.Count != 0)
                HealthState = new HealthStateVector(components);
        }
        
        public SystemState GetNextState(RepairAction action)
        {
            DiagnosisSet nextDiagSet = computeNextState(action);
            if (nextDiagSet == null)
                return this; // return null?
            SystemState nextState=null;
            if (HealthState == null)
                nextState = new SystemState(null);
            else
                nextState = new SystemState(HealthState.Components);
            nextState.Diagnoses = nextDiagSet;
            return nextState;
        }
        public void SetNextState(RepairAction action)
        {
            DiagnosisSet nextDiagSet = computeNextState(action);
            if (nextDiagSet == null)
                return;
            Diagnoses = nextDiagSet;
        }
        private DiagnosisSet computeNextState(RepairAction action) //the resulted diagnoses could be not subset minimal!
        {
            if (m_diagnoses == null || m_diagnoses.Count == 0 || action == null || action.Count == 0)
                return null;
            DiagnosisSet ans = new DiagnosisSet();
            foreach (Diagnosis diag in m_diagnoses.Diagnoses)
            {
                Diagnosis toAdd = new Diagnosis();
                foreach (Comp g in diag.Comps)
                {
                    if (!action.Contains(g))
                    {
                        toAdd.AddCompToDiagnosis(g);
                    }
                }
                if (toAdd.Comps.Count != 0)
                    ans.AddDiagnosis(toAdd);
            }
            return ans;
        }

        public override bool Equals(object obj) //comparing only the diagnosis set
        {
            if (obj is SystemState)
            {
                SystemState state = (SystemState)obj;
                if (state == null || state.Diagnoses == null || this.Diagnoses == null)
                    return false;
                return m_diagnoses.Equals(state.m_diagnoses);
            }
            else
                return base.Equals(obj);
        }

        public double SystemRepair(RepairAction repairAction) 
        {
            double sysRepair = 0;
            if (m_diagnoses == null || repairAction == null || repairAction.Count == 0)
                return sysRepair;
            if (m_diagnoses.Count == 0)
                return 1; //system is repaired
            foreach (Diagnosis diag in m_diagnoses.Diagnoses)
            {
                if (diag.Comps == null || diag.Comps.Count == 0 || diag.Comps.Count > repairAction.Count)
                    continue;
                //double p = diag.Probability;
                double p = diag.Probability/m_diagnoses.SetProbability; //changes
                foreach (Comp g in diag.Comps)
                {
                    if (!repairAction.Contains(g))
                    {
                        p = 0;
                        break;
                    }

                }
                sysRepair += p;
            }
            return sysRepair;
        }
        public override int GetHashCode()
        {
            return m_diagnoses.GetHashCode();
        }

    }
}
