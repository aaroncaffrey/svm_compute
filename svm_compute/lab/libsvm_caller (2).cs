using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using svm_compute;

namespace svm_compute
{

    public class libsvm_caller
    {
        /*
            grid.py

            Usage: grid.py [grid_options] [svm_options] dataset

            grid_options :
            -log2c {begin,end,step | "null"} : set the range of c (default -5,15,2)
                begin,end,step -- c_range = 2^{begin,...,begin+k*step,...,end}
                "null"         -- do not grid with c
            -log2g {begin,end,step | "null"} : set the range of g (default 3,-15,-2)
                begin,end,step -- g_range = 2^{begin,...,begin+k*step,...,end}
                "null"         -- do not grid with g
            -v n : n-fold cross validation (default 5)
            -svmtrain pathname : set svm executable path and name
            -gnuplot {pathname | "null"} :
                pathname -- set gnuplot executable path and name
                "null"   -- do not plot
            -out {pathname | "null"} : (default dataset.out)
                pathname -- set output file path and name
                "null"   -- do not output file
            -png pathname : set graphic output file path and name (default dataset.png)
            -resume [pathname] : resume the grid task using an existing output file (default pathname is dataset.out)
                This is experimental. Try this option only if some parameters have been checked for the SAME data.

            svm_options : additional options for svm-train

         */

        public static void wait_libsvm()
        {
            program.WriteLine($@"Checking for libsvm...");

            var file_train = Path.Combine(libsvm_caller.libsvm_exes_path, "svm-train.exe");
            var file_predict = Path.Combine(libsvm_caller.libsvm_exes_path, "svm-predict.exe");

            var file_train_exists = false;
            var file_predict_exists = false;

            do
            {
                file_train_exists = (File.Exists(file_train) && new FileInfo(file_train).Length > 0);
                file_predict_exists = (File.Exists(file_predict) && new FileInfo(file_predict).Length > 0);

                if (!file_train_exists)
                {
                    program.WriteLine($@"Libsvm not found: {file_train}");
                }

                if (!file_predict_exists)
                {
                    program.WriteLine($@"Libsvm not found: {file_predict}");
                }

                if (!file_train_exists || !file_predict_exists)
                {
                    try
                    {
                        var delay = new TimeSpan(0, 0, 10);
                        program.WriteLine($@"wait_libsvm(): Task.Delay({delay.ToString()}).Wait();", true, ConsoleColor.Red);
                        Task.Delay(delay).Wait();

                    }
                    catch (Exception e)
                    {
                        program.WriteLine($"{nameof(wait_libsvm)}(): " + e.ToString(), true, ConsoleColor.DarkGray);

                    }
                }

            } while (!file_train_exists || !file_predict_exists);

            program.WriteLine($@"Libsvm found: {file_train}");
            program.WriteLine($@"Libsvm found: {file_predict}");
        }

        public enum libsvm_kernel_type : int
        {
            //@default = rbf,
            linear = 0,
            polynomial = 1,
            rbf = 2,
            sigmoid = 3,
            precomputed = 4,
        }

        public enum libsvm_svm_type : int
        {
            //@default = c_svc,
            c_svc = 0,
            nu_svc = 1,
            one_class_svm = 2,
            epsilon_svr = 3,
            nu_svr = 4,
        }

        /*
            svm-train.exe

            Usage: svm-train [options] training_set_file [model_file]
            options:
            -s svm_type : set type of SVM (default 0)
                    0 -- C-SVC              (multi-class classification)
                    1 -- nu-SVC             (multi-class classification)
                    2 -- one-class SVM
                    3 -- epsilon-SVR        (regression)
                    4 -- nu-SVR             (regression)
            -t kernel_type : set type of kernel function (default 2)
                    0 -- linear: u'*v
                    1 -- polynomial: (gamma*u'*v + coef0)^degree
                    2 -- radial basis function: exp(-gamma*|u-v|^2)
                    3 -- sigmoid: tanh(gamma*u'*v + coef0)
                    4 -- precomputed kernel (kernel values in training_set_file)
            -d degree : set degree in kernel function (default 3)
            -g gamma : set gamma in kernel function (default 1/num_features)
            -r coef0 : set coef0 in kernel function (default 0)
            -c cost : set the parameter C of C-SVC, epsilon-SVR, and nu-SVR (default 1)
            -n nu : set the parameter nu of nu-SVC, one-class SVM, and nu-SVR (default 0.5)
            -p epsilon : set the epsilon in loss function of epsilon-SVR (default 0.1)
            -m cachesize : set cache memory size in MB (default 100)
            -e epsilon : set tolerance of termination criterion (default 0.001)
            -h shrinking : whether to use the shrinking heuristics, 0 or 1 (default 1)
            -b probability_estimates : whether to train a SVC or SVR model for probability estimates, 0 or 1 (default 0)
            -wi weight : set the parameter C of class i to weight*C, for C-SVC (default 1)
            -v n: n-fold cross validation mode
            -q : quiet mode (no outputs)
        */


        /*
            svm-predict.exe

            Usage: svm-predict [options] test_file model_file output_file
            options:
            -b probability_estimates: whether to predict probability estimates, 0 or 1 (default 0); for one-class SVM only 0 is supported
            -q : quiet mode (no outputs)
         */

        //public static string temp_output_folder = @"{program.root_folder}\";

