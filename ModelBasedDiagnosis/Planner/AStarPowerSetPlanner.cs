using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    class AStarPowerSetPlanner: AStarPlanner
    {
        public AStarPowerSetPlanner(BatchCostEstimator costEstimator) : base(costEstimator){}
        public override RepairAction Plan(SystemState state)
        {
            ResetProperties();

            List<Comp> components = state.Diagnoses.Components;
            AStarVertex bestRepairAction = null; //its cost is the upper bound of the optimal solution
            PriorityQueue<AStarVertex> openList = new PriorityQueue<AStarVertex>(); //a priority queue with fvalue as the priority
            List<AStarVertex> closedList = new List<AStarVertex>();

            double fmax = 0; //the lower bound of the optimal solution
            double fmin = -1;

            //initial states
            foreach (Comp c in components)
            {
                //create vertex from component g and insert it to OPEN
                List<Comp> listg = new List<Comp>();
                listg.Add(c);
                AStarVertex v = new AStarVertex(new RepairAction(listg), costEstimator, state);
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
                    continue; //insert to closed list?
                if (Stop())
                    break;

                foreach (Comp c in components)
                {
                    if (Stop())
                        break;
                    if (!bestV.Action.R.Contains(c))
                    {
                        List<Comp> listg = new List<Comp>(bestV.Action.R);
                        listg.Add(c);
                        AStarVertex v = new AStarVertex(new RepairAction(listg), costEstimator, state);
                        openList.Enqueue(v, v.FVal);
                        if (v.Wastedcost < bestRepairAction.Wastedcost)
                            bestRepairAction = v;
                    }
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
            return "AStar_PowerSet";
        }
    }
}
