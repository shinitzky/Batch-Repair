using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    abstract class RepairActionSearcher
    {
       public int K { get; set; }
       public abstract RepairActionsSet ComputePossibleAcions(SystemState state);
       public abstract string Type();
    }
}
