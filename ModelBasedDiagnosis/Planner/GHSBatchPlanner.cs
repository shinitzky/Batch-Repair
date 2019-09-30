using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ModelBasedDiagnosis
{
    class GHSBatchPlanner : BatchPlanner
    {
        public GHSBatchPlanner(BatchCostEstimator costEstimator)
        {
            this.costEstimator = costEstimator;
        }
        public GHSBatchPlanner(BatchCostEstimator costEstimator, int bound): this(costEstimator)
        {
            Bound = bound;
        }

        public override RepairAction Plan(SystemState state)
        {
            ResetProperties();

            if (state == null || state.Diagnoses == null || state.Diagnoses.Count == 0)
                return null;
            
            //improve the code appearance 

            List<Comp> bestRepairAction = new List<Comp>();
            SystemState currState = state;
            double totalUtility = double.MaxValue;
            List<Comp> chosenAction;
            int expandedCounter = 0;
            while (!Stop())
            {
                double min = double.MaxValue;
                chosenAction = null;
                foreach (Diagnosis diagnosis in currState.Diagnoses.Diagnoses)
                {
                    if (Stop())
                        break;
                    List<Comp> action = new List<Comp>(diagnosis.Comps);
                    if (action == null || action.Count == 0)
                        continue;
                    List<Comp> testedAction = new List<Comp>(action);
                    if(bestRepairAction != null && bestRepairAction.Count > 0)
                        testedAction.AddRange(bestRepairAction);
                       
                    double utility = costEstimator.WastedCostUtility(new RepairAction(testedAction), state); //one option: action,currState, second option: testedAction,state
                    if (utility < min)
                    {
                        min = utility;
                        chosenAction = action;
                    }
                }
                currentDepth++;
                expandedCounter++;
                if (chosenAction != null && chosenAction.Count > 0)
                {
                    if (totalUtility < min || min == 0)
                    {
                        if(bestRepairAction.Count==0)
                            bestRepairAction.AddRange(chosenAction);
                        break;
                    }
                    bestRepairAction.AddRange(chosenAction);
                    currState = currState.GetNextState(new RepairAction (chosenAction));
                    totalUtility = min;
                }
                else
                    break;

            }
            RepairAction chosenRepairAction = new RepairAction(bestRepairAction);
            FillIterationDetails(chosenRepairAction, expandedCounter, totalUtility);
            return chosenRepairAction;
        }
        public override string Algorithm()
        {
            return "GHS";
            
        }
    }
}
