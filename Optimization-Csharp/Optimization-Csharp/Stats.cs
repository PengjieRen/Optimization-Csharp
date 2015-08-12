using Optimization.Objective;
using Optimization.Optimizer.GradientBased;
using Optimization.Optimizer.StopingCriteria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Optimizer.Stats
{

    public interface IOptimizerStats
    {
        void CollectInitStats(IOptimizer optimizer, IBaseObjective objective, IStopingCriteria stoping);
        void CollectIterationStats(IOptimizer optimizer, IBaseObjective objective, IStopingCriteria stoping);
        void CollectFinalStats(IOptimizer optimizer, IBaseObjective objective, IStopingCriteria stoping, bool success);
        String Print();
    }
    public class OptimizerStats:IOptimizerStats
    {
        protected double _Start = 0;
        protected double _TotalTime = 0;

        public void Reset()
        {
            _Start = 0;
            _TotalTime = 0;
        }

        public void StartTime()
        {
            _Start = DateTime.Now.Millisecond;
        }
        public void StopTime()
        {
            _TotalTime += DateTime.Now.Millisecond - _Start;
        }

        public String Print()
        {
            StringBuilder res = new StringBuilder();
            res.Append("Total time " + _TotalTime / 1000 + " seconds \n");
            return res.ToString();
        }


        public void CollectInitStats(IOptimizer optimizer, IBaseObjective objective, IStopingCriteria stoping)
        {
            StartTime();
        }

        public void CollectIterationStats(IOptimizer optimizer, IBaseObjective objective, IStopingCriteria stoping)
        {

        }


        public void CollectFinalStats(IOptimizer optimizer, IBaseObjective objective, IStopingCriteria stoping, bool success)
        {
            StopTime();
        }


    }
}
