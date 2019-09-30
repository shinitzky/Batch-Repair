using System;
using System.Diagnostics;

namespace ModelBasedDiagnosis
{
    abstract class BatchPlanner
    {
        public DiagnosesCroper DiagCroper { get; set; }
        public TimeSpan Timeout;
        protected BatchCostEstimator costEstimator;
      
        protected CSVExport myExport;
        public BatchPlannerIterationDetails IterationDetails { get; protected set; }
        protected bool iteraionDetailsFilled;
        protected Stopwatch totalStopWatch;
        protected bool foundOpt;
        protected int currentDepth;
        public int Bound { get; protected set; }
        public bool Bounded
        {
            get
            {
                if (Bound > 0)
                    return true;
                return false;
            }
        }
      
        private int croplimit;
        public int DiagCropLimit
        {
            get
            {
                return croplimit;
            }
            set
            {
                if (value > 0)
                    croplimit = value;
            }
        }
        public BatchPlanner()
        {
            totalStopWatch = new Stopwatch();
            iteraionDetailsFilled = false;
            Timeout = new TimeSpan(0, 5, 0);
            myExport = new CSVExport();
            DiagCropLimit = 100;
            Bound = -1;
            currentDepth = 0;
        }
        public abstract RepairAction Plan(SystemState state);
        public string Type()
        {
            string ans =  Algorithm() + " " + ObjectiveFunction();
            if (Bounded)
                ans += " Bound=" + Bound;
            return ans;

        }
        public abstract string Algorithm();
        public string ObjectiveFunction()
        {
            if (costEstimator == null)
                return "";
            return costEstimator.Type();
        }

        public override string ToString()
        {
            return Type();
        }
        public virtual void ExportIterationDetails(string system, int obs, int iteration, bool isFixed)
        {
            if (!iteraionDetailsFilled)
                return;
            myExport.AddRow();
            myExport["System"] = system;
            myExport["Observation"] = obs;
            myExport["Iteration"] = iteration;
            myExport["Is Fixed"] = isFixed.ToString();
            myExport["Runtime(ms)"] = IterationDetails.RunTime;
            myExport["# Expanded"] = IterationDetails.NumOfExpanded;
            myExport["Found opt"] = IterationDetails.FoundOpt.ToString();
            myExport["Chosen repair action"] = IterationDetails.ChosenRepairAction.ToString();
            myExport["Cost"] = IterationDetails.Cost;
            myExport["Wasted Cost"] = IterationDetails.WastedCost;
        }
        public virtual void CreateIterationDetailsFile(string fileName)
        {
            myExport.ExportToFile(fileName + ".csv");
            myExport = new CSVExport();
        }

        public virtual void FillIterationDetails(RepairAction chosenAction, int counter, double wastedCost)
        {
            iteraionDetailsFilled = true;
            totalStopWatch.Stop();
            IterationDetails.RunTime = totalStopWatch.Elapsed.TotalMilliseconds;
            IterationDetails.ChosenRepairAction = chosenAction;
            IterationDetails.Cost = costEstimator.ComputeRepairCost(chosenAction);
            IterationDetails.FoundOpt = foundOpt;
            IterationDetails.NumOfExpanded = counter;
            IterationDetails.WastedCost = wastedCost;
        }

        public virtual void ResetProperties()
        {
            IterationDetails = new BatchPlannerIterationDetails();
            iteraionDetailsFilled = false;
            foundOpt = true;
            currentDepth = 0;
            totalStopWatch.Restart();
        }
        public bool Stop()
        {
            if (Bounded && (currentDepth >= Bound))
                return true;
            if (totalStopWatch.Elapsed >= Timeout)
            {
                foundOpt = false;
                return true;
            }
            return false;
        }
    }
}
