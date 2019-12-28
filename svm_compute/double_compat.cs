using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace svm_compute
{
    public static class double_compat
    {
        public static double fix_double(string double_value)
        {
            var infinity = "∞";
            var neg_infinity = "-∞";
            var pos_infinity = "+∞";
            var NaN = "NaN";

            double value = 0d;

            if (double.TryParse(double_value, NumberStyles.Float, CultureInfo.InvariantCulture, out value)) return fix_double(value);

            if (double_value.Contains(pos_infinity)) value = double.PositiveInfinity;
            else if (double_value.Contains(neg_infinity)) value = double.NegativeInfinity;
            else if (double_value.Contains(infinity)) value = double.PositiveInfinity;
            else if (double_value.Contains(NaN)) value = double.NaN;
            //else if (!double.TryParse(double_value, NumberStyles.Float, CultureInfo.InvariantCulture, out value)) throw new Exception();

            value = fix_double(value);

            return value;
        }

        public static double fix_double(double value)
        {
            // the doubles must be compatible with libsvm which is written in C (C and CSharp have different min/max values for double)
            const double c_double_max = (double)1.79769e+308;
            const double c_double_min = (double)-c_double_max;
            const double double_zero = (double)0;

            if (value >= c_double_min && value <= c_double_max)
            {
                return value;
            }
            else if (double.IsPositiveInfinity(value) || value >= c_double_max || value >= double.MaxValue)
            {
                value = c_double_max;
            }
            else if (double.IsNegativeInfinity(value) || value <= c_double_min || value <= double.MinValue)
            {
                value = c_double_min;
            }
            else if (double.IsNaN(value))
            {
                value = double_zero;
            }

            return value;
        }
    }
}
