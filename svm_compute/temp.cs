using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using svm_compute;

namespace svm_compute
{
    public class temp
    {
        public static String WildCardToRegular(String value)
        {
            
            //var param_list = new List<(string key, string value)>() { (nameof(value), value.ToString()), };

            //if (program.write_console_log) program.WriteLine($@"{nameof(WildCardToRegular)}({string.Join(", ", param_list.Select(a => $"{a.key}=\"{a.value}\"").ToList())});");
            

            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }

        // this function runs svm on slave compute unit
        //public static run_svm_return run_svm_local(run_svm_params x)
        //{
        //    //var r = run_svm(x.outer_cv_random_seed, x.random_skips, x.return_predictions, x.return_performance, x.return_meta_data, x.experiment_id1, x.experiment_id2, x.starting_point, x.direction, x.algorithm_direction, x.perf_selection_rule, x.output_threshold_adjustment_performance, x.class_names,
        //    //    x.kernel_parameter_search_methods, x.svm_types, x.kernels, /*x.train_test_splits,*/ x.math_operations,
        //    //    x.scaling_methods, x.prediction_methods, x.training_resampling_methods, x.example_instance_list,
        //    //    x.weights, x.randomisation_cv_folds, x.outer_cv_folds, x.inner_cv_folds, x.cross_validation_metrics_class_list, x.cross_validation_metrics,
        //    //    x.max_tasks);

        //    var r = run_svm(x);

        //    //save_svm_return(save_filename_performance,save_filename_predictions,run_svm_result);

        //    return r;
        //}


        //public static object eta_lock = new object();
        //public static DateTime start_time;
        //public static int items_done;

        //public static int items_total;
        //public static void update_eta(int inc, string fname, string pname)
        //{
        //    
        //    var param_list = new List<(string key, string value)>()
        //    {
        //        (nameof(inc),inc.ToString()),
        //        (nameof(fname),fname.ToString()),
        //        (nameof(pname),pname.ToString()),
        //    };
        //
        //    if (program.write_console_log) program.WriteLine($@"{nameof(update_eta)}({string.Join(", ", param_list.Select(a => $"{a.key}=\"{a.value}\"").ToList())});");
        //    
        //    lock (program.eta_lock)
        //    {
        //        program.items_done += inc;
        //
        //
        //        var time_remaining = TimeSpan.FromTicks(DateTime.Now.Subtract(program.start_time).Ticks * (program.items_total - (program.items_done /*+ 1*/)) / (program.items_done /*+ 1*/));
        //
        //
        //
        //        if (program.write_console_log) program.WriteLine($@"{fname}:({pname}): ETA: ({time_remaining.Days}d {time_remaining.Hours}h {time_remaining.Minutes}m {time_remaining.Seconds}s) - {program.items_done} / {program.items_total}");
        //
        //    }
        //}


        //public static void calc_ffs_bfs_performance(
        //    List<(string cluster_name, string dimension, string alphabet, string neighbourhood_name, string group_name, int num_features)> feature_group_list,
        //    List<(int fid, string source, string group, string member, string perspective)> dataset_headers,
        //    string[] remove_sources,
        //    string[] remove_groups,
        //    string[] remove_members,
        //    string[] remove_perspectives,
        //    int class_id_feature_index,
        //    List<(List<(string comment_header, string comment_value)> comment_columns, List<(int fid, double fv)> feature_data)> dataset_instance_list,
        //    bool output_threshold_adjustment_performance,
        //    List<(int class_id, string class_name)> class_names,
        //    List<libsvm_caller.kernel_parameter_search_method> kernel_parameter_search_methods,
        //    List<libsvm_caller.libsvm_svm_type> svm_types,
        //    List<libsvm_caller.libsvm_kernel_type> kernels,
        //    List<program.math_operation> math_operations,
        //    List<program.scaling_method> scaling_methods,
        //    List<program.test_prediction_method> test_prediction_methods,
        //    List<program.resampling_method> resampling_methods,
        //    int randomisation_cv_folds,
        //    int outer_cv_folds,
        //    int inner_cv_folds,
        //    List<int> cross_validation_metrics_class_list,
        //    libsvm_grid.cross_validation_metrics cross_validation_metrics,
        //    int? fs_perf_class)
        //{