        public static string python_path = Path.Combine($@"c:\Anaconda3\");

        public const string libsvm_root_folder = @"c:\libsvm\";
        public static string libsvm_exes_path = Path.Combine($@"{libsvm_root_folder}", @"windows\");
        public static string libsvm_tools_path = Path.Combine($@"{libsvm_root_folder}", @"tools\");

        public const string libsvm_eval_mod_root_folder = @"c:\libsvm-eval\";
        public static string libsvm_eval_mod_exes_path = Path.Combine($@"{libsvm_eval_mod_root_folder}", @"windows\");
        public static string libsvm_eval_mod_tools_path = Path.Combine($@"{libsvm_eval_mod_root_folder}", @"tools\");

        //public static int probability_estimates = 1;

        //public static outer_cross_validate()
        //{

        //}

        public static List<List<string>> split_folds(List<string> list, int folds)
        {
            //#if DEBUG
            var param_list = new List<(string key, string value)>()
            {
                (nameof(list),list.ToString()),
                (nameof(folds),folds.ToString()),
            };

            program.WriteLine($@"{nameof(split_folds)}({String.Join(", ", param_list.Select(a => $"{a.key}=\"{a.value}\"").ToList())});");
            //#endif
            return list.Select((item, index) => new { index, item })
                .GroupBy(x => x.index % folds)
                .Select(x => x.Select(y => y.item).ToList()).ToList();
        }


        public static List<performance_measure.confusion_matrix> cross_validate_model_internal_logic(
            bool use_modded_libsvm,
            string full_training_file,
            bool output_threshold_adjustment_performance,
            int model_index,
            double? cost = null,
            double? gamma = null,
            double? epsilon = null,
            double? coef0 = null,
            double? degree = null,
            List<(int class_id, double weight)> weights = null,
            libsvm_svm_type svm_type = libsvm_svm_type.c_svc,
            libsvm_kernel_type kernel = libsvm_kernel_type.rbf,

            int inner_cv_folds = 5,
            bool probability_estimates = false,
            bool shrinking_heuristics = true,
            TimeSpan? point_max_time = null,
            TimeSpan? process_max_time = null,
            int? max_tasks = null,
            bool echo = false,
            bool echo_err = true)
        {
            //#if DEBUG
            var param_list = new List<(string key, string value)>()
            {
                (nameof(output_threshold_adjustment_performance),output_threshold_adjustment_performance.ToString()),
                (nameof(model_index),model_index.ToString()),
                (nameof(cost),cost.ToString()),
                (nameof(gamma),gamma.ToString()),
                (nameof(epsilon),epsilon.ToString()),
                (nameof(coef0),coef0.ToString()),
                (nameof(degree),degree.ToString()),
                (nameof(inner_cv_folds),inner_cv_folds.ToString()),
                (nameof(full_training_file),full_training_file.ToString()),

                (nameof(svm_type),svm_type.ToString()),
                (nameof(kernel),kernel.ToString()),
                (nameof(probability_estimates),probability_estimates.ToString()),
                (nameof(shrinking_heuristics),shrinking_heuristics.ToString()),
                (nameof(echo),echo.ToString()),
                (nameof(echo_err),echo_err.ToString())
            };

            program.WriteLine($@"{nameof(cross_validate_model_internal_logic)}({String.Join(", ", param_list.Select(a => $"{a.key}=\"{a.value}\"").ToList())});");
            //#endif
            var full_training_data = File.ReadAllLines(full_training_file).ToList();

            // todo: 5 metrics averaged or 1 metric?

            // make class ratios the same in each fold
            //var class_ids = full_training_data.Select(a => a.Substring(0, a.IndexOf(' '))).Distinct().ToList();
            var classes_full_training_data = full_training_data.GroupBy(a => a.Substring(0, a.IndexOf(' '))).Select(a => (class_id: a.Key, list: a.ToList())).ToList();
            var classes_training_data_folds = classes_full_training_data.Select(a => (class_id: a.class_id, list: split_folds(a.list, inner_cv_folds))).ToList();

            var total_groups = classes_training_data_folds.Max(a => a.list.Count);

            var training_data_folds = Enumerable.Range(0, total_groups).Select(i => classes_training_data_folds.SelectMany(a => a.list[i]).ToList()).ToList();

            //var probability_estimates = true;

            if (max_tasks == null)
            {
                max_tasks = training_data_folds.Count;
            }

            if (max_tasks < 0)
            {
                max_tasks = Environment.ProcessorCount * Math.Abs(max_tasks.Value) * 10;
            }

            if (max_tasks > training_data_folds.Count)
            {
                max_tasks = training_data_folds.Count;
            }

            var tasks = new List<Task<List<performance_measure.prediction>>>();

            var prediction_list = new List<performance_measure.prediction>();

