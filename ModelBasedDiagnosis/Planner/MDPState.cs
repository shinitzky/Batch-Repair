using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    class MDPState
    {
        public int Level { get; set; }
        public SystemState State { get; set; }
        public RepairAction LatestChosenAction { get; set; } 
        public bool SystemRepairedState { get; set; }
        public int NumberOfVisits { get; set; }
        private Dictionary<RepairAction, int> numberOfTimesChosen;
        private Dictionary<RepairAction, double> stateActionValue;
        public MDPState()
        {
            SystemRepairedState = false;
            NumberOfVisits = 0;
            numberOfTimesChosen = new Dictionary<RepairAction, int>();
            stateActionValue = new Dictionary<RepairAction, double>();
        }

        public MDPState(SystemState state, int level) : this()
        {
            State = state;
            Level = level;
        }
        public int GetNumOfTimesChosen(RepairAction action)
        {
            int nb;
            if (numberOfTimesChosen.TryGetValue(action, out nb))
                return numberOfTimesChosen[action];
            else
                return 0;
        }
        public void UpdateStateActionValue(RepairAction action, double value)
        {
            if (!numberOfTimesChosen.ContainsKey(action))
                numberOfTimesChosen.Add(action, 0);
            numberOfTimesChosen[action]++;
            if (!stateActionValue.ContainsKey(action))
                stateActionValue.Add(action, 0);
            stateActionValue[action] += value;
        }
        public double GetStateActionValue(RepairAction action)
        {
            double value;
            int actionNb;
            if (stateActionValue.TryGetValue(action, out value))
            {
                double ans = stateActionValue[action];
                if (numberOfTimesChosen.TryGetValue(action, out actionNb))
                    ans = ans / actionNb;
                return ans;
            }
            else
                return 0;
        }
        public RepairAction GetBestValueAction() //the largest
        {
            if (stateActionValue.Count == 0)
                return LatestChosenAction;
            RepairAction bestValAction = null;
            double bestVal = -10000000;
            foreach (RepairAction action in stateActionValue.Keys)
            {
                double val = GetStateActionValue(action);
                if (bestValAction == null || val > bestVal)
                {
                    bestVal = val;
                    bestValAction = action;
                }
            }
            if (bestValAction == null)
                return LatestChosenAction;
            return bestValAction;
        }
        public bool IsEmptyState()
        {
            if (State == null || State.Diagnoses == null || State.Diagnoses.Count == 0)
                return true;
            return false;
        }
        public override bool Equals(object obj)
        {
            if (obj is MDPState)
            {
                SystemState state = ((MDPState)obj).State;
                if (state == null || state.Diagnoses == null || State.Diagnoses == null)
                {
                    return false;
                }
                return State.Equals(state);
            }
            else
                return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return State.GetHashCode();
        }

    }
}
