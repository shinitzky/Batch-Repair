using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    class AStarVertex
    {
        public static int Bound;
        public bool Bounded
        {
            get
            {
                if (Bound > 0)
                    return true;
                return false;
            }
        }

        public RepairAction Action;
        public double FVal;
        public double Wastedcost;
        public int Depth;

        public AStarVertex(RepairAction repairAction, BatchCostEstimator costEstimator, SystemState state, int depth)
        {
            if (repairAction == null || repairAction.Count == 0)
            {
                Action = null;
                FVal = -1;
                Depth = 0;
            }

            else
            {
                Action = repairAction;
                Wastedcost = costEstimator.WastedCostUtility(repairAction, state);
                Depth = depth;
                if (Bounded && Depth == Bound)
                    FVal = Wastedcost;
                else
                    calcFval(costEstimator, state);
            }
        }

        public AStarVertex(RepairAction repairAction, BatchCostEstimator costEstimator, SystemState state)
            : this(repairAction, costEstimator, state, 0) { }

        //this function computes all the list of all the diagnoses which are not a subset of repairAction
        private List<Diagnosis> computeDiagNotSubV(SystemState state)
        {
            List<Diagnosis> ans = new List<Diagnosis>();
            foreach (Diagnosis diag in state.Diagnoses.Diagnoses)
            {
                foreach (Comp c in diag.Comps)
                {
                    if (!Action.Contains(c))
                    {
                        ans.Add(diag);
                        break;
                    }
                }
            }
            return ans;
        }

        //this function calculates the fvalue of this vertex  
        private void calcFval(BatchCostEstimator costEstimator, SystemState state)
        {
            double overhead = costEstimator.Overhead;
            List<double> l_fp = new List<double>();
            List<double> l_sr = new List<double>();

            foreach (Comp c in state.Diagnoses.Components)
            {
                if (!Action.Contains(c))
                {
                    double hs = state.HealthState.GetCompHealthState(c);
                    double cost = ((1 - hs) * c.Cost);

                    l_fp.Add(cost);
                    l_sr.Add(hs);
                }
            }

            l_sr.Sort();
            l_sr.Reverse();
            l_fp.Sort();

            double fpv = costEstimator.FPCost(Action, state.HealthState);
            double srv = state.SystemRepair(Action);
            double fmin = Wastedcost;
            double sum_hs = l_sr.Sum();
            double sum_sr = 0;
            double sum_dfp = 0;
            int b = Bound - Depth;
            int size = Math.Min(l_fp.Count, l_sr.Count);

            if (b > size || !Bounded)
                b = size;

            for (int i = 0; i < b; i++)
            {
                sum_dfp += l_fp[i];
                sum_sr += l_sr[i];

                double srvi = sum_sr + srv;
                if (srvi > 1)
                    srvi = 1;

                double fnvi = (sum_hs - sum_sr) * overhead;

                if (fpv + ((1 - srv) * fnvi) > Wastedcost)
                    fnvi = overhead;
                if (fpv + ((1 - srv) * fnvi) > Wastedcost)
                    fnvi = 0;

                double fvi = fpv + sum_dfp + ((1 - srvi) * fnvi);

                if (fvi < fmin)
                    fmin = fvi;
            }
            FVal = fmin;
        }


        public override int GetHashCode()
        {
            if (Action == null || Action.Count == 0)
                return 17;
            unchecked
            {
                int hash = 17;
                foreach (Comp c in Action.R)
                {
                    hash = hash * 23 + c.Id * 13;
                }
                return hash;
            }
        }

        public override string ToString()
        {
            if (Action != null)
                return Action.ToString();
            return null;
        }

        public override bool Equals(object obj)
        {
            if (obj is AStarVertex)
            {
                AStarVertex v = (AStarVertex)obj;
                if (v == null || v.Action == null || v.Action.Count != Action.Count)
                    return false;
                foreach (Comp c in v.Action.R)
                {
                    if (!Action.Contains(c))
                        return false;
                }
                return true;
            }
            else
                return base.Equals(obj);
        }
    }

}
