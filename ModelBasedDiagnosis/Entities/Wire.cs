using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    public class Wire
    {
        public enum WireType { i, o, z };
        public int Id { get; private set; }
        public WireType Type { get; private set; }
        private bool val;
        public bool Value 
        { 
            get
            {
                return val;
            }
            set
            {
                val = value;
                if(OutputComponents!=null&&OutputComponents.Count!=0)
                {
                    foreach(Gate comp in OutputComponents)
                        comp.SetValue();
                }
            }
        }
        public Gate InputComponent { get; set; }
            //the Component that the wire is his output 
        public List<Gate> OutputComponents { get; set; }
            //the Component that the wire is his input
        public void ChangeValue(bool value) //no propogation
        {
            val = value;
        }

        public Wire(int id, WireType type)
        {
            this.Id = id;
            this.Type = type;
        }
        public void AddOutputComponent(Gate Component)
        {
            if (Component != null)
            {
                if (OutputComponents == null)
                    OutputComponents = new List<Gate>();
                OutputComponents.Add(Component);
            }
        }

     
     
    }
}
