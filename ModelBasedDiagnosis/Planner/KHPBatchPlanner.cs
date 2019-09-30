using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    class KHPBatchPlanner : BatchPlanner
    {
        private int k;
        public KHPBatchPlanner(int k, BatchCostEstimator bce)
        {
            this.k = k;
            costEstimator = bce;
        }
        public override RepairAction Plan(SystemState state)
        {
            InitialParameters();
            if (state == null || state.HealthState == null || state.HealthState.Count == 0)
                return null;
            List<Comp> ans = new List<Comp>();
            List<double> hsVector = new List<double>(state.HealthState.CurrentHealthState);
            int counter = 0;
            double value = 0;
            for (int i = 0; i < k; i++)
            {
                double max = 0;
                Comp comp = null;
                int index = -1;
                for (int j = 0; j < hsVector.Count; j++)
                {
                    counter++;
                    Comp c = state.HealthState.Components[j];
                    double hs = hsVector[j];
                    if (hs > max)
                    {
                        max = hs;
                        comp = c;
                        index = j;
                    }
                }
                if (comp != null)
                {
                    value += max;
                    ans.Add(comp);
                    hsVector[index] = 0;
                }

            }
            if (ans.Count == 0)
                return null;
            RepairAction bestRepairAction = new RepairAction(ans);
            FillIterationDetails(bestRepairAction, counter, value);
            return bestRepairAction;
        }
        public void InitialParameters()
        {
            IterationDetails = new BatchPlannerIterationDetails();
            iteraionDetailsFilled = false;
            foundOpt = true;
            currentDepth = 0;
            totalStopWatch.Restart();

        }
        /*public override void ExportIterationDetails(string system, int obs, int iteration, bool isFixed)
        {
            return;
        }*/
        public override string Algorithm()
        {
            return "KHigestProb_" + k;
        }
    }
}
