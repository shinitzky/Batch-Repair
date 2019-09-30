using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace ModelBasedDiagnosis
{
    class Simulator
    {
        private IDiagnoser Diagnoser;
        private ISCASParser parser;
        private IFunction gateFunc;
        Random rnd;
        public Simulator()
        {
            parser = new ISCASParser();
            gateFunc = new FlipFunction();
            rnd = new Random();
        }
        public Simulator(IDiagnoser Algorithm)
        {
            Diagnoser = Algorithm;
            if (Diagnoser != null && Diagnoser is DiagnosesSearcher)
                gateFunc = ((DiagnosesSearcher)Diagnoser).function;
            else
                gateFunc = new FlipFunction();
            parser = new ISCASParser();
        }
        private void createNewDiagnoser()
        {
            FlipFunction flip = new FlipFunction();
            TimeSpan x = new TimeSpan(0, 1, 0);
            IterativeDeepening salgo = new IterativeDeepening(flip, x);
            ConesAlgorithm algo = new ConesAlgorithm(salgo);
            Diagnoser = algo;
        }
        public void CreateObsReal(string fileModel, string fileObs, bool minCard)
        {
            List<Observation> observationsList = parser.ReadObsModelFiles(fileModel, fileObs);
            if (observationsList == null || observationsList.Count == 0)
                return;
            Dictionary<int, Gate> compDic = observationsList[0].TheModel.CreateCompDic();
            StreamWriter sw;
            string filePath;
            if(minCard)
                filePath = "groundedDiagnoses/";
            else
                filePath = "diagnoses/";
            Dictionary<int, string> ans = new Dictionary<int, string>();
            foreach(Observation obs in observationsList)
            {
                if (ans.ContainsKey(obs.Id))
                    continue;
                DiagnosisSet diagnoses = null;
                if (minCard)
                {
                    string diagFileName = filePath + obs.TheModel.Id + "_iscas85_" + obs.Id + ".all";
                    diagnoses = parser.ReadGroundedDiagnosesFile(diagFileName, compDic);
                }
                else
                {
                    string diagFileName = filePath + obs.TheModel.Id + "_" + obs.Id + "_Diag.txt";
                    diagnoses = parser.ReadDiagnosisFile(diagFileName, compDic);

                }
                Diagnosis chosenDiag = chooseReal(diagnoses);
                ans.Add(obs.Id, chosenDiag.ToString());
            }
            string realFileName = observationsList[0].TheModel.Id + "";
            if (minCard)
                realFileName += "_minCard";
            realFileName += "_Real.txt";
            sw = new StreamWriter(realFileName, false);
            foreach (int ob in ans.Keys)
            {
                string toWrite = ob + ":" + ans[ob];
                sw.WriteLine(toWrite);
            }
            sw.Close();
        }
        private Diagnosis chooseReal(DiagnosisSet diagnoses) 
        {
            double val = rnd.NextDouble();
            if (val == 1)
                return diagnoses.Diagnoses.Last();
            else if (val == 0)
                return diagnoses.Diagnoses.First();
            double sum = 0;
            foreach (Diagnosis diag in diagnoses.Diagnoses)
            {
                sum += (diag.Probability / diagnoses.SetProbability);
                if (sum >= val)
                    return diag;
            }
            return diagnoses.Diagnoses.Last();
        }
        public void BatchRepair(string diagPath, string fileModel, string fileObs, string fileReal, BatchPlanner planner, double overhead, bool minCard, int maxNumOfDiag) //do it more generic
        {
            bool findDiagnoses = false; //

            List<Observation> observationsList = parser.ReadObsModelFiles(fileModel, fileObs);
            Dictionary<int, List<int>> obsReal = parser.ReadRealObsFiles(fileReal);
            if (observationsList == null || observationsList.Count == 0 || obsReal == null || obsReal.Count == 0)
                return;
            createNewDiagnoser();
            SystemModel model = observationsList[0].TheModel;
            Dictionary<int, Gate> compDic = model.CreateCompDic();
            Stopwatch stopwatch = new Stopwatch();
            CSVExport myExport = new CSVExport();
           
            foreach (Observation obs in observationsList)
            {
                if (!obsReal.ContainsKey(obs.Id))
                    continue;
                List<int> realComp = new List<int>(obsReal[obs.Id]);
                int counter = 0;
                double cost = 0;
                int numberOfFixed = 0;
                stopwatch.Start();
                DiagnosisSet diagnoses = null;

                if (findDiagnoses) //
                    diagnoses = Diagnoser.FindDiagnoses(obs);
                else
                {
                    if (minCard)
                    {
                        string diagFileName = diagPath + model.Id + "_iscas85_" + obs.Id + ".all";
                        diagnoses = parser.ReadGroundedDiagnosesFile(diagFileName, compDic);
                    }
                    else
                    {
                        string diagFileName = diagPath + model.Id + "_" + obs.Id + "_Diag.txt";
                        diagnoses = parser.ReadDiagnosisFile(diagFileName, compDic);

                    }
                }
                if (diagnoses.Count == 1)
                    continue;
                if (maxNumOfDiag > 0 && diagnoses.Count> maxNumOfDiag)
                    continue;
                SystemState state = new SystemState(new List<Comp>(model.Components));
                state.Diagnoses = diagnoses;

                while (realComp.Count != 0)
                {
                    if (state.Diagnoses != null && state.Diagnoses.Count != 0)
                    {
                        RepairAction repairAction = planner.Plan(state);
                        if (repairAction != null) 
                        {
                            counter++;  
                            cost += overhead;
                            state.SetNextState(repairAction);
                            foreach (Gate gate in repairAction.R)
                            {
                                cost += gate.Cost;
                                numberOfFixed++;
                                if (realComp.Contains(gate.Id))
                                    realComp.Remove(gate.Id);
                            }
                            obs.TheModel.SetValue(obs.InputValues);
                            if (realComp.Count == 0)//the system is fixed
                            {
                                planner.ExportIterationDetails(model.Id, obs.Id, counter, true);
                                break;
                            }
                            else
                                planner.ExportIterationDetails(model.Id, obs.Id, counter, false);
                            foreach (int gid in realComp)
                            {
                                Gate g;
                                if (compDic.TryGetValue(gid, out g))
                                {
                                    gateFunc.Operate(g);
                                }
                            }
                            obs.OutputValues = model.GetValue();
                            DiagnosisSet newDiagnoses = new DiagnosisSet();
                            foreach (Diagnosis diag in state.Diagnoses.Diagnoses)
                            {
                                if (Diagnoser.IsConsistent(obs, diag))
                                    newDiagnoses.AddDiagnosis(diag);
                            }
                            state.Diagnoses = newDiagnoses;
                        }
                        else
                            break;
                    }
                    else
                        break;
                }

                stopwatch.Stop();
                if (realComp.Count > 0)
                    continue;
                
                TimeSpan time = stopwatch.Elapsed;
                stopwatch.Reset();
                myExport.AddRow();
                myExport["System"] = model.Id;
                myExport["Algorithm"] = planner.Algorithm();
                if (planner.Bounded)
                    myExport["Bound"] = planner.Bound;
                else
                    myExport["Bound"] = "No Bound";
                myExport["Objective Function"] = planner.ObjectiveFunction();
                myExport["Overhead"] = overhead;
                myExport["Observation"] = obs.Id;
                myExport["# Diagnoses"] = diagnoses.Count;
                myExport["Runtime"] = time;
                myExport["# Iterations"] = counter;
                myExport["Cost"] = cost;
                myExport["# Fixed Components"] = numberOfFixed;
            }
            string fileName = model.Id+ "_" + planner.Type() + "_o=" + overhead;
            if (maxNumOfDiag > 0)
                fileName += "_MaxDiag" + maxNumOfDiag;
            myExport.ExportToFile(fileName+".csv");
            planner.CreateIterationDetailsFile(fileName + "_IterationDetails");
        }
      
        public void CalcProbabilityMass(string fileModel, string fileObs)
        {
            createNewDiagnoser();
            ConesAlgorithm conesAlgo = (ConesAlgorithm)Diagnoser;
            List<Observation> observationsList = parser.ReadObsModelFiles(fileModel, fileObs);
            CSVExport myExport = new CSVExport();
            SystemModel model = observationsList[0].TheModel;
            Dictionary<int, Gate> compDic = model.CreateCompDic();
            string diagnosesFilesPath = "diagnoses/";
            string tldiagnosesFilesPath = "tldiagnoses/";
            model.createCones();
            Dictionary<int, Cone> conesDic = new Dictionary<int, Cone>();
            foreach(Cone c in model.Cones)
            {
                if (!conesDic.ContainsKey(c.Id))
                    conesDic.Add(c.Id, c);
            }
            foreach (Observation obs in observationsList)
            {
                string diagFileName = diagnosesFilesPath + model.Id + "_" + obs.Id + "_Diag.txt";
                DiagnosisSet allDiagnoses =  parser.ReadDiagnosisFile(diagFileName, compDic);
                string tldFileName = tldiagnosesFilesPath + model.Id + "_iscas85_" + obs.Id + ".tld"; //!!! txt?
                DiagnosisSet tlDiagnoses = parser.ReadTLDiagnosisFile(tldFileName, conesDic);
                DiagnosisSet minCardDiagnoses = conesAlgo.AbstractDiagGrounding(obs, tlDiagnoses);
                myExport.AddRow();
                myExport["Observation"] = obs.Id;
                myExport["# all diagnoses"] = allDiagnoses.Count;
                myExport["all diagnoses probability"] = allDiagnoses.SetProbability;
                myExport["# min card diagnoses"] = minCardDiagnoses.Count;
                myExport["min card diagnoses probability"] = minCardDiagnoses.SetProbability;
 
            }
            myExport.ExportToFile(model.Id + "_probabilityMass.csv");
        }

        public void CreateDiagnosesFiles(string fileModel, string fileObs)
        {
            createNewDiagnoser();
            List<Observation> observationsList = parser.ReadObsModelFiles(fileModel, fileObs);
            
            foreach(Observation obs in observationsList)
            {
                obs.TheModel.SetValue(obs.InputValues);
                DiagnosisSet diagnoses = Diagnoser.FindDiagnoses(obs);
                if (diagnoses == null || diagnoses.Count == 0)
                    continue;
                StreamWriter sw = new StreamWriter(obs.TheModel.Id+"_"+obs.Id+"_Diag.txt");
                foreach (Diagnosis diag in diagnoses.Diagnoses)
                    sw.WriteLine(diag.ToString());
                sw.Close();

            }
        }
    }
}
