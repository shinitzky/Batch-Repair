using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace ModelBasedDiagnosis.PhysiotherapyDomain
{
    class PhysiotherapySimulator
    {
        private PhysiotherapyCaseParser caseParser;

        public PhysiotherapySimulator()
        {
            caseParser = new PhysiotherapyCaseParser();
        }

        public void BatchRepair(string casesFilesDir, BatchPlanner planner, double overhead, int maxNumOfDiag)
        {
            Console.WriteLine(planner.Type() + " o=" + overhead);
            Stopwatch stopwatch = new Stopwatch();
            CSVExport myExport = new CSVExport();
            List<PhysioCaseInstance> physioCases = new List<PhysioCaseInstance>();
            List<string> files = Directory.GetFiles(casesFilesDir).ToList();
            foreach(string file in files)
            {
                PhysioCaseInstance physioCase = caseParser.ParseCase(file);
                if (physioCase != null)
                    physioCases.Add(physioCase);
            }
            foreach(PhysioCaseInstance physioCase in physioCases)
            {
                if (physioCase.Diagnoses.Count < 2 || (physioCase.Diagnoses.Count > maxNumOfDiag && maxNumOfDiag>0))//!!
                    continue;
               // Console.WriteLine(physioCase.Id); //!!
                int iterationCounter = 0;
                double totalCost = 0;
                int numberOfFixed = 0;
                int expanded = 0;
                bool foundOpt = true;
                bool finished = false;
                List<Comp> toRepair = new List<Comp>(physioCase.RealDiagnosis.Comps);
                SystemState currSystemState = new SystemState(physioCase.Diagnoses.Components);
                currSystemState.Diagnoses = physioCase.Diagnoses; //!! check for bug
                stopwatch.Start();
                while (!finished)
                {
                    RepairAction action = planner.Plan(currSystemState);
                    if (action == null)
                        break; //!!
                    iterationCounter++;
                    totalCost += overhead;
                    numberOfFixed += action.Count;
                    foreach (Comp comp in action.R)
                    {
                        totalCost += comp.Cost;
                        if (toRepair.Contains(comp))
                            toRepair.Remove(comp);
                    }
                    if (toRepair.Count > 0)
                        currSystemState.SetNextState(action);
                    else
                        finished = true;
                    if(iterationCounter == 1)
                    {
                        expanded = planner.IterationDetails.NumOfExpanded;
                        foundOpt = planner.IterationDetails.FoundOpt;
                    }
                    planner.ExportIterationDetails("1", physioCase.Id, iterationCounter, finished);
                }
                if (!finished)//!!
                    continue;
                stopwatch.Stop();
                int time = stopwatch.Elapsed.Milliseconds;
                stopwatch.Reset();
                myExport.AddRow();
                //myExport["System"] = model.Id;
                myExport["Algorithm"] = planner.Algorithm();
                if (planner.Bounded)
                    myExport["Bound"] = planner.Bound;
                else
                    myExport["Bound"] = "No Bound";
                myExport["Objective Function"] = planner.ObjectiveFunction();
                myExport["Overhead"] = overhead;
                myExport["Observation"] = physioCase.Id;
                myExport["# Diagnoses"] = physioCase.Diagnoses.Count;
                myExport["Runtime(ms)"] = time;
                myExport["# Iterations"] = iterationCounter;
                myExport["Cost"] = totalCost;
                myExport["# Fixed Components"] = numberOfFixed;
                myExport["# Expanded In First Iteration"] = expanded;
                myExport["Found Opt"] = foundOpt;
            }
            string fileName = "Physiotherapy "+ planner.Type() + "_o=" + overhead;
            if (maxNumOfDiag > 0)
                fileName += "_MaxDiag" + maxNumOfDiag;
            myExport.ExportToFile(fileName + ".csv");
            planner.CreateIterationDetailsFile(fileName + "_IterationDetails");
        }

        public void Cardinality(string casesFilesDir, int maxNumOfDiag)
        {
            CSVExport myExport = new CSVExport();
            List<PhysioCaseInstance> physioCases = new List<PhysioCaseInstance>();
            List<string> files = Directory.GetFiles(casesFilesDir).ToList();
            foreach (string file in files)
            {
                PhysioCaseInstance physioCase = caseParser.ParseCase(file);
                if (physioCase != null)
                    physioCases.Add(physioCase);
            }
            foreach (PhysioCaseInstance physioCase in physioCases)
            {
                if (physioCase.Diagnoses.Count < 2 || (maxNumOfDiag > 0 && physioCase.Diagnoses.Count > maxNumOfDiag))
                    continue;
                myExport.AddRow();
                myExport["Observation"] = physioCase.Id;
                myExport["RealDiagCard"] = physioCase.RealDiagCardinality;
                myExport["MaxCard"] = physioCase.MaxCardinality;
                myExport["MinCard"] = physioCase.MinCardinality;

            }
            myExport.ExportToFile("Physiotherapy_Cardinality.csv");
        }

        public void RealWC(string casesFilesDir, int maxNumOfDiag, BatchCostEstimator bce)
        {
            CSVExport myExport = new CSVExport();
            List<PhysioCaseInstance> physioCases = new List<PhysioCaseInstance>();
            string bceType = bce.Type();
            string overhead = bce.Overhead +"";
            List<string> files = Directory.GetFiles(casesFilesDir).ToList();
            foreach (string file in files)
            {
                PhysioCaseInstance physioCase = caseParser.ParseCase(file);
                if (physioCase != null)
                    physioCases.Add(physioCase);
            }
            foreach (PhysioCaseInstance physioCase in physioCases)
            {
                if (physioCase.Diagnoses.Count < 2 || (maxNumOfDiag > 0 && physioCase.Diagnoses.Count > maxNumOfDiag))
                    continue;
                SystemState currSystemState = new SystemState(physioCase.Diagnoses.Components);
                currSystemState.Diagnoses = physioCase.Diagnoses; //!! check for bug
                double wc = bce.WastedCostUtility(new RepairAction(physioCase.RealDiagnosis.Comps), currSystemState);
                myExport.AddRow();
                myExport["Objective Function"] = bceType;
                myExport["Overhead"] = overhead;
                myExport["Observation"] = physioCase.Id;
                myExport["RealDiag"] = physioCase.RealDiagnosis;
                myExport["RealWC"] = wc;

            }
            string fileName = "Physiotherapy_RealWC";
            if (maxNumOfDiag > 0)
                fileName += "_maxDiag=" + maxNumOfDiag;
            fileName+="_"+ bceType + "_o=" + overhead;
            myExport.ExportToFile(fileName+ ".csv");
        }
    }
}
