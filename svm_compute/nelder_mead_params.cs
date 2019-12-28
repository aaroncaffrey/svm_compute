using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Math.Convergence;
using svm_compute;

namespace svm_compute
{
    public class nelder_mead_params
    {
        private class nelder_mead_internal_params
        {
            public libsvm_caller.libsvm_kernel_type kernel = libsvm_caller.libsvm_kernel_type.rbf;

            public string training_file;
            public string train_stdout_file = null;
            public string train_stderr_file = null;

            public TimeSpan? point_max_time = null;
            public TimeSpan? process_max_time = null;

            public List<(int class_id, double weight)> weights = null;

            //public double[] worst_values = null;
            //public double[] best_values = null;

            public readonly libsvm_grid.best_rate_container best_rate_container = new libsvm_grid.best_rate_container();

            //public double worst_rate = 100;
            //public double best_rate = 0;

            private readonly object best_rate_lock = new object();

            public int iterations = 0;

            //public double[] lowest_values = null;
            //public double[] highest_values = null;


            //public double[] lowest_values_rates = null;
            //public double[] highest_values_rates = null;

            public libsvm_caller.svm_implementation svm_implementation;
            public libsvm_caller.libsvm_svm_type svm_type = libsvm_caller.libsvm_svm_type.c_svc;

            public int inner_cv_folds = 5;
            public bool cv_probability_estimates = false;
            public bool cv_shrinking_heuristics = true;

            public readonly int round_digits = 12;

            public int index_cost = -1;
            public int index_gamma = -1;
            public int index_epsilon = -1;
            public int index_coef0 = -1;
            public int index_degree = -1;

            public bool search_cost = false;
            public bool search_gamma = false;
            public bool search_epsilon = false;
            public bool search_coef0 = false;
            public bool search_degree = false;

