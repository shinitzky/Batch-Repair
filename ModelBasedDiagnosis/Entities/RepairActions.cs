using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    public abstract class RepairActions
    {
        public abstract RepairAction NextAction();
        public abstract void AddAction(SortedSet<Comp> action);
    }
}
