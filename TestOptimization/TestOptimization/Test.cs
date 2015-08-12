using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Optimization.Utils;
using Optimization.LineSearch;
using Optimization.Optimizer.GradientBased;
using Optimization.Optimizer.Stats;
using Optimization.Optimizer.StopingCriteria;

namespace TestOptimization
{
    class Test
    {
        static void Main(string[] args)
        {
            //double[] array = new double[] { 3, 1, 4, 5, 2 };
            //ArrayMath.SortDescending(array);
            //foreach (double i in array)
            //    Console.WriteLine(i);

            TestIDifferentiableBaseObjective o1 = new TestIDifferentiableBaseObjective(0);
            TestIProjectedDifferentiableBaseObjective o2 = new TestIProjectedDifferentiableBaseObjective(0);
            TestIStochasticDifferentiableBaseObjective o3 = new TestIStochasticDifferentiableBaseObjective(0);
            TestIProjectedStochasticDifferentiableBaseObjective o4 = new TestIProjectedStochasticDifferentiableBaseObjective(0);

            ILineSearchMethod lineSearch1 = new ArmijoMinimizationLineSearch(0, new PickFirstStep(1));
            ILineSearchMethod lineSearch2 = new ProjectedArmijoMinimizationLineSearch(0, new PickFirstStep(1));

            IStopingCriteria stoping1 = new GradientL2NormStopingCriteria(1E-4);
            IStopingCriteria stoping2 = new AverageValueDifferenceStopingCriteria(1E-12);
            IStopingCriteria stoping3 = new StochasticGradientL2NormStopingCriteria(1E-4, o3.DatasetSize());
            IStopingCriteria stoping4 = new StochasticAverageValueDifferenceStopingCriteria(1E-12, o3.DatasetSize());

            CompositeStopingCriteria stoping = new CompositeStopingCriteria();
            //stoping.Add(stoping1);
            //stoping.Add(stoping2);
            stoping.Add(stoping3);
            stoping.Add(stoping4);
            

            IOptimizerStats stats = new OptimizerStats();


            //double[] rs = OptimizerUtils.Optimize_BatchGradientDescent(o1, lineSearch1, stoping, stats, 1, 100000);
            double[] rs = OptimizerUtils.Optimize_StochasticGradientDescent(o3, lineSearch1, stoping, stats, 1, 10000);
            Console.WriteLine(ArrayMath.ToString<double>(rs));


            Console.ReadKey();
        }
    }
}
