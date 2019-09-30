using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    class FlipFunction: IFunction
    {
        public void Operate(Gate comp)
        {
            comp.Output.Value = !comp.GetValue();
        }
    }
}
