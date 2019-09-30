using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    class OneInputComponent: Gate
    {
        private Wire input1;
        public Wire Input1
        {
            get
            {
                return input1;
            }
            set
            {
                input1 = value;
                input1.AddOutputComponent(this);
            }
        }
        public OneInputComponent(int id, Type type) : base(id, type) { }

        public override bool GetValue()
        {
            if (type == Type.buffer)
                return Input1.Value;
            else if (type == Type.not)
                return !Input1.Value;
            return false;
        }
    }
}
