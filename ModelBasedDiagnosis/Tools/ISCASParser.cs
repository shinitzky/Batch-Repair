using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ModelBasedDiagnosis
{
    class ISCASParser
    {
        public DiagnosisSet ReadTLDiagnosisFile(string fileName, Dictionary<int, Cone> conesDic)
        {
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(fs);
            string allText = reader.ReadToEnd();
            fs.Close();
            reader.Close();
            fs = null;
            reader = null;
            DiagnosisSet ans = new DiagnosisSet();
            char[] delrow = new char[2];
            delrow[0] = '\n';
            delrow[1] = '\r';
            List<string> rows = allText.Split(delrow, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (rows == null || rows.Count == 0)
                return null;
            foreach (string row in rows)
            {
                if (!row.StartsWith("[gate"))
                    continue;
                List<string> delDiag = new List<string>();
                delDiag.Add("[");
                delDiag.Add("]");
                delDiag.Add(".");
                delDiag.Add(",");
                delDiag.Add("gate");
                List<string> diagString = row.Split(delDiag.ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                Diagnosis diag = new Diagnosis();
                bool addDiag = true;
                foreach(string comp in diagString)
                {
                    int compId = 0;
                    if (!Int32.TryParse(comp, out compId)) 
                    {
                        //in case of not numeric id:
                        string toascii = "";
                        foreach (char c in comp)
                        {
                            int i = c;
                            toascii += i;
                        }
                        if (!Int32.TryParse(toascii, out compId))
                        {
                            Console.WriteLine("Parsing error");
                            return null;
                        }
                    }
                    if (conesDic.ContainsKey(compId))
                        diag.AddCompToDiagnosis(conesDic[compId]);
                    else
                    {
                        addDiag = false;
                        break;
                    }
                }
                if (addDiag)
                    ans.AddDiagnosis(diag);
            }
            return ans;
        }

        public DiagnosisSet ReadGroundedDiagnosesFile(string fileName, Dictionary<int, Gate> compDic)
        {
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(fs);
            string allText = reader.ReadToEnd();
            fs.Close();
            reader.Close();
            fs = null;
            reader = null;
            DiagnosisSet ans = new DiagnosisSet();
            char[] delrow = new char[2];
            delrow[0] = '\n';
            delrow[1] = '\r';
            List<string> rows = allText.Split(delrow, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (rows == null || rows.Count == 0)
                return null;
            foreach (string row in rows)
            {
                if (!row.StartsWith("[[gate"))
                    continue;
                string sDiags = row.Remove(row.Length - 2, 2).Substring(1);
                List<string> delDiag = new List<string>();
                delDiag.Add("[");
                delDiag.Add("]");
                List<string> diagsString = sDiags.Split(delDiag.ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                List<List<Gate>> grounded = new List<List<Gate>>();
                foreach(string d in diagsString)
                {
                    delDiag.Add(".");
                    delDiag.Add(",");
                    delDiag.Add("gate");
                    List<string> scomps = d.Split(delDiag.ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                    List<Gate> comps = new List<Gate>();
                    foreach(string comp in scomps)
                    {
                        int compId = 0;
                        if (!Int32.TryParse(comp, out compId)) 
                        {
                            //in case of not numeric id:
                            string toascii = "";
                            foreach (char c in comp)
                            {
                                int i = c;
                                toascii += i;
                            }
                            if (!Int32.TryParse(toascii, out compId))
                            {
                                Console.WriteLine("Parsing error");
                                return null;
                            }
                        }
                        if (compDic.ContainsKey(compId))
                            comps.Add(compDic[compId]);

                    }
                    grounded.Add(comps);


                }
                grounded = grounded.Where(x => x.Count > 0).ToList();
                List<Diagnosis> diagnoses = generateDiagsFromGrounded(grounded);
                diagnoses.ForEach(x => ans.AddDiagnosis(x));

            }
            
            return ans;
        }
        private List<Diagnosis> generateDiagsFromGrounded(List<List<Gate>> grounded)
        {
            List<Diagnosis> ans = new List<Diagnosis>();
            int numOfGroups = 0;
            grounded.ForEach(list => 
            {
                if (list.Count > 1)
                    numOfGroups++;
            });
            if (numOfGroups == 0)
            {
                List<Diagnosis> diagnoses = new List<Diagnosis>();
                Diagnosis diag = new Diagnosis();
                grounded.ForEach(x =>
                {
                    if (x.Count == 1)
                        diag.AddCompToDiagnosis(x.First());
                });
                diagnoses.Add(diag);
                return diagnoses;
            }
            else if(numOfGroups ==1)
            {
                List<Diagnosis> diagnoses = new List<Diagnosis>();
                List<Gate> bigGroup = grounded.Find(x => x.Count > 1);
                foreach (Gate g in bigGroup)
                {
                    Diagnosis diag = new Diagnosis();
                    diag.AddCompToDiagnosis(g);
                    grounded.Where(x => x.Count == 1).ToList().ForEach(x=> 
                    {
                        diag.AddCompToDiagnosis(x.First());
                    });
                    diagnoses.Add(diag);
                }
                return diagnoses;
            
            }
            else
            {
                List<Diagnosis> diagnoses = new List<Diagnosis>();
                List<Gate> bigGroup = grounded.Find(x => x.Count > 1);
                List<List<Gate>> newGrounded = new List<List<Gate>>(grounded);
                newGrounded.Remove(bigGroup);
                foreach(Gate g in bigGroup)
                {
                    List<Gate> single = new List<Gate>();
                    single.Add(g);
                    newGrounded.Add(single);
                    diagnoses.AddRange(generateDiagsFromGrounded(newGrounded));
                    newGrounded.Remove(single);
                }
                return diagnoses;
            }
        }
        public DiagnosisSet ReadDiagnosisFile(string fileName, Dictionary<int, Gate> compDic)
        {
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(fs);
            string allText = reader.ReadToEnd();
            fs.Close();
            reader.Close();
            fs = null;
            reader = null;
            DiagnosisSet ans = new DiagnosisSet();
            char[] delrow = new char[2];
            delrow[0] = '\n';
            delrow[1] = '\r';
            List<string> rows = allText.Split(delrow, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (rows == null || rows.Count == 0)
                return null;
            foreach (string row in rows)
            {
                char[] del = new char[1];
                del[0] = ' ';
                List<string> sdiag = row.Split(del, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (sdiag == null || sdiag.Count == 0)
                    continue;
                List<int> list = new List<int>();
                bool addDiag = true;
                for (int i = 0; i < sdiag.Count; i++)
                {
                    int ComponentID = 0;
                    if (!Int32.TryParse(sdiag[i], out ComponentID))
                    {
                        addDiag = false;
                        break;
                    }
                    list.Add(ComponentID);
                }
                if (addDiag)
                {
                    //convert list to diagnosis
                    List<Gate> diag = new List<Gate>();
                    foreach (int gid in list)
                    {
                        Gate gate;
                        if (compDic.TryGetValue(gid, out gate))
                        {
                            diag.Add(gate);
                        }
                    }
                    if (diag.Count > 0)
                    {
                        Diagnosis diagnosis = new Diagnosis(new List<Comp>(diag));
                        ans.AddDiagnosis(diagnosis);
                    }
                }
            }
            return ans;

        }
        public Dictionary<int, List<int>> ReadRealObsFiles(string fileName)
        {
            Dictionary<int, List<int>> ans = new Dictionary<int, List<int>>();
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(fs);
            string model_allText = reader.ReadToEnd();
            fs.Close();
            reader.Close();
            fs = null;
            reader = null;

            char[] delrow = new char[2];
            delrow[0] = '\n';
            delrow[1] = '\r';
            List<string> rows = model_allText.Split(delrow, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (rows == null || rows.Count == 0)
                return null;

            foreach (string row in rows)
            {
                char[] del = new char[2];
                del[0] = ':';
                del[1] = ',';
                List<string> obReal = row.Split(del, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (obReal == null || obReal.Count == 0)
                    continue;
                int obId = 0;
                if (!Int32.TryParse(obReal[0], out obId) || ans.ContainsKey(obId))
                    continue;
                List<int> list = new List<int>();
                bool addObs = true;
                for (int i = 1; i < obReal.Count; i++)
                {
                    int ComponentID = 0;
                    if (!Int32.TryParse(obReal[i], out ComponentID))
                    {
                        addObs = false;
                        break;
                    }
                    list.Add(ComponentID);
                }
                if(addObs)
                    ans.Add(obId, list);
            }
            return ans;
        }
        public List<Observation> ReadObsModelFiles(string fileModel, string fileObs) //path?
        {
            //reading model file
            List<Observation> observationsList = new List<Observation>();
            FileStream fs = new FileStream(fileModel, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(fs);
            string model_allText = reader.ReadToEnd();
            fs.Close();
            reader.Close();
            fs = null;
            reader = null;

            char[] delrow = new char[2];
            delrow[0] = '\n';
            delrow[1] = '\r';
            List<string> rows = model_allText.Split(delrow, StringSplitOptions.RemoveEmptyEntries).ToList();
            if(rows==null || rows.Count < 4)
            {
                Console.WriteLine("Parsing error");
                return null;
            }

            //build Model
            SystemModel theModel;
            string modelID = rows[0].Substring(0, rows[0].Length - 1);
            List<Wire> inputs = new List<Wire>();
            List<Wire> outputs = new List<Wire>();
            List<Wire> internalWires = new List<Wire>();
            Dictionary<Wire.WireType, Dictionary<int, Wire>> wiresDictionary = new Dictionary<Wire.WireType, Dictionary<int, Wire>>();
            wiresDictionary.Add(Wire.WireType.i, new Dictionary<int, Wire>());
            wiresDictionary.Add(Wire.WireType.o, new Dictionary<int, Wire>());
            wiresDictionary.Add(Wire.WireType.z, new Dictionary<int, Wire>());
          
            char[] del = new char[6];
            del[0]='.';
            del[1]=',';
            del[2]='[';
            del[3]=']';
            del[4]='(';
            del[5]=')';

            //input & output
            string[] inputArr = rows[1].Split(del, StringSplitOptions.RemoveEmptyEntries);
            string[] outputArr = rows[2].Split(del, StringSplitOptions.RemoveEmptyEntries);


            for (int i=0; i < inputArr.Length; i++)
            {
                //check if the Value is Valid? 2<=len<=3, starts with i/o/z, end with a number
                //check if theres as similar wire exist
                string wireName = inputArr[i];
                int wid; 
                if(Int32.TryParse(wireName.Substring(1),out wid))
                {
                    if (wireName.StartsWith("i"))
                    {
                        Wire w = new Wire(wid, Wire.WireType.i);
                        inputs.Add(w);
                        wiresDictionary[Wire.WireType.i].Add(wid, w);
                    }
                        
                }
            }
            for (int j = 0; j < outputArr.Length; j++)
            {
                string wireName = outputArr[j];
                int wid;
                if (Int32.TryParse(wireName.Substring(1), out wid))
                {
                    if (wireName.StartsWith("o"))
                    {
                        Wire w = new Wire(wid, Wire.WireType.o);
                        outputs.Add(w);
                        wiresDictionary[Wire.WireType.o].Add(wid, w);
                    }
                }
            }
            theModel = new SystemModel(modelID, inputs, outputs);

            //creating components
            for (int i = 3; i < rows.Count; i++)
            {
                if (!String.IsNullOrEmpty(rows[i])) 
                    theModel.AddComponent(CreateComponent(rows[i].Split(del, StringSplitOptions.RemoveEmptyEntries),theModel,wiresDictionary));
            }

            //sort model
            theModel.SortComponents();

            //reading observation fila
            delrow = new char[1];
            delrow[0] = '.';
            del = new char[7];
            del[0] = '\r';
            del[1] = ',';
            del[2] = '[';
            del[3] = ']';
            del[4] = '(';
            del[5] = ')';
            del[6] = '\n';
            rows = null;
            fs = new FileStream(fileObs, FileMode.Open, FileAccess.Read);
            reader = new StreamReader(fs);
            string ob_allText = reader.ReadToEnd();
            rows = ob_allText.Split(delrow, StringSplitOptions.RemoveEmptyEntries).ToList();
            fs.Close();
            reader.Close();
            fs = null;
            reader = null;

            if (rows == null || rows.Count == 0)
            {
                Console.WriteLine("Parsing error");
                return null;
            }

            //build observation
            for (int i = 0; i < rows.Count; i++)
            {
                string[] obArr = rows[i].Split(del, StringSplitOptions.RemoveEmptyEntries);
                if (obArr == null || obArr.Length == 0&& i!=rows.Count-1)
                {
                    Console.WriteLine("Parsing error");
                    return null;
                }
                if (obArr.Length == 2 + inputs.Count + outputs.Count)
                {
                    if (!obArr[0].Equals(modelID))
                    {
                        Console.WriteLine("Parsing error");
                        return null;
                    }
                    Observation toAdd = CreateObservation(obArr);
                    if (toAdd != null) 
                    {
                        toAdd.TheModel = theModel;
                        observationsList.Add(toAdd);
                    }
                }
            }
            return observationsList;
        }

        private Gate CreateComponent(string[] compArr, SystemModel theModel, Dictionary<Wire.WireType, Dictionary<int, Wire>> wiresDictionary)
        {
            if(compArr==null||compArr.Length<4)
            {
                Console.WriteLine("Parsing error");
                return null;
            }

            int id;

            if(compArr[1].Length<5 || !compArr[1].StartsWith("gate"))
            {
                Console.WriteLine("Parsing error");
                return null;
            }

            string sid = compArr[1].Substring(4);
            if (!Int32.TryParse(sid, out id)) 
            {
                //in case of not numeric id:
                string toascii = "";
                foreach (char c in sid)
                {
                    int i = c;
                    toascii += i;
                }
                if (!Int32.TryParse(toascii, out id))
                {
                    Console.WriteLine("Parsing error");
                    return null;
                }
            }
            Wire output = null;
            string oname = compArr[2];
            int oid;
            if(Int32.TryParse(oname.Substring(1),out oid))
            {
                if (oname.StartsWith("o"))
                {
                   if(!wiresDictionary[Wire.WireType.o].TryGetValue(oid,out output))
                   {
                       output = new Wire(oid,Wire.WireType.o);
                       wiresDictionary[Wire.WireType.o].Add(oid, output);
                       theModel.Output.Add(output);
                   }

                }
                else if(oname.StartsWith("z"))
                {
                    if (!wiresDictionary[Wire.WireType.z].TryGetValue(oid, out output))
                    {
                        output = new Wire(oid, Wire.WireType.z);
                        wiresDictionary[Wire.WireType.z].Add(oid, output);
                        theModel.Internal.Add(output);
                    }
                }
                //cant be i 
            }
            if (compArr.Length == 4)
            {
                OneInputComponent ans = null;
                if (compArr[0].Equals("inverter"))
                    ans = new OneInputComponent(id, Gate.Type.not);
                if (compArr[0].Equals("buffer"))
                    ans = new OneInputComponent(id, Gate.Type.buffer);
                if (ans != null)
                {
                    if(output!=null)
                        ans.Output = output;
                    string iname = compArr[3];
                    int inid;
                    Wire input = null;
                    if (Int32.TryParse(iname.Substring(1), out inid))
                    {
                        if (iname.StartsWith("o"))
                        {
                            if (!wiresDictionary[Wire.WireType.o].TryGetValue(inid, out input))
                            {
                                input = new Wire(inid, Wire.WireType.o);
                                wiresDictionary[Wire.WireType.o].Add(inid, input);
                                theModel.Output.Add(input);
                            }

                        }
                        else if (iname.StartsWith("z"))
                        {
                            if (!wiresDictionary[Wire.WireType.z].TryGetValue(inid, out input))
                            {
                                input = new Wire(inid, Wire.WireType.z);
                                wiresDictionary[Wire.WireType.z].Add(inid, input);
                                theModel.Internal.Add(input);
                            }
                        }
                        else if (iname.StartsWith("i"))
                        {
                            if (!wiresDictionary[Wire.WireType.i].TryGetValue(inid, out input))
                            {
                                input = new Wire(inid, Wire.WireType.i);
                                wiresDictionary[Wire.WireType.i].Add(inid, input);
                                theModel.Input.Add(input);
                            }
                        }
                    }
                    if (input != null)
                        ans.Input1 = input;
                }
                return ans;
            }
            if (compArr.Length >= 5)
            {
                MultipleInputComponent ans = null;
                if (compArr[0].StartsWith("and"))
                    ans = new MultipleInputComponent(id, Gate.Type.and);
                if (compArr[0].StartsWith("nor"))
                    ans = new MultipleInputComponent(id, Gate.Type.nor);
                if (compArr[0].StartsWith("xor"))
                    ans = new MultipleInputComponent(id, Gate.Type.xor);
                if (compArr[0].StartsWith("nand"))
                    ans = new MultipleInputComponent(id, Gate.Type.nand);
                if (compArr[0].StartsWith("or"))
                    ans = new MultipleInputComponent(id, Gate.Type.or);
                if (ans != null)
                {
                    if (output != null)
                        ans.Output = output;
                    for (int i = 3; i < compArr.Length; i++)
                    {
                        string iname = compArr[i];
                        int inid;
                        Wire input = null;
                        if (Int32.TryParse(iname.Substring(1), out inid))
                        {
                            if (iname.StartsWith("o"))
                            {
                                if (!wiresDictionary[Wire.WireType.o].TryGetValue(inid, out input))
                                {
                                    input = new Wire(inid, Wire.WireType.o);
                                    wiresDictionary[Wire.WireType.o].Add(inid, input);
                                    theModel.Output.Add(input);
                                }
                            }
                            else if (iname.StartsWith("z"))
                            {
                                if (!wiresDictionary[Wire.WireType.z].TryGetValue(inid, out input))
                                {
                                    input = new Wire(inid, Wire.WireType.z);
                                    wiresDictionary[Wire.WireType.z].Add(inid, input);
                                    theModel.Internal.Add(input);
                                }
                            }
                            else if (iname.StartsWith("i"))
                            {
                                if (!wiresDictionary[Wire.WireType.i].TryGetValue(inid, out input))
                                {
                                    input = new Wire(inid, Wire.WireType.i);
                                    wiresDictionary[Wire.WireType.i].Add(inid, input);
                                    theModel.Input.Add(input);
                                }
                            }
                        }
                        if (input != null)
                            ans.AddInput(input); 
                    }
                }
                return ans;
            }
            return null;
        }

        private Observation CreateObservation(string[] obArr)
        {
            if (obArr == null || obArr.Length < 3)
            {
                Console.WriteLine("Parsing error");
                return null;
            }

            int id;
            if (!Int32.TryParse(obArr[1], out id))
            {
                Console.WriteLine("Parsing error");
                return null;
            }

            List<bool> inputVals = new List<bool>();
            List<bool> outputVals = new List<bool>();

            for (int i = 2; i < obArr.Length; i++)
            {
                if (obArr[i].Length > 4)
                {
                    Console.WriteLine("Parsing error");
                    return null;
                }
                if (obArr[i].Contains('i')) //input
                {
                    if (obArr[i].StartsWith("-"))
                        inputVals.Add(false);
                    else
                        inputVals.Add(true);
                }
                else if (obArr[i].Contains('o')) //output
                {
                    if (obArr[i].StartsWith("-"))
                        outputVals.Add(false);
                    else
                        outputVals.Add(true);
                }
            }
            Observation ans = new Observation(id, inputVals.ToArray(), outputVals.ToArray());
            return ans;
        }

    }
}
