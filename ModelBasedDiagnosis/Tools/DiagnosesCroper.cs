using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    public enum DiagnosesCroperType { hs, wc, wce}

    public class DiagnosesCroper
    {
        public DiagnosesCroperType CroperType { get; set; }
        private int m_croplimit;
        public int DiagCropLimit
        {
            get
            {
                return m_croplimit;
            }
            set
            {
                if (value > 0)
                    m_croplimit = value;
            }
        }
        public DiagnosesCroper(DiagnosesCroperType type)
        {
            DiagCropLimit = 30;
            CroperType = type;
        }
        public DiagnosisSet CropDiagnoses(SystemState systemState)
        {
            if (CroperType == DiagnosesCroperType.hs)
                return cropHS(systemState);
            return null;
        }
        private DiagnosisSet cropHS(SystemState systemState)
        {
            DiagnosisSet newDiagSet = new DiagnosisSet();
            PriorityQueue<Diagnosis> diagPrQ = new PriorityQueue<Diagnosis>();
            foreach(Diagnosis diag in systemState.Diagnoses.Diagnoses)
            {
                double sumHS = 0;
                foreach(Comp c in diag.Comps)
                {
                    sumHS += systemState.HealthState.GetCompHealthState(c);
                }
                diagPrQ.Enqueue(diag, -sumHS);
            }
            while(newDiagSet.Count < DiagCropLimit && !diagPrQ.IsEmpty())
            {
                Diagnosis diag = (Diagnosis)diagPrQ.Dequeue();
                if (diag != null)
                    newDiagSet.AddDiagnosis(diag);
            }
            int compCount = systemState.Diagnoses.Components.Count;
            while(newDiagSet.Components.Count < compCount && !diagPrQ.IsEmpty())
            {
                Diagnosis diag = (Diagnosis)diagPrQ.Dequeue();
                if (diag != null)
                    newDiagSet.AddDiagnosis(diag);
            }
            return newDiagSet;
        }
    }
}
