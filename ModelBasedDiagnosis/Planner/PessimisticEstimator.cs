using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    class PessimisticEstimator : BatchCostEstimator
    {
        public enum PessimisticEstimationType { regular, nextState, worstCase } //change names!!
        private PessimisticEstimationType type;
        public enum FFPType { noFFP, FFP, FFPplusOverhead}
        public FFPType ffpType { get; private set; }
        public PessimisticEstimator(double overhead) : this(overhead, PessimisticEstimationType.regular, FFPType.noFFP){ }
        public PessimisticEstimator(double overhead, PessimisticEstimationType type) : this(overhead, type, FFPType.noFFP) { }
        public PessimisticEstimator(double overhead, PessimisticEstimationType type, FFPType ffpType)
        {
            Overhead = overhead;
            this.type = type;
            this.ffpType = ffpType;
        }
        public override double StateCost(SystemState state) //esuming the systemState is already without R (the next)
        {
            double stateCost = 0;
            if (state == null || state.HealthState == null || state.HealthState.Count == 0)
                return stateCost;

            for(int i=0; i<state.HealthState.Count; i++)
            {
                double h = state.HealthState.CurrentHealthState[i];
                Comp c = state.HealthState.Components[i];
                stateCost += (h * (c.Cost+Overhead));
            }           
            return stateCost;
        }
        public override double FNCost(RepairAction repairAction, SystemState state)
        {
            if (state == null || repairAction == null || repairAction.Count == 0)
                return 0; 
            if (type == PessimisticEstimationType.regular)
                return calcFNCostRegular(repairAction, state.HealthState);
            else if (type == PessimisticEstimationType.worstCase)
                return calcFNCostWorstCase(repairAction, state.Diagnoses.Components);
            else
                return calcFNCostNextState(repairAction, state);
        }
        private double calcFNCostRegular(RepairAction repairAction, HealthStateVector HealthState)
        {
            double FN = 0;
            double FFP = 0;
            if (HealthState == null || HealthState.Count == 0)
                return 0;
            for (int i = 0; i < HealthState.Count; i++)
            {
                Comp c = HealthState.Components[i];
                double h = HealthState.CurrentHealthState[i];
                if (h == 0)
                    continue;
                if (!repairAction.Contains(c))
                {
                    FN += (h*Overhead);
                    if (ffpType == FFPType.FFP)
                        FFP += ((1 - h) * (c.Cost));
                    else if (ffpType == FFPType.FFPplusOverhead)
                        FFP += ((1 - h) * (c.Cost + Overhead));
                }
            }
            if (ffpType != FFPType.noFFP)
                return FN + FFP;
            return FN;
        }
        private double calcFNCostWorstCase(RepairAction repairAction, List<Comp> components)
        {
            double ans = 0;
            if (components == null || components.Count == 0)
                return ans;
            foreach(Comp c in components)
            {
                if (!repairAction.Contains(c))
                    ans += Overhead;
            }
            return ans;
        }
        private double calcFNCostNextState(RepairAction repairAction, SystemState state)
        {
            SystemState nextState = state.GetNextState(repairAction);
            if (nextState.Diagnoses.Count != state.Diagnoses.Count)
                return calcFNCostRegular(repairAction, nextState.HealthState);
            else
                return calcFNCostRegular(repairAction, state.HealthState);
        }
        public override string Type()
        {
            string ans = "Pessimistic";
            if (type != PessimisticEstimationType.regular)
                ans += "_" + type.ToString();
            if (ffpType == FFPType.FFP)
                ans += "_withFFP";
            else if(ffpType == FFPType.FFPplusOverhead)
                ans += "_withFFPplusOverhead";
            return ans;
        }
    }
}
