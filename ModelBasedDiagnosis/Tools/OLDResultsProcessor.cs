using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using System.IO;

namespace ModelBasedDiagnosis
{
    class OLDResultsProcessor
    {
        public List<ResultsRow> ResultsRows;

        public OLDResultsProcessor()
        {
            ResultsRows = new List<ResultsRow>();
        }

        public void Clear()
        {
            ResultsRows.Clear();
        }
        
        public void MergeResultsFiles(string resultsDirPath, string mergedFileName)
        {
            if (ResultsRows.Count > 0) //!!
                Clear();
            ReadResultsFiles(resultsDirPath);
            WriteResultsFile(mergedFileName);
        }

        
        public void CreateNotTimeOutResultFile(string fileName)
        {
            if (ResultsRows.Count > 0)//!!
                Clear();
            ReadFullResultsFile(fileName);
            if (ResultsRows == null || ResultsRows.Count == 0)
                return;
            FilterResultsToNoTimeout();
            WriteResultsFile(Path.GetFileName(fileName) + "_NoTimeout");
        }
        public void FilterResultsToNoTimeout()
        {
            Dictionary<string, HashSet<int>> sysObsTimeoutDic = new Dictionary<string, HashSet<int>>();
            Dictionary<string, List<ResultsRow>> sysResRowDic = new Dictionary<string, List<ResultsRow>>();
            foreach(ResultsRow row in ResultsRows)
            {
                string system = row.ResultInstance.System;
                if (!sysObsTimeoutDic.ContainsKey(system))
                    sysObsTimeoutDic.Add(system, new HashSet<int>());
                if (!sysResRowDic.ContainsKey(system))
                    sysResRowDic.Add(system, new List<ResultsRow>());
                if (row.FoundOpt.StartsWith("T"))
                    sysResRowDic[system].Add(row);
                else
                    sysObsTimeoutDic[system].Add(row.Observation);
            }
            ResultsRows.Clear();
            foreach (string system in sysResRowDic.Keys)
            {
                HashSet<int> observations;
                if (sysObsTimeoutDic.TryGetValue(system, out observations))
                {
                    foreach (ResultsRow row in sysResRowDic[system])
                    {
                        if (!observations.Contains(row.Observation))
                        {
                            ResultsRows.Add(row);
                        }
                    }
                }

            }

        }
        public void ReadFullResultsFile(string fileName)
        {
            
            using (TextFieldParser parser = new TextFieldParser(fileName))//need to add dir path?
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.ReadFields();//first row is the headlines
                while (!parser.EndOfData)
                {
                    //Processing row
                    ResultsRow row = new ResultsRow();
                    string[] fields = parser.ReadFields();
                    row.ResultInstance = new InstanceDescription();
                    row.ResultInstance.System = fields[0];
                    row.ResultInstance.Algorithm = fields[1];
                    row.ResultInstance.ObjectiveFunction = fields[2];
                    row.ResultInstance.Overhead = Int32.Parse(fields[3]);
                    row.Observation = Int32.Parse(fields[4]);
                    row.NumberOfDiagnoses = Int32.Parse(fields[5]);
                    row.Runtime = fields[6];
                    row.NumberOfIterations = Int32.Parse(fields[7]);
                    row.Cost = Int32.Parse(fields[8]);
                    row.NumberOfFixedComponents = Int32.Parse(fields[9]);
                    row.ExpandedInFirstIteraion = Int32.Parse(fields[10]);
                    row.FoundOpt = fields[11];
                    ResultsRows.Add(row);
                       
                }
            }
      
            
        }
        public void WriteResultsFile(string mergedFileName)
        {
            if (ResultsRows == null || ResultsRows.Count == 0)
                return;
            CSVExport myExport = new CSVExport();
            foreach (ResultsRow row in ResultsRows)
            {
                myExport.AddRow();
                myExport["System"] = row.ResultInstance.System;
                myExport["Algorithm"] = row.ResultInstance.Algorithm;
                myExport["Objective Function"] = row.ResultInstance.ObjectiveFunction;
                myExport["Overhead"] = row.ResultInstance.Overhead;
                myExport["Observation"] = row.Observation;
                myExport["# Diagnoses"] = row.NumberOfDiagnoses;
                myExport["Runtime"] = row.Runtime;
                myExport["# Iterations"] = row.NumberOfIterations;
                myExport["Cost"] = row.Cost;
                myExport["# Fixed Components"] = row.NumberOfFixedComponents;
                myExport["# Expanded In First Iteration"] = row.ExpandedInFirstIteraion;
                myExport["Found Opt"] = row.FoundOpt;
            }
            myExport.ExportToFile(mergedFileName + ".csv");
        }
        public void ReadResultsFiles(string resultsDirPath)
        {
            List<string> resutlsFiles = Directory.GetFiles(resultsDirPath).ToList();
            resutlsFiles.Sort();
            //assuming that after sort all iterDet files will be located just one cell afer their matching result file.
            int i = 0;
            while (i < resutlsFiles.Count)
            {
                string mainFileName = resutlsFiles[i];
                i++;
                if (i == resutlsFiles.Count)
                    break;
                if (mainFileName.Contains("IterationDetails"))
                    continue;
                string iterDetfileName = resutlsFiles[i];
                i++;
                if (!iterDetfileName.Contains(mainFileName.Substring(0,mainFileName.Length - 4)))//!!check!!
                    continue;
                ProcessOneInstance(resultsDirPath, mainFileName, iterDetfileName);
            }


        }

