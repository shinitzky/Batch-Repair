using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ModelBasedDiagnosis
{
    class ConesAlgorithm:IDiagnoser
    {
        private DiagnosesSearcher searchAlgorithm;
        public ConesAlgorithm(DiagnosesSearcher searchAlgo)
        {
            searchAlgorithm = searchAlgo;
        }
        public void addToGoodCompList(int compID)
        {
            searchAlgorithm.addToGoodCompList(compID);
        }
        public void resetGoodCompList()
        {
            searchAlgorithm.resetGoodCompList();
        }
        public bool IsConsistent(Observation observation,Diagnosis diagnosis)
        {
            return searchAlgorithm.IsConsistent(observation,diagnosis);
        }
        public DiagnosisSet FindDiagnoses(Observation observation)
        {
            if (searchAlgorithm == null || observation == null || observation.TheModel == null || observation.TheModel.Components == null || observation.TheModel.Components.Count == 0)
                return null; //throw
            DiagnosisSet diagnoses;
            DiagnosisSet abstractDiag;
            abstractDiag = FindAbstractDiag(observation);
            diagnoses = AbstractDiagGrounding(observation, abstractDiag);

            return diagnoses;
        }
        public DiagnosisSet FindAbstractDiag(Observation observation)
        {
            DiagnosisSet abstractDiag;
            if (observation.TheModel.Cones.Count == 0)
                observation.TheModel.createCones();
            List<Cone> cones = observation.TheModel.Cones;
            SystemModel toTest = new SystemModel(observation.TheModel.Id, observation.TheModel.Input, observation.TheModel.Output);
            foreach (Cone c in cones)
            {
                toTest.AddComponent(c);
            }
            Observation obs = new Observation(observation.Id, observation.InputValues, observation.OutputValues);
            obs.TheModel = toTest;
            abstractDiag = searchAlgorithm.FindDiagnoses(obs);
            return abstractDiag;
        }
        public DiagnosisSet AbstractDiagGrounding(Observation observation,DiagnosisSet abstractDiag) //improve and correct code!!!!
        {
            int count = 0;
            Observation obs;
            DiagnosisSet diagnoses = new DiagnosisSet();
            Dictionary<int, DiagnosisSet> coneDiagDic = new Dictionary<int, DiagnosisSet>();
            List<List<Comp>> tempDiag = new List<List<Comp>>();
            List<List<Comp>> temp = new List<List<Comp>>();
            foreach (Diagnosis diag in abstractDiag.Diagnoses)
            {
                List<Comp> openList = diag.Comps.ToList();
                observation.SetWiresToCorrectValue();
                if (openList.Count == 0)
                    continue;
                foreach (Cone c in openList)
                {
                    if (coneDiagDic.ContainsKey(c.Id))
                        continue;
                    count++;
                    bool[] obIn = new bool[c.cone.Input.Count];
                    bool[] obOut = new bool[1];
                    for (int i = 0; i < obIn.Length; i++)
                    {
                        obIn[i] = c.cone.Input[i].Value;
                    }
                    obOut[0] = !c.Output.Value;
                    obs = new Observation(observation.Id + count, obIn, obOut);
                    obs.TheModel = c.cone;
                    coneDiagDic.Add(c.Id, searchAlgorithm.FindDiagnoses(obs));
                    c.Output.Value = obOut[0];
                }
                foreach (Cone c in openList) //X
                {
                    if (coneDiagDic[c.Id] == null || coneDiagDic[c.Id].Diagnoses.Count == 0)
                        continue;
                    if (tempDiag.Count == 0)
                    {
                        foreach (Diagnosis d in coneDiagDic[c.Id].Diagnoses)
                        {
                            tempDiag.Add(new List<Comp>(d.Comps));
                        }
                        continue;
                    }
                    if (coneDiagDic[c.Id].Diagnoses.Count == 1)
                    {
                        for (int i = 0; i < tempDiag.Count; i++)
                            tempDiag[i].AddRange(new List<Comp>(coneDiagDic[c.Id].Diagnoses.First().Comps));
                        continue;
                    }
                    temp.AddRange(tempDiag);
                    tempDiag.Clear();
                    foreach (Diagnosis d in coneDiagDic[c.Id].Diagnoses)
                    {
                        List<Comp> listD = d.Comps.ToList();
                        foreach (List<Comp> listTemp in temp)
                        {
                            List<Comp> listTempD = new List<Comp>(listTemp);
                            listTempD.AddRange(new List<Comp>(listD));
                            tempDiag.Add(new List<Comp>(listTempD));
                            listTempD.Clear();
                        }
                    }
                    temp.Clear();
                }
                foreach (List<Comp> list in tempDiag)
                {
                    diagnoses.AddDiagnosis(new Diagnosis(list));
                }
                tempDiag.Clear();
            }
            return diagnoses;
        }
        public bool Stop()//no use in this class
        {
            return false;
        }
        public void CheckMinCard(Observation observation, CSVExport myExport)
        {
            if (searchAlgorithm == null || observation == null || observation.TheModel == null || observation.TheModel.Components == null || observation.TheModel.Components.Count == 0)
                return; //throw
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            DiagnosisSet diagnoses;
            DiagnosisSet abstractDiag;
            searchAlgorithm.agenda = DiagnosesSearcher.Agenda.minCard;
            abstractDiag = FindAbstractDiag(observation);
            string foundMC;
            if (searchAlgorithm.FoundMinCard)
                foundMC = "yes";
            else
                foundMC = "No";
            TimeSpan firstMC = searchAlgorithm.FirstMinCard;
            TimeSpan absDiagtime = stopwatch.Elapsed;
            diagnoses = AbstractDiagGrounding(observation, abstractDiag);
            stopwatch.Stop();
            TimeSpan alltime = stopwatch.Elapsed;
            myExport.AddRow();
            myExport["System"] = observation.TheModel.Id;
            myExport["Observation"] = observation.Id;
            myExport["All MC found?"] = foundMC;
            myExport["# abstract diagnoses"] = abstractDiag.Count;
            myExport["# diagnoses"] = diagnoses.Count;
            myExport["runtime until 1st found"] = firstMC;
            myExport["runtime abstract"] = absDiagtime;
            myExport["runtime all"] = alltime;
        }
    }
}
