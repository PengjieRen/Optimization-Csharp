using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Utils
{

    public class Interpolation {

	/**
	 * Fits a cubic polinomyal to a function given two points,
	 * such that either gradB is bigger than zero or funcB >= funcA
	 * 
	 * NonLinear Programming appendix C
	 * @param funcA
	 * @param gradA
	 * @param funcB
	 * @param gradB
	 */
	public static double CubicInterpolation(double a, 
			double funcA, double gradA, double b,double funcB, double gradB ){
		if(gradB < 0 && funcA > funcB){
			return -1;
		}
		
		double z = 3*(funcA-funcB)/(b-a) + gradA + gradB;
		double w = Math.Sqrt(z*z - gradA*gradB);
		double min = b -(gradB+w-z)*(b-a)/(gradB-gradA+2*w);
		return min;
	}
	
	public static double QuadraticInterpolation(double initFValue, 
			double initGrad, double point,double pointFValue){
				double min = -1*initGrad*point*point/(2*(pointFValue-initGrad*point-initFValue));
		return min;
	}
	
}




    public class ArrayMath
    {

        public static String ToString<T>(T[] v) {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            foreach (T t in v)
                sb.Append(t+",");
            sb.Append("]");
            return sb.ToString();
        }

        public static void Fill(double[] v, double value)
        {
            for (int i = 0; i < v.Length; i++)
            {
                v[i] = value;
            }
        }

        public static void SortDescending(double[] v) {
            Array.Sort(v, (a, b) => b.CompareTo(a));
        }

        public static void SortAscending(double[] v)
        {
            Array.Sort(v, (a, b) => a.CompareTo(b));
        }

        public static double DotProduct(double[] v1, double[] v2)
        {
            double result = 0;
            for (int i = 0; i < v1.Length; i++)
                result += v1[i] * v2[i];
            return result;
        }

        public static double TwoNormSquared(double[] v) {
		double result = 0;
		foreach(double d in v)
			result += d*d;
		return result;
	}

        public static bool ContainsInvalid(double[] v)
        {
            for (int i = 0; i < v.Length; i++)
                if (Double.IsNaN(v[i]) || Double.IsInfinity(v[i]))
                    return true;
            return false;
        }

        public static void CorrectInvalid(double[] v)
        {
            for (int i = 0; i < v.Length; i++)
                if (Double.IsInfinity(v[i]))
                    v[i] = 1E-4;
        }



        public static double SafeAdd(double[] toAdd)
        {
            // Make sure there are no positive infinities
            double sum = 0;
            for (int i = 0; i < toAdd.Length; i++)
            {
                sum += toAdd[i];
            }

            return sum;
        }


        public static double[] ArrayMinus(double[] w, double[] v){
            double[] result = (double[])w.Clone();
		for(int i=0; i<w.Length;i++){
			result[i] -= v[i];
		}
		return result;
	}

        public static double[] ArrayAdd(double[] w, double[] v){
            double[] result = (double[])w.Clone();
		for(int i=0; i<w.Length;i++){
			result[i] += v[i];
		}
		return result;
	}

        public static double[] ArrayMinus(double[] result, double[] w, double[] v)
        {
            for (int i = 0; i < w.Length; i++)
            {
                result[i] = w[i] - v[i];
            }
            return result;
        }

        public static double[] Negation(double[] w){
		double[] result  = new double[w.Length];
		for(int i=0; i<w.Length;i++){
			result[i] = -w[i];
		}
		return result;
	}

        public static double[,] OuterProduct(double[] w, double[] v){
		double[,] result = new double[w.Length,v.Length];
		for(int i = 0; i < w.Length; i++){
			for(int j = 0; j < v.Length; j++){
				result[i,j] = w[i]*v[j];
			}
		}
		return result;
	}

        public static bool IsAllPositive(double[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] < 0) return false;
            }
            return true;
        }

        /**
         * results = a*W*V
         * @param w
         * @param v
         * @param a
         * @return
         */
        public static double[,] WeightedOuterProduct(double[] w, double[] v, double a){
		double[,] result = new double[w.Length,v.Length];
		for(int i = 0; i < w.Length; i++){
			for(int j = 0; j < v.Length; j++){
				result[i,j] = a*w[i]*v[j];
			}
		}
		return result;
	}


        public static void SetEqual(double[][][][] dest, double[][][][] source)
        {
            for (int i = 0; i < source.Length; i++)
            {
                SetEqual(dest[i], source[i]);
            }
        }


        public static void SetEqual(double[][][] dest, double[][][] source)
        {
            for (int i = 0; i < source.Length; i++)
            {
                Set(dest[i], source[i]);
            }
        }


        public static void Set(double[][] dest, double[][] source)
        {
            for (int i = 0; i < source.Length; i++)
            {
                SetEqual(dest[i], source[i]);
            }
        }

        public static void SetEqual(double[] dest, double[] source)
        {
            Array.Copy(source, 0, dest, 0, source.Length);
        }

        public static void PlusEquals(double[][][][] array, double val)
        {
            for (int i = 0; i < array.Length; i++)
            {
                PlusEquals(array[i], val);
            }
        }

        public static void PlusEquals(double[][][] array, double val)
        {
            for (int i = 0; i < array.Length; i++)
            {
                PlusEquals(array[i], val);
            }
        }

        /**
         * w = w + a*v
         * @param w
         * @param v
         * @param a
         */
        public static void PlusEquals(double[] w, double[] v, double a)
        {
            for (int i = 0; i < w.Length; i++)
            {
                w[i] += a * v[i];
            }
        }

        public static void PlusEquals(double[][] array, double val)
        {
            for (int i = 0; i < array.Length; i++)
            {
                PlusEquals(array[i], val);
            }
        }

        public static void PlusEquals(double[] array, double val)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] += val;
            }
        }

        /**
         * w = w - a*v
         * @param w
         * @param v
         * @param a
         */
        public static void MinusEquals(double[] w, double[] v, double a)
        {
            for (int i = 0; i < w.Length; i++)
            {
                w[i] -= a * v[i];
            }
        }
        /**
         * v = w - a*v
         * @param w
         * @param v
         * @param a
         */
        public static void MinusEqualsInverse(double[] w, double[] v, double a)
        {
            for (int i = 0; i < w.Length; i++)
            {
                v[i] = w[i] - a * v[i];
            }
        }

        public static double Sum(double[] array)
        {
            double res = 0;
            for (int i = 0; i < array.Length; i++)
            {
                res += array[i];
            }

            return res;
        }

        public static double Cosine(double[] a,
                double[] b)
        {
            return (DotProduct(a, b) + 1e-5) / (Math.Sqrt(DotProduct(a, a) + 1e-5) * Math.Sqrt(DotProduct(b, b) + 1e-5));
        }

        public static double Max(double[] ds) {
		double max = Double.NegativeInfinity;
		foreach(double d in ds) max = Math.Max(d,max);
		return max;
	}

        public static void Exponentiate(double[] a)
        {
            for (int i = 0; i < a.Length; i++)
            {
                a[i] = Math.Exp(a[i]);
            }
        }

        public static int Sum(int[] array)
        {
            int res = 0;
            for (int i = 0; i < array.Length; i++)
            {
                res += array[i];
            }

            return res;
        }

        /**
         * 
         * @param vector
         * @return
         */
        public static double L2Norm(double[] vector)
        {
            double value = 0;
            for (int i = 0; i < vector.Length; i++)
            {
                double v = vector[i];
                value += v * v;
            }
            return Math.Sqrt(value);
        }

        public static void ScalarMultiplication(double[] w, double v)
        {
            int w1 = w.Length;
            for (int w_i1 = 0; w_i1 < w1; w_i1++)
            {
                w[w_i1] *= v;
            }
        }

        public static void TimesEquals(double[] array, double val)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] *= val;
            }
        }



        /**
         * sums part of the array -- the sum(array) method is equivalent to 
         * sumPart(array, 0, array.length)
         * @param array
         * @param start included in sum
         * @param end excluded from sum
         * @return
         */
        public static double SumPart(double[] array, int start, int end)
        {
            double res = 0;
            for (int i = start; i < end; i++)
            {
                res += array[i];
            }
            return res;
        }
			 

    }
}
