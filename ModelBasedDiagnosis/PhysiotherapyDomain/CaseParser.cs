using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ModelBasedDiagnosis.PhysiotherapyDomain
{
    class CaseParser
    {

        public PhysioCaseInstance ParseCase(string fileName)
        {
            char[] delrow = new char[1];
            delrow[0] = ',';
            int progress = 0;

            PhysioCaseInstance ans = new PhysioCaseInstance();

            //extract case number from fileName
            string caseNumber = fileName.Substring(fileName.LastIndexOf('_') +1);
            caseNumber = caseNumber.Substring(0, caseNumber.IndexOf(".txt"));
            int num;
            if(Int32.TryParse(caseNumber, out num))
            {
                ans.Id = num;
            }

            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(fs);



            while (!reader.EndOfStream && progress < 3)
            {
                string line = reader.ReadLine();
                if (string.IsNullOrEmpty(line))
                    continue;
                //read real
                if (progress == 0)
                {
                    ans.RealDiagnosis = new Diagnosis();
                    List<string> realDiagComps = line.Split(delrow, StringSplitOptions.RemoveEmptyEntries).ToList();
                    foreach (string c in realDiagComps)
                    {
                        PhysioComp comp = new PhysioComp(c);
                        ans.RealDiagnosis.AddCompToDiagnosis(comp);
                    }
                    progress++;
                    ans.RealDiagCardinality = ans.RealDiagnosis.Diag.Count;
                }
                //read obs
                else if (progress == 1)
                {
                    ans.Observation = new List<PhysioComp>();
                    List<string> obs = line.Split(delrow, StringSplitOptions.RemoveEmptyEntries).ToList();
                    foreach (string c in obs)
                    {
                        PhysioComp comp = new PhysioComp(c);
                        ans.Observation.Add(comp);
                    }
                    progress++;
                }
                //read diagnoses
                else if (progress == 2)
                {
                    ans.Diagnoses = new DiagnosisSet();
                    do
                    {
                        Diagnosis diag = new Diagnosis();
                        List<string> diagComps = line.Split(delrow, StringSplitOptions.RemoveEmptyEntries).ToList();
                        foreach (string c in diagComps)
                        {
                            PhysioComp comp = new PhysioComp(c);
                            diag.AddCompToDiagnosis(comp);
                        }
                        if (diag.Diag.Count > 0)
                            ans.Diagnoses.AddDiagnosis(diag);
                        if (reader.EndOfStream)
                            break;
                        line = reader.ReadLine();
                    }
                    while (!string.IsNullOrEmpty(line));
                    progress++;
                }
            }
            
            if(progress>2 && ans.ValidCaseInstance())
            {
                int maxCard = ans.RealDiagCardinality;
                int minCard = ans.RealDiagCardinality;
                foreach (Diagnosis diag in ans.Diagnoses.Diagnoses)
                {
                    int card = diag.Diag.Count;
                    if(card > maxCard)
                        maxCard = card;
                    if(card < minCard)
                        minCard = card;
                }
                ans.MaxCardinality = maxCard;
                ans.MinCardinality = minCard;
                return ans;
            }
            return null;
        }
    }
}