            for (var index = 0; index < training_data_folds.Count; index++)
            {
                var fold_index = index;

                if (max_tasks == 0)
                {
                    var p = cross_validate_model_internal_task_logic(use_modded_libsvm, training_data_folds, model_index, fold_index, full_training_file, cost, gamma, epsilon, coef0, degree, weights, svm_type, kernel, probability_estimates, shrinking_heuristics, point_max_time, process_max_time, echo, echo_err);
                    prediction_list.AddRange(p);
                }
                else
                {
                    var task = Task.Run(() => cross_validate_model_internal_task_logic(use_modded_libsvm, training_data_folds, model_index, fold_index, full_training_file, cost, gamma, epsilon, coef0, degree, weights, svm_type, kernel, probability_estimates, shrinking_heuristics, point_max_time, process_max_time, echo, echo_err));

                    tasks.Add(task);

                    var incomplete_tasks = tasks.Where(a => !a.IsCompleted).ToList();

                    while (max_tasks > 0 && incomplete_tasks.Count >= max_tasks)
                    {
                        program.WriteLine($@"{nameof(cross_validate_model_internal_logic)}(): Task.WaitAny(tasks.ToArray<Task>());", true, ConsoleColor.Cyan);

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

            if (tasks != null && tasks.Count > 0)
            {
                program.WriteLine($@"{nameof(cross_validate_model_internal_logic)}(): Task.WaitAll(tasks.ToArray<Task>());", true, ConsoleColor.Cyan);
                Task.WaitAll(tasks.ToArray<Task>());

                prediction_list.AddRange(tasks.SelectMany(a => a.Result).ToList());
            }

            var perf = performance_measure.load_prediction_file(prediction_list, output_threshold_adjustment_performance);
            return perf;

        }

        private static List<performance_measure.prediction> cross_validate_model_internal_task_logic(
            bool use_modded_libsvm,
            List<List<string>> training_data_folds,
            int model_index,
            int fold_index,
            string full_training_file,
            double? cost = null,
            double? gamma = null,
            double? epsilon = null,
            double? coef0 = null,
            double? degree = null,
            List<(int class_id, double weight)> weights = null,
            libsvm_svm_type svm_type = libsvm_svm_type.c_svc,
            libsvm_kernel_type kernel = libsvm_kernel_type.rbf,
            bool probability_estimates = false,
            bool shrinking_heuristics = true,
            TimeSpan? point_max_time = null,
            TimeSpan? process_max_time = null,
            bool echo = false,
            bool echo_err = true)
        {
            //#if DEBUG
            var param_list = new List<(string key, string value)>()
            {
                (nameof(training_data_folds),training_data_folds.ToString()),
                (nameof(model_index),model_index.ToString()),
                (nameof(fold_index),fold_index.ToString()),
                (nameof(full_training_file),full_training_file.ToString()),
                (nameof(cost),cost.ToString()),
                (nameof(gamma),gamma.ToString()),
                (nameof(epsilon),epsilon.ToString()),
                (nameof(coef0),coef0.ToString()),
                (nameof(degree),degree.ToString()),
                (nameof(svm_type),svm_type.ToString()),
                (nameof(kernel),kernel.ToString()),
                (nameof(probability_estimates),probability_estimates.ToString()),
                (nameof(shrinking_heuristics),shrinking_heuristics.ToString()),
                (nameof(echo),echo.ToString()),
                (nameof(echo_err),echo_err.ToString())
            };

            program.WriteLine($@"{nameof(cross_validate_model_internal_task_logic)}({String.Join(", ", param_list.Select(a => $"{a.key}=\"{a.value}\"").ToList())});");
            //#endif
            var training_data = training_data_folds.Where((a, k) => k != fold_index).SelectMany(a => a).ToList();
            var validation_data = training_data_folds[fold_index];

            var training_data_filename = $"{full_training_file}.cv_t_{fold_index + 1}_{training_data_folds.Count}_{model_index + 1}";
            var validation_data_filename = $"{full_training_file}.cv_v_{fold_index + 1}_{training_data_folds.Count}_{model_index + 1}";
            var prediction_filename = $"{validation_data_filename}.predict";

            var model_filename = $"{training_data_filename}.model";


            Directory.CreateDirectory(Path.GetDirectoryName(training_data_filename));
            File.WriteAllLines(training_data_filename, training_data);

            Directory.CreateDirectory(Path.GetDirectoryName(validation_data_filename));
            File.WriteAllLines(validation_data_filename, validation_data);


            var inner_cv_folds = 0; // this function outputs the folders and validates them, therefore, no need for libsvm to do it.

            var eval_method = libsvm_cv_eval_methods.accuracy;
            

            var train_result = train(training_data_filename, model_filename, cost, gamma, epsilon, coef0, degree, weights, svm_type, kernel, eval_method, inner_cv_folds, probability_estimates, shrinking_heuristics, point_max_time, echo, echo_err);

            var predict_result = predict(use_modded_libsvm, validation_data_filename, model_filename, prediction_filename, probability_estimates, echo, echo_err);

            var predictions = performance_measure.load_prediction_file_regression_values(validation_data_filename, prediction_filename);

            File.Delete(training_data_filename);
            File.Delete(validation_data_filename);
            File.Delete(model_filename);
            File.Delete(prediction_filename);

            return predictions;
        }

        public enum kernel_parameter_search_method : int
        {
            grid_libsvm_python,
            grid_internal,
            nelder_mead
        }

        public enum libsvm_cv_eval_methods : int
        {
            // the libsvm eval mod outputs all of these values to the console.  this setting controls which one will the CV variable be set to.
            precision = 0,
            recall = 1,
            fscore = 2,
            bac = 3,
            auc = 4,
            accuracy = 5,
            ap = 6
        }

