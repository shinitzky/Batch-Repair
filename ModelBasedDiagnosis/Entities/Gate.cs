using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    public class Gate : Comp 
    {
        public int Order { get; set; }
        public enum Type {and, or, xor, nor, nand, buffer, not, cone}
        protected Type type;
        private Wire output;
        public virtual Wire Output {
            get
            {
                return output;
            }
            set 
            {
                output = value;
                output.InputComponent = this; 
            } 
        }

        public Gate(int id, Type type) : base(id)
        {
            this.type = type;
        }
        
        public virtual bool GetValue() { return Output.Value; }

        public virtual void SetValue() // give the output wire the value that it should have
        {
            Output.Value = GetValue();
        }
        public static int CompareComponents(Gate x, Gate y)
        {
            if (x == null || y == null)
                return 0;
            if (x.Order == y.Order)
                return 0;
            if (x.Order > y.Order)
                return 1;
            else
                return -1;
        }
        /*public override bool Equals(object obj)
        {
            if (obj is Gate)
            {
                Gate g = (Gate)obj;
                if (g == null)
                    return false;
                if(g.Id == Id && g.type == type)
                    return true;
                return false;
            }
            else
                return base.Equals(obj);
        }*/

        public override object ComparingValue()
        {
            return Order;
        }
        public class GateComparer : IComparer<Gate>
        {
            public int Compare(Gate x, Gate y) //by order
            {
                if (x == null || y == null)
                    return 0;
                if (x.Order == y.Order)
                    return 0;
                if (x.Order > y.Order)
                    return 1;
                else
                    return -1;
            }
        }
    }
}
