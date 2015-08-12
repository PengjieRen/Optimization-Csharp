using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Optimization.Utils;

namespace Optimization.Projection
{
    public interface IProjection
    {
        void Project(double[] Points);
    }

    public class BoundsProjection : IProjection {

        private double A, B;
        private bool IgnoreA = false;
        private bool IgnoreB = false;
        public BoundsProjection(double LowerBound, double UpperBound)
        {
            if (Double.IsInfinity(LowerBound))
            {
                this.IgnoreA = true;
            }
            else
            {
                this.A = LowerBound;
            }
            if (Double.IsInfinity(UpperBound))
            {
                this.IgnoreB = true;
            }
            else
            {
                this.B = UpperBound;
            }
        }

        void IProjection.Project(double[] Points)
        {
            for (int i = 0; i < Points.Length; i++)
            {
                if (!IgnoreA && Points[i] < A)
                {
                    Points[i] = A;
                }
                else if (!IgnoreB && Points[i] > B)
                {
                    Points[i] = B;
                }
            }
        }
    }

    public class SimplexProjection : IProjection {
        private double Scale;
        public SimplexProjection(double Scale)
        {
            this.Scale = Scale;
        }

        void IProjection.Project(double[] Points)
        {
            double[] ds = new double[Points.Length];
            Array.Copy(Points,0,ds,0,ds.Length);
            
            //If sum is smaller then zero then its ok
            for (int i = 0; i < ds.Length; i++) ds[i] = ds[i] > 0 ? ds[i] : 0;
            double sum = ArrayMath.Sum(ds);
            if (Scale - sum >= 0)
            {// -1.E-10 ){
                Array.Copy(ds, 0, Points, 0, ds.Length);
                //System.out.println("Not projecting");
                return;
            }
            //System.out.println("projecting " + sum + " scontraints " + scale);	
            ArrayMath.SortDescending(ds);
            double currentSum = 0;
            double previousTheta = ds[0];
            double theta = 0;
            for (int i = 0; i < ds.Length; i++)
            {
                currentSum += ds[i];
                theta = (currentSum - Scale) / (i + 1);
                if (ds[i] <= theta)
                {
                    break;
                }
                previousTheta = theta;
            }

            for (int i = 0; i < Points.Length; i++)
            {
                Points[i] = Math.Max(Points[i] - previousTheta, 0);
            }
        }
    }
}
