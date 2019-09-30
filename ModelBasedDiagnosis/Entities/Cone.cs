using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    class Cone: Gate //multiple
    {
        public SystemModel cone { get; set; }
        public override Wire Output
        {
            get
            {
                if (cone != null)
                    return cone.Output[0];
                else return null;
            }
            set
            {
                if (cone != null) 
                {
                    if (cone.Output.Count == 0)
                        cone.Output.Add(value);
                    else
                        cone.Output[0] = value;
                }
            }
        }
        public Cone(int id):base(id,Type.cone)
        {
            cone = new SystemModel(id + "");
        }

        public override bool GetValue()
        {
            //output.inputComponent.getvalue
            return cone.Output[0].Value;
        }
        public override void SetValue()
        {
            //base.SetValue();
            foreach (Gate g in cone.InputComponents)
            {
                g.SetValue();
            }
        }
        public void SetInputOutputComponents()
        {
            if(cone==null || cone.Components== null|| cone.Components.Count==0)
                return;
            foreach (Gate Component in cone.Components)
            {
                if (!cone.InputComponents.Contains(Component))
                {
                    if (Component is OneInputComponent) 
                    { 
                        if(((OneInputComponent)Component).Input1.InputComponent==null ||  !cone.Components.Contains(((OneInputComponent)Component).Input1.InputComponent))
                            cone.InputComponents.Add(Component);
                    }
                    else if (Component is MultipleInputComponent)
                    {
                        foreach (Wire w in ((MultipleInputComponent)Component).Input)
                        {
                            if (w.InputComponent == null || !cone.Components.Contains(w.InputComponent))
                                cone.InputComponents.Add(Component);
                        }
                    }
                }
                if (!cone.OutputComponents.Contains(Component))
                {
                    if (Component.Output.OutputComponents == null || Component.Output.OutputComponents.Count == 0)
                        cone.OutputComponents.Add(Component);
                    else
                    {
                        bool add = true;
                        foreach (Gate g in Component.Output.OutputComponents)
                        {
                            if(cone.Components.Contains(g))
                            {
                                add = false;
                                break;
                            }
                        }
                        if (add)
                            cone.OutputComponents.Add(Component);
                    }
                }
            }
        }
    }
}
