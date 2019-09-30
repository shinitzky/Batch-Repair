using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    interface IFunction
    {
        void Operate(Gate comp);
    }
}
