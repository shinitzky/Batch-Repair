using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using System.IO;

namespace ModelBasedDiagnosis
{
    class ResultsProcessor
    {
        public List<ResultsRow> ResultsRows;

        public void MergeResultsFiles(string resultsDirPath, string mergedFileName)
        {
            ResultsRows = new List<ResultsRow>();
            readResultsFiles(resultsDirPath);
            writeResultsFile(mergedFileName);
        }

        public void MergeResultsAndIterDetFiles(string resultsDirPath, string mergedFileName)
        {
            ResultsRows = new List<ResultsRow>();
            readResultsAndIterDetFiles(resultsDirPath);
            writeResultsFile(mergedFileName);
        }

       
        public void CreateNotTimeOutResultFile(string fileName)
        {
            ResultsRows = new List<ResultsRow>();
            ResultsRows.AddRange(readFile(fileName));
            if (ResultsRows.Count == 0)
                return;
            filterResultsToNoTimeout();
            writeResultsFile(Path.GetFileName(fileName) + "_NoTimeout");
        }

        private void filterResultsToNoTimeout()
        {
            HashSet<string> obsTimeoutList = new HashSet<string>();
            Dictionary<string, List<ResultsRow>> obsResRowDic = new Dictionary<string, List<ResultsRow>>();
            foreach (ResultsRow row in ResultsRows)
            {
                string obs = row.Observation;
                if (obsTimeoutList.Contains(obs))
                    continue;
                if (row.FoundOpt.StartsWith("F"))
                    obsTimeoutList.Add(obs);
                else
                {
                    if (!obsResRowDic.ContainsKey(obs))
                        obsResRowDic.Add(obs, new List<ResultsRow>());
                    obsResRowDic[obs].Add(row);
                }
            }
            ResultsRows.Clear();
            foreach (string obs in obsResRowDic.Keys)
            {
                if (obsTimeoutList.Contains(obs))
                    continue;
                ResultsRows.AddRange(obsResRowDic[obs]);
            }

        }
        private void writeResultsFile(string mergedFileName)
        {
            if (ResultsRows == null || ResultsRows.Count == 0)
                return;
            CSVExport myExport = new CSVExport();
            foreach (ResultsRow row in ResultsRows)
            {
                myExport.AddRow();
                myExport["System"] = row.System;
                myExport["Algorithm"] = row.Algorithm;
                myExport["Bound"] = row.Bound;
                myExport["Objective Function"] = row.ObjectiveFunction;
                myExport["Overhead"] = row.Overhead;
                myExport["Observation"] = row.Observation;
                myExport["# Diagnoses"] = row.NumberOfDiagnoses;               
                myExport["Runtime(ms)"] = row.Runtime;
                myExport["# Iterations"] = row.NumberOfIterations;
                myExport["Cost"] = row.Cost;
                myExport["# Fixed Components"] = row.NumberOfFixedComponents;
                myExport["# Expanded In First Iteration"] = row.ExpandedInFirstIteraion;
                myExport["Found Opt"] = row.FoundOpt;
                myExport["First Action Chosen"] = row.FirstActionChosen;
                myExport["WC First Action"] = row.WCfirstAction;

            }
            myExport.ExportToFile(mergedFileName + ".csv");
        }
        private void readResultsFiles(string resultsDirPath)
        {
            List<string> resutlsFiles = Directory.GetFiles(resultsDirPath).ToList();
            resutlsFiles.Sort();
            foreach(string fileName in resutlsFiles)
            {
                if (fileName.Contains("IterationDetails"))
                    continue;
               ResultsRows.AddRange(readFile(fileName));
            }

        }
        private void readResultsAndIterDetFiles(string resultsDirPath)
        {

            List<ResultsRow> rows = new List<ResultsRow>();
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
                if (!iterDetfileName.Contains(mainFileName.Substring(0, mainFileName.Length - 4)))//!!check!!
                    continue;
                ResultsRows.AddRange(readIterDetFile(iterDetfileName, readFile(mainFileName)));
            }

        }
        

        private List<ResultsRow> readIterDetFile(string iterDetfileName, List<ResultsRow> rows)
        {
            Dictionary<string, string[]> dicIterDetObs = new Dictionary<string, string[]>();
           
            using (TextFieldParser parser = new TextFieldParser(iterDetfileName))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                while (!parser.EndOfData)
                {
                    //Processing row
                    string[] fields = parser.ReadFields();
                    if (!fields[2].Equals("1") || fields.Length < 10)
                        continue;
                    string obs = fields[1];
                    dicIterDetObs[obs] = fields;
                }
            }

            foreach(ResultsRow row in rows)
            {
                string[] fields;
                if (dicIterDetObs.TryGetValue(row.Observation, out fields))
                {
                    row.ExpandedInFirstIteraion = fields[5];
                    row.FoundOpt = fields[6];
                    row.FirstActionChosen = fields[7];
                    row.WCfirstAction = fields[9];
                }
            }

            return rows;
        }
      
        private List<ResultsRow> readFile(string fileName)
        {
            List<ResultsRow> rows = new List<ResultsRow>();
            using (TextFieldParser parser = new TextFieldParser(fileName))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                bool sys = false;
                string[] fields = parser.ReadFields();//first row is the headlines
                if (fields[0].Contains("System"))
                    sys = true;
                while (!parser.EndOfData)
                {
                    //Processing row
                    ResultsRow row = new ResultsRow();
                    fields = parser.ReadFields();
                    int index = 0;
                    if (sys)
                    {
                        row.System = fields[index];
                        index++;
                    }
                    else
                        row.System = "1";
                    row.Algorithm = fields[index];
                    index++;
                    row.Bound = fields[index];
                    index++;
                    row.ObjectiveFunction = fields[index];
                    index++;
                    row.Overhead = fields[index];
                    index++;
                    row.Observation = fields[index];
                    index++;
                    row.NumberOfDiagnoses = fields[index];
                    index++;
                    row.Runtime = fields[index];
                    index++;
                    row.NumberOfIterations = fields[index];
                    index++;
                    row.Cost = fields[index];
                    index++;
                    row.NumberOfFixedComponents = fields[index];
                    rows.Add(row);
                }
            }
            return rows;
        }
        public class ResultsRow
        {
            public string System;
            public string Algorithm;
            public string Bound;
            public string ObjectiveFunction;
            public string Overhead;
            public string Observation;         
            public string NumberOfDiagnoses;
            public string Runtime;
            public string NumberOfIterations;
            public string Cost;
            public string NumberOfFixedComponents;
            public string ExpandedInFirstIteraion;
            public string FoundOpt;
            public string FirstActionChosen;
            public string WCfirstAction; 

        }
    }
}


