using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ModelBasedDiagnosis
{
    class MDPPlannerTester : HeuristicBatchPlanner
    {
        private Dictionary<int, MDPState> optimalStateInLevel = new Dictionary<int, MDPState>();
        private Dictionary<MDPState, Dictionary<RepairAction, MDPState>> nextStatesDic = new Dictionary<MDPState, Dictionary<RepairAction, MDPState>>();
        private Random rnd;
        public int Iterations { get; set; }
        public int RequestedDepth { get; set; }
        //for testing:
        private int actionNbBest = 0;
        private double valueBest = 0;
        private double initStateNB = 0;
        private double uBestAction = 0;
        private int plannerIterationCounter = 0;
        //to do - clear function
        private int numOfDiag;

        public MDPPlannerTester(RepairActionSearcher repairActionSearcher, BatchCostEstimator costeEstimator)
            : base(repairActionSearcher, costeEstimator)
        {
            rnd = new Random();
            IterationDetails = new BatchPlannerIterationDetails();
            Iterations = 5;
            RequestedDepth = 5;
        }
        public MDPPlannerTester(RepairActionSearcher repairActionSearcher, BatchCostEstimator costeEstimator, int iterations, int requestedDepth)
    : this(repairActionSearcher, costeEstimator)
        {
            Iterations = iterations;
            RequestedDepth = requestedDepth;
        }
        public MDPPlannerTester(RepairActionSearcher repairActionSearcher, BatchCostEstimator costeEstimator, int iterations)
: this(repairActionSearcher, costeEstimator, iterations, 5)
        {
        }

        public override RepairAction Plan(SystemState state)
        {
            //for testing
            plannerIterationCounter++;
            IterationDetails = new BatchPlannerIterationDetails();
            nextStatesDic.Clear();
            iteraionDetailsFilled = false;
            RepairAction chosenAction = null;
            foundOpt = true;
            currentDepth = 0;
            if (state == null || state.Diagnoses == null || state.Diagnoses.Count == 0)
                return chosenAction;
            totalStopWatch.Restart();
            if (state.Diagnoses.Count == 1)
            {
                chosenAction = new RepairAction(state.Diagnoses.Diagnoses.First().Comps);
                FillIterationDetails(chosenAction, 1, costEstimator.WastedCostUtility(chosenAction, state));
                return chosenAction;

            }
            /* SystemState sysState = state;
             bool cropDiagnoses = true;
             if (DiagCroper == null || state.Diagnoses.Count <= DiagCropLimit)
                 cropDiagnoses = false;
             if(cropDiagnoses)
             {
                 DiagCroper.DiagCropLimit = DiagCropLimit;
                 DiagnosisSet newDiagSet = DiagCroper.CropDiagnoses(state);
                 if(newDiagSet!=null && newDiagSet.Count!=0)
                 {
                     sysState = new SystemState(newDiagSet.Components); //!!
                     sysState.Diagnoses = newDiagSet; //!!
                 }
             }*/
            numOfDiag = state.Diagnoses.Count;
            int counter = 0;
            CSVExport iterationsTable = new CSVExport(); //for testing
            MDPState mdpState = new MDPState(state, 0);
            for (int j = 0; j < Iterations; j++) //playSeq
            {
                if (Stop())
                    break;
                playOneSeq(mdpState);
                //for testing
                if (mdpState.LatestChosenAction != null || mdpState.LatestChosenAction.Count > 0)
                {
                    iterationsTable.AddRow();
                    iterationsTable["iteration"] = j;
                    iterationsTable["action"] = mdpState.LatestChosenAction.ToString();
                    iterationsTable["U"] = uBestAction;
                    iterationsTable["stateActionValue"] = valueBest;
                    iterationsTable["actionNB"] = actionNbBest;
                    iterationsTable["stateNB"] = initStateNB;
                    if (actionNbBest != 0)
                    {
                        double val = Math.Log10(initStateNB);
                        iterationsTable["log10(stateNB)"] = val;
                        val = val / actionNbBest;
                        iterationsTable["log10(stateNB)/actionNB"] = val;
                        val = Math.Sqrt(val);
                        iterationsTable["sqrt(log10(stateNB)/actionNB)"] = val;
                        val = numOfDiag * val;
                        iterationsTable["alpha*sqrt(log10(stateNB)/actionNB)"] = val;
                    }
                    else
                    {
                        iterationsTable["log10(stateNB)"] = 0;
                        iterationsTable["log10(stateNB)/actionNB"] = 0;
                        iterationsTable["sqrt(log10(stateNB)/actionNB)"] = 0;
                        iterationsTable["alpha*sqrt(log10(stateNB)/actionNB)"] = 0;
                    }
                }
                counter++;
            }
            //for testing
            iterationsTable.ExportToFile("MDPiterations" + plannerIterationCounter + ".csv");

            chosenAction = mdpState.GetBestValueAction();
            FillIterationDetails(chosenAction, counter, costEstimator.WastedCostUtility(chosenAction, state));

            return chosenAction;
        }


        private void playOneSeq(MDPState state)
        {
            optimalStateInLevel.Clear();
            optimalStateInLevel[0] = state;
            int j = 0;
            for (j = 1; j <= RequestedDepth; j++)
            {
                MDPState optimalState = descendByUCB1(optimalStateInLevel[j - 1]);
                if (optimalState == null)
                    break;
                optimalStateInLevel[j] = optimalState;
                //if system repaired - break
                if (optimalStateInLevel[j].SystemRepairedState || optimalStateInLevel[j].State.Diagnoses.Count == 0) //!!
                    break;
            }
            //the value inside should be 0 if system repaired and if not should be FN
            double value = 0;
            j = optimalStateInLevel.Keys.Max();//!!
            if (!optimalStateInLevel[j].SystemRepairedState)
                value = -costEstimator.StateCost(optimalStateInLevel[j].State);
            updateValue(value, j - 1); //!!
        }


        private MDPState descendByUCB1(MDPState state)
        {
            RepairActionsSet actions = repairActionSearcher.ComputePossibleAcions(state.State); //change maybe
            if (actions == null)
                return null;
            RepairAction currentAction = actions.NextAction();
            double U;
            RepairAction bestAction = null;
            double ans = double.MinValue;
            while (currentAction != null && currentAction.Count != 0)
            {
                int actionNb = state.GetNumOfTimesChosen(currentAction);
                if (actionNb == 0)
                    U = (-costEstimator.FPCost(currentAction, state.State.HealthState)) /(currentAction.Count);
                //U = -(costEstimator.ComputeRepairCost(currentAction) + costEstimator.FNCost(currentAction, state.State)); 
                //U = 0; //for testing
                //U = 10000;
                else
                {
                    //double alpha = state.GetStateActionValue(currentAction); //it's already divided by actionNb
                    //changes 17/5
                    double alpha = state.GetStateActionValue(currentAction);
                    U = alpha + numOfDiag * (Math.Sqrt(Math.Log10(state.NumberOfVisits) / actionNb));
                  //  U = alpha + 2 * (Math.Sqrt(Math.Log10(state.NumberOfVisits) / actionNb));

                    //U = alpha - alpha * (Math.Sqrt(Math.Log10(state.NumberOfVisits) / actionNb));
                    //U = (state.GetStateActionValue(currentAction) / actionNb) + 2 * (Math.Sqrt(Math.Log10(state.NumberOfVisits) / actionNb));
                    //U = Math.Sqrt(Math.Log10(state.NumberOfVisits) / actionNb); //for testing
                }

                if (U > ans) //update best action
                {
                    bestAction = currentAction;
                    ans = U;
                }
                currentAction = actions.NextAction();
            }
            MDPState nextState = null;
            if (bestAction != null)
            {
                //for testing
                if (state.Level == 0)
                {
                    initStateNB = state.NumberOfVisits;
                    valueBest = state.GetStateActionValue(bestAction);
                    actionNbBest = state.GetNumOfTimesChosen(bestAction);
                    uBestAction = ans;
                }

                state.LatestChosenAction = bestAction;
                nextState = ComputeNextState(state, bestAction);
            }
            state.NumberOfVisits++;
            return nextState;
        }

        private void updateValue(double value, int level)
        {
            for (int i = level; i >= 0; i--)
            {
                value += -costEstimator.ComputeRepairCost(optimalStateInLevel[i].LatestChosenAction);
                optimalStateInLevel[i].UpdateStateActionValue(optimalStateInLevel[i].LatestChosenAction, value);
            }
        }

        public MDPState ComputeNextState(MDPState state, RepairAction action)
        {
            double sysRepair = state.State.SystemRepair(action);
            bool returnNextState;
            if (sysRepair == 0)
                returnNextState = true;
            else if (sysRepair == 1)
                returnNextState = false;
            else
            {
                if (rnd.NextDouble() > sysRepair)
                    returnNextState = true;
                else
                    returnNextState = false;
            }
            if (returnNextState)
            {
                if (!nextStatesDic.ContainsKey(state))
                    nextStatesDic.Add(state, new Dictionary<RepairAction, MDPState>());
                else if (nextStatesDic[state].ContainsKey(action))
                    return nextStatesDic[state][action];
                SystemState nextSysState = state.State.GetNextState(action);
                MDPState nextState = new MDPState(nextSysState, state.Level + 1);
                nextStatesDic[state].Add(action, nextState);
                return nextState;
            }
            else
            {
                MDPState nextState = new MDPState();
                nextState.SystemRepairedState = true;
                nextState.Level = state.Level + 1;
                return nextState;
            }

        }

      
        public override string Algorithm()
        {
            return "MDPTester depth=" + RequestedDepth + " iterations= " + Iterations + " " + repairActionSearcher.Type();
        }

    }
}
