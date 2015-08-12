using Optimization.LineSearch;
using Optimization.Objective;
using Optimization.Optimizer.Stats;
using Optimization.Optimizer.StopingCriteria;
using Optimization.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Optimizer.GradientBased
{
    public class OptimizerUtils {

        public static double[] Optimize_BatchGradientDescent(IDifferentiableBaseObjective o, ILineSearchMethod lineSearch, IStopingCriteria stoping, IOptimizerStats stats, int debugLevel,int maxIter)
        {
            BatchGradientDescent bgd = new BatchGradientDescent(debugLevel,lineSearch);
            bgd.MaxIterations = maxIter;
            bool rs = bgd.Optimize(o, stats, stoping);
            return o.Parameters;
        }

        public static double[] Optimize_StochasticGradientDescent(IStochasticDifferentiableBaseObjective o, ILineSearchMethod lineSearch, IStopingCriteria stoping, IOptimizerStats stats, int debugLevel, int maxIter)
        {
            StochasticGradientDescent sgd = new StochasticGradientDescent(debugLevel, lineSearch);
            sgd.MaxIterations = maxIter;
            bool rs = sgd.Optimize(o, stats, stoping);
            return o.Parameters;
        }
    }
    public interface IOptimizer
    {

        double CurrentStep { get; }
        double CurrentValue { get; }
        int CurrentIteration { get; }
        int MaxIterations { get; set; }

        bool Optimize(IBaseObjective o, IOptimizerStats stats, IStopingCriteria stoping);
        void Reset();
    }

    public abstract class AbstractGradientOptimizer : IOptimizer { 
        protected double _CurrentStep;
        protected double[] _CurrentGradient;
        protected double[] _CurrentDirection;
        protected double _CurrentValue;
        protected int _CurrentIteration;
        protected int _MaxIterations=10000;

        protected int _DebugLevel = 0;

        public AbstractGradientOptimizer(int debugLevel) {
            this._DebugLevel = debugLevel;
        }

        public double CurrentStep { get { return _CurrentStep; } }
        public double[] CurrentDirection { get { return _CurrentDirection; } }
        public double[] CurrentGradient { get { return _CurrentGradient; } }
        public double CurrentValue { get { return _CurrentValue; }  }
        public int CurrentIteration { get { return _CurrentIteration; }  }
        public int MaxIterations { get { return _MaxIterations; } set { _MaxIterations = value; } }

        public abstract bool Optimize(IBaseObjective o, IOptimizerStats stats, IStopingCriteria stoping);
        protected virtual void DoBeforeOptimization(IBaseObjective o, IOptimizerStats stats, IStopingCriteria stoping)
        { 
        _CurrentDirection=new double[o.Dimension];
        _CurrentGradient = new double[o.Dimension];
        }
        protected abstract void DoAfterOptimization(IBaseObjective o, IOptimizerStats stats, IStopingCriteria stoping);
        protected abstract void DoBeforeEachIteration(IBaseObjective o, IOptimizerStats stats, IStopingCriteria stoping);
        protected abstract void DoAfterEachIteration(IBaseObjective o, IOptimizerStats stats, IStopingCriteria stoping);
        protected abstract void DoBeforeUpdateStep(IBaseObjective o, IOptimizerStats stats, IStopingCriteria stoping);
        protected abstract void DoAfterUpdateStep(IBaseObjective o, IOptimizerStats stats, IStopingCriteria stoping);

        public virtual void Reset() {
            _CurrentStep = 0;
            _CurrentDirection = null;
            _CurrentValue = 0;
            _CurrentIteration = 0;
            _MaxIterations = 10000;
        }
    }

    public class BatchGradientDescent : AbstractGradientOptimizer, IOptimizer
    {
        protected ILineSearchMethod _LineSearch;
        protected DifferentiableObjectiveWrapper _Dow;

        public BatchGradientDescent(int debugLevel,ILineSearchMethod lineSearch):base(debugLevel)
        {
            this._LineSearch = lineSearch;
        }

        public override bool Optimize(IBaseObjective o, IOptimizerStats stats, IStopingCriteria stoping)
        {

            if (o.GetType().GetInterface(typeof(IDifferentiableBaseObjective).Name) != null)
                _Dow = new DifferentiableObjectiveWrapper((IDifferentiableBaseObjective)o);
            else
            {
                Console.WriteLine("Not a IDifferentiableBaseObjective for BatchGradientDescent.");
                Environment.Exit(-1);
            }

            IDifferentiableBaseObjective oo = (IDifferentiableBaseObjective)o;

            stats.CollectInitStats(this, oo, stoping);
            this.DoBeforeOptimization(oo, stats, stoping);


            for (_CurrentIteration = 0; _CurrentIteration < _MaxIterations; _CurrentIteration++)
            {
                if (_DebugLevel > 0)
                {
                    Console.WriteLine(_CurrentIteration + " : " + _CurrentStep + " : " + ArrayMath.L2Norm(_CurrentGradient)+ " : "+ ArrayMath.ToString<double>(oo.Parameters));
                }
                DoBeforeEachIteration(oo, stats, stoping);

                //update gradient and value and find a direction
                oo.CalculateGradient();
                oo.CalculateValue();
                _CurrentValue = oo.Value;
                _CurrentGradient = oo.Gradients;
                _CurrentDirection = ArrayMath.Negation(_CurrentGradient);

                //stop?
                if (stoping.IsConvergent(oo))
                {
                    DoAfterOptimization(o, stats, stoping);
                    stats.CollectFinalStats(this, oo, stoping, true);
                    return true;
                }

                //is a descent direction?
                if (ArrayMath.DotProduct(_CurrentGradient, _CurrentDirection) > 0)
                {
                    Console.WriteLine("Not a descent direction.");
                    stats.Print();
                    Environment.Exit(-1);
                }

                DoBeforeUpdateStep(o, stats, stoping);

                //find a step and update parameter
                _CurrentStep = _LineSearch.DoLineSearch(_Dow);
                if (_CurrentStep>0&&_CurrentStep < 1E-100)
                {
                    _CurrentStep = 1E-100;
                    _Dow.UpdateAlpha(_CurrentStep);
                }
                

                //find a valid step?
                if (_CurrentStep == -1)
                {
                    //Console.WriteLine("Failed to find a step");
                    //stats.Print();
                    //Console.ReadKey();
                    //Environment.Exit(-1);
                }
                o.Parameters = o.Parameters;

                DoAfterUpdateStep(oo, stats, stoping);

                DoAfterEachIteration(oo, stats, stoping);
                stats.CollectIterationStats(this, oo, stoping);
            }

            DoAfterOptimization(o, stats, stoping);
            stats.CollectFinalStats(this, o, stoping, false);
            return false;
        }
        protected override void DoBeforeOptimization(IBaseObjective o, IOptimizerStats stats, IStopingCriteria stoping)
        {
            base.DoBeforeOptimization(o, stats, stoping);
            if (o.GetType().GetInterface(typeof(IDifferentiableBaseObjective).Name) != null)
                _Dow = new DifferentiableObjectiveWrapper((IDifferentiableBaseObjective)o);
            else
            {
                Console.WriteLine("Not a IDifferentiableBaseObjective for BatchGradientDescent.");
                Environment.Exit(-1);
            }
                
        }
        protected override void DoAfterOptimization(IBaseObjective o, IOptimizerStats stats, IStopingCriteria stoping) { }
        protected override void DoBeforeEachIteration(IBaseObjective o, IOptimizerStats stats, IStopingCriteria stoping) { }
        protected override void DoAfterEachIteration(IBaseObjective o, IOptimizerStats stats, IStopingCriteria stoping) { }
        protected override void DoBeforeUpdateStep(IBaseObjective o, IOptimizerStats stats, IStopingCriteria stoping) { }
        protected override void DoAfterUpdateStep(IBaseObjective o, IOptimizerStats stats, IStopingCriteria stoping) { }
    }



    public class StochasticGradientDescent : AbstractGradientOptimizer, IOptimizer
    {
        protected ILineSearchMethod _LineSearch;
        protected DifferentiableObjectiveWrapper _Dow;

        public StochasticGradientDescent(int debugLevel, ILineSearchMethod lineSearch)
            : base(debugLevel)
        {
            this._LineSearch = lineSearch;
        }

        public override bool Optimize(IBaseObjective o, IOptimizerStats stats, IStopingCriteria stoping)
        {

            if (o.GetType().GetInterface(typeof(IStochasticDifferentiableBaseObjective).Name) != null)
                _Dow = new DifferentiableObjectiveWrapper((IStochasticDifferentiableBaseObjective)o);
            else
            {
                Console.WriteLine("Not a IStochasticDifferentiableBaseObjective for StochasticGradientDescent.");
                Environment.Exit(-1);
            }

            IStochasticDifferentiableBaseObjective oo = (IStochasticDifferentiableBaseObjective)o;


            int i = 1;
            //Initialize structures
            stats.CollectInitStats(this, oo, stoping);
            DoBeforeOptimization(oo, stats, stoping);

            for (_CurrentIteration = 0; _CurrentIteration < _MaxIterations; _CurrentIteration++)
            {

                if (_DebugLevel > 0)
                {
                    Console.WriteLine(_CurrentIteration + " : " + _CurrentStep + " : " + ArrayMath.L2Norm(_CurrentGradient) + " : " + ArrayMath.ToString<double>(oo.Parameters));
                }

                DoBeforeEachIteration(oo, stats, stoping);

                for (; i < oo.DatasetSize(); i++)
                {

                    oo.NextDataSample();

                    //update gradient and value and find a direction
                    oo.CalculateGradient();
                    oo.CalculateValue();
                    _CurrentValue = oo.Value;
                    _CurrentGradient = oo.Gradients;
                    _CurrentDirection = ArrayMath.Negation(_CurrentGradient);

                    if (ArrayMath.DotProduct(_CurrentGradient, _CurrentDirection) > 0)
                    {
                        Console.WriteLine("Not a descent direction.");
                        stats.Print();
                        Environment.Exit(-1);
                    }

                    DoBeforeUpdateStep(oo, stats, stoping);

                    //find a step and update parameter
                    _CurrentStep = _LineSearch.DoLineSearch(_Dow);
                    if (_CurrentStep > 0 && _CurrentStep < 1E-100)
                    {
                        _CurrentStep = 1E-100;
                        _Dow.UpdateAlpha(_CurrentStep);
                    }
                    

                    //find a valid step?
                    if (_CurrentStep == -1)
                    {
                        //Console.WriteLine("Failed to find a step(Ignore)");
                        //stats.Print();
                        //Console.ReadKey()
                        //Environment.Exit(-1);
                    }
                    o.Parameters = o.Parameters;

                    DoAfterUpdateStep(oo, stats, stoping);

                    DoAfterEachIteration(oo, stats, stoping);
                    stats.CollectIterationStats(this, oo, stoping);

                    if (stoping.IsConvergent(oo))
                    {
                        DoAfterOptimization(o, stats, stoping);
                        stats.CollectFinalStats(this, oo, stoping, true);
                        return true;
                    }

                }
                i = 0;
                DoAfterEachIteration(o, stats, stoping);
            }

            DoAfterOptimization(o, stats, stoping);
            stats.CollectFinalStats(this, oo, stoping, false);
            return false;

        }
        protected override void DoBeforeOptimization(IBaseObjective o, IOptimizerStats stats, IStopingCriteria stoping)
        {
            base.DoBeforeOptimization(o, stats, stoping);
            if (o.GetType().GetInterface(typeof(IDifferentiableBaseObjective).Name) != null)
                _Dow = new DifferentiableObjectiveWrapper((IDifferentiableBaseObjective)o);
            else
            {
                Console.WriteLine("Not a IDifferentiableBaseObjective for StochasticGradientDescent.");
                Environment.Exit(-1);
            }

        }
        protected override void DoAfterOptimization(IBaseObjective o, IOptimizerStats stats, IStopingCriteria stoping) { }
        protected override void DoBeforeEachIteration(IBaseObjective o, IOptimizerStats stats, IStopingCriteria stoping) { }
        protected override void DoAfterEachIteration(IBaseObjective o, IOptimizerStats stats, IStopingCriteria stoping) { }
        protected override void DoBeforeUpdateStep(IBaseObjective o, IOptimizerStats stats, IStopingCriteria stoping) { }
        protected override void DoAfterUpdateStep(IBaseObjective o, IOptimizerStats stats, IStopingCriteria stoping) { }
    }


}


