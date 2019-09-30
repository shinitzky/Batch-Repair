using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    class RandomBatchPlanner: BatchPlanner
    {
        private Random rnd;
        public RandomBatchPlanner()
        {
            rnd = new Random();
        }
        public override RepairAction Plan(SystemState state)
        {
            if (state == null || state.Diagnoses == null || state.Diagnoses.Count == 0)
                return null;
            int numOfDiagnoses = state.Diagnoses.Count;
            int diagIndex = rnd.Next(numOfDiagnoses);
            Diagnosis chosenDiag = state.Diagnoses.Diagnoses.ToArray()[diagIndex];
            return new RepairAction(chosenDiag.Comps);
        }
        public override void CreateIterationDetailsFile(string fileName)
        {
            return;
        }
        public override void ExportIterationDetails(string system, int obs, int iteration, bool isFixed)
        {
            return;
        }
        public override string Algorithm()
        {
            return "RandomBatchPlanner";
        }
    }
}
