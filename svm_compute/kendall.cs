using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace svm_compute
{
    public class kendall
    {
        public static double kendall_tau(double[] array1, double[] array2)
        {
            if (array1.Length != array2.Length)
                throw new ArgumentException("Arrays must be the same length");

            int n = array1.Length;
            double tau = 0;
            for (int i = 1; i < n; i++)
            for (int j = 0; j < i; j++)
            {
                tau += Math.Sign(array1[i] - array1[j]) * Math.Sign(array2[i] - array2[j]);
            }
            int combin = n * (n - 1) / 2;
            return combin == 0 ? 0 : tau / combin;
        }

        public static double kendall_tau(List<double> array1, List<double> array2)
        {
            if (array1.Count != array2.Count)
                throw new ArgumentException("Arrays must be the same length");

            int n = array1.Count;
            double tau = 0;
            for (int i = 1; i < n; i++)
            for (int j = 0; j < i; j++)
            {
                tau += Math.Sign(array1[i] - array1[j]) * Math.Sign(array2[i] - array2[j]);
            }
            int combin = n * (n - 1) / 2;
            return combin == 0 ? 0 : tau / combin;
        }

    }
}
