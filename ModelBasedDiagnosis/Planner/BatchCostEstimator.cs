using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    public abstract class BatchCostEstimator
    {
        public double Overhead { get; set; }

        public double WastedCostUtility(RepairAction repairAction, SystemState state)
        {
            if (state == null || repairAction == null)
                return 0;
            double fp = FPCost(repairAction, state.HealthState);
            double sysNotRep = (1 - state.SystemRepair(repairAction));
            double fn = FNCost(repairAction, state);
            return fp + (sysNotRep * fn);
        }
        public double ComputeRepairCost(RepairAction repairAction)
        {
            double ans = 0;
            if (repairAction == null || repairAction.Count == 0)
                return ans;
            foreach (Comp c in repairAction.R)
            {
                if (c != null)
                    ans += c.Cost;
            }
            ans += Overhead;
            return ans;
        }

        public double FPCost(RepairAction repairAction,HealthStateVector HealthState)
        {
            double ans = 0;
            if (repairAction == null || repairAction.Count == 0 || HealthState == null || HealthState.Count == 0)
                return ans;
            foreach (Comp c in repairAction.R)
            {
                ans += ((1- HealthState.GetCompHealthState(c)) * c.Cost);
            }
            return ans;
        }

        public abstract double StateCost(SystemState state);
        public abstract double FNCost(RepairAction repairAction, SystemState state);
        public abstract string Type();
    }
}