        public static (double? cost, double? gamma, double? epsilon, double? coef0, double? degree, double? cv_rate)
            grid_parameter_search(
                string training_file,
                kernel_parameter_search_method kernel_parameter_search_method,
                List<(int class_id, double weight)> weights = null,
                libsvm_svm_type svm_type = libsvm_svm_type.c_svc,
                libsvm_kernel_type kernel = libsvm_kernel_type.rbf,
                List<int> cross_validation_metric_class_list = null,
                performance_measure.confusion_matrix.cross_validation_metrics cross_validation_metrics = performance_measure.confusion_matrix.cross_validation_metrics.ACC,
                int? inner_cv_folds = 5,
                bool probability_estimates = false,
                bool shrinking_heuristics = true,
                TimeSpan? point_max_time = null,
                TimeSpan? process_max_time = null,
                bool echo = false,
                bool echo_err = true
                )
        {
            //#if DEBUG
            var param_list = new List<(string key, string value)>()
            {
                (nameof(kernel_parameter_search_method),kernel_parameter_search_method.ToString()),
                (nameof(svm_type),svm_type.ToString()),
                (nameof(kernel),kernel.ToString()),
                (nameof(training_file),training_file.ToString()),
                (nameof(cross_validation_metric_class_list),cross_validation_metric_class_list?.ToString() ?? null),
                (nameof(cross_validation_metrics),cross_validation_metrics.ToString()),
                (nameof(inner_cv_folds),inner_cv_folds.ToString()),
                (nameof(probability_estimates),probability_estimates.ToString()),
                (nameof(shrinking_heuristics),shrinking_heuristics.ToString()),
                (nameof(echo),echo.ToString()),
                (nameof(echo_err),echo_err.ToString())
            };

            program.WriteLine($@"{nameof(grid_parameter_search)}({String.Join(", ", param_list.Select(a => $"{a.key}=\"{a.value}\"").ToList())});");
            //#endif
            //var kernel_parameter_search_method = libsvm_caller.kernel_parameter_search_method.internal_implementation;

            //const bool use_python_grid_script = true;

            var max_tasks_grid_search = (int?)null;

            if (kernel_parameter_search_method == kernel_parameter_search_method.grid_internal)
            {
                var br = libsvm_grid.grid_parameter_search(training_file, cross_validation_metric_class_list, cross_validation_metrics, weights, svm_type, kernel, inner_cv_folds.Value, probability_estimates, shrinking_heuristics, point_max_time, process_max_time, max_tasks_grid_search, echo, echo_err);

                return (br.best_cost, br.best_gamma, br.best_epsilon, br.best_coef0, br.best_degree, br.best_rate);
            }
            else if (kernel_parameter_search_method == kernel_parameter_search_method.grid_libsvm_python)
            {
                var grid_params = new List<string>();

                if (kernel == libsvm_kernel_type.linear)
                {
                    grid_params.Add("-log2g null");
                }

                grid_params.Add("-gnuplot null");
                grid_params.Add("-out null");

                var libsvm_params = new List<string>();
                if (probability_estimates)
                {
                    libsvm_params.Add($@"-b {(probability_estimates ? "1" : "0")}");
                }

                if (svm_type != libsvm_svm_type.c_svc)
                {
                    libsvm_params.Add($@"-s {(int)svm_type}");
                }

                if (kernel != libsvm_kernel_type.rbf)
                {
                    libsvm_params.Add($@"-t {(int)kernel}");
                }

                if (inner_cv_folds != null && inner_cv_folds >= 2)
                {
                    libsvm_params.Add($@"-v {inner_cv_folds}");
                }

                if (!shrinking_heuristics)
                {
                    libsvm_params.Add($@"-h {(shrinking_heuristics ? "1" : "0")}");
                }

                if (!String.IsNullOrWhiteSpace(training_file))
                {
                    libsvm_params.Add($@"""{training_file}""");
                }

                if (weights != null && weights.Count > 0)
                {
                    foreach (var weight in weights.OrderBy(a => a).ToList())
                    {
                        libsvm_params.Add($"-w{weight.class_id} {weight.weight}");
                    }
                }

                var use_modded_libsvm = libsvm_grid.should_use_modded_libsvm(cross_validation_metric_class_list, cross_validation_metrics);
               
                var cv_result = libsvm_caller.run_py_cmd(Path.Combine(use_modded_libsvm ? libsvm_eval_mod_tools_path : libsvm_tools_path, "grid.py"), $"{String.Join(" ", grid_params)} {String.Join(" ", libsvm_params)}", point_max_time, echo, echo_err);

                var cv_data_split = cv_result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Last().Split();

                var cost = (double?)Double.Parse(cv_data_split[0], NumberStyles.Float, CultureInfo.InvariantCulture);
                var gamma = (kernel == libsvm_kernel_type.linear) ? (double?)null : (double?)double.Parse(cv_data_split[1], NumberStyles.Float, CultureInfo.InvariantCulture);

                var epsilon = (double?)null; // grid.py does not search for epsilon by default // (svm_type == libsvm_svm_type.epsilon_svr || svm_type==libsvm_svm_type.nu_svr) ? (double?) null : (double?)double.Parse(cv_data_split[xx], NumberStyles.Float, CultureInfo.InvariantCulture);

                var coef0 = (double?)null; // grid.py does not search for coef0 by default
                var degree = (double?)null; // grid.py does not search for degree by default

                var cv_rate = (double?)Double.Parse(cv_data_split[(kernel == libsvm_kernel_type.linear) ? 1 : 2], CultureInfo.InvariantCulture);



                return (cost, gamma, epsilon, coef0, degree, cv_rate);
            }
            else
            {
                throw new Exception();
            }

        }

        public static string run_py_cmd(string cmd, string args, TimeSpan? process_max_time = null, bool echo = false, bool echo_err = true)
        {
            //#if DEBUG
            var param_list = new List<(string key, string value)>()
            {
                (nameof(cmd),cmd),
                (nameof(args),args),
                (nameof(echo),echo.ToString()),
                (nameof(echo_err),echo_err.ToString())
            };

            program.WriteLine($@"{nameof(run_py_cmd)}({String.Join(", ", param_list.Select(a => $@"{a.key}=""{a.value}""").ToList())});");
            //#endif
            var start = new ProcessStartInfo
            {
                FileName = Path.Combine($@"{python_path}", $@"python.exe"),
                Arguments = $@"""{cmd}"" {args}",
                UseShellExecute = false,
                CreateNoWindow = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                
            };

            if (!File.Exists(start.FileName)) throw new Exception();

            var access_allowed = false;
            var access_attempt = 0;


            //var sw_time_taken = new Stopwatch();
            


            while (!access_allowed)
            {
                try
                {
                    access_attempt++;

                    using (var process = Process.Start(start))
                    {
                        if (process == null) return null;


                        try { process.PriorityBoostEnabled = true; } catch (Exception) { } finally { }
                        try { process.PriorityClass = ProcessPriorityClass.High; } catch (Exception) { } finally { }


                        using (var reader = process.StandardOutput)
                        {
                            var result = reader.ReadToEndAsync(); // Here is the result of StdOut(for example: print "test")
                            var stderr = process.StandardError.ReadToEndAsync(); // Here are the exceptions from our Python script

                            //if (!sw_time_taken.IsRunning)
                            //{
                            //    sw_time_taken.Start();
                            //}

                            while (!result.IsCompleted && !stderr.IsCompleted && process_max_time != null && !process.HasExited)
                            {
                                
                                //var time_taken = sw_time_taken.Elapsed;

                                var time_taken = DateTime.Now - process.StartTime;
                                var cpu_time = process.TotalProcessorTime;

                                if (process_max_time != null && time_taken > process_max_time.Value && cpu_time > process_max_time.Value)
                                {
                                    try { process.CancelOutputRead(); } catch (Exception) { } finally { }
                                    try { process.CancelErrorRead(); } catch (Exception) { } finally { }
                                    try { process.CloseMainWindow(); } catch (Exception) { } finally { }
                                    try { process.Close(); } catch (Exception) { } finally { }
                                    try { process.Kill(); } catch (Exception) { } finally { }
                                    try { process.Dispose(); } catch (Exception) { } finally { }

                                    //#if DEBUG
                                    program.WriteLine($@"Error: {nameof(run_py_cmd)}() took too long - cancelled ({time_taken.ToString()}).", true, ConsoleColor.Yellow);
                                    //#endif
                                    return null;
                                }

                                try
                                {
                                    var delay = new TimeSpan(0, 0, 0, 0, 100);
                                    program.WriteLine($@"run_py_cmd(): Task.Delay({delay.ToString()}).Wait();", true, ConsoleColor.Red);
                                    Task.Delay(delay).Wait();
                                }
                                catch (Exception e)
                                {
                                    program.WriteLine($"{nameof(run_py_cmd)}(): " + e.ToString(), true, ConsoleColor.DarkGray);
                                }
                            }

                            if (echo && !String.IsNullOrWhiteSpace(result.Result))
                            {
                                program.WriteLine($@"{nameof(run_py_cmd)}: {result.Result}");
                            }

                            if (echo_err && !String.IsNullOrWhiteSpace(stderr.Result))
                            {
                                var stderr_str = String.Concat(stderr.Result.Select(a => a >= 32 ? a : '_').ToList());
                                program.WriteLine($@"Error at: public static string {nameof(run_py_cmd)}(string cmd=|{cmd}|, string args=|{args}|, bool echo=|{echo}|, bool echo_err=|{echo_err}|) {stderr_str}");
                            }

                            if (!process.HasExited)
                            {
                                if (process_max_time != null)
                                {
                                    process.WaitForExit((int)process_max_time.Value.TotalMilliseconds);
                                }
                                else
                                {
                                    process.WaitForExit();
                                }

                            }

                            access_allowed = true;
                            return result.Result;
                        }
                    }
                }
                catch (Exception e)
                {
                    access_allowed = false;
                    program.WriteLine($@"run_py_cmd({cmd}, {args}): {e.Message}");
                    try
                    {
                        var delay = new TimeSpan(0, 0, 0, 0, 50);
                        program.WriteLine($@"run_py_cmd(): Task.Delay({delay.ToString()}).Wait();", true, ConsoleColor.Red);
                        Task.Delay(delay).Wait();

                    }
                    catch (Exception exception)
                    {
                        program.WriteLine($"{nameof(run_py_cmd)}(): " + exception.ToString(), true, ConsoleColor.DarkGray);

                    }
                }

                if (access_attempt >= Int32.MaxValue) break;
            }

            return null;
        }

        public static string train(
            string train_file,
            string model_out_file,
            double? cost = null,
            double? gamma = null,
            double? epsilon = null,
            double? coef0 = null,
            double? degree = null,
            List<(int class_id, double weight)> weights = null,
            libsvm_svm_type svm_type = libsvm_svm_type.c_svc,
            libsvm_kernel_type kernel = libsvm_kernel_type.rbf,
            libsvm_cv_eval_methods eval_method = libsvm_cv_eval_methods.accuracy,
            int? inner_cv_folds = null,
            bool probability_estimates = false,
            bool shrinking_heuristics = true,
            TimeSpan? process_max_time = null,
            bool echo = false,
            bool echo_err = true
            )
        {
            if (eval_method!=libsvm_cv_eval_methods.accuracy && probability_estimates)
            {
                throw new Exception("eval modded libsvm does not support probability estimates");
            }

            //#if DEBUG
            var param_list = new List<(string key, string value)>()
            {
                (nameof(train_file),train_file.ToString()),
                (nameof(model_out_file),model_out_file.ToString()),
                (nameof(cost),cost.ToString()),
                (nameof(gamma),gamma.ToString()),
                (nameof(epsilon),epsilon.ToString()),
                (nameof(coef0),coef0.ToString()),
                (nameof(degree),degree.ToString()),

                (nameof(svm_type),svm_type.ToString()),
                (nameof(kernel),kernel.ToString()),
                (nameof(inner_cv_folds),inner_cv_folds.ToString()),

                (nameof(probability_estimates),probability_estimates.ToString()),
                (nameof(shrinking_heuristics),shrinking_heuristics.ToString()),
                (nameof(echo),echo.ToString()),
                (nameof(echo_err),echo_err.ToString())
            };

            //program.WriteLine($@"{nameof(train)}({string.Join(", ", param_list.Select(a => $"{a.key}=\"{a.value}\"").ToList())});");
            //#endif

            var libsvm_params = new List<string>();
            if (probability_estimates) { libsvm_params.Add($@"-b {(probability_estimates ? "1" : "0")}"); }
            if (svm_type != libsvm_svm_type.c_svc) { libsvm_params.Add($@"-s {(int)svm_type}"); }
            if (kernel != libsvm_kernel_type.rbf) { libsvm_params.Add($@"-t {(int)kernel}"); }
            if (inner_cv_folds != null && inner_cv_folds >= 2) { libsvm_params.Add($@"-v {inner_cv_folds}"); }
            if (cost != null) { libsvm_params.Add($@"-c {cost.Value}"); }
            if (gamma != null && kernel != libsvm_kernel_type.linear) { libsvm_params.Add($@"-g {gamma.Value}"); }

            if (epsilon != null && (svm_type == libsvm_svm_type.epsilon_svr || svm_type == libsvm_svm_type.nu_svr)) { libsvm_params.Add($@"-p {epsilon.Value}"); }

            if (coef0 != null && (kernel == libsvm_kernel_type.sigmoid || kernel == libsvm_kernel_type.polynomial)) { libsvm_params.Add($@"-r {coef0.Value}"); }
            if (degree != null && kernel == libsvm_kernel_type.polynomial) { libsvm_params.Add($@"-d {degree.Value}"); }

            if (weights != null && weights.Count > 0)
            {
                foreach (var weight in weights.OrderBy(a => a).ToList())
                {
                    libsvm_params.Add($@"-w{weight.class_id} {weight.weight}");
                }
            }

            if (eval_method != libsvm_cv_eval_methods.accuracy && probability_estimates)
            {
                throw new Exception("Error: libsvm eval modded cannot be used in conjunction with probability estimates.");
            }

            if (eval_method != libsvm_cv_eval_methods.accuracy && inner_cv_folds >= 2)
            {

                libsvm_params.Add($@"-z {(int)eval_method}");
            }

            if (!shrinking_heuristics) { libsvm_params.Add($@"-h {(shrinking_heuristics ? "1" : "0")}"); }

            if (!String.IsNullOrWhiteSpace(train_file)) { libsvm_params.Add($@"""{train_file}"""); }
            if (!String.IsNullOrWhiteSpace(model_out_file) && (inner_cv_folds == null || inner_cv_folds <= 1)) { libsvm_params.Add($@"""{model_out_file}"""); }

            var start = new ProcessStartInfo
            {
                FileName = Path.Combine(eval_method != libsvm_cv_eval_methods.accuracy ? $@"{libsvm_eval_mod_exes_path}" : $@"{libsvm_exes_path}", $@"svm-train.exe"),
                Arguments = String.Join(" ", libsvm_params),
                UseShellExecute = false,
                CreateNoWindow = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            //program.WriteLine(start.FileName + " " + start.Arguments);
            var access_allowed = false;
            var access_attempt = 0;

            //var sw_time_taken = new Stopwatch();


            while (!access_allowed)
            {
                try
                {
                    access_attempt++;

                    using (var process = Process.Start(start))
                    {
                        if (process == null) return null;

                        try { process.PriorityBoostEnabled = true; } catch (Exception) { } finally { }
                        try { process.PriorityClass = ProcessPriorityClass.High; } catch (Exception) { } finally { }


                        using (var reader = process.StandardOutput)
                        {
                            var result = reader.ReadToEndAsync(); // Here is the result of StdOut(for example: print "test")

                            var stderr = process.StandardError.ReadToEndAsync(); // Here are the exceptions from our Python script

                            //if (!sw_time_taken.IsRunning) sw_time_taken.Start();

                            while (!result.IsCompleted && !stderr.IsCompleted && process_max_time != null && !process.HasExited)
                            {
                                var time_taken = DateTime.Now - process.StartTime;
                                var cpu_time = process.TotalProcessorTime;

                                //process.WaitForExit((int)process_max_time.Value.TotalMilliseconds);

                                if (process_max_time != null && time_taken > process_max_time.Value && cpu_time > process_max_time.Value)
                                {
                                    try { process.CancelOutputRead(); } catch (Exception) { } finally { }
                                    try { process.CancelErrorRead(); } catch (Exception) { } finally { }
                                    try { process.CloseMainWindow(); } catch (Exception) { } finally { }
                                    try { process.Close(); } catch (Exception) { } finally { }
                                    try { process.Kill(); } catch (Exception) { } finally { }
                                    try { process.Dispose(); } catch (Exception) { } finally { }

                                    //#if DEBUG
                                    program.WriteLine($@"Error: {nameof(train)}() took too long - cancelled ({time_taken.ToString()}).", true, ConsoleColor.Yellow);
                                    //#endif
                                    return null;
                                }

                                try
                                {
                                    var delay = new TimeSpan(0, 0, 0, 0, 100);
                                    program.WriteLine($@"train(): Task.Delay({delay.ToString()}).Wait();", true, ConsoleColor.Red);
                                    Task.Delay(delay).Wait();
                                }
                                catch (Exception e)
                                {
                                    program.WriteLine($"{nameof(train)}(): " + e.ToString(), true, ConsoleColor.DarkGray);

                                }
                            }

                            if (echo && !String.IsNullOrWhiteSpace(result.Result))
                            {
                                program.WriteLine($@"{nameof(train)}: {result.Result}");
                            }

                            if (echo_err && !String.IsNullOrWhiteSpace(stderr.Result))
                            {
                                try
                                {

                                
                                    var stderr_str = String.Concat(stderr.Result.Select(a => a >= 32 ? a : '_').ToList());

                                    var err = stderr_str.Replace("_", "").Replace("WARNING: reaching max number of iterations", "").Trim();

                                    if (err.Length == 0)
                                    {
                                        program.WriteLine($@"train(): WARNING: reaching max number of iterations: train_file={train_file}, model_out_file={model_out_file}, cost={cost}, gamma={gamma}, epsilon={epsilon}, coef0={coef0}, degree={degree}, weights={weights}, svm_type={svm_type}, kernel={kernel}, eval_method={eval_method}, inner_cv_folds={inner_cv_folds}, probability_estimates={probability_estimates}, shrinking_heuristics={shrinking_heuristics}, process_max_time={process_max_time}.");
                                    }
                                    else
                                    {


                                        program.WriteLine($@"Error at: public static string {nameof(train)}(double? cost=|{cost}|, double? gamma=|{gamma}|, double? epsilon=|{epsilon}|, libsvm_svm_type svm_type=|{svm_type}|, libsvm_kernel_type kernel=|{kernel}|, int? n_fold_cv=|{inner_cv_folds}|, string train_file=|{train_file}|, string model_out_file=|{model_out_file}|, bool probability_estimates=|{probability_estimates}|, bool echo=|{echo}|, bool echo_err=|{echo_err}|) stderr={stderr_str}");
                                    }

                                }
                                catch (Exception e)
                                {
                                    program.WriteLine("train(): " + e.ToString(), true, ConsoleColor.Red);
                                }
                            }

                            if (!process.HasExited)
                            {
                                if (process_max_time != null)
                                {
                                    process.WaitForExit((int)process_max_time.Value.TotalMilliseconds);
                                }
                                else
                                {
                                    process.WaitForExit();
                                }
                            }

                            access_allowed = true;
                            return result.Result;
                        }
                    }
                }
                catch (Exception e)
                {
                    access_allowed = false;

                    program.WriteLine($@"train({cost}, {gamma}, {epsilon}, {coef0}, {degree}, {kernel}, {train_file}, {model_out_file}): {e.Message}");
                    try
                    {
                        var delay = new TimeSpan(0, 0, 0, 0, 50);
                        program.WriteLine($@"train(): Task.Delay({delay.ToString()}).Wait();", true, ConsoleColor.Red);
                        Task.Delay(delay).Wait();

                    }
                    catch (Exception exception)
                    {
                        program.WriteLine($"{nameof(train)}(): " + exception.ToString(), true, ConsoleColor.DarkGray);

                    }
                }

                if (access_attempt >= Int32.MaxValue) break;
            }

            return null;

        }

        //        public static string train(
        //            double? cost,
        //            double? gamma,
        //            double? coef0,
        //            double? degree,
        //            libsvm_svm_type svm_type,
        //            libsvm_kernel_type kernel,
        //            int? n_fold_cv,
        //            string train_file,
        //            string model_out_file,
        //            bool probability_estimates,
        //            bool shrinking_heuristics,
        //            bool echo = false,
        //            bool echo_err = true)
        //        {
        ////#if DEBUG
        //            var param_list = new List<(string key, string value)>()
        //            {

        //                (nameof(cost),cost.ToString()),
        //                (nameof(gamma),gamma.ToString()),
        //                (nameof(svm_type),svm_type.ToString()),
        //                (nameof(kernel),kernel.ToString()),
        //                (nameof(n_fold_cv),n_fold_cv.ToString()),
        //                (nameof(train_file),train_file.ToString()),
        //                (nameof(model_out_file),model_out_file.ToString()),
        //                (nameof(probability_estimates),probability_estimates.ToString()),
        //                (nameof(shrinking_heuristics),shrinking_heuristics.ToString()),
        //                (nameof(echo),echo.ToString()),
        //                (nameof(echo_err),echo_err.ToString())
        //            };

        //            program.WriteLine($@"{nameof(train)}({string.Join(", ", param_list.Select(a => $"{a.key}=\"{a.value}\"").ToList())});");
        ////#endif

        //            var libsvm_params = new List<string>();
        //            if (probability_estimates) { libsvm_params.Add($@"-b {(probability_estimates ? "1" : "0")}"); }
        //            if (svm_type != libsvm_svm_type.c_svc) { libsvm_params.Add($@"-s {(int)svm_type}"); }
        //            if (kernel != libsvm_kernel_type.rbf) { libsvm_params.Add($@"-t {(int)kernel}"); }
        //            if (n_fold_cv != null && n_fold_cv >= 2) { libsvm_params.Add($@"-v {n_fold_cv}"); }
        //            if (cost != null) { libsvm_params.Add($@"-c {cost.Value}"); }
        //            if (gamma != null && kernel != libsvm_kernel_type.linear) { libsvm_params.Add($@"-g {gamma.Value}"); }

        //            if (coef0 != null && (kernel == libsvm_kernel_type.sigmoid || kernel == libsvm_kernel_type.polynomial)) { libsvm_params.Add($@"-r {coef0.Value}"); }
        //            if (degree != null && kernel == libsvm_kernel_type.polynomial) { libsvm_params.Add($@"-d {degree.Value}"); }



        //            if (!shrinking_heuristics) { libsvm_params.Add($@"-h {(shrinking_heuristics ? "1" : "0")}"); }

        //            if (!string.IsNullOrWhiteSpace(train_file)) { libsvm_params.Add($"\"{train_file}\""); }
        //            if (!string.IsNullOrWhiteSpace(model_out_file) && (n_fold_cv == null || n_fold_cv <= 1)) { libsvm_params.Add($"\"{model_out_file}\""); }

        //            //var libsvm_params = new List<string>();
        //            //if (probability_estimates) {libsvm_params.Add($@"-b {(probability_estimates?"1":"0")}");}
        //            //if (svm_type != libsvm_svm_type.c_svc) {libsvm_params.Add($@"-s {(int)svm_type}");}
        //            //if (kernel != libsvm_kernel_type.rbf) {libsvm_params.Add($@"-t {(int)kernel}");}
        //            //if (n_fold_cv != null && n_fold_cv >= 2) {libsvm_params.Add($@"-v {n_fold_cv}");}
        //            //if (cost != null) {libsvm_params.Add($@"-c {cost.Value}");}
        //            //if (gamma != null && kernel != libsvm_kernel_type.linear) {libsvm_params.Add($@"-g {gamma.Value}");}
        //            //if (!shrinking_heuristics) { libsvm_params.Add($@"-h {(shrinking_heuristics?"1":"0")}"); }

        //            //if (!string.IsNullOrWhiteSpace(train_file)) {libsvm_params.Add($"\"{train_file}\"");}
        //            //if (!string.IsNullOrWhiteSpace(model_out_file) && (n_fold_cv == null || n_fold_cv <= 1)) {libsvm_params.Add($"\"{model_out_file}\"");}

        //            var start = new ProcessStartInfo
        //            {
        //                FileName = $@"{libsvm_exes_path}svm-train.exe",
        //                Arguments = string.Join(" ", libsvm_params),
        //                UseShellExecute = false,
        //                CreateNoWindow = false,
        //                RedirectStandardOutput = true,
        //                RedirectStandardError = true
        //            };

        //            //program.WriteLine(start.FileName + " " + start.Arguments);
        //            var access_allowed = false;
        //            var access_attempt = 0;

        //            while (!access_allowed)
        //            {
        //                try
        //                {
        //                    access_attempt++;

        //                    using (var process = Process.Start(start))
        //                    {
        //                        if (process == null) return null;

        //                        using (var reader = process.StandardOutput)
        //                        {
        //                            var result = reader.ReadToEnd(); // Here is the result of StdOut(for example: print "test")
        //                            var stderr = process.StandardError.ReadToEnd(); // Here are the exceptions from our Python script

        //                            if (echo && !string.IsNullOrWhiteSpace(result))
        //                            {
        //                                program.WriteLine($@"{nameof(train)}: {result}");
        //                            }
        //                            if (echo_err && !string.IsNullOrWhiteSpace(stderr))
        //                            {
        //                                stderr = string.Concat(stderr.Select(a => a >= 32 ? a : '_').ToList());
        //                                program.WriteLine($@"Error at: public static string {nameof(train)}(double? cost=|{cost}|, double? gamma=|{gamma}|, libsvm_svm_type svm_type=|{svm_type}|, libsvm_kernel_type kernel=|{kernel}|, int? n_fold_cv=|{n_fold_cv}|, string train_file=|{train_file}|, string model_out_file=|{model_out_file}|, bool probability_estimates=|{probability_estimates}|, bool echo=|{echo}|, bool echo_err=|{echo_err}|) stderr={stderr}");
        //                            }

        //                            process.WaitForExit();
        //                            access_allowed = true;
        //                            return result;
        //                        }
        //                    }
        //                }
        //                catch (Exception e)
        //                {
        //                    access_allowed = false;

        //                    program.WriteLine($@"train({cost}, {gamma}, {kernel}, {train_file}, {model_out_file}): {e.Message}");
        //                    Task.Delay(50).Wait();
        //                }

        //                if (access_attempt >= int.MaxValue) break;
        //            }

        //            return null;

        //        }

        public static string predict(bool use_modded_libsvm, string test_file, string model_file, string predictions_out_file, bool probability_estimates, bool echo = false, bool echo_err = true)
        {
            if (use_modded_libsvm && probability_estimates)
            {
                throw new Exception("eval modded libsvm does not support probability estimates");
            }

            //#if DEBUG
            var param_list = new List<(string key, string value)>()
            {

                (nameof(test_file),test_file.ToString()),
                (nameof(model_file),model_file.ToString()),
                (nameof(predictions_out_file),predictions_out_file.ToString()),




                (nameof(probability_estimates),probability_estimates.ToString()),

                (nameof(echo),echo.ToString()),
                (nameof(echo_err),echo_err.ToString())
            };

            program.WriteLine($@"{nameof(predict)}({String.Join(", ", param_list.Select(a => $@"{a.key}=""{a.value}""").ToList())});");
            //#endif

            var libsvm_params = new List<string>();
            if (probability_estimates) { libsvm_params.Add($@"-b 1"); }

            if (use_modded_libsvm)
            {
                //libsvm_params.Add($@"-z {eval_mod_options.ap}");
            }

            if (!String.IsNullOrWhiteSpace(test_file)) { libsvm_params.Add($@"""{test_file}"""); }
            if (!String.IsNullOrWhiteSpace(model_file)) { libsvm_params.Add($@"""{model_file}"""); }
            if (!String.IsNullOrWhiteSpace(predictions_out_file)) { libsvm_params.Add($@"""{predictions_out_file}"""); }

            var start = new ProcessStartInfo
            {
                FileName = Path.Combine(use_modded_libsvm ? libsvm_eval_mod_exes_path : libsvm_exes_path, $@"svm-predict.exe"),
                Arguments = String.Join(" ", libsvm_params),
                UseShellExecute = false,
                CreateNoWindow = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var access_allowed = false;
            var access_attempt = 0;

            while (!access_allowed)
            {
                try
                {
                    access_attempt++;

                    using (var process = Process.Start(start))
                    {
                        if (process == null) return null;

                        using (var reader = process.StandardOutput)
                        {
                            var result = reader.ReadToEnd(); // Here is the result of StdOut(for example: print "test")
                            var stderr = process.StandardError.ReadToEnd(); // Here are the exceptions from our Python script

                            if (echo && !String.IsNullOrWhiteSpace(result))
                            {
                                program.WriteLine($@"{nameof(predict)}: {result}");
                            }

                            if (echo_err && !String.IsNullOrWhiteSpace(stderr))
                            {
                                stderr = String.Concat(stderr.Select(a => a >= 32 ? a : '_').ToList());
                                program.WriteLine($@"Error at: public static string {nameof(predict)}(string test_file=|{test_file}|, string model_file=|{model_file}|, string predictions_out_file=|{predictions_out_file}|, bool probability_estimates=|{probability_estimates}|, bool echo=|{echo}|, bool echo_err=|{echo_err}|) {stderr}");
                            }

                            process.WaitForExit();
                            access_allowed = true;
                            return result;
                        }
                    }
                }
                catch (Exception e)
                {
                    access_allowed = false;

                    program.WriteLine($@"predict({test_file}, {model_file}, {predictions_out_file}): {e.Message}");
                    try
                    {
                        var delay = new TimeSpan(0, 0, 0, 0, 100);

                        program.WriteLine($@"predict(): Task.Delay({delay.ToString()}).Wait();", true, ConsoleColor.Red);
                        Task.Delay(delay).Wait();

                    }
                    catch (Exception exception)
                    {
                        program.WriteLine($"{nameof(predict)}(): " + exception.ToString(), true, ConsoleColor.DarkGray);

                    }
                }

                if (access_attempt >= Int32.MaxValue) break;
            }

            return null;
        }
    }
}


