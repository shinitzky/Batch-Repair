using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis.PhysiotherapyDomain
{
    class PhysioCaseInstance
    {
        public int Id { get; set; }
        public Diagnosis RealDiagnosis { get; set; }
        public DiagnosisSet Diagnoses { get; set; }
        public List<PhysioComp> Observation { get; set; }

        public int RealDiagCardinality { get; set; }
        public int MinCardinality { get; set; }
        public int MaxCardinality { get; set; }

        public bool ValidCaseInstance()
        {
            if (Id > 0 && Diagnoses != null && Diagnoses.Count != 0 && Observation != null && Observation.Count != 0 && RealDiagnosis != null && RealDiagnosis.Diag.Count != 0)
                return true;
            return false;
        }
    }
}
