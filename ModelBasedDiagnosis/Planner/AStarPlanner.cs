using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    abstract class AStarPlanner : BatchPlanner
    {
        public AStarPlanner(BatchCostEstimator costEstimator)
        {
            this.costEstimator = costEstimator;
        }
        public AStarPlanner(BatchCostEstimator costEstimator, int bound): this(costEstimator)
        {
            Bound = bound;
        }
        public override void ResetProperties()
        {
            if(Bounded)
                AStarVertex.Bound = Bound;
            else
                AStarVertex.Bound = 0;
            IterationDetails = new AStarIterationDetails();
            iteraionDetailsFilled = false;
            foundOpt = true;
            currentDepth = 0;
            totalStopWatch.Restart();

        }

        public override void ExportIterationDetails(string system, int obs, int iteration, bool isFixed)
        {
            base.ExportIterationDetails(system, obs, iteration, isFixed);
            if (!iteraionDetailsFilled)
                return;
            myExport["Fval of chosen"] = ((AStarIterationDetails)IterationDetails).ChosenFval;
            myExport["Fminmax"] = ((AStarIterationDetails)IterationDetails).Fminmax;
            myExport["Fmin start"] = ((AStarIterationDetails)IterationDetails).Fminstart;
        }
        
    }
}
