using Optimization.Projection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Objective
{

    public interface IBaseObjective
    {
        double[] Parameters{get;set;}
        int Dimension { get; }
        double Value { get; }
        int DebugLevel { get; set; }

        void CalculateValue();
        String ToString();

    }

    public interface IDifferentiableBaseObjective : IBaseObjective
    {
        double[] Gradients { get; }


        void CalculateGradient();
    }


    public interface IProjectedDifferentiableBaseObjective : IDifferentiableBaseObjective
    {

        void Project(double[] Points);
        
    }

    public interface IStochasticDifferentiableBaseObjective : IDifferentiableBaseObjective
    {
        double[] CurrentDataSample { get; }
        void NextDataSample();
        int DatasetSize();
    }

    public interface IProjectedStochasticDifferentiableBaseObjective : IStochasticDifferentiableBaseObjective, IProjectedDifferentiableBaseObjective
    {
    }


    public abstract class AbstractBaseObjective:IBaseObjective{
        protected double[] _Parameters;
        protected double _Value = 0;
        protected int _DebugLevel = 0;

        double[] IBaseObjective.Parameters { get { return _Parameters; } set { this._Parameters = value; } }
        int IBaseObjective.Dimension { get { return _Parameters.Length; } }
        double IBaseObjective.Value { get { return _Value; } }
        int IBaseObjective.DebugLevel { get { return _DebugLevel; } set { this._DebugLevel = value; } }

         public AbstractBaseObjective(int DebugLevel)
        {
            this._DebugLevel = DebugLevel;
            _Parameters = InitParameters();
        }

         public abstract double[] InitParameters();

        String IBaseObjective.ToString()
        {
            return _Parameters.ToString();
        }

        void IBaseObjective.CalculateValue() { _Value = ValueAt(_Parameters); }
        public abstract double ValueAt(double[] parameters);
    }

    public abstract class AbstractDifferentiableBaseObjective :AbstractBaseObjective, IDifferentiableBaseObjective
    {

        protected double[] _Gradient;
       
        double[] IDifferentiableBaseObjective.Gradients { get { return _Gradient; } }

        public AbstractDifferentiableBaseObjective(int DebugLevel)
            : base(DebugLevel)
        {
            _Gradient=new double[_Parameters.Length];
        }

        void IDifferentiableBaseObjective.CalculateGradient() { _Gradient = GradientAt(_Parameters); }

        public abstract double[] GradientAt(double[] parameters);
    }

    public abstract class AbstractProjectedDifferentiableBaseObjective :AbstractDifferentiableBaseObjective, IProjectedDifferentiableBaseObjective {

        protected IProjection _Projection;

        double[] IBaseObjective.Parameters
        {
            get { return base._Parameters; }
            set
            {
                base._Parameters = value;
                _Projection.Project(base._Parameters);//important!!!
            }
        }

        public AbstractProjectedDifferentiableBaseObjective(int DebugLevel)
            : base(DebugLevel)
        { 
        
        }

        void IProjectedDifferentiableBaseObjective.Project(double[] Points)
        {
            _Projection.Project(Points);
        }
    }

    public abstract class AbstractStochasticDifferentiableBaseObjective : AbstractDifferentiableBaseObjective, IStochasticDifferentiableBaseObjective {
        protected double[] _CurrentDataSample;

        double[] IStochasticDifferentiableBaseObjective.CurrentDataSample { get { return _CurrentDataSample; } }

        public AbstractStochasticDifferentiableBaseObjective(int DebugLevel)
            : base(DebugLevel)
        { 
        
        }

        public abstract int DatasetSize();

        public abstract void NextDataSample();
        public override double ValueAt(double[] parameters) { return ValueAt(parameters, _CurrentDataSample); }
        public abstract double ValueAt(double[] parameters, double[] datasample);

        public override double[] GradientAt(double[] parameters) { return GradientAt(parameters, _CurrentDataSample); }
        public abstract double[] GradientAt(double[] parameters, double[] datasample);
    }

    public abstract class AbstractProjectedStochasticDifferentiableBaseObjective : AbstractDifferentiableBaseObjective,IProjectedStochasticDifferentiableBaseObjective
    {
        protected IProjection _Projection;
        protected double[] _CurrentDataSample;

        double[] IBaseObjective.Parameters
        {
            get { return base._Parameters; }
            set
            {
                base._Parameters = value;
                _Projection.Project(base._Parameters);//important!!!
            }
        }

        double[] IStochasticDifferentiableBaseObjective.CurrentDataSample { get { return _CurrentDataSample; } }

        public AbstractProjectedStochasticDifferentiableBaseObjective(int DebugLevel)
            : base(DebugLevel)
        { 
        
        }


        void IProjectedDifferentiableBaseObjective.Project(double[] Points)
        {
            _Projection.Project(Points);
        }

        public abstract int DatasetSize();

        public abstract void NextDataSample();

        public override double ValueAt(double[] parameters) { return ValueAt(parameters, _CurrentDataSample); }
        public abstract double ValueAt(double[] parameters, double[] datasample);

        public override double[] GradientAt(double[] parameters) { return GradientAt(parameters, _CurrentDataSample); }
        public abstract double[] GradientAt(double[] parameters, double[] datasample);
    }
    
}
