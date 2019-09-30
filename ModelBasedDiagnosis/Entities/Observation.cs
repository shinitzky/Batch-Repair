using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    class Observation
    {
        public int Id { get; private set; }
        public bool[] InputValues { get; set; }
        public bool[] OutputValues { get; set; }
        public Dictionary<Wire, bool> CorrectValues { get; private set; }
        private SystemModel model;
        public SystemModel TheModel 
        { 
            get
            {
                return model;
            }
            set
            {
                if (value!=null && value.Input.Count == InputValues.Length && value.Output.Count == OutputValues.Length)
                {
                    model = value;
                    model.SetValue(InputValues);
                    SetCorrectValuesDictionary();
                }
            }
        }

        public Observation(int id)
        {
            Id = id;
            CorrectValues = new Dictionary<Wire, bool>();
        }
        public Observation(int id, bool[] inputVals, bool[] outputVals)
        {
            Id = id;
            InputValues = inputVals;
            OutputValues = outputVals;
            CorrectValues = new Dictionary<Wire, bool>();
        }
        public Observation(int id, bool[] inputVals, bool[] outputVals, SystemModel model)
        {
            Id = id;
            TheModel = model;
            InputValues = inputVals;
            OutputValues = outputVals;
            CorrectValues = new Dictionary<Wire, bool>();
            SetCorrectValuesDictionary();
        }
        public void SetWiresToCorrectValue()
        {
            if (CorrectValues != null && CorrectValues.Count > 0)
            {
                foreach (Wire wire in CorrectValues.Keys)
                {
                    bool val = CorrectValues[wire];
                    if(wire.Value!=val)
                    {
                        //wire.Value = val;//with propogation
                        wire.ChangeValue(val);//no propogation
                    }
                }
            }
        }
        public void SetCorrectValuesDictionary()
        {
            if (TheModel == null||InputValues==null)
                return;
            model.SetValue(InputValues);
            foreach (Wire wire in model.Input)
            {
                if (!CorrectValues.ContainsKey(wire))
                    CorrectValues.Add(wire, wire.Value);
                else
                    CorrectValues[wire] = wire.Value;
            }
            foreach (Wire wire in model.Internal)
            {
                if (!CorrectValues.ContainsKey(wire))
                    CorrectValues.Add(wire, wire.Value);
                else
                    CorrectValues[wire] = wire.Value;
            }
            foreach (Wire wire in model.Output)
            {
                if (!CorrectValues.ContainsKey(wire))
                    CorrectValues.Add(wire, wire.Value);
                else
                    CorrectValues[wire] = wire.Value;
            }
        }
    }
}
