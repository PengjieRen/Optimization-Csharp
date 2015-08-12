using Optimization.Objective;
using Optimization.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Optimizer.StopingCriteria
{
    public class CompositeStopingCriteria:IStopingCriteria {
        List<IStopingCriteria> _Stops;

        public CompositeStopingCriteria() {
            _Stops = new List<IStopingCriteria>();
        }

        public void Add(IStopingCriteria stop){
        _Stops.Add(stop);
        }

        public List<IStopingCriteria> StopingCriterias { get{return _Stops;}}

        public bool IsConvergent(IBaseObjective o)
        {
            foreach (IStopingCriteria stop in _Stops)
            {
                if (!stop.IsConvergent(o))
                    return false;
            }
            return true;
        }
    }
    public interface IStopingCriteria
    {
        bool IsConvergent(IBaseObjective obj);
    }

    public class GradientL2NormStopingCriteria : IStopingCriteria{
	
	/**
	 * Stop if gradientNorm/(originalGradientNorm) smaller
	 * than gradientConvergenceValue
	 */
	protected double _GradientConvergenceValue;


    public GradientL2NormStopingCriteria(double gradientConvergenceValue)
    {
        this._GradientConvergenceValue = gradientConvergenceValue;
	}
	
    public bool IsConvergent(IBaseObjective o)
    {
        IDifferentiableBaseObjective oo=null;
        if (o.GetType().GetInterface(typeof(IDifferentiableBaseObjective).Name) != null)
            oo = (IDifferentiableBaseObjective)o;
        else
        {
            Console.WriteLine("Not a IDifferentiableBaseObjective for GradientL2NormStopingCriteria.");
            Environment.Exit(-1);
        }

		double norm = ArrayMath.L2Norm(oo.Gradients);
        if (norm < _GradientConvergenceValue)
        {
			return true;
		}
		return false;
	}
}


    public class AverageValueDifferenceStopingCriteria : IStopingCriteria
    {
	
	/**
	 * Stop if the different between values is smaller than a treshold
	 */
	protected double _ValueConvergenceValue=10E-6;
	protected double _PreviousValue = Double.NaN;
	protected double _CurrentValue = Double.NaN;

    public AverageValueDifferenceStopingCriteria(double valueConvergenceValue)
    {
        this._ValueConvergenceValue = valueConvergenceValue;
	}

    public bool IsConvergent(IBaseObjective o)
    {
        IDifferentiableBaseObjective oo = null;
        if (o.GetType().GetInterface(typeof(IDifferentiableBaseObjective).Name) != null)
            oo = (IDifferentiableBaseObjective)o;
        else
        {
            Console.WriteLine("Not a IDifferentiableBaseObjective for ValueDifferenceStopingCriteria.");
            Environment.Exit(-1);
        }

		if(ArrayMath.L2Norm(oo.Gradients) == 0){
			return true;
		}
		if(_CurrentValue == _PreviousValue){
			return true;
		}
		if(Double.IsNaN(_CurrentValue)){
            _CurrentValue = oo.Value;
			return false;
		}else {
            _PreviousValue = _CurrentValue;
            _CurrentValue = oo.Value;
            double valueDiff = Math.Abs(_CurrentValue - _PreviousValue);
            double valueAverage = Math.Abs(_PreviousValue + _CurrentValue + 0.000000001) / 2.0;
            if (valueDiff / valueAverage < _ValueConvergenceValue)
            {
				return true;
			}
		}
		return false;
	}
}

    public class StochasticGradientL2NormStopingCriteria : IStopingCriteria
    {
	
	
	/**
	 * Stop if gradientNorm/(originalGradientNorm) smaller
	 * than gradientConvergenceValue
	 */
	protected double _GradientConvergenceValue;

    protected int _Datanum;

    public StochasticGradientL2NormStopingCriteria(double gradientConvergenceValue, int datanum)
    {
        this._GradientConvergenceValue = gradientConvergenceValue;
        this._Datanum = datanum;
	}

    protected int i = 1;
    protected double[] _Gradents;
    public bool IsConvergent(IBaseObjective o)
    {

        IDifferentiableBaseObjective oo = null;
        if (o.GetType().GetInterface(typeof(IDifferentiableBaseObjective).Name) != null)
            oo = (IDifferentiableBaseObjective)o;
        else
        {
            Console.WriteLine("Not a IDifferentiableBaseObjective for GradientL2NormStopingCriteria.");
            Environment.Exit(-1);
        }

        if (_Gradents == null)
		{
            _Gradents = new double[oo.Gradients.Length];
		}
		
		
        _Gradents = ArrayMath.ArrayAdd(_Gradents, oo.Gradients);
		
		if(i%_Datanum==0)
		{
            double norm = ArrayMath.L2Norm(_Gradents);
			if(norm < _GradientConvergenceValue){
				return true;
			}
            ArrayMath.Fill(_Gradents, 0);
		}
        i++;
		return false;
		
	}
}


    public class StochasticAverageValueDifferenceStopingCriteria : IStopingCriteria{
	
	/**
	 * Stop if the different between values is smaller than a treshold
	 */
	protected double _ValueConvergenceValue=10E-6;
	protected double _PreviousValue = Double.NaN;
	protected double _CurrentValue = Double.NaN;
	
	int datanum;

    public StochasticAverageValueDifferenceStopingCriteria(double valueConvergenceValue, int datanum)
    {
        this._ValueConvergenceValue = valueConvergenceValue;
		this.datanum=datanum;
	}
	
	public void reset(){
        _PreviousValue = Double.NaN;
        _CurrentValue = Double.NaN;
	}


	private int i=1;
	private double sum=0;
    public bool IsConvergent(IBaseObjective o)
    {

        IDifferentiableBaseObjective oo = null;
        if (o.GetType().GetInterface(typeof(IDifferentiableBaseObjective).Name) != null)
            oo = (IDifferentiableBaseObjective)o;
        else
        {
            Console.WriteLine("Not a IDifferentiableBaseObjective for GradientL2NormStopingCriteria.");
            Environment.Exit(-1);
        }

		sum+=oo.Value;
		if(i%datanum==0)
		{
            if (Double.IsNaN(_CurrentValue))
			{
                _CurrentValue = sum;
            }
            else if (Double.IsNaN(_PreviousValue))
			{
                _PreviousValue = _CurrentValue;
                _CurrentValue = sum;
            }
            else if (Math.Abs(_CurrentValue - _PreviousValue) / (Math.Abs(_PreviousValue + _CurrentValue + 0.000000001) / 2.0) < _ValueConvergenceValue)
			{
				return true;
			}else
			{
                _PreviousValue = _CurrentValue;
                _CurrentValue = sum;
			}
			sum=0;
		}
        i++;
		return false;
	}
}
}
