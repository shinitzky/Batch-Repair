using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    interface IDiagnoser
    {
        DiagnosisSet FindDiagnoses(Observation observation);
        bool IsConsistent(Observation observation, Diagnosis diagnosis);
    }
}
