using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelBasedDiagnosis.PhysiotherapyDomain;

namespace ModelBasedDiagnosis
{
    class Program
    {
        static void Main(string[] args)
        {
            ExperimentsRunner er = new ExperimentsRunner();
            string path = "";
            List<double> overheads = new List<double>();
            overheads.Add(25);
            overheads.Add(10);
            overheads.Add(15);
            overheads.Add(20);
            overheads.Add(5);
            List<int> bounds = new List<int>();
            bounds.Add(4);
            List<RepairAlgorithmType> algorithms = new List<RepairAlgorithmType>();
            algorithms.Add(RepairAlgorithmType.KHP);
            List<string> systems = new List<string>();
            systems.Add("c880");
            foreach(double overhead in overheads)
            {
                BatchCostEstimator bce = new OptimisticEstimator(overhead);
                er.RunIscas(path, algorithms, bounds, bce, 30, systems);
            }
            systems.Clear();
            systems.Add("74182");
            systems.Add("74283");
            systems.Add("c432");
            foreach (double overhead in overheads)
            {
                BatchCostEstimator bce = new OptimisticEstimator(overhead);
                er.RunIscas(path, algorithms, bounds, bce, 30, systems);
                er.RunPhysio(path, algorithms, bounds, bce, 30);
            }

        }
    }
}
