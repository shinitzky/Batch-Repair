using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    public class RepairAction
    {
        private HashSet<Comp> r;
        public HashSet<Comp> R
        {
            get
            {
                return r;
            }
            set
            {
                if (value == null)
                    r = new HashSet<Comp>();
                else
                    r = new HashSet<Comp>(value); //!!
            }

        }
        public int Count { get { return R.Count; } }
        public RepairAction()
        {
            R = new HashSet<Comp>();
        }

        public RepairAction(HashSet<Comp> repairAction)
        {
            R = repairAction;
        }
        public RepairAction(List<Comp> repairAction): this(new HashSet<Comp>(repairAction))
        {
        }
        public bool Contains(Comp comp)
        {
            return R.Contains(comp);
        } 
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj is RepairAction)
            {
                RepairAction action = (RepairAction)obj;
                if (action.R == null || R ==null)
                    return false;
                if (R.Count != action.R.Count)
                    return false;
                foreach (Comp c in R)
                {
                    if (!action.R.Contains(c))
                        return false;
                }
                return true;
            }
            else
                return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            int hash = 13;
            foreach (Comp comp in R)
            {
                hash = (hash * 7) + comp.GetHashCode();
            }
            return hash;
        }

        public override string ToString()
        {
            string repairAction = "";
            foreach(Comp c in R)
            {
                repairAction += c.Name + ",";
            }
            return repairAction;
        }

    }
}
