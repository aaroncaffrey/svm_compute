using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using svm_compute;

namespace svm_compute
{
    public static class libsvm_grid
    {
        //        private static List<double> range_f(double begin, double end, double step)
        //        {
        ////#if DEBUG
        //            program.WriteLine($@"{nameof(range_f)}(...);");
        ////#endif

        //            var seq = new List<double>();

        //            while (true)
        //            {
        //                if (step > 0 && begin > end) break;

        //                if (step < 0 && begin < end) break;

        //                seq.Add(begin);

        //                begin = begin + step;
        //            }

        //            return seq;
        //        }

        //        private static List<double> permute_sequence(List<double> seq)
        //        {
        ////#if DEBUG
        //            program.WriteLine($@"{nameof(permute_sequence)}(...);");
        ////#endif
        //            var n = seq.Count;

        //            if (n <= 1) return seq;

        //            var mid = (int)(n / 2);

        //            var seq_left = seq.Take(mid).ToList();
        //            var left = permute_sequence(seq_left);

        //            var seq_right = seq.Skip(mid + 1).ToList();
        //            var right = permute_sequence(seq_right);

        //            var seq_middle = seq.Skip(mid).Take(1).ToList();

        //            while (left.Count > 0 || right.Count > 0)
        //            {
        //                if (left.Count > 0)
        //                {
        //                    seq_middle.Add(left[0]);
        //                    left = left.Skip(1).ToList();
        //                }

        //                if (right.Count > 0)
        //                {
        //                    seq_middle.Add(right[0]);
        //                    right = right.Skip(1).ToList();
        //                }
        //            }

        //            return seq_middle;
        //        }

        //        private static List<List<(double c, double g)>> calculate_jobs(libsvm_caller.libsvm_kernel_type kernel, double c_begin, double c_end, double c_step, double g_begin, double g_end, double g_step)
        //        {
        ////#if DEBUG
        //            program.WriteLine($@"{nameof(calculate_jobs)}(...);");
        ////#endif

        //            var c_seq = permute_sequence(range_f(c_begin, c_end, c_step));

        //            var g_seq = (kernel == libsvm_caller.libsvm_kernel_type.linear) ? new List<double>() : permute_sequence(range_f(g_begin, g_end, g_step));

        //            var nr_c = (double)c_seq.Count;

        //            var nr_g = (double)g_seq.Count;

        //            var i = 0;
        //            var j = 0;

        //            var jobs = new List<List<(double c, double g)>>();

        //            while (i < nr_c || j < nr_g)
        //            {
        //                if (i / nr_c < j / nr_g)
        //                {
        //                    // increase C resolution
        //                    var line = Enumerable.Range(0, j).Select(k => (c_seq[i], g_seq[k])).ToList();

        //                    i = i + 1;
        //                    jobs.Add(line);
        //                }
        //                else
        //                {
        //                    // increase g resolution
        //                    var line = Enumerable.Range(0, i).Select(k => (c_seq[k], g_seq[j])).ToList();

        //                    j = j + 1;
        //                    jobs.Add(line);
        //                }
        //            }

        //            jobs = jobs.Where(a => a.Count > 0).ToList();
        //            return jobs;
        //        }

        public class best_rate_container
        {
            private readonly object _rate_lock = new object();
            public double? best_rate = null;
            public double? best_cost = null;
            public double? best_gamma = null;
            public double? best_epsilon = null;
            public double? best_coef0 = null;
            public double? best_degree = null;

            //public double? best_log2c = 0;

            //public double? best_log2g = 0;

            public void update_rate(double? cost, double? gamma, double? epsilon, double? coef0, double? degree, double? rate)
            {
                //#if DEBUG

                var param_list = new List<(string key, string value)>()
                {
                    (nameof(cost),cost?.ToString()),
                    (nameof(gamma),gamma?.ToString()),
                    (nameof(epsilon),epsilon?.ToString()),
                    (nameof(coef0),coef0?.ToString()),
                    (nameof(degree),degree?.ToString()),
                    (nameof(rate),rate?.ToString()),

                };

                program.WriteLine($@"{nameof(update_rate)}({string.Join(", ", param_list.Select(a => $"{a.key}=\"{a.value}\"").ToList())});");
                //#endif

