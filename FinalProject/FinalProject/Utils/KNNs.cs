using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace NutritionalInfoApp.Utils
{
    /// <summary>
    /// Summary description for KNN
    /// </summary>
    public class KNNs
    {
        public KNNs()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        /// <summary>
        /// Calculates the Euclidean Distance Measure between two data points
        /// </summary>
        /// <param name="X">An array with the values of an object or datapoint</param>
        /// <param name="Y">An array with the values of an object or datapoint</param>
        /// <returns>Returns the Euclidean Distance Measure Between Points X and Points Y</returns>
        public static double EuclideanDistance(double[] X, double[] Y)
        {
            int count = 0;

            double distance = 0.0;

            double sum = 0.0;


            if (X.GetUpperBound(0) != Y.GetUpperBound(0))
            {
                throw new System.ArgumentException("the number of elements in X must match the number of elements in Y");
            }
            else
            {
                count = X.Length;
            }

            for (int i = 0; i < count; i++)
            {
                sum = sum + Math.Pow(Math.Abs(X[i] - Y[i]), 2);
            }

            distance = Math.Sqrt(sum);

            return distance;
        }

        /// <summary>
        /// Calculates the Minkowski Distance Measure between two data points
        /// </summary>
        /// <param name="X">An array with the values of an object or datapoint</param>
        /// <param name="Y">An array with the values of an object or datapoint</param>
        /// <returns>Returns the Minkowski Distance Measure Between Points X and Points Y</returns>
        public static double ChebyshevDistance(double[] X, double[] Y)
        {
            int count = 0;

            if (X.GetUpperBound(0) != Y.GetUpperBound(0))
            {
                throw new System.ArgumentException("the number of elements in X must match the number of elements in Y");
            }
            else
            {
                count = X.Length;
            }
            double[] sum = new double[count];

            for (int i = 0; i < count; i++)
            {
                sum[i] = Math.Abs(X[i] - Y[i]);
            }
            double max = double.MinValue;
            foreach (double num in sum)
            {
                if (num > max)
                {
                    max = num;
                }
            }
            return max;
        }
        /// <summary>
        /// Calculates the Manhattan Distance Measure between two data points
        /// </summary>
        /// <param name="X">An array with the values of an object or datapoint</param>
        /// <param name="Y">An array with the values of an object or datapoint</param>
        /// <returns>Returns the Manhattan Distance Measure Between Points X and Points Y</returns>
        public static double ManhattanDistance(double[] X, double[] Y)
        {
            int count = 0;

            double distance = 0.0;

            double sum = 0.0;


            if (X.GetUpperBound(0) != Y.GetUpperBound(0))
            {
                throw new System.ArgumentException("the number of elements in X must match the number of elements in Y");
            }
            else
            {
                count = X.Length;
            }

            for (int i = 0; i < count; i++)
            {
                sum = sum + Math.Abs(X[i] - Y[i]);
            }

            distance = sum;

            return distance;
        }

    }
}