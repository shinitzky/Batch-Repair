using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBasedDiagnosis
{
    class AStarIterationDetails: BatchPlannerIterationDetails
    {
        public double Fminmax { get; set; } 
        public double Fminstart { get; set; }
        public double ChosenFval { get; set; }

    }
}