                lock (_rate_lock)
                {
                    if ((best_rate == null) || (rate > best_rate) ||
                        ((rate == best_rate) && (
                         (cost != null && best_cost != null && cost < best_cost) ||
                         (gamma != null && best_gamma != null && gamma < best_gamma) ||
                         (epsilon != null && best_epsilon != null && epsilon < best_epsilon) ||
                         (coef0 != null && best_coef0 != null && coef0 < best_coef0) ||
                         (degree != null && best_degree != null && degree < best_degree)
                         )))
                    {
                        best_rate = rate;
                        best_cost = cost;
                        best_gamma = gamma;
                        best_epsilon = epsilon;
                        best_coef0 = coef0;
                        best_degree = degree;
                    }
                }
            }
        }


        public static double? cross_validate_model_internal(
            bool use_modded_libsvm,
            string full_training_file,
            bool output_threshold_adjustment_performance,
            int model_index,
            List<int> metric_class_list = null,
            performance_measure.confusion_matrix.cross_validation_metrics cross_validation_metrics = performance_measure.confusion_matrix.cross_validation_metrics.ACC,
            libsvm_caller.libsvm_svm_type svm_type = libsvm_caller.libsvm_svm_type.c_svc,
            libsvm_caller.libsvm_kernel_type kernel = libsvm_caller.libsvm_kernel_type.rbf,
            int inner_cv_folds = 5,
            bool probability_estimates = false,
            bool shrinking_heuristics = true,
            double? cost = null,
            double? gamma = null,
            double? epsilon = null,
            double? coef0 = null,
            double? degree = null,
            List<(int class_id, double weight)> weights = null,
            TimeSpan? point_max_time = null,
            TimeSpan? process_max_time = null,
            bool echo = false,
            bool echo_err = true
            )
        {
            //#if DEBUG

            var param_list = new List<(string key, string value)>()
            {
                (nameof(full_training_file               ),          full_training_file               ?.ToString()),
                (nameof(output_threshold_adjustment_performance     ),          output_threshold_adjustment_performance     .ToString()),
                (nameof(model_index                      ),          model_index                      .ToString()),
                (nameof(metric_class_list                ),          metric_class_list                ?.ToString()),
                (nameof(cross_validation_metrics         ),          cross_validation_metrics         .ToString()),
                (nameof(svm_type                         ),          svm_type                         .ToString()),
                (nameof(kernel                           ),          kernel                           .ToString()),
                (nameof(inner_cv_folds                   ),          inner_cv_folds                   .ToString()),
                (nameof(probability_estimates            ),          probability_estimates            .ToString()),
                (nameof(shrinking_heuristics             ),          shrinking_heuristics             .ToString()),
                (nameof(cost                             ),          cost                             ?.ToString()),
                (nameof(gamma                            ),          gamma                            ?.ToString()),
                (nameof(epsilon                          ),          epsilon                          ?.ToString()),
                (nameof(coef0                            ),          coef0                            ?.ToString()),
                (nameof(degree                           ),          degree                           ?.ToString()),
                (nameof(weights                          ),          weights                          ?.ToString()),
                (nameof(point_max_time                   ),          point_max_time                   ?.ToString()),
                (nameof(process_max_time                 ),          process_max_time                 ?.ToString()),
                (nameof(echo                             ),          echo                             .ToString()),
                (nameof(echo_err                         ),          echo_err                         .ToString()),
            };

            program.WriteLine($@"{nameof(cross_validate_model_internal)}({string.Join(", ", param_list.Select(a => $"{a.key}=\"{a.value}\"").ToList())});");
            //#endif


            var max_tasks_internal_cv = (int?)null;

            var perf = libsvm_caller.cross_validate_model_internal_logic(use_modded_libsvm, full_training_file, output_threshold_adjustment_performance, model_index, cost, gamma, epsilon, coef0, degree, weights, svm_type, kernel, inner_cv_folds, probability_estimates, shrinking_heuristics, point_max_time, process_max_time, max_tasks_internal_cv, echo, echo_err);

            if (metric_class_list != null && metric_class_list.Count > 0)
            {
                perf = perf.Where(a => metric_class_list.Contains(a.class_id.Value)).ToList();
            }

            if (perf == null || perf.Count == 0)
            {
                return null;//(log2c, log2g, c, g, null);
            }

            var metric_values = new List<double>();

            if (cross_validation_metrics == 0) throw new Exception();

            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.TP)) { metric_values.Add(perf.Select(a => a.TP).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.FP)) { metric_values.Add(perf.Select(a => a.FP).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.TN)) { metric_values.Add(perf.Select(a => a.TN).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.FN)) { metric_values.Add(perf.Select(a => a.FN).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.TPR)) { metric_values.Add(perf.Select(a => a.TPR).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.TNR)) { metric_values.Add(perf.Select(a => a.TNR).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.PPV)) { metric_values.Add(perf.Select(a => a.PPV).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.Precision)) { metric_values.Add(perf.Select(a => a.Precision).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.Prevalence)) { metric_values.Add(perf.Select(a => a.Prevalence).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.MCR)) { metric_values.Add(perf.Select(a => a.MCR).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.ER)) { metric_values.Add(perf.Select(a => a.ER).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.NER)) { metric_values.Add(perf.Select(a => a.NER).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.CNER)) { metric_values.Add(perf.Select(a => a.CNER).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.Kappa)) { metric_values.Add(perf.Select(a => a.Kappa).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.Overlap)) { metric_values.Add(perf.Select(a => a.Overlap).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.RND_ACC)) { metric_values.Add(perf.Select(a => a.RND_ACC).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.Support)) { metric_values.Add(perf.Select(a => a.Support).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.BaseRate)) { metric_values.Add(perf.Select(a => a.BaseRate).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.Youden)) { metric_values.Add(perf.Select(a => a.Youden).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.NPV)) { metric_values.Add(perf.Select(a => a.NPV).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.FNR)) { metric_values.Add(perf.Select(a => a.FNR).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.FPR)) { metric_values.Add(perf.Select(a => a.FPR).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.FDR)) { metric_values.Add(perf.Select(a => a.FDR).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.FOR)) { metric_values.Add(perf.Select(a => a.FOR).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.ACC)) { metric_values.Add(perf.Select(a => a.ACC).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.GM)) { metric_values.Add(perf.Select(a => a.GM).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.F1S)) { metric_values.Add(perf.Select(a => a.F1S).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.G1S)) { metric_values.Add(perf.Select(a => a.G1S).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.MCC)) { metric_values.Add(perf.Select(a => a.MCC).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.BM_)) { metric_values.Add(perf.Select(a => a.BM_).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.MK_)) { metric_values.Add(perf.Select(a => a.MK_).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.BAC)) { metric_values.Add(perf.Select(a => a.BAC).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.ROC_AUC_Approx_All)) { metric_values.Add(perf.Select(a => a.ROC_AUC_Approx_All).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.ROC_AUC_Approx_11p)) { metric_values.Add(perf.Select(a => a.ROC_AUC_Approx_11p).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.ROC_AUC_All)) { metric_values.Add(perf.Select(a => a.ROC_AUC_All).Average()); }
            //if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.ROC_AUC2_11p)) { metric_values.Add(perf.Select(a => a.ROC_AUC_11p).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.PR_AUC_Approx_All)) { metric_values.Add(perf.Select(a => a.PR_AUC_Approx_All).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.PR_AUC_Approx_11p)) { metric_values.Add(perf.Select(a => a.PR_AUC_Approx_11p).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.PRI_AUC_Approx_All)) { metric_values.Add(perf.Select(a => a.PRI_AUC_Approx_All).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.PRI_AUC_Approx_11p)) { metric_values.Add(perf.Select(a => a.PRI_AUC_Approx_11p).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.AP_All)) { metric_values.Add(perf.Select(a => a.AP_All).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.AP_11p)) { metric_values.Add(perf.Select(a => a.AP_11p).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.API_All)) { metric_values.Add(perf.Select(a => a.API_All).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.API_11p)) { metric_values.Add(perf.Select(a => a.API_11p).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.Brier_All)) { metric_values.Add(perf.Select(a => a.Brier_All).Average()); }
            //if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.Brier_11p)) { metric_values.Add(perf.Select(a => a.Brier_11p).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.LRP)) { metric_values.Add(perf.Select(a => a.LRP).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.LRN)) { metric_values.Add(perf.Select(a => a.LRN).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.F1B_00)) { metric_values.Add(perf.Select(a => a.F1B_00).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.F1B_01)) { metric_values.Add(perf.Select(a => a.F1B_01).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.F1B_02)) { metric_values.Add(perf.Select(a => a.F1B_02).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.F1B_03)) { metric_values.Add(perf.Select(a => a.F1B_03).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.F1B_04)) { metric_values.Add(perf.Select(a => a.F1B_04).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.F1B_05)) { metric_values.Add(perf.Select(a => a.F1B_05).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.F1B_06)) { metric_values.Add(perf.Select(a => a.F1B_06).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.F1B_07)) { metric_values.Add(perf.Select(a => a.F1B_07).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.F1B_08)) { metric_values.Add(perf.Select(a => a.F1B_08).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.F1B_09)) { metric_values.Add(perf.Select(a => a.F1B_09).Average()); }
            if (cross_validation_metrics.HasFlag(performance_measure.confusion_matrix.cross_validation_metrics.F1B_10)) { metric_values.Add(perf.Select(a => a.F1B_10).Average()); }

            var average_of_metrics = metric_values.Average();

            return average_of_metrics;//(log2c, log2g, c, g, average_of_metrics);
        }

        public static cross_validation.libsvm_cv_perf cross_validate_model_libsvm(
            string training_file,
            int model_index = 0,
            libsvm_caller.libsvm_svm_type svm_type = libsvm_caller.libsvm_svm_type.c_svc,
            libsvm_caller.libsvm_kernel_type kernel = libsvm_caller.libsvm_kernel_type.rbf,
            libsvm_caller.libsvm_cv_eval_methods eval_method = libsvm_caller.libsvm_cv_eval_methods.accuracy,
            int inner_cv_folds = 5,
            bool probability_estimates = false,
            bool shrinking_heuristics = true,
            double? cost = null,
            double? gamma = null,
            double? epsilon = null,
            double? coef0 = null,
            double? degree = null,
            List<(int class_id, double weight)> weights = null,
            TimeSpan? point_max_time = null,
            TimeSpan? process_max_time = null,
            bool echo = false,
            bool echo_err = true
            )
        {
            //#if DEBUG

            var param_list = new List<(string key, string value)>()
            {
                (nameof(model_index),model_index.ToString()),
                (nameof(training_file),training_file.ToString()),
                (nameof(svm_type),svm_type.ToString()),
                (nameof(kernel),kernel.ToString()),
                (nameof(inner_cv_folds),inner_cv_folds.ToString()),
                (nameof(probability_estimates),probability_estimates.ToString()),
                (nameof(shrinking_heuristics),shrinking_heuristics.ToString()),
            };

            program.WriteLine($@"{nameof(cross_validate_model_libsvm)}({string.Join(", ", param_list.Select(a => $"{a.key}=\"{a.value}\"").ToList())});");
            //#endif

            if (cost == null && gamma == null && epsilon == null && coef0 == null && degree == null) throw new Exception();

            if (inner_cv_folds <= 1) throw new Exception();
            //var probability_estimates = false;

            var model_filename = $@"{training_file}_{(model_index + 1)}.model";
            

            var train_result = libsvm_caller.train(training_file, model_filename, cost, gamma, epsilon, coef0, degree, weights, svm_type, kernel, eval_method, inner_cv_folds, probability_estimates, shrinking_heuristics, point_max_time, echo, echo_err);

            if (string.IsNullOrWhiteSpace(train_result))
            {
                return new cross_validation.libsvm_cv_perf();
            }

            var train_result_lines = train_result.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            var cv_perf = new cross_validation.libsvm_cv_perf(train_result_lines);

            return cv_perf;
        }

        //public class grid_parameters
        //{
        //    public double c_exp_begin = -5;
        //    public double c_exp_end = 15;
        //    public double c_exp_step = 2;

        //    public double g_exp_begin = 3;
        //    public double g_exp_end = -15;
        //    public double g_exp_step = -2;
        //}

        public static bool should_use_modded_libsvm(List<int> cross_validation_metric_class_list = null, performance_measure.confusion_matrix.cross_validation_metrics cross_validation_metrics = performance_measure.confusion_matrix.cross_validation_metrics.ACC)
        {
            var use_modded_libsvm = false;

            var libsvm_eval_option = libsvm_caller.libsvm_cv_eval_methods.accuracy;

            if (cross_validation_metrics != performance_measure.confusion_matrix.cross_validation_metrics.None && cross_validation_metrics != performance_measure.confusion_matrix.cross_validation_metrics.ACC)
            {
                if (cross_validation_metric_class_list == null || cross_validation_metric_class_list.Count == 0 || (cross_validation_metric_class_list.Count == 1 && cross_validation_metric_class_list.First() == 1))
                {

                    switch (cross_validation_metrics)
                    {
                        case performance_measure.confusion_matrix.cross_validation_metrics.ACC:
                            libsvm_eval_option = libsvm_caller.libsvm_cv_eval_methods.accuracy;
                            break;
                        case performance_measure.confusion_matrix.cross_validation_metrics.AP_All:
                            libsvm_eval_option = libsvm_caller.libsvm_cv_eval_methods.ap;
                            break;
                        case performance_measure.confusion_matrix.cross_validation_metrics.ROC_AUC_All:
                            libsvm_eval_option = libsvm_caller.libsvm_cv_eval_methods.auc;
                            break;
                        case performance_measure.confusion_matrix.cross_validation_metrics.ROC_AUC_Approx_All:
                            libsvm_eval_option = libsvm_caller.libsvm_cv_eval_methods.auc;
                            break;
                        case performance_measure.confusion_matrix.cross_validation_metrics.BAC:
                            libsvm_eval_option = libsvm_caller.libsvm_cv_eval_methods.bac;
                            break;
                        case performance_measure.confusion_matrix.cross_validation_metrics.F1S:
                            libsvm_eval_option = libsvm_caller.libsvm_cv_eval_methods.fscore;
                            break;
                        case performance_measure.confusion_matrix.cross_validation_metrics.Precision:
                            libsvm_eval_option = libsvm_caller.libsvm_cv_eval_methods.precision;
                            break;
                        case performance_measure.confusion_matrix.cross_validation_metrics.PPV:
                            libsvm_eval_option = libsvm_caller.libsvm_cv_eval_methods.precision;
                            break;
                        case performance_measure.confusion_matrix.cross_validation_metrics.TPR:
                            libsvm_eval_option = libsvm_caller.libsvm_cv_eval_methods.recall;
                            break;
                        default:
                            break;
                    }

                    if (libsvm_eval_option != libsvm_caller.libsvm_cv_eval_methods.accuracy)
                    {
                        use_modded_libsvm = true;
                    }
                }
            }

            return use_modded_libsvm;
        }

        public static libsvm_caller.libsvm_cv_eval_methods get_modded_libsvm_eval_method(List<int> cross_validation_metric_class_list = null, performance_measure.confusion_matrix.cross_validation_metrics cross_validation_metrics = performance_measure.confusion_matrix.cross_validation_metrics.ACC)
        {
            var libsvm_eval_option = libsvm_caller.libsvm_cv_eval_methods.accuracy;

            if (cross_validation_metrics != performance_measure.confusion_matrix.cross_validation_metrics.None && cross_validation_metrics != performance_measure.confusion_matrix.cross_validation_metrics.ACC)
            {
                if (cross_validation_metric_class_list == null || cross_validation_metric_class_list.Count == 0 || (cross_validation_metric_class_list.Count == 1 && cross_validation_metric_class_list.First() == 1))
                {

                    switch (cross_validation_metrics)
                    {
                        case performance_measure.confusion_matrix.cross_validation_metrics.ACC:
                            libsvm_eval_option = libsvm_caller.libsvm_cv_eval_methods.accuracy;
                            break;
                        case performance_measure.confusion_matrix.cross_validation_metrics.AP_All:
                            libsvm_eval_option = libsvm_caller.libsvm_cv_eval_methods.ap;
                            break;
                        case performance_measure.confusion_matrix.cross_validation_metrics.ROC_AUC_All:
                            libsvm_eval_option = libsvm_caller.libsvm_cv_eval_methods.auc;
                            break;
                        case performance_measure.confusion_matrix.cross_validation_metrics.ROC_AUC_Approx_All:
                            libsvm_eval_option = libsvm_caller.libsvm_cv_eval_methods.auc;
                            break;
                        case performance_measure.confusion_matrix.cross_validation_metrics.BAC:
                            libsvm_eval_option = libsvm_caller.libsvm_cv_eval_methods.bac;
                            break;
                        case performance_measure.confusion_matrix.cross_validation_metrics.F1S:
                            libsvm_eval_option = libsvm_caller.libsvm_cv_eval_methods.fscore;
                            break;
                        case performance_measure.confusion_matrix.cross_validation_metrics.Precision:
                            libsvm_eval_option = libsvm_caller.libsvm_cv_eval_methods.precision;
                            break;
                        case performance_measure.confusion_matrix.cross_validation_metrics.PPV:
                            libsvm_eval_option = libsvm_caller.libsvm_cv_eval_methods.precision;
                            break;
                        case performance_measure.confusion_matrix.cross_validation_metrics.TPR:
                            libsvm_eval_option = libsvm_caller.libsvm_cv_eval_methods.recall;
                            break;
                        default:
                            break;
                    }
                }
            }

            return libsvm_eval_option;
        }

        public static best_rate_container grid_parameter_search(
                string training_file,
                List<int> cross_validation_metric_class_list = null,
                performance_measure.confusion_matrix.cross_validation_metrics cross_validation_metrics = performance_measure.confusion_matrix.cross_validation_metrics.ACC,
                List<(int class_id, double weight)> weights = null,

                libsvm_caller.libsvm_svm_type svm_type = libsvm_caller.libsvm_svm_type.c_svc,
                libsvm_caller.libsvm_kernel_type kernel = libsvm_caller.libsvm_kernel_type.rbf,
                int inner_cv_folds = 5,
                bool probability_estimates = false,
                bool shrinking_heuristics = true,
                TimeSpan? point_max_time = null,
                TimeSpan? process_max_time = null,
                int? max_tasks = null,
                bool echo = false,
                bool echo_err = true,

                double? cost_exp_begin = -5,
                double? cost_exp_end = 15,
                double? cost_exp_step = 2,

                double? gamma_exp_begin = 3,
                double? gamma_exp_end = -15,
                double? gamma_exp_step = -2,

                double? epsilon_exp_begin = null,//8,
                double? epsilon_exp_end = null,//-1,
                double? epsilon_exp_step = null,//1,

                double? coef0_exp_begin = null,
                double? coef0_exp_end = null,
                double? coef0_exp_step = null,

                double? degree_exp_begin = null,
                double? degree_exp_end = null,
                double? degree_exp_step = null,

                int recursions = 0

            )
        {
            //#if DEBUG

            var param_list = new List<(string key, string value)>()
            {

                (nameof(training_file),training_file.ToString()),
                (nameof(cross_validation_metric_class_list),cross_validation_metric_class_list?.ToString() ?? null),
                (nameof(cross_validation_metrics),cross_validation_metrics.ToString()),
                (nameof(svm_type),svm_type.ToString()),
                (nameof(kernel),kernel.ToString()),
                (nameof(inner_cv_folds),inner_cv_folds.ToString()),
                (nameof(probability_estimates),probability_estimates.ToString()),
                (nameof(shrinking_heuristics),shrinking_heuristics.ToString()),
                (nameof(cost_exp_begin),cost_exp_begin.ToString()),
                (nameof(cost_exp_end),cost_exp_end.ToString()),
                (nameof(cost_exp_step),cost_exp_step.ToString()),
                (nameof(gamma_exp_begin),gamma_exp_begin.ToString()),
                (nameof(gamma_exp_end),gamma_exp_end.ToString()),
                (nameof(gamma_exp_step),gamma_exp_step.ToString()),
                (nameof(epsilon_exp_begin),epsilon_exp_begin.ToString()),
                (nameof(epsilon_exp_end),epsilon_exp_end.ToString()),
                (nameof(epsilon_exp_step),epsilon_exp_step.ToString()),
                (nameof(recursions),recursions.ToString()),
            };

            program.WriteLine($@"{nameof(grid_parameter_search)}({string.Join(", ", param_list.Select(a => $"{a.key}=\"{a.value}\"").ToList())});");
            //#endif

            if (kernel == libsvm_caller.libsvm_kernel_type.precomputed)
            {
                throw new Exception();
            }

            var shuffle_grid_points = true;

            // 0 tasks = 0m 19s
            // 1 task = 3m 36s
            // 2 tasks = 2m 51s
            // 110 tasks = 0m 27s

            

            var output_threshold_adjustment_performance = false;

            var use_internal_cv = (cross_validation_metrics != performance_measure.confusion_matrix.cross_validation_metrics.None && cross_validation_metrics != performance_measure.confusion_matrix.cross_validation_metrics.ACC) ||
                                  (cross_validation_metric_class_list != null && cross_validation_metric_class_list.Count > 0);


            var use_modded_libsvm = should_use_modded_libsvm(cross_validation_metric_class_list, cross_validation_metrics);

            if (use_modded_libsvm)
            {
                use_internal_cv = false;
            }

            //Console.WriteLine($"use_internal_cv = {use_internal_cv}");

            if (cost_exp_step == 0)
            {
                if (cost_exp_end - cost_exp_begin != 0) cost_exp_step = (cost_exp_end - cost_exp_begin) / 10;
                else cost_exp_step = null;
            }

            if (gamma_exp_step == 0)
            {

                if (gamma_exp_end - gamma_exp_begin != 0) gamma_exp_step = (gamma_exp_end - gamma_exp_begin) / 10;
                else gamma_exp_step = null;
            }

            if (epsilon_exp_step == 0)
            {

                if (epsilon_exp_end - epsilon_exp_begin != 0) epsilon_exp_step = (epsilon_exp_end - epsilon_exp_begin) / 10;
                else epsilon_exp_step = null;
            }


            if (coef0_exp_step == 0)
            {
                if (coef0_exp_end - coef0_exp_begin != 0) coef0_exp_step = (coef0_exp_end - coef0_exp_begin) / 10;
                else coef0_exp_step = null;
            }

            if (degree_exp_step == 0)
            {
                if (degree_exp_end - degree_exp_begin != 0) degree_exp_step = (degree_exp_end - degree_exp_begin) / 10;
                else degree_exp_step = null;
            }

            var cost_exp_list = new List<double?>();
            var gamma_exp_list = new List<double?>();
            var epsilon_exp_list = new List<double?>();
            var coef0_exp_list = new List<double?>();
            var degree_exp_list = new List<double?>();

            var search_grid_points = new List<(double? cost, double? gamma, double? epsilon, double? coef0, double? degree)>();

            // always search for cost, unless not specified
            if (cost_exp_begin != null && cost_exp_end != null && cost_exp_step != null)
            {
                for (var c_exp = cost_exp_begin; (c_exp <= cost_exp_end && c_exp >= cost_exp_begin) || (c_exp >= cost_exp_end && c_exp <= cost_exp_begin); c_exp += cost_exp_step)
                {
                    var cost = Math.Pow(2.0, c_exp.Value);
                    cost_exp_list.Add(cost); //(c_exp, c));
                }
            }

            // search gamma only if kernel isn't linear
            if (kernel != libsvm_caller.libsvm_kernel_type.linear && gamma_exp_begin != null && gamma_exp_end != null && gamma_exp_step != null)
            {
                for (var g_exp = gamma_exp_begin; (g_exp <= gamma_exp_end && g_exp >= gamma_exp_begin) || (g_exp >= gamma_exp_end && g_exp <= gamma_exp_begin); g_exp += gamma_exp_step)
                {
                    var gamma = Math.Pow(2.0, g_exp.Value);
                    gamma_exp_list.Add(gamma);
                }
            }


            // search epsilon only if svm type is svr
            if ((svm_type == libsvm_caller.libsvm_svm_type.epsilon_svr || svm_type == libsvm_caller.libsvm_svm_type.nu_svr) && epsilon_exp_begin != null && epsilon_exp_end != null && epsilon_exp_step != null)
            {
                for (var p_exp = epsilon_exp_begin; (p_exp <= epsilon_exp_end && p_exp >= epsilon_exp_begin) || (p_exp >= epsilon_exp_end && p_exp <= epsilon_exp_begin); p_exp += epsilon_exp_step)
                {
                    var epsilon = Math.Pow(2.0, p_exp.Value);
                    epsilon_exp_list.Add(epsilon);
                }
            }

            // search for coef0 only for sigmoid and polynomial
            if ((kernel == libsvm_caller.libsvm_kernel_type.sigmoid || kernel == libsvm_caller.libsvm_kernel_type.polynomial) && coef0_exp_begin != null && coef0_exp_end != null && coef0_exp_step != null)
            {
                for (var r_exp = coef0_exp_begin; (r_exp <= coef0_exp_end && r_exp >= coef0_exp_begin) || (r_exp >= coef0_exp_end && r_exp <= coef0_exp_begin); r_exp += coef0_exp_step)
                {
                    var coef0 = Math.Pow(2.0, r_exp.Value);
                    coef0_exp_list.Add(coef0);
                }
            }

            // search for degree only for polynomial
            if (kernel == libsvm_caller.libsvm_kernel_type.polynomial && degree_exp_begin != null && degree_exp_end != null && degree_exp_step != null)
            {
                for (var d_exp = degree_exp_begin; (d_exp <= degree_exp_end && d_exp >= degree_exp_begin) || (d_exp >= degree_exp_end && d_exp <= degree_exp_begin); d_exp += degree_exp_step)
                {
                    var degree = Math.Pow(2.0, d_exp.Value);
                    degree_exp_list.Add(degree);
                }
            }



            if (cost_exp_list == null || cost_exp_list.Count == 0) cost_exp_list = new List<double?>() { null };
            if (gamma_exp_list == null || gamma_exp_list.Count == 0) gamma_exp_list = new List<double?>() { null };
            if (epsilon_exp_list == null || epsilon_exp_list.Count == 0) epsilon_exp_list = new List<double?>() { null };
            if (coef0_exp_list == null || coef0_exp_list.Count == 0) coef0_exp_list = new List<double?>() { null };
            if (degree_exp_list == null || degree_exp_list.Count == 0) degree_exp_list = new List<double?>() { null };

            for (var c_index = 0; c_index < cost_exp_list.Count; c_index++)
            {
                for (var g_index = 0; g_index < gamma_exp_list.Count; g_index++)
                {
                    for (var p_index = 0; p_index < epsilon_exp_list.Count; p_index++)
                    {
                        for (var r_index = 0; r_index < coef0_exp_list.Count; r_index++)
                        {
                            for (var d_index = 0; d_index < degree_exp_list.Count; d_index++)
                            {
                                search_grid_points.Add((cost_exp_list[c_index], gamma_exp_list[g_index], epsilon_exp_list[p_index], coef0_exp_list[r_index], degree_exp_list[d_index]));
                            }
                        }
                    }
                }
            }

            search_grid_points = search_grid_points.Distinct().OrderByDescending(a => a.cost).ThenByDescending(a => a.gamma).ThenByDescending(a => a.epsilon).ThenByDescending(a => a.coef0).ThenByDescending(a => a.degree).ToList();


            if (shuffle_grid_points)
            {
                var shuffle_random = new Random(1);
                search_grid_points.shuffle(shuffle_random);
            }

            var tasks = new List<Task>();

            if (max_tasks == null)
            {
                max_tasks = search_grid_points.Count;
            }

            if (max_tasks < 0)
            {
                max_tasks = Environment.ProcessorCount * Math.Abs(max_tasks.Value) * 10;
            }

            if (max_tasks > search_grid_points.Count)
            {
                max_tasks = search_grid_points.Count;
            }

            var best_rate = new best_rate_container();

            for (var index = 0; index < search_grid_points.Count; index++)
            {
                var point = search_grid_points[index];
                var br = best_rate;
                var model_index = index;

                if (max_tasks == 0)
                {
                    grid_search_task_logic(use_modded_libsvm, training_file, output_threshold_adjustment_performance, model_index, br, use_internal_cv, cross_validation_metric_class_list, cross_validation_metrics, svm_type, kernel, inner_cv_folds, probability_estimates, shrinking_heuristics, point.cost, point.gamma, point.epsilon, point.coef0, point.degree, weights, point_max_time, process_max_time, echo, echo_err);
                }
                else
                {
                    var task = Task.Run(() => grid_search_task_logic(use_modded_libsvm, training_file, output_threshold_adjustment_performance, model_index, br, use_internal_cv, cross_validation_metric_class_list, cross_validation_metrics, svm_type, kernel, inner_cv_folds, probability_estimates, shrinking_heuristics, point.cost, point.gamma, point.epsilon, point.coef0, point.degree, weights, point_max_time, process_max_time, echo, echo_err));
                    tasks.Add(task);

                    if (index != search_grid_points.Count - 1)
                    {
                        var incomplete_tasks = tasks.Where(a => !a.IsCompleted).ToList();

                        while (max_tasks > 0 && incomplete_tasks.Count >= max_tasks)
                        {
                            program.WriteLine($@"grid_parameter_search(): Task.WaitAny(tasks.ToArray<Task>());", true, ConsoleColor.Cyan);

                            try
                            {
                                Task.WaitAny(incomplete_tasks.ToArray<Task>());
                            }
                            catch (Exception)
                            {

                            }

                            incomplete_tasks = tasks.Where(a => !a.IsCompleted).ToList();
                        }
                    }
                }
            }

            if (tasks != null && tasks.Count > 0)
            {
                program.WriteLine($@"grid_parameter_search(): Task.WaitAll(tasks.ToArray<Task>());", true, ConsoleColor.Cyan);
                Task.WaitAll(tasks.ToArray<Task>());
            }


            if (recursions > 0)
            {
                var cost_resolution = 1.0;
                var gamma_resolution = 1.0;
                var epsilon_resolution = 1.0;
                var coef0_resolution = 1.0;
                var degree_resolution = 1.0;

                if (recursions == 1)
                {
                    cost_resolution = cost_resolution / 10;
                    gamma_resolution = kernel == libsvm_caller.libsvm_kernel_type.linear ? 0.0 : gamma_resolution / 10;
                    //epsilon_resolution = epsilon_resolution / 10;
                    //coef0_resolution = coef0_resolution / 10;
                    //degree_resolution = degree_resolution / 10;
                }

                // todo: these values need checking, perhaps changing to their pre-power values - they are for a fine-grain search to find the local optimum

                var recursion_cost_begin = best_rate.best_cost == null ? (double?)null : best_rate.best_cost.Value - cost_resolution;
                var recursion_cost_end = best_rate.best_cost == null ? (double?)null : best_rate.best_cost.Value + cost_resolution;
                var recursion_cost_inc = best_rate.best_cost == null ? (double?)null : cost_resolution / 10;

                var recursion_gamma_begin = kernel == libsvm_caller.libsvm_kernel_type.linear || best_rate.best_gamma == null ? (double?)null : best_rate.best_gamma.Value - gamma_resolution;
                var recursion_gamma_end = kernel == libsvm_caller.libsvm_kernel_type.linear || best_rate.best_gamma == null ? (double?)null : best_rate.best_gamma.Value + gamma_resolution;
                var recursion_gamma_inc = kernel == libsvm_caller.libsvm_kernel_type.linear || best_rate.best_gamma == null ? (double?)null : gamma_resolution / 10;

                var recursion_epsilon_begin = (svm_type != libsvm_caller.libsvm_svm_type.epsilon_svr && svm_type != libsvm_caller.libsvm_svm_type.nu_svr) || best_rate.best_epsilon == null ? (double?)null : best_rate.best_epsilon.Value - epsilon_resolution;
                var recursion_epsilon_end = (svm_type != libsvm_caller.libsvm_svm_type.epsilon_svr && svm_type != libsvm_caller.libsvm_svm_type.nu_svr) || best_rate.best_epsilon == null ? (double?)null : best_rate.best_epsilon.Value + epsilon_resolution;
                var recursion_epsilon_inc = (svm_type != libsvm_caller.libsvm_svm_type.epsilon_svr && svm_type != libsvm_caller.libsvm_svm_type.nu_svr) || best_rate.best_epsilon == null ? (double?)null : epsilon_resolution / 10;

                var recursion_coef0_begin = (kernel != libsvm_caller.libsvm_kernel_type.sigmoid && kernel != libsvm_caller.libsvm_kernel_type.polynomial) || best_rate.best_coef0 == null ? (double?)null : best_rate.best_coef0.Value - coef0_resolution;
                var recursion_coef0_end = (kernel != libsvm_caller.libsvm_kernel_type.sigmoid && kernel != libsvm_caller.libsvm_kernel_type.polynomial) || best_rate.best_coef0 == null ? (double?)null : best_rate.best_coef0.Value + coef0_resolution;
                var recursion_coef0_inc = (kernel != libsvm_caller.libsvm_kernel_type.sigmoid && kernel != libsvm_caller.libsvm_kernel_type.polynomial) || best_rate.best_coef0 == null ? (double?)null : coef0_resolution / 10;

                var recursion_degree_begin = kernel != libsvm_caller.libsvm_kernel_type.polynomial || best_rate.best_degree == null ? (double?)null : best_rate.best_degree.Value - degree_resolution;
                var recursion_degree_end = kernel != libsvm_caller.libsvm_kernel_type.polynomial || best_rate.best_degree == null ? (double?)null : best_rate.best_degree.Value + degree_resolution;
                var recursion_degree_inc = kernel != libsvm_caller.libsvm_kernel_type.polynomial || best_rate.best_degree == null ? (double?)null : degree_resolution / 10;

                var br = grid_parameter_search(
                    training_file,
                    cross_validation_metric_class_list,
                    cross_validation_metrics,
                    weights,
                    svm_type,
                    kernel,
                    inner_cv_folds,
                    probability_estimates,
                    shrinking_heuristics,
                    point_max_time,
                    process_max_time,
                    max_tasks,
                    echo,
                    echo_err,
                    recursion_cost_begin,
                    recursion_cost_end,
                    recursion_cost_inc,
                    recursion_gamma_begin,
                    recursion_gamma_end,
                    recursion_gamma_inc,
                    recursion_epsilon_begin,
                    recursion_epsilon_end,
                    recursion_epsilon_inc,
                    recursion_coef0_begin,
                    recursion_coef0_end,
                    recursion_coef0_inc,
                    recursion_degree_begin,
                    recursion_degree_end,
                    recursion_degree_inc,
                    recursions - 1);

                if (br.best_rate > best_rate.best_rate)
                {
                    best_rate = br;
                }
            }

            return best_rate;
        }

        private static void grid_search_task_logic(
            bool use_modded_libsvm,
            string training_file,
            bool output_threshold_adjustment_performance,
            int model_index,
            best_rate_container best_rate,
            bool use_internal_cv = false,
            List<int> cross_validation_metric_class_list = null,
            performance_measure.confusion_matrix.cross_validation_metrics cross_validation_metrics = performance_measure.confusion_matrix.cross_validation_metrics.ACC,
            libsvm_caller.libsvm_svm_type svm_type = libsvm_caller.libsvm_svm_type.c_svc,
            libsvm_caller.libsvm_kernel_type kernel = libsvm_caller.libsvm_kernel_type.rbf,
            int inner_cv_folds = 5,
            bool probability_estimates = false,
            bool shrinking_heuristics = true,
            double? cost = null,
            double? gamma = null,
            double? epsilon = null,
            double? coef0 = null,
            double? degree = null,
            List<(int class_id, double weight)> weights = null,
            TimeSpan? point_max_time = null,
            TimeSpan? process_max_time = null,
            bool echo = false,
            bool echo_err = true

            )
        {
            //#if DEBUG
            var param_list = new List<(string key, string value)>()
            {
               (nameof(training_file), training_file?.ToString()),
               (nameof(output_threshold_adjustment_performance), output_threshold_adjustment_performance.ToString()),
               (nameof(model_index), model_index.ToString()),
               (nameof(best_rate), best_rate?.ToString()),
               (nameof(use_internal_cv ), use_internal_cv.ToString()),
               (nameof(cross_validation_metric_class_list ), cross_validation_metric_class_list?.ToString()),
               (nameof(cross_validation_metrics ), cross_validation_metrics.ToString()),
               (nameof(svm_type ), svm_type.ToString()),
               (nameof(kernel ), kernel.ToString()),
               (nameof(inner_cv_folds ), inner_cv_folds.ToString()),
               (nameof(probability_estimates ), probability_estimates.ToString()),
               (nameof(shrinking_heuristics ), shrinking_heuristics.ToString()),
               (nameof(cost ), cost?.ToString()),
               (nameof(gamma ), gamma?.ToString()),
               (nameof(epsilon ), epsilon?.ToString()),
               (nameof(coef0 ), coef0?.ToString()),
               (nameof(degree ), degree?.ToString()),
               (nameof(weights), weights?.ToString()),
               (nameof(point_max_time ), point_max_time?.ToString()),
               (nameof(process_max_time ), process_max_time?.ToString()),
               (nameof(echo ), echo.ToString()),
               (nameof(echo_err ), echo_err.ToString()),
            };

            program.WriteLine($@"{nameof(grid_search_task_logic)}({string.Join(", ", param_list.Select(a => $"{a.key}=\"{a.value}\"").ToList())});");
            //#endif

            var cv_rate = 0d;

            if (use_internal_cv)
            {
                program.WriteLine($@"Warning: using internal CV (slow)");
                var cv_rate_full = cross_validate_model_internal(use_modded_libsvm, training_file, output_threshold_adjustment_performance, model_index, cross_validation_metric_class_list, cross_validation_metrics, svm_type, kernel, inner_cv_folds, probability_estimates, shrinking_heuristics, cost, gamma, epsilon, coef0, degree, weights, point_max_time, process_max_time, echo, echo_err);
                cv_rate = cv_rate_full ?? 0d;
            }
            else
            {
                var cv_rate_full = cross_validate_model_libsvm(training_file, model_index, svm_type, kernel, get_modded_libsvm_eval_method(cross_validation_metric_class_list,cross_validation_metrics),inner_cv_folds, probability_estimates, shrinking_heuristics, cost, gamma, epsilon, coef0, degree, weights, point_max_time, process_max_time, echo, echo_err);
                cv_rate = cv_rate_full.v_cross_validation;
            }

            if (cv_rate > 0) // != null)
            {
                best_rate.update_rate(cost, gamma, epsilon, coef0, degree, cv_rate);//, cv_rate.g, cv_rate.rate.Value);
            }
        }
    }
}