        //reading one result file and insert into the dictionary. 
        public void ProcessOneInstance(string dirPath, string mainFileName, string iterDetfileName)
        {
            Dictionary<int, ResultsRow> instanceObsResultDic = new Dictionary<int, ResultsRow>();
            InstanceDescription instanceDesc = ExtractInstanceDescFromFileName(Path.GetFileName(mainFileName));
            using (TextFieldParser parser = new TextFieldParser(mainFileName))//need to add dir path?
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.ReadFields();//first row is the headlines
                while (!parser.EndOfData)
                {
                    //Processing row
                    ResultsRow row = new ResultsRow();
                    row.ResultInstance = instanceDesc;
                    string[] fields = parser.ReadFields();
                    if (fields.Length < 8)//!!!!!!!!!!!!!!!!!!
                        continue;
                    //fields[0] system
                    if (String.IsNullOrEmpty(fields[0]))
                        continue; //!!! 
                    row.Observation = Int32.Parse(fields[1]); //!!
                    row.NumberOfDiagnoses = Int32.Parse(fields[2]);
                    row.Runtime = fields[3];
                    row.NumberOfIterations = Int32.Parse(fields[4]);
                    row.Cost = Int32.Parse(fields[5]);
                    row.NumberOfFixedComponents = Int32.Parse(fields[6]);
                    instanceObsResultDic.Add(row.Observation, row);
                }
            }
            
            using (TextFieldParser parser = new TextFieldParser(iterDetfileName))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                while (!parser.EndOfData)
                {
                    //Processing row
                    string[] fields = parser.ReadFields();
                    if (!fields[2].Equals("1"))//Check!
                        continue;
                    int obs = Int32.Parse(fields[1]);
                    ResultsRow row;
                    if (!instanceObsResultDic.TryGetValue(obs,out row))
                        continue;
                    row.ExpandedInFirstIteraion = Int32.Parse(fields[5]);
                    row.FoundOpt = fields[6];
                    //check if the row in the dictionary has been changed!    

                }
            }
            foreach(ResultsRow row in instanceObsResultDic.Values)
            {
                ResultsRows.Add(row);
            }
        }

        public InstanceDescription ExtractInstanceDescFromFileName(string mainFileName)
        {
            InstanceDescription instanceDesc = new InstanceDescription();

            int indexOfFirst_ = mainFileName.IndexOf('_');
            instanceDesc.System = mainFileName.Substring(0, indexOfFirst_);

            string algoDetails = mainFileName.Substring(indexOfFirst_ + 1);
            int indexOfoverhead = algoDetails.IndexOf("_o=");
            instanceDesc.Overhead = Int32.Parse(algoDetails.Substring(indexOfoverhead + 3, 2));

            algoDetails = algoDetails.Substring(0, indexOfoverhead);
            if (algoDetails.Contains("MDP"))
            {
                instanceDesc.Algorithm = "MDP bound=" + algoDetails[algoDetails.Length - 1];
                algoDetails = algoDetails.Substring(0, algoDetails.LastIndexOf(' '));
                int indexObjFunc = algoDetails.LastIndexOf(' ');
                instanceDesc.ObjectiveFunction = algoDetails.Substring(indexObjFunc + 1);
            }
            else
            {
                int indexObjFunc = algoDetails.LastIndexOf(' ');
                instanceDesc.ObjectiveFunction = algoDetails.Substring(indexObjFunc + 1);
                instanceDesc.Algorithm = algoDetails.Substring(0, indexObjFunc);

            }
                return instanceDesc;
        }

        public class InstanceDescription
        {
            public string System;
            public string Algorithm;
            public string ObjectiveFunction;
            public int Overhead;
        }
        public class ResultsRow
        {
            public InstanceDescription ResultInstance;
            public int Observation;
            public int NumberOfDiagnoses;
            public string Runtime;
            public int NumberOfIterations;
            public int Cost;
            public int NumberOfFixedComponents;
            public int ExpandedInFirstIteraion; //iterationDetails
            public string FoundOpt;//iterationDetails
        }
    }
}
