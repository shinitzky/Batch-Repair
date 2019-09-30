using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    class MultipleInputComponent:Gate
    {
        public List<Wire> Input;

        public void AddInput(Wire wire)
        {
            Input.Add(wire);
            wire.AddOutputComponent(this);
        }
        public MultipleInputComponent(int id, Type type) : base(id,type)
        {
            Input = new List<Wire>();
        }

        public override bool GetValue()
        {
            if (Input.Count == 0)
                return false;
            switch(type)
            {
                case Type.and:
                    foreach (Wire wire in Input)
                    {
                        if (wire.Value == false)
                            return false;
                    }
                    return true;
                case Type.nand:
                    foreach(Wire wire in Input)
                    {
                        if (wire.Value == false)
                            return true;
                    }
                    return false;
                case Type.nor:
                    foreach (Wire wire in Input)
                    {
                        if (wire.Value == true)
                            return false;
                    }
                    return true;
                case Type.or:
                    foreach (Wire wire in Input)
                    {
                        if (wire.Value == true)
                            return true;
                    }
                    return false;
                case Type.xor:
                    int t=0;
                    foreach (Wire wire in Input)
                    {
                        if (wire.Value == true)
                            t++;
                    }
                    if (t % 2 == 0)
                        return false;
                    return true;
            }
            return false;

        }
    }
}
