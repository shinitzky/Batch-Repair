using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    public class OptimisticEstimator:BatchCostEstimator
    {
        public OptimisticEstimator(double overhead)
        {
            Overhead = overhead;
        }
        public override double StateCost(SystemState state)
        {
            return Overhead;
        }
        public override double FNCost(RepairAction repairAction, SystemState state)
        {
            return Overhead;
        }
        public override string Type()
        {
            return "Optimistic";
        }
    }
}
