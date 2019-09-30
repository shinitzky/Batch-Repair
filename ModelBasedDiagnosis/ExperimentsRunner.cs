using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelBasedDiagnosis.PhysiotherapyDomain;

namespace ModelBasedDiagnosis
{
    public enum RepairAlgorithm { AsP, AsU, MDP, HC, UK, HP, KHP}

    class ExperimentsRunner
    {
        private BatchCostEstimator bce;
        public BatchCostEstimator Bce
        {
            get
            {
                return bce;
            }
            set
            {
                bce = value;
                overhead = bce.Overhead;
            }
        }
        private double overhead;
        private string path;
        private int maxDiag;
        public ExperimentsRunner(BatchCostEstimator bce)
        {
            if (bce == null)
                Bce = new PessimisticEstimator(overhead, PessimisticEstimator.PessimisticEstimationType.nextState, PessimisticEstimator.FFPType.FFP);
            else
                Bce = bce;
        }
        
        public ExperimentsRunner() : this(null)
        {
           
        }

        public void RunAll(bool iscas, string filesPath, List<RepairAlgorithm> algorithms, List<int> bounds, int maxDiag)
        {
            path = filesPath;
            this.maxDiag = maxDiag;
            List<BatchPlanner> planners = createPlanners(algorithms, bounds);
            foreach(BatchPlanner planner in planners)
            {
                if (iscas)
                {
                    RunISCASRepairAlgorithm(planner, "74182");
                    RunISCASRepairAlgorithm(planner, "74283");
                    RunISCASRepairAlgorithm(planner, "c432");
                    RunISCASRepairAlgorithm(planner, "c880");
                }
                else
                    RunRepairAlgorithmPhysio(planner);

            }
        }

        public void RunIscas(string filesPath, List<RepairAlgorithm> algorithms, List<int> bounds, int maxDiag, string system)
        {
            path = filesPath;
            overhead = Bce.Overhead;
            this.maxDiag = maxDiag;
            List<BatchPlanner> planners = createPlanners(algorithms, bounds);
            foreach (BatchPlanner planner in planners)
                    RunISCASRepairAlgorithm(planner, system);                   

        }
        public void RunPhysio(string filesPath, List<RepairAlgorithm> algorithms, List<int> bounds, int maxDiag)
        {
            path = filesPath;
            overhead = Bce.Overhead;
            this.maxDiag = maxDiag;
            List<BatchPlanner> planners = createPlanners(algorithms, bounds);
            foreach (BatchPlanner planner in planners)
                    RunRepairAlgorithmPhysio(planner);

        }
        public void RunRepairAlgorithmPhysio(BatchPlanner planner)
        {
            PhysiotherapySimulator sim = new PhysiotherapySimulator();
            sim.BatchRepair(path, planner, overhead, maxDiag);
        }
        public void RunISCASRepairAlgorithm(BatchPlanner planner, string system)
        {
            string fileModel = path + "systems/" + system + ".txt";
            string fileObs = path + "observations/" + system + "_iscas85.obs";
            string fileReal = path + "real/" + system + "_minCard_Real.txt";
            string diagPath = path + "groundedDiagnoses/";
            Simulator sim = new Simulator();
            sim.BatchRepair(diagPath, fileModel, fileObs, fileReal, planner, overhead, true, maxDiag);
        }
        private List<BatchPlanner> createPlanners(List<RepairAlgorithm> algorithms, List<int> bounds)
        {
            List<BatchPlanner> planners = new List<BatchPlanner>();
            foreach(RepairAlgorithm algorithm in algorithms)
            {
                switch (algorithm)
                {
                    case RepairAlgorithm.AsP:
                        planners.Add(new AStarPowerSetPlanner(Bce));
                        break;
                    case RepairAlgorithm.AsU:
                        planners.Add(new AStarUnionPlanner(Bce));
                        foreach (int bound in bounds)
                            planners.Add(new AStarUnionPlanner(Bce, bound));
                        break;
                    case RepairAlgorithm.HC:
                        planners.Add(new GHSBatchPlanner(Bce));
                        foreach (int bound in bounds)
                            planners.Add(new GHSBatchPlanner(Bce, bound));
                        break;
                    case RepairAlgorithm.KHP:
                        foreach (int bound in bounds) 
                                planners.Add(new KHPBatchPlanner(bound, Bce));
                        break;
                    case RepairAlgorithm.HP:
                        planners.Add(new KHPBatchPlanner(1, Bce));
                        break;
                    case RepairAlgorithm.MDP:
                        foreach (int bound in bounds)
                        {
                            RepairActionSearcher ras = new UnionBasedSearcher(bound, false);
                            planners.Add(new MDPPlanner(ras, Bce, 10000, 5));
                        }
                        break;
                    case RepairAlgorithm.UK:
                        foreach (int bound in bounds)
                        {
                            RepairActionSearcher ras = new UnionBasedSearcher(bound, false);
                            planners.Add(new HeuristicBatchPlanner(ras,Bce));
                        }
                        break;
                }
            }
            return planners;
        }

    }
}
