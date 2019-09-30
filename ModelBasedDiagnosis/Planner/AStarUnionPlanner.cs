using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ModelBasedDiagnosis
{
    class AStarUnionPlanner: AStarPlanner
    {
        public AStarUnionPlanner(BatchCostEstimator costEstimator, int bound): base (costEstimator, bound){}
        public AStarUnionPlanner(BatchCostEstimator costEstimator) : base(costEstimator) {}
        public override RepairAction Plan(SystemState state)
        {
            ResetProperties();

            AStarVertex bestRepairAction = null; //its cost is the upper bound of the optimal solution
            PriorityQueue<AStarVertex> openList = new PriorityQueue<AStarVertex>(); //a priority queue with fvalue as the priority
            List<AStarVertex> closedList = new List<AStarVertex>();
            double fmax = 0; //the lower bound of the optimal solution
            double fmin = -1;

            //initial state
            foreach (Diagnosis diag in state.Diagnoses.Diagnoses)
            {
                AStarVertex v = new AStarVertex(new RepairAction(diag.Comps), costEstimator, state, 1);
                if (bestRepairAction == null || v.Wastedcost < bestRepairAction.Wastedcost)
                    bestRepairAction = v;
                if (fmin == -1)
                    fmin = v.FVal;
                else if (v.FVal < fmin)
                    fmin = v.FVal;
                openList.Enqueue(v, v.FVal);
            }
           
            while (!openList.IsEmpty())
            {
                AStarVertex bestV = (AStarVertex)openList.Dequeue(); //extract the best vertex from OPEN
                if (closedList.Contains(bestV))
                    continue;

                //update fmax
                if (bestV.FVal > fmax)
                    fmax = bestV.FVal;

                //halting condition
                if (bestRepairAction != null && fmax >= bestRepairAction.Wastedcost)
                    break;
                if (bestV.FVal == bestV.Wastedcost) //no point to create his children
                    continue;//insert to closed list?
                currentDepth = bestV.Depth;

                if (Stop())
                    break;

                //create all of best V childs and insert them to OPEN
                foreach (Diagnosis diag in state.Diagnoses.Diagnoses)
                {
                    if (Stop())
                        break;
                    List<Comp> listd = new List<Comp>(bestV.Action.R);
                    foreach (Comp c in diag.Comps)
                    {
                        if (!listd.Contains(c))
                            listd.Add(c);
                    }

                    if (listd.Count == bestV.Action.R.Count)
                        continue;

                    AStarVertex v = new AStarVertex(new RepairAction(listd), costEstimator, state, bestV.Depth+1);
                    openList.Enqueue(v, v.FVal);
                    if (v.Wastedcost < bestRepairAction.Wastedcost)
                        bestRepairAction = v;
                }
                closedList.Add(bestV); //add best vertex to CLOSED
            }

            FillIterationDetails(bestRepairAction.Action, closedList.Count +1, bestRepairAction.Wastedcost);
            ((AStarIterationDetails)IterationDetails).ChosenFval = bestRepairAction.FVal; 
            ((AStarIterationDetails)IterationDetails).Fminmax = fmax;
            ((AStarIterationDetails)IterationDetails).Fminstart = fmin;

            return bestRepairAction.Action;
        }

       
        public override string Algorithm()
        {
            return "AStar_Union";
        }


    }
}
