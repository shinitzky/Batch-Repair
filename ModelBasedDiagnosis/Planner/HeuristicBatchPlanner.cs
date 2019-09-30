using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ModelBasedDiagnosis
{
    class HeuristicBatchPlanner : BatchPlanner
    {
        protected RepairActionSearcher repairActionSearcher;
        public HeuristicBatchPlanner(RepairActionSearcher repairActionSearcher, BatchCostEstimator costEstimator)
        {
            this.costEstimator = costEstimator;
            this.repairActionSearcher = repairActionSearcher;
            Bound = repairActionSearcher.K;
        }

        public override RepairAction Plan(SystemState state)
        {
            ResetProperties();
            if (state == null || state.Diagnoses == null || state.Diagnoses.Count == 0)
                return null;
            RepairActionsSet actions = repairActionSearcher.ComputePossibleAcions(state);
            if (actions == null)
                return null;
            double min = double.MaxValue;
            RepairAction bestRepairAction = null;
            RepairAction action = actions.NextAction();
            int counter = 0;
            while (action != null && action.Count > 0)
            {
                counter++;
                if (Stop())
                    break;
                double val = costEstimator.WastedCostUtility(action, state);
                if (val < min)
                {
                    min = val;
                    bestRepairAction = action;
                }
                action = actions.NextAction();
            }
            FillIterationDetails(bestRepairAction, counter, min);
            return bestRepairAction;
        }

        public override string Algorithm()
        {
            return "HeuristicBP" + " " + repairActionSearcher.Type();
        }
    }
}
