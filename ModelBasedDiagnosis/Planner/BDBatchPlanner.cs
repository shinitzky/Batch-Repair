using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    class BDBatchPlanner:BatchPlanner
    {
        public override RepairAction Plan(SystemState state)
        {
            if (state == null || state.Diagnoses == null || state.Diagnoses.Count == 0)
                return null;
            double max = -1;
            RepairAction ans = null;
            foreach (Diagnosis diag in state.Diagnoses.Diagnoses)
            {
                if (diag.Probability > max)
                {
                    ans = new RepairAction(diag.Comps);
                    max = diag.Probability;
                }
            }
            return ans;
        }
        public override void ExportIterationDetails(string system, int obs, int iteration, bool isFixed)
        {
            return;
        }
        public override string Algorithm()
        {
            return "BatchBestDiagnosis";

        }
        
    }
}
