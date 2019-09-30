using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ModelBasedDiagnosis
{
    class MDPPlanner : HeuristicBatchPlanner
    {
        private Dictionary<int, MDPState> optimalStateInLevel = new Dictionary<int, MDPState>();
        private Dictionary<MDPState, Dictionary<RepairAction, MDPState>> nextStatesDic = new Dictionary<MDPState, Dictionary<RepairAction, MDPState>>();
        private Random rnd;
        private int numOfDiag;
        private int nodesCounter;
        public int Iterations { get; set; }
        public int Depth { get; set; }
        public MDPPlanner(RepairActionSearcher repairActionSearcher, BatchCostEstimator costeEstimator)
            : base(repairActionSearcher, costeEstimator)
        {
            rnd = new Random();
            IterationDetails = new BatchPlannerIterationDetails();
            Iterations = 100; //defult parameter
            Depth = 2; //defult parameter
        }
        public MDPPlanner(RepairActionSearcher repairActionSearcher, BatchCostEstimator costeEstimator, int iterations, int depth)
    : this(repairActionSearcher, costeEstimator)
        {
            Iterations = iterations;
            Depth = depth;
        }
        public override RepairAction Plan(SystemState state)
        {
            ResetProperties();
            nodesCounter = 1;
            RepairAction chosenAction = null;
            nextStatesDic.Clear();
            numOfDiag = state.Diagnoses.Count;

            if (state == null || state.Diagnoses == null || numOfDiag == 0)
                return chosenAction;

            if (numOfDiag == 1)
            {
                chosenAction = new RepairAction(state.Diagnoses.Diagnoses.First().Comps);
                FillIterationDetails(chosenAction, nodesCounter, costEstimator.WastedCostUtility(chosenAction, state));
                return chosenAction;
            }

            MDPState initMDPState = new MDPState(state, 0);

            for (int j = 0; j < Iterations; j++)
            {
                if (Stop())
                    break;

                performOneIteration(initMDPState);
            }

            chosenAction = initMDPState.GetBestValueAction();
            FillIterationDetails(chosenAction, nodesCounter, costEstimator.WastedCostUtility(chosenAction, state));

            return chosenAction;
        }
        private void performOneIteration(MDPState state)
        {
            optimalStateInLevel.Clear();
            optimalStateInLevel[0] = state;
            int j;
            for (j = 1; j <= Depth; j++)
            {
                MDPState optimalState = descendByUCB(optimalStateInLevel[j - 1]);
                if (optimalState == null)
                    break;
                optimalStateInLevel[j] = optimalState;
                //if system repaired - break
                if (optimalStateInLevel[j].SystemRepairedState || optimalStateInLevel[j].State.Diagnoses.Count == 0)
                    break;
            }

            //the value should be 0 if system repaired and FN if not.
            double value = 0;
            j = optimalStateInLevel.Keys.Max();
            nodesCounter += j;
            if (!optimalStateInLevel[j].SystemRepairedState)
                value = -costEstimator.StateCost(optimalStateInLevel[j].State);
            updateValue(value, j - 1);
        }
        private MDPState descendByUCB(MDPState state)
        {
            RepairActionsSet actions = repairActionSearcher.ComputePossibleAcions(state.State);

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
                    U = (-costEstimator.FPCost(currentAction, state.State.HealthState)) / (currentAction.Count);

                else
                {
                    double alpha = state.GetStateActionValue(currentAction);
                    U = alpha + numOfDiag * (Math.Sqrt(Math.Log10(state.NumberOfVisits) / actionNb));
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
            bool systemNotFixed = stateTransition(state, action);

            if (systemNotFixed)
            {
                if (nextStatesDic.ContainsKey(state))
                    return nextStatesDic[state][action];

                nextStatesDic.Add(state, new Dictionary<RepairAction, MDPState>());
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
        private bool stateTransition(MDPState state, RepairAction action) //return true if the system is not fixed in the next state 
        {
            double sysRepair = state.State.SystemRepair(action);
            if (sysRepair == 0)
                return true;
            else if (sysRepair == 1)
                return false;
            else
            {
                if (rnd.NextDouble() > sysRepair)
                    return true;
                else
                    return false;
            }
        }
        public override string Algorithm()
        {
            return "MDP depth=" + Depth + " iterations= " + Iterations + " " + repairActionSearcher.Type();
        }

    }
}
