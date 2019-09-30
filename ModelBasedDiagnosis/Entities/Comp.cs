using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    public class Comp
    {
        public int Id {get; protected set;}
        public double Cost { get; set; }
        public double P { get; set; } //probability to be faulty 
        public string Name { get; protected set; }

        public Comp(int id): this(id, 5, 0.01) { }

        public Comp(string name) : this(name, 5, 0.01) { }

        public Comp(int id, string name) : this(id, name, 5, 0.01) { }

        public Comp(int id, double cost, double p)
        {
            Id = id;
            Cost = cost;
            P = p;
            Name = id + "";
        }

        public Comp(string name, double cost, double p)
        {
            Name = name;
            Cost = cost;
            P = p;
            Id = name.GetHashCode();
        }

        public Comp(int id, string name, double cost, double p)
        {
            Id = id;
            Name = name;
            Cost = cost;
            P = p;
        }

        public virtual object ComparingValue()
        {
            return Name;
        }
        public class CompComparer : IComparer<Comp>
        {
            public int Compare(Comp x, Comp y) //by order
            {
                if (x == null || y == null)
                    return 0;
                string xCompareVal = x.ComparingValue().ToString();
                string yCompareVal = y.ComparingValue().ToString();
                if (xCompareVal.Equals(yCompareVal))
                    return 0;
                return xCompareVal.CompareTo(yCompareVal);
               
            }
        }

        public override bool Equals(object obj)
        {
            if(obj!=null && obj is Comp)
                if(Name.Equals(((Comp)obj).Name))
                    return true;
            return false;
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public override string ToString()
        {
            return ComparingValue().ToString();
        }

    }
}
