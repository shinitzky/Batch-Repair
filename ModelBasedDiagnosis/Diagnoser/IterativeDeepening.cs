using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
namespace ModelBasedDiagnosis
{
    class IterativeDeepening:DiagnosesSearcher
    {
        private List<Gate> openList;
        private List<Gate> notClosed;
        private List<Gate> notInDiag;
        private Stopwatch stopwatch;
       // public int num;
        //public double dis;
        
        public IterativeDeepening(IFunction function, TimeSpan timeSpan)
            : base(function, timeSpan)
        {
            openList = new List<Gate>();
            notClosed = new List<Gate>();
            stopwatch = new Stopwatch();
        }
        public IterativeDeepening(IFunction function):base(function)
        {
            openList = new List<Gate>();
            notClosed = new List<Gate>();
            stopwatch = new Stopwatch();
        }
        public IterativeDeepening() : this(null) { }

        public override DiagnosisSet FindDiagnoses(Observation observation)
        {
            if (function == null || observation == null || observation.TheModel == null || observation.TheModel.Components == null || observation.TheModel.Components.Count == 0)
                return null; //throw
            //isProperSupersetOf
            this.observation = observation;
            DiagnosisSet diagnoses = new DiagnosisSet();
            List<Comp> comps = new List<Comp>(observation.TheModel.Components);
            hSVector = new HealthStateVector(comps);
            notInDiag = new List<Gate>();
            observation.TheModel.SetValue(observation.InputValues);
            FoundMinCard = false;
            FirstMinCard = new TimeSpan();
            trie = new Trie<string>();
            stopwatch.Start(); 
            bool toContinue;
            foreach (Gate Component in observation.TheModel.Components)
            {
                if (closed.Contains(Component.Id))
                {
                    if (Component is Cone)
                    {
                        if (((Cone)Component).cone.Components.Count == 0)
                            continue;
                        else
                        {
                            toContinue = true;
                            foreach (Gate g in ((Cone)Component).cone.Components)
                            {
                                if (!closed.Contains(g.Id)) 
                                {
                                    toContinue = false;
                                    break;
                                }
                            }
                            if (toContinue)
                                continue;
                        }
                    }
                    else
                        continue;
                }
                    
                function.Operate(Component);
                if (isDamaged())
                {
                    List<Comp> diag = new List<Comp>();
                    diag.Add(Component);
                    diagnoses.AddDiagnosis(new Diagnosis(diag));
                    if (diagnoses.Count == 1)
                        FirstMinCard = stopwatch.Elapsed;
                    trie.Put(Component.Id + "", Component.Id + "");
                }
                else
                    notClosed.Add(Component);
                
                Component.SetValue();
            }
            notInDiag.AddRange(notClosed);
            int depth;
            toContinue = true;
            if (agenda == Agenda.HealthState && CheckHSDistance(1, 0.1, diagnoses))
                toContinue = false;
            if (diagnoses.Count > 0)
            {
                FoundMinCard = true;
                if(agenda == Agenda.minCard)
                    toContinue=false;
            }
            for (depth = 2; toContinue&&depth <= notClosed.Count; depth++)
            {
                if (Stop())
                    break;
                if (diagnoses.Count > 0)
                {
                    FoundMinCard = true;
                    if (agenda == Agenda.minCard)
                        break;
                }
                foreach (Gate g in notClosed)
                {
                    if (Stop()||!toContinue)
                    {
                        break;
                    }
                    openList.Add(g);
                    trie.Matcher.ResetMatch();
                    toContinue = deepCheck(depth,diagnoses);
                    openList.Remove(g);
                }
            }
            notClosed.Clear();
            openList.Clear();
            stopwatch.Stop();
            stopwatch.Reset();
            return diagnoses;
        }

        private bool deepCheck(int depth, DiagnosisSet diagnoses)
        {
            bool ans = true;
            int index = notClosed.IndexOf(openList.Last()); //index in notClosed of last Component that added to openlist
            for(int i=index+1; ans==true&&i>=0&&i< notClosed.Count;i++)
            {
                Gate Component = notClosed[i];
                //maybe add check - openlist.last.id < Component
                openList.Add(Component);
                if (depth == 2)
                {
                    if (openList.Count > 2 && isInTrie(openList))
                    {
                        openList.Remove(Component);
                        continue;
                    }
                    string str = "";
                    List<Comp> diag = new List<Comp>();
                    foreach (Comp g in openList)
                    {
                        function.Operate((Gate)g);
                        
                        if (g != openList.Last())
                            str += g.Id + " ";
                        else
                            str += g.Id;
                        diag.Add(g);
                    }
                    if (isDamaged())
                    {
                        diagnoses.AddDiagnosis(new Diagnosis(diag));
                        if (diagnoses.Count == 1)
                            FirstMinCard = stopwatch.Elapsed;
                        trie.Put(str, str);
                        foreach (Comp g in openList)
                        {
                            if (notInDiag.Contains(g))
                                notInDiag.Remove((Gate)g);
                        }
                        if (agenda == Agenda.HealthState&&CheckHSDistance(1, 0.1, diagnoses))
                            ans=false;
                    }
                    foreach (Gate g in openList) 
                    {
                        g.SetValue();
                    }

                }

                else if (!isInTrie(openList))
                {
                    ans = deepCheck(depth-1, diagnoses);
                }

                openList.Remove(Component);
            }
            return ans;
        }
        private bool CheckHSDistance(int num, double distance, DiagnosisSet diagnoses)
        {
            if (diagnoses.Count - hSVector.DiagnosesCounter < num)
                return false;
            bool ans = false;
            List<double> oldHSVector = hSVector.CurrentHealthState;
            hSVector.CalcHealthState(diagnoses);
            double dis = CalcHSVectorDistance(oldHSVector);
            if (dis <= distance)
                ans = true;
            return ans;          
        }
        public override bool Stop() 
        {
            stopwatch.Stop();
            TimeSpan time = stopwatch.Elapsed;
            stopwatch.Start();
            if (time > x)
                return true;
            else
                return false;
        }
    }
}