        //}



        //var train_test_splits = new /*List<(double train, double unused, double test)>*/()
        //{
        //    //(train: 0.05, unused: 0.00, test: 1-0.05),
        //(train: 0.10, unused: 0.00, test: 1-0.10),
        //(train: 0.15, unused: 0.00, test: 1-0.15),
        //(train: 0.20, unused: 0.00, test: 1-0.20),
        //(train: 0.25, unused: 0.00, test: 1-0.25),
        //(train: 0.30, unused: 0.00, test: 1-0.30),
        //(train: 0.35, unused: 0.00, test: 1-0.35),
        //(train: 0.40, unused: 0.00, test: 1-0.40),
        //(train: 0.45, unused: 0.00, test: 1-0.45),
        //(train: 0.50, unused: 0.00, test: 1-0.50),
        //(train: 0.55, unused: 0.00, test: 1-0.55),
        //(train: 0.60, unused: 0.00, test: 1-0.60),
        //(train: 0.65, unused: 0.00, test: 1-0.65),
        //(train: 0.70, unused: 0.00, test: 1-0.70),
        //(train: 0.75, unused: 0.00, test: 1-0.75),
        //(train: 0.80, unused: 0.00, test: 1-0.80),
        //(train: 0.85, unused: 0.00, test: 1-0.85),
        //(train: 0.90, unused: 0.00, test: 1-0.90),
        //(train: 0.95, unused: 0.00, test: 1-0.95),

        //(train: 0.05, unused: 1-(0.05+0.20), test: 0.20),
        //(train: 0.10, unused: 1-(0.10+0.20), test: 0.20),
        //(train: 0.15, unused: 1-(0.15+0.20), test: 0.20),
        //(train: 0.20, unused: 1-(0.20+0.20), test: 0.20),
        //(train: 0.25, unused: 1-(0.25+0.20), test: 0.20),
        //(train: 0.30, unused: 1-(0.30+0.20), test: 0.20),
        //(train: 0.35, unused: 1-(0.35+0.20), test: 0.20),
        //(train: 0.40, unused: 1-(0.40+0.20), test: 0.20),
        //(train: 0.45, unused: 1-(0.45+0.20), test: 0.20),
        //(train: 0.50, unused: 1-(0.50+0.20), test: 0.20),
        //(train: 0.55, unused: 1-(0.55+0.20), test: 0.20),
        //(train: 0.60, unused: 1-(0.60+0.20), test: 0.20),
        //(train: 0.65, unused: 1-(0.65+0.20), test: 0.20),
        //(train: 0.70, unused: 1-(0.70+0.20), test: 0.20),
        //(train: 0.75, unused: 1-(0.75+0.20), test: 0.20),
        //    (train: 0.80, unused: 1 - (0.80 + 0.20), test: 0.20),
        //};



        //    // add class id feature to all feature combinations

        //    feature_combinations_list.ForEach(a => a.selected_fid_list.Insert(0, class_id_feature_index));
        //}






        //save_results(save_filename_performance, new List<string> { performance_measure.confusion_matrix.csv_header }, false);


        //var class_ids = dataset_instance_list.Select(a => a.feature_data[0].fv).Distinct().ToList();
        //var total_classes = class_ids.Count;


        //lock (eta_lock)
        //{
        //    var total_experiments = feature_combinations_list.Count * train_test_splits.Count * resampling_methods.Count * math_operations.Count * scaling_methods.Count * svm_types.Count * kernels.Count * outer_cv_folds;
        //    if (program.write_console_log) program.WriteLine($@"{nameof(Main)}: {nameof(total_experiments)}={total_experiments}");

        //    program.items_total += total_experiments;

        //}



        //temp.start_time = DateTime.Now;

        //var tasks = new List<Task>();
        //if (max_tasks < 0)
        //{
        //    max_tasks = Environment.ProcessorCount * Math.Abs(max_tasks) * 10;
        //}


        //
        // 1. make classifier with first item in all clusters
        // 2. iterate through each cluster to find better performance
        // 3. remove clusters with poor performance
        //
        //

        //var experiment_id1 = "";
        //var experiment_id2 = "";


    }
}
