using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    class BatchPlannerIterationDetails
    {
        public RepairAction ChosenRepairAction { get; set; }
        public int NumOfExpanded { get; set; }
        public double RunTime { get; set; }
        public double WastedCost { get; set; }
        public double Cost { get; set; }
        public bool FoundOpt { get; set; }
    }
}
