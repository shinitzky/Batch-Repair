using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis.PhysiotherapyDomain
{
    public class PhysioComp : Comp
    {
        public string Name { get; protected set; }

        public PhysioComp(string name)
        {
            Name = name;
            Id = Name.GetHashCode();
            Cost = 5; //!!
        }

        public override string CompName()
        {
            return Name;
        }
        public override object ComparingValue()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            if(obj is PhysioComp)
            {
                PhysioComp o = (PhysioComp)obj;
                if (o.Name.Equals(Name))
                    return true;
            }
            return false;
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
