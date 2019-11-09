using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelBasedDiagnosis.PhysiotherapyDomain;

namespace ModelBasedDiagnosis
{
    public enum RepairAlgorithmType { AsP, AsU, MDP, HC, UK, HP, KHP}

    class ExperimentsRunner
    {
        
        public ExperimentsRunner() //: this(null)
        {}

        public void RunIscas(string filesPath, List<RepairAlgorithmType> algorithms, List<int> bounds, BatchCostEstimator bce, int maxDiag, List<string> systems)
        {
            List<BatchPlanner> planners = createPlanners(algorithms, bounds, bce);
            foreach (BatchPlanner planner in planners)
                foreach(string system in systems)
                    RunISCASRepairAlgorithm(filesPath, planner, bce.Overhead, maxDiag, system);                   

        }
        public void RunPhysio(string filesPath, List<RepairAlgorithmType> algorithms, List<int> bounds, BatchCostEstimator bce, int maxDiag)
        {
            List<BatchPlanner> planners = createPlanners(algorithms, bounds, bce);
            foreach (BatchPlanner planner in planners)
                    RunRepairAlgorithmPhysio(filesPath, planner, bce.Overhead, maxDiag);

        }
        public void RunRepairAlgorithmPhysio(string filesPath, BatchPlanner planner, double overhead, int maxDiag)
        {
            PhysiotherapySimulator sim = new PhysiotherapySimulator();
            sim.BatchRepair(filesPath, planner, overhead, maxDiag);
        }
        public void RunISCASRepairAlgorithm(string filesPath, BatchPlanner planner, double overhead, int maxDiag, string system)
        {
            string fileModel = filesPath + "systems/" + system + ".txt";
            string fileObs = filesPath + "observations/" + system + "_iscas85.obs";
            string fileReal = filesPath + "real/" + system + "_minCard_Real.txt";
            string diagPath = filesPath + "groundedDiagnoses/";
            Simulator sim = new Simulator();
            sim.BatchRepair(diagPath, fileModel, fileObs, fileReal, planner, overhead, true, maxDiag);
        }
        private List<BatchPlanner> createPlanners(List<RepairAlgorithmType> algorithms, List<int> bounds, BatchCostEstimator bce)
        {
            List<BatchPlanner> planners = new List<BatchPlanner>();
            foreach(RepairAlgorithmType algorithm in algorithms)
            {
                switch (algorithm)
                {
                    case RepairAlgorithmType.AsP:
                        planners.Add(new AStarPowerSetPlanner(bce));
                        break;
                    case RepairAlgorithmType.AsU:
                        planners.Add(new AStarUnionPlanner(bce));
                        foreach (int bound in bounds)
                            planners.Add(new AStarUnionPlanner(bce, bound));
                        break;
                    case RepairAlgorithmType.HC:
                        planners.Add(new GHSBatchPlanner(bce));
                        foreach (int bound in bounds)
                            planners.Add(new GHSBatchPlanner(bce, bound));
                        break;
                    case RepairAlgorithmType.KHP:
                        foreach (int bound in bounds) 
                                planners.Add(new KHPBatchPlanner(bound, bce));
                        break;
                    case RepairAlgorithmType.HP:
                        planners.Add(new KHPBatchPlanner(1, bce));
                        break;
                    case RepairAlgorithmType.MDP:
                        foreach (int bound in bounds)
                        {
                            RepairActionSearcher ras = new UnionBasedSearcher(bound, false);
                            planners.Add(new MDPPlanner(ras, bce, 10000, 5));
                        }
                        break;
                    case RepairAlgorithmType.UK:
                        foreach (int bound in bounds)
                        {
                            RepairActionSearcher ras = new UnionBasedSearcher(bound, false);
                            planners.Add(new HeuristicBatchPlanner(ras, bce));
                        }
                        break;
                }
            }
            return planners;
        }

    }
}