            public double nelder_mead_callback(double[] x_a)
            {
                if (x_a == null || x_a.Length == 0) return 0;

                var x = x_a.ToArray();

                var cost = index_cost < 0 ? (double?)null : Math.Round(x[index_cost], round_digits);
                var gamma = index_gamma < 0 ? (double?)null : Math.Round(x[index_gamma], round_digits);
                var epsilon = index_epsilon < 0 ? (double?)null : Math.Round(x[index_epsilon], round_digits);
                var coef0 = index_coef0 < 0 ? (double?)null : Math.Round(x[index_coef0], round_digits);
                var degree = index_degree < 0 ? (double?)null : Math.Round(x[index_degree], round_digits);

                if ((cost != null && cost.Value < 0) /*|| (gamma != null && gamma.Value < 0)*/ /*|| (epsilon != null && epsilon.Value < 0) || (coef0 != null && coef0.Value < 0)*/ || (degree != null && degree.Value < 0)) return 0;

                var model_index = 0;
                var echo = false;
                var echo_err = true;


                var rate_full = libsvm_grid.cross_validate_model_libsvm(
                    svm_implementation,

                    training_file,
                    train_stdout_file,
                    train_stderr_file,
                    model_index,
                    svm_type,
                    kernel,
                    libsvm_caller.libsvm_cv_eval_methods.accuracy,
                    inner_cv_folds,

                    cv_probability_estimates,
                    cv_shrinking_heuristics,

                    cost,
                    gamma,
                    epsilon,
                    coef0,
                    degree,
                    weights,
                    point_max_time,
                    process_max_time,
                    echo,
                    echo_err
                    );

                var rate = rate_full.v_cross_validation;

                


                lock (best_rate_lock)
                {
                    best_rate_container.update_rate(cost, gamma, epsilon, coef0, degree, rate, rate_full);

                    iterations++;

                    //if (rate == 0) return 0;

                    //if (best_values == null || best_values.Length == 0)
                    //{
                    //    best_values = x;
                    //    best_rate = rate;
                    //}

                    //if (worst_values == null || best_values.Length == 0)
                    //{
                    //    worst_values = x;
                    //    worst_rate = rate;
                    //}

                    //if (lowest_values == null || lowest_values.Length == 0)
                    //{
                    //    lowest_values = x.Select(a => double.MaxValue).ToArray();
                    //    lowest_values_rates = x.Select(a => 0.0).ToArray();
                    //}

                    //if (search_cost && cost != null && cost.Value < lowest_values[index_cost])
                    //{
                    //    lowest_values[index_cost] = cost.Value;
                    //    if (rate > lowest_values_rates[index_cost]) lowest_values_rates[index_cost] = rate;
                    //}

                    //if (search_gamma && gamma != null && gamma.Value < lowest_values[index_gamma])
                    //{
                    //    lowest_values[index_gamma] = gamma.Value;
                    //    if (rate > lowest_values_rates[index_gamma]) lowest_values_rates[index_gamma] = rate;
                    //}

                    //if (search_epsilon && epsilon != null && epsilon.Value < lowest_values[index_epsilon])
                    //{
                    //    lowest_values[index_epsilon] = epsilon.Value;
                    //    if (rate > lowest_values_rates[index_epsilon]) lowest_values_rates[index_epsilon] = rate;
                    //}

                    //if (search_coef0 && coef0 != null && coef0.Value < lowest_values[index_coef0])
                    //{
                    //    lowest_values[index_coef0] = coef0.Value;
                    //    if (rate > lowest_values_rates[index_coef0]) lowest_values_rates[index_coef0] = rate;
                    //}

                    //if (search_degree && degree != null && degree.Value < lowest_values[index_degree])
                    //{
                    //    lowest_values[index_degree] = degree.Value;
                    //    if (rate > lowest_values_rates[index_degree]) lowest_values_rates[index_degree] = rate;
                    //}

                    //if (highest_values == null || highest_values.Length == 0)
                    //{
                    //    highest_values = x.Select(a => double.MinValue).ToArray();
                    //    highest_values_rates = x.Select(a => 0.0).ToArray();
                    //}

                    //if (search_cost && cost != null && cost.Value > highest_values[index_cost])
                    //{
                    //    highest_values[index_cost] = cost.Value;
                    //    if (rate > highest_values_rates[index_cost]) highest_values_rates[index_cost] = rate;
                    //}

                    //if (search_gamma && gamma != null && gamma.Value > highest_values[index_gamma])
                    //{
                    //    highest_values[index_gamma] = gamma.Value;
                    //    if (rate > highest_values_rates[index_gamma]) highest_values_rates[index_gamma] = rate;
                    //}

                    //if (search_epsilon && epsilon != null && epsilon.Value > highest_values[index_epsilon])
                    //{
                    //    highest_values[index_epsilon] = gamma.Value;
                    //    if (rate > highest_values_rates[index_epsilon]) highest_values_rates[index_epsilon] = rate;
                    //}

                    //if (search_coef0 && coef0 != null && coef0.Value > highest_values[index_coef0])
                    //{
                    //    highest_values[index_coef0] = coef0.Value;
                    //    if (rate > highest_values_rates[index_coef0]) highest_values_rates[index_coef0] = rate;
                    //}

                    //if (search_degree && degree != null && degree.Value > highest_values[index_degree])
                    //{
                    //    highest_values[index_degree] = degree.Value;
                    //    if (rate > highest_values_rates[index_degree]) highest_values_rates[index_degree] = rate;
                    //}


                    
                    //if (rate > best_rate ||
                    //    (Math.Abs(rate - best_rate) < 0.00001 &&
                    //     ((this.search_cost && cost.Value < best_values[index_cost]) ||
                    //     (this.search_gamma && gamma.Value < best_values[index_gamma]) ||
                    //     (this.search_epsilon &&  epsilon.Value < best_values[index_epsilon]) ||
                    //     (this.search_coef0 && coef0.Value < best_values[index_coef0]) ||
                    //     (this.search_degree && degree.Value < best_values[index_degree])
                    //    ))
                    //   )
                    //{
                    //    best_values = x;
                    //    best_rate = rate;
                    //    best_libsvm_cv_perf = rate_full;
                    //}

                    //if (rate < worst_rate)
                    //{
                    //    worst_rate = rate;
                    //    worst_values = x;
                    //}
                }

                return rate;
            }

        }

        public class nelder_mead_svm_params
        {
            
            public libsvm_caller.libsvm_svm_type svm_type;
            public libsvm_caller.libsvm_kernel_type kernel;
            public int iterations;

