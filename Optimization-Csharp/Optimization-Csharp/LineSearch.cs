using Optimization.Objective;
using Optimization.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.LineSearch
{
    /**
     * 
     * A wrapper for DifferentiableObjective
     * 
     **/
    public class DifferentiableObjectiveWrapper
    {

        private IDifferentiableBaseObjective _Objective;
        //This records the history
        private int _Iterations;
        private List<double> _Steps;
        private List<double> _Values;
        private List<double> _Gradients;

        //This variables cannot change
        private double[] _OriginalParameters;
        private double[] _OriginalSearchDirection;


        public DifferentiableObjectiveWrapper(IDifferentiableBaseObjective o)
        {
            this._Objective = o;
            _OriginalParameters = new double[o.Dimension];
            _OriginalSearchDirection = new double[o.Dimension];
            _Steps = new List<double>();
            _Values = new List<double>();
            _Gradients = new List<double>();
	}
        /// <summary>
        /// Reset and clear all histories.
        /// </summary>
        public void Reset()
        {
            //Copy initial values
            Array.Copy(_Objective.Parameters, 0, _OriginalParameters, 0, _Objective.Dimension);
            Array.Copy(ArrayMath.Negation(_Objective.Gradients), 0, _OriginalSearchDirection, 0, _Objective.Dimension);

            //Initialize variables
            _Iterations = 0;
            _Steps.Clear();
            _Values.Clear();
            _Gradients.Clear();

            _Objective.CalculateValue();
            _Values.Add(_Objective.Value);
            _Objective.CalculateGradient();
            _Gradients.Add(ArrayMath.DotProduct(_Objective.Gradients, _OriginalSearchDirection));
            _Steps.Add(0);
        }

        /// <summary>
        /// Update the objective with the step, i.e., parameters=parameters+alpha*direction
        /// It can be called multiple times. 
        /// But every time is updated based on the same state where Reset() is called. 
        /// </summary>
        /// <param name="alpha">the step</param>
        public void UpdateAlpha(double alpha){
		if(alpha < 0){
			Console.WriteLine("alpha may not be smaller that zero");
            Console.ReadKey();
            Environment.Exit(-1);
		}

		
		//x_t+1 = x_t + alpha*direction
        Array.Copy(_OriginalParameters, 0, _Objective.Parameters,0,_Objective.Dimension);
        ArrayMath.PlusEquals(_Objective.Parameters, _OriginalSearchDirection, alpha);
        //if (_Objective.GetType().GetInterface(typeof(IProjectedDifferentiableBaseObjective).Name) != null || _Objective.GetType().GetInterface(typeof(IProjectedStochasticDifferentiableBaseObjective).Name) != null)
        //{
        //    ((IProjectedDifferentiableBaseObjective)_Objective).Project(_Objective.Parameters);
        //}
        //_Objective.Parameters = _Objective.Parameters;

        _Iterations++;
        _Steps.Add(alpha);
        _Objective.CalculateValue();
        _Values.Add(_Objective.Value);
        _Objective.CalculateGradient();
		_Gradients.Add(ArrayMath.DotProduct(_Objective.Gradients,_OriginalSearchDirection));		
	}

        public double GetValue(int iter)
        {
            return _Values.ElementAt(iter);
        }

        public double GetCurrentValue()
        {
            return _Values.ElementAt(_Iterations);
        }

        public double GetOriginalValue()
        {
            return _Values.ElementAt(0);
        }

        public double GetGradient(int iter)
        {
            return _Gradients.ElementAt(iter);
        }

        public double GetCurrentGradient()
        {
            return _Gradients.ElementAt(_Iterations);
        }

        public double GetInitialGradient()
        {
            return _Gradients.ElementAt(0);
        }

        public int GetIteration(){
        return _Iterations;
        }


        public double GetCurrentAlpha()
        {
            return _Steps.ElementAt(_Iterations);
        }

    }

    /**
     * 
     * Wolfe Conditions
     * 
     **/
    public class WolfeConditions
    {
        private int _DebugLevel = 0;
        public void SetDebugLevel(int DebugLevel)
        {
            _DebugLevel = DebugLevel;
        }

        public static bool IsSuficientDecrease(DifferentiableObjectiveWrapper o, double c1)
        {
            double value = o.GetOriginalValue() + c1 * o.GetCurrentAlpha() * o.GetInitialGradient();
            return o.GetCurrentValue() <= value;
        }




        public static bool IsSufficientCurvature(DifferentiableObjectiveWrapper o, double c1, double c2)
        {
            return Math.Abs(o.GetCurrentGradient()) <= -c2 * o.GetInitialGradient();
        }
    }

    public class PickFirstStep
    {
        double _InitValue;
        public PickFirstStep(double initValue)
        {
            _InitValue = initValue;
        }

        public double GetFirstStep(ILineSearchMethod ls)
        {
            return _InitValue;
        }
        public void CollectInitValues(ILineSearchMethod ls)
        {

        }

        public void CollectFinalValues(ILineSearchMethod ls)
        {

        }
    }


    /**
     * 
     * Line Search Interface
     * 
     **/
    public interface ILineSearchMethod
    {

        double DoLineSearch(DifferentiableObjectiveWrapper o);

        double GetInitialGradient();
        double GetPreviousInitialGradient();
        double GetPreviousStepUsed();
        void SetDebugLevel(int level);
        void Reset();
    }

    /**
     * Implements Back Tracking Line Search as described on page 37 of Numerical Optimization.
     * Also known as armijo rule
     * 
     **/ 
    public class ArmijoMinimizationLineSearch : ILineSearchMethod
    {
        /**
	 * How much should the step size decrease at each iteration.
	 */
	private double _ContractionFactor = 0.5;
	private double _C1 = 0.0001;
	
	private double _Sigma1 = 0.1;
	private double _Sigma2 = 0.9;

	private int _MaxIterations = 10;

    private double _PreviousStepPicked = -1;
    private double _PreviousInitGradientDot = -1;
    private double _CurrentInitGradientDot = -1;
    private int _DebugLevel = 0;

    private PickFirstStep _PickFirstStep;
    public ArmijoMinimizationLineSearch(int debugLevel, PickFirstStep pickFirstStep)
    {
        this._DebugLevel = debugLevel;
        this._PickFirstStep = pickFirstStep;
    }

    void ILineSearchMethod.SetDebugLevel(int level)
    {
        _DebugLevel = level;
	}

    void ILineSearchMethod.Reset()
    {
        _PreviousStepPicked = -1;
        _PreviousInitGradientDot = -1;
        _CurrentInitGradientDot = -1;
	}

    double ILineSearchMethod.DoLineSearch(DifferentiableObjectiveWrapper o)
    {

		o.Reset();//important!

		_CurrentInitGradientDot = o.GetInitialGradient();

		int nrIterations = 0;
        o.UpdateAlpha(_PickFirstStep.GetFirstStep(this));	
		while(!WolfeConditions.IsSuficientDecrease(o,_C1)){
			if(nrIterations >= _MaxIterations){
				return -1;
			}
			double alpha=o.GetCurrentAlpha();
			double alphaTemp = 
				Interpolation.QuadraticInterpolation(o.GetOriginalValue(), o.GetInitialGradient(), alpha, o.GetCurrentValue());
			if(alphaTemp >= _Sigma1 || alphaTemp <= _Sigma2*o.GetCurrentAlpha()){
				alpha = alphaTemp;
			}else{
				alpha = alpha*_ContractionFactor;
			}
			o.UpdateAlpha(alpha);
			nrIterations++;			
		}
		
		_PreviousInitGradientDot = _CurrentInitGradientDot;
		_PreviousStepPicked = o.GetCurrentAlpha();
		return o.GetCurrentAlpha();
	}

    double ILineSearchMethod.GetInitialGradient()
    {
		return _CurrentInitGradientDot;
		
	}

    double ILineSearchMethod.GetPreviousInitialGradient()
    {
		return _PreviousInitGradientDot;
	}

    double ILineSearchMethod.GetPreviousStepUsed()
    {
		return _PreviousStepPicked;
	}
    }


    /**
 * Implements Back Tracking Line Search as described on page 37 of Numerical Optimization.
 * Also known as armijo rule
 * 
 **/
    public class ProjectedArmijoMinimizationLineSearch : ILineSearchMethod
    {
        /**
	 * How much should the step size decrease at each iteration.
	 */
        private double _ContractionFactor = 0.5;
        private double _C1 = 1E-4;

        private double _Sigma1 = 0.1;
        private double _Sigma2 = 0.9;

        private int _MaxIterations = 10;

        private double _PreviousStepPicked = -1;
        private double _PreviousInitGradientDot = -1;
        private double _CurrentInitGradientDot = -1;
        private int _DebugLevel = 0;

        private PickFirstStep _PickFirstStep;
        public ProjectedArmijoMinimizationLineSearch(int debugLevel, PickFirstStep pickFirstStep)
        {
            this._DebugLevel = debugLevel;
            this._PickFirstStep = pickFirstStep;
        }

        void ILineSearchMethod.SetDebugLevel(int level)
        {
            _DebugLevel = level;
        }

        void ILineSearchMethod.Reset()
        {
            _PreviousStepPicked = -1;
            _PreviousInitGradientDot = -1;
            _CurrentInitGradientDot = -1;
        }

        double ILineSearchMethod.DoLineSearch(DifferentiableObjectiveWrapper o)
        {

            o.Reset();//important!

            _CurrentInitGradientDot = o.GetInitialGradient();

            int nrIterations = 0;
            o.UpdateAlpha(_PickFirstStep.GetFirstStep(this));
            while (o.GetCurrentValue() >
            o.GetOriginalValue() + _C1 * o.GetCurrentGradient())
            {
                if (nrIterations >= _MaxIterations)
                {
                    return -1;
                }
                double alpha = o.GetCurrentAlpha();
                double alphaTemp =
                    Interpolation.QuadraticInterpolation(o.GetOriginalValue(), o.GetInitialGradient(), alpha, o.GetCurrentValue());
                if (alphaTemp >= _Sigma1 || alphaTemp <= _Sigma2 * o.GetCurrentAlpha())
                {
                    alpha = alphaTemp;
                }
                else
                {
                    alpha = alpha * _ContractionFactor;
                }
                if (alpha < 0)
                    alpha = 1E-6;
                o.UpdateAlpha(alpha);
                nrIterations++;
            }

            _PreviousInitGradientDot = _CurrentInitGradientDot;
            _PreviousStepPicked = o.GetCurrentAlpha();
            return o.GetCurrentAlpha();
        }

        double ILineSearchMethod.GetInitialGradient()
        {
            return _CurrentInitGradientDot;

        }

        double ILineSearchMethod.GetPreviousInitialGradient()
        {
            return _PreviousInitGradientDot;
        }

        double ILineSearchMethod.GetPreviousStepUsed()
        {
            return _PreviousStepPicked;
        }
    }

    
}