            public libsvm_grid.best_rate_container best_rate_container;
            //public double? best_cost;
            //public double? best_gamma;
            //public double? best_epsilon;
            //public double? best_coef0;
            //public double? best_degree;
            //public double? best_rate;

            //public double? worst_cost;
            //public double? worst_gamma;
            //public double? worst_epsilon;
            //public double? worst_coef0;
            //public double? worst_degree;
            //public double? worst_rate;

            //public double? lowest_cost;
            //public double? lowest_cost_rate;
            //public double? lowest_gamma;
            //public double? lowest_gamma_rate;
            //public double? lowest_epsilon;
            //public double? lowest_epsilon_rate;
            //public double? lowest_coef0;
            //public double? lowest_coef0_rate;
            //public double? lowest_degree;
            //public double? lowest_degree_rate;

            //public double? highest_cost;
            //public double? highest_cost_rate;
            //public double? highest_gamma;
            //public double? highest_gamma_rate;
            //public double? highest_epsilon;
            //public double? highest_epsilon_rate;
            //public double? highest_coef0;
            //public double? highest_coef0_rate;
            //public double? highest_degree;
            //public double? highest_degree_rate;
            public TimeSpan total_time;
        }

        public static nelder_mead_svm_params search(
            libsvm_caller.svm_implementation svm_implementation,
            string filename_training,
            string train_stdout_file,
            string train_stderr_file,

            libsvm_caller.libsvm_svm_type svm_type = libsvm_caller.libsvm_svm_type.c_svc,
            libsvm_caller.libsvm_kernel_type kernel = libsvm_caller.libsvm_kernel_type.rbf,

            int inner_cv_folds = 5,
            bool cv_probability_estimates = false,
            bool cv_shrinking_heuristics = true,
            List<(int class_id, double weight)> weights = null,
            TimeSpan? point_max_time = null,
            TimeSpan? process_max_time = null
            )
        {
            //var result_list = new List<nelder_mead_svm_params>();


            if (kernel == libsvm_caller.libsvm_kernel_type.precomputed)
            {
                throw new Exception();
            }



            var data = new nelder_mead_internal_params();

            data.train_stdout_file = train_stdout_file;
            data.train_stderr_file = train_stderr_file;

            data.svm_implementation = svm_implementation;
            data.svm_type = svm_type;
            data.kernel = kernel;
            data.inner_cv_folds = inner_cv_folds;
            data.cv_probability_estimates = cv_probability_estimates;
            data.cv_shrinking_heuristics = cv_shrinking_heuristics;
            data.training_file = filename_training;
            data.weights = weights;
            data.point_max_time = point_max_time;
            data.process_max_time = process_max_time;

            //data.best_values = null;
            //data.best_rate = 0;

            //data.worst_values = null;
            //data.worst_rate = 0;

            //data.lowest_values = null;
            //data.highest_values = null;


            //data.lowest_values_rates = null;
            //data.highest_values_rates = null;


            data.iterations = 0;

            data.search_cost = true;
            data.search_gamma = (data.kernel != libsvm_caller.libsvm_kernel_type.linear);
            data.search_epsilon = (data.svm_type == libsvm_caller.libsvm_svm_type.epsilon_svr || data.svm_type == libsvm_caller.libsvm_svm_type.nu_svr);
            data.search_coef0 = (data.kernel == libsvm_caller.libsvm_kernel_type.sigmoid || data.kernel == libsvm_caller.libsvm_kernel_type.polynomial);
            data.search_degree = (data.kernel == libsvm_caller.libsvm_kernel_type.polynomial);

            var x_index = 0;
            if (data.search_cost) data.index_cost = x_index++;
            if (data.search_gamma) data.index_gamma = x_index++;
            if (data.search_epsilon) data.index_epsilon = x_index++;
            if (data.search_coef0) data.index_coef0 = x_index++;
            if (data.search_degree) data.index_degree = x_index++;

            var step_up = true;

            //self.c_begin, self.c_end, self.c_step = -5,  15,  2
            //self.g_begin, self.g_end, self.g_step =  3, -15, -2

            var c_min = Math.Round(Math.Pow(2.0, -5), data.round_digits);
            var c_max = Math.Round(Math.Pow(2.0, 15), data.round_digits);
            var c_step = Math.Round(Math.Abs(c_max - c_min) / 50, data.round_digits);

            var g_min = Math.Round(Math.Pow(2.0, -15), data.round_digits);
            var g_max = Math.Round(Math.Pow(2.0, 3), data.round_digits);
            var g_step = Math.Round(Math.Abs(g_max - g_min) / 50, data.round_digits);

            var p_min = Math.Round(Math.Pow(2.0, -8), data.round_digits);
            var p_max = Math.Round(Math.Pow(2.0, -1), data.round_digits);
            var p_step = Math.Round(Math.Abs(p_max - p_min) / 50, data.round_digits);

            var d_min = Math.Round(0.0, data.round_digits);
            var d_max = Math.Round(30.0, data.round_digits);
            var d_step = Math.Round(Math.Abs(d_max - d_min) / 50, data.round_digits);

            var coef0_min = Math.Round(-10.0, data.round_digits);
            var coef0_max = Math.Round(10.0, data.round_digits);
            var coef0_step = Math.Round(Math.Abs(coef0_max - coef0_min) / 50, data.round_digits);

            if (step_up) c_step = Math.Round(Math.Pow(2.0, 2), data.round_digits);
            if (step_up) g_step = Math.Round(Math.Pow(2.0, -2), data.round_digits);
            if (step_up) p_step = Math.Round(Math.Pow(2.0, 1), data.round_digits);

            var lb_list = new List<double>();
            var ub_list = new List<double>();
            var step_list = new List<double>();

            var nov = 0;

            if (data.search_cost)
            {
                nov++;
                lb_list.Add(c_min);
                ub_list.Add(c_max);
                step_list.Add(c_step);
            }

            if (data.search_gamma)
            {
                nov++;
                lb_list.Add(g_min);
                ub_list.Add(g_max);
                step_list.Add(g_step);
            }

            if (data.search_epsilon)
            {
                nov++;
                lb_list.Add(p_min);
                ub_list.Add(p_max);
                step_list.Add(p_step);
            }

            if (data.search_coef0)
            {
                nov++;
                lb_list.Add(coef0_min);
                ub_list.Add(coef0_max);
                step_list.Add(coef0_step);
            }

            if (data.search_degree)
            {
                nov++;
                lb_list.Add(d_min);
                ub_list.Add(d_max);
                step_list.Add(d_step);
            }

            if (nov == 0)
            {
                throw new Exception();
            }

            var con = new GeneralConvergence(nov);
            con.MaximumTime = process_max_time ?? TimeSpan.Zero;
            //con.MaximumEvaluations = 200;
            //con.Evaluations = 200;
            con.StartTime = DateTime.Now;

            var solver = new Accord.Math.Optimization.NelderMead2(numberOfVariables: nov)
            {
                Function = data.nelder_mead_callback,
                LowerBounds = lb_list.ToArray(),
                UpperBounds = ub_list.ToArray(),
                StepSize = step_list.ToArray(),
                //MaximumValue = 0,
                Convergence = con
            };

            bool success = solver.Maximize();
            double[] solution = solver.Solution;
            double max = solver.Value;

            var result = new nelder_mead_svm_params();

            result.svm_type = data.svm_type;
            result.kernel = data.kernel;
            result.iterations = data.iterations;
            result.total_time = DateTime.Now.Subtract(solver.Convergence.StartTime);
            result.best_rate_container = data.best_rate_container;

            //result.best_cost = data.best_values == null || !data.search_cost ? (double?)null : Math.Round(data.best_values[data.index_cost], data.round_digits);
            //result.best_gamma = data.best_values == null || !data.search_gamma ? (double?)null : Math.Round(data.best_values[data.index_gamma], data.round_digits);
            //result.best_epsilon = data.best_values == null || !data.search_epsilon ? (double?)null : Math.Round(data.best_values[data.index_epsilon], data.round_digits);
            //result.best_coef0 = data.best_values == null || !data.search_coef0 ? (double?)null : Math.Round(data.best_values[data.index_coef0], data.round_digits);
            //result.best_degree = data.best_values == null || !data.search_degree ? (double?)null : Math.Round(data.best_values[data.index_degree], data.round_digits);


            //result.worst_cost = data.worst_values == null || !data.search_cost ? (double?)null : Math.Round(data.worst_values[data.index_cost], data.round_digits);
            //result.worst_gamma = data.worst_values == null || !data.search_gamma ? (double?)null : Math.Round(data.worst_values[data.index_gamma], data.round_digits);
            //result.worst_epsilon = data.worst_values == null || !data.search_epsilon ? (double?)null : Math.Round(data.worst_values[data.index_epsilon], data.round_digits);
            //result.worst_coef0 = data.worst_values == null || !data.search_coef0 ? (double?)null : Math.Round(data.worst_values[data.index_coef0], data.round_digits);
            //result.worst_degree = data.worst_values == null || !data.search_degree ? (double?)null : Math.Round(data.worst_values[data.index_degree], data.round_digits);



            //result.lowest_cost = data.lowest_values == null || !data.search_cost ? (double?)null : Math.Round(data.lowest_values[data.index_cost], data.round_digits);
            //result.lowest_gamma = data.lowest_values == null || !data.search_gamma ? (double?)null : Math.Round(data.lowest_values[data.index_gamma], data.round_digits);
            //result.lowest_epsilon = data.lowest_values == null || !data.search_epsilon ? (double?)null : Math.Round(data.lowest_values[data.index_epsilon], data.round_digits);
            //result.lowest_coef0 = data.lowest_values == null || !data.search_coef0 ? (double?)null : Math.Round(data.lowest_values[data.index_coef0], data.round_digits);
            //result.lowest_degree = data.lowest_values == null || !data.search_degree ? (double?)null : Math.Round(data.lowest_values[data.index_degree], data.round_digits);

            //result.highest_cost = data.highest_values == null || !data.search_cost ? (double?)null : Math.Round(data.highest_values[data.index_cost], data.round_digits);
            //result.highest_gamma = data.highest_values == null || !data.search_gamma ? (double?)null : Math.Round(data.highest_values[data.index_gamma], data.round_digits);
            //result.highest_epsilon = data.highest_values == null || !data.search_epsilon ? (double?)null : Math.Round(data.highest_values[data.index_epsilon], data.round_digits);
            //result.highest_coef0 = data.highest_values == null || !data.search_coef0 ? (double?)null : Math.Round(data.highest_values[data.index_coef0], data.round_digits);
            //result.highest_degree = data.highest_values == null || !data.search_degree ? (double?)null : Math.Round(data.highest_values[data.index_degree], data.round_digits);



            //result.lowest_cost_rate = data.lowest_values_rates == null || !data.search_cost ? (double?)null : Math.Round(data.lowest_values_rates[data.index_cost], data.round_digits);
            //result.lowest_gamma_rate = data.lowest_values_rates == null || !data.search_gamma ? (double?)null : Math.Round(data.lowest_values_rates[data.index_gamma], data.round_digits);
            //result.lowest_epsilon_rate = data.lowest_values_rates == null || !data.search_epsilon ? (double?)null : Math.Round(data.lowest_values_rates[data.index_epsilon], data.round_digits);
            //result.lowest_coef0_rate = data.lowest_values_rates == null || !data.search_coef0 ? (double?)null : Math.Round(data.lowest_values_rates[data.index_coef0], data.round_digits);
            //result.lowest_degree_rate = data.lowest_values_rates == null || !data.search_degree ? (double?)null : Math.Round(data.lowest_values_rates[data.index_degree], data.round_digits);

            //result.highest_cost_rate = data.highest_values_rates == null || !data.search_cost ? (double?)null : Math.Round(data.highest_values_rates[data.index_cost], data.round_digits);
            //result.highest_gamma_rate = data.highest_values_rates == null || !data.search_gamma ? (double?)null : Math.Round(data.highest_values_rates[data.index_gamma], data.round_digits);
            //result.highest_epsilon_rate = data.highest_values_rates == null || !data.search_epsilon ? (double?)null : Math.Round(data.highest_values_rates[data.index_epsilon], data.round_digits);
            //result.highest_coef0_rate = data.highest_values_rates == null || !data.search_coef0 ? (double?)null : Math.Round(data.highest_values_rates[data.index_coef0], data.round_digits);
            //result.highest_degree_rate = data.highest_values_rates == null || !data.search_degree ? (double?)null : Math.Round(data.highest_values_rates[data.index_degree], data.round_digits);

            //result.best_rate = data.best_rate;
            //result.worst_rate = data.worst_rate;




            if (program.write_console_log) program.WriteLine($@"SVM: {svm_type}, Kernel: {kernel}, iterations = {data.iterations}, time = {result.total_time.Days}d {result.total_time.Hours}h {result.total_time.Minutes}m {result.total_time.Seconds}s");
            //if (program.write_console_log) program.WriteLine($@"Best: " + string.Join(", ", new string[] {
            //                        $"{(result.best_cost != null ? $"cost = {result.best_cost}" : "")}",
            //                        $"{(result.best_gamma != null ? $"gamma = {result.best_gamma}" : "")}",
            //                        $"{(result.best_epsilon != null ? $"epsilon = {result.best_epsilon}" : "")}",
            //                        $"{(result.best_coef0 != null ? $"coef0 = {result.best_coef0}" : "")}",
            //                        $"{(result.best_degree != null ? $"degree = {result.best_degree}" : "")}",
            //                        $"rate = {result.best_rate}"
            //                        }.Where(a => a != null && a.Length > 0).ToList()));

            //if (program.write_console_log) program.WriteLine($@"Worst: " + string.Join(", ", new string[] {
            //                        $"{(result.worst_cost != null ? $"cost = {result.worst_cost}" : "")}",
            //                        $"{(result.worst_gamma != null ? $"gamma = {result.worst_gamma}" : "")}",
            //                        $"{(result.worst_epsilon != null ? $"epsilon = {result.worst_epsilon}" : "")}",
            //                        $"{(result.worst_coef0 != null ? $"coef0 = {result.worst_coef0}" : "")}",
            //                        $"{(result.worst_degree != null ? $"degree = {result.worst_degree}" : "")}",
            //                        $"rate = {result.worst_rate}"
            //                        }.Where(a => a != null && a.Length > 0).ToList()));

            //if (program.write_console_log) program.WriteLine($@"Lowest values for individual parameters: " + string.Join(", ", new string[] {
            //                        $"{(result.lowest_cost != null ? $"cost = {result.lowest_cost} (rate = {result.lowest_cost_rate})" : "")}",
            //                        $"{(result.lowest_gamma != null ? $"gamma = {result.lowest_gamma} (rate = {result.lowest_gamma_rate})" : "")}",
            //                        $"{(result.lowest_epsilon != null ? $"epsilon = {result.lowest_epsilon} (rate = {result.lowest_epsilon_rate})" : "")}",
            //                        $"{(result.lowest_coef0 != null ? $"coef0 = {result.lowest_coef0} (rate = {result.lowest_coef0_rate})" : "")}",
            //                        $"{(result.lowest_degree != null ? $"degree = {result.lowest_degree} (rate = {result.lowest_degree_rate})" : "")}"
            //                        }.Where(a => a != null && a.Length > 0).ToList()));

            //if (program.write_console_log) program.WriteLine($@"Highest values for individual parameters: " + string.Join(", ", new string[] {
            //                        $"{(result.highest_cost != null ? $"cost = {result.highest_cost} (rate = {result.highest_cost_rate})" : "")}",
            //                        $"{(result.highest_gamma != null ? $"gamma = {result.highest_gamma} (rate = {result.highest_gamma_rate})" : "")}",
            //                        $"{(result.highest_epsilon != null ? $"epsilon = {result.highest_epsilon} (rate = {result.highest_epsilon_rate})" : "")}",
            //                        $"{(result.highest_coef0 != null ? $"coef0 = {result.highest_coef0} (rate = {result.highest_coef0_rate})" : "")}",
            //                        $"{(result.highest_degree != null ? $"degree = {result.highest_degree} (rate = {result.highest_degree_rate})" : "")}"
            //                        }.Where(a => a != null && a.Length > 0).ToList()));

            //if (program.write_console_log) program.WriteLine($@"");
            


            //Console.ReadLine();

            return result;

        }


    }
}
