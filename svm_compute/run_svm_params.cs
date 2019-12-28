using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace svm_compute
{

    public class run_svm_params
    {
        public int feature_count;
        public int group_count;
        public int outer_cv_random_seed;
        public int random_skips;
        public bool return_predictions;
        public bool return_performance;
        public bool return_meta_data;
        public bool return_roc_xy;
        public bool return_pr_xy;
        public bool output_threshold_adjustment_performance;
        public List<(int class_id, string class_name)> class_names;
        public List<libsvm_caller.kernel_parameter_search_method> kernel_parameter_search_methods;
        public List<libsvm_caller.libsvm_svm_type> svm_types;
        public List<libsvm_caller.libsvm_kernel_type> kernels;
        public List<cross_validation.scaling_method> scaling_methods;
        public List<cross_validation.test_prediction_method> prediction_methods;
        public List<cross_validation.resampling_method> training_resampling_methods;
        public List<example_instance> example_instance_list;
        public List<(int class_id, double weight)> weights;
        public int randomisation_cv_folds;
        public int outer_cv_folds;
        public int outer_cv_folds_to_skip;
        public int inner_cv_folds;
        public List<int> cross_validation_metrics_class_list;
        public performance_measure.confusion_matrix.cross_validation_metrics cross_validation_metrics;

        public double? cost;
        public double? gamma;
        public double? epsilon;
        public double? coef0;
        public double? degree;

        [JsonIgnore]
        public bool run_remote;

        [JsonIgnore]
        private object property_lock = new object();

        public void set_defaults()
        {
            lock (property_lock)
            {
                feature_count = 0;
                group_count = 0;
                run_remote = false;
                outer_cv_random_seed = 1;
                random_skips = 0;
                return_predictions = false;
                return_performance = true;
                return_meta_data = false;
                return_roc_xy = false;
                return_pr_xy = false;
                //delete_model_file = true;
                //delete_test_file = true;
                //delete_predict_file = true;
                //delete_training_file = true;
                output_threshold_adjustment_performance = false;
                class_names = null;
                kernel_parameter_search_methods = new List<libsvm_caller.kernel_parameter_search_method>() { libsvm_caller.kernel_parameter_search_method.grid_internal };
                svm_types = new List<libsvm_caller.libsvm_svm_type>() { libsvm_caller.libsvm_svm_type.c_svc };
                kernels = new List<libsvm_caller.libsvm_kernel_type>() { libsvm_caller.libsvm_kernel_type.rbf };
                scaling_methods = new List<cross_validation.scaling_method>() { cross_validation.scaling_method.scale_zero_to_plus_one };
                prediction_methods = new List<cross_validation.test_prediction_method>() { cross_validation.test_prediction_method.test_set_natural_imbalanced };
                training_resampling_methods = new List<cross_validation.resampling_method>() { cross_validation.resampling_method.downsample_with_full_dataset };
                example_instance_list = null; 
                weights = null;
                randomisation_cv_folds = 1;
                outer_cv_folds = 5;
                outer_cv_folds_to_skip = 0;
                inner_cv_folds = 5;
                cross_validation_metrics_class_list = null;
                cross_validation_metrics = performance_measure.confusion_matrix.cross_validation_metrics.ACC;
                //max_tasks = null;
            }
        }

        public void print()
        {
            lock (property_lock)
            {
                //if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(use_modded_libsvm)} = {this?.use_modded_libsvm}");

                if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(feature_count)} = {this?.feature_count}");
                if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(group_count)} = {this?.group_count}");

                if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(run_remote)} = {this?.run_remote}");
                if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(outer_cv_random_seed)} = {this?.outer_cv_random_seed}");
                if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(random_skips)} = {this?.random_skips}");
                if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(return_predictions)} = {this?.return_predictions}");
                if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(return_performance)} = {this?.return_performance}");
                if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(return_meta_data)} = {this?.return_meta_data}");

                if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(return_roc_xy)} = {this?.return_roc_xy}");
                if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(return_pr_xy)} = {this?.return_pr_xy}");
                //if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(delete_model_file)} = {this?.delete_model_file}");
                //if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(delete_test_file)} = {this?.delete_test_file}");
                //if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(delete_predict_file)} = {this?.delete_predict_file}");
                //if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(delete_training_file)} = {this?.delete_training_file}");

                //if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(experiment_id1)} = {this?.experiment_id1}");
                //if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(experiment_id2)} = {this?.experiment_id2}");
                //if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(experiment_id3)} = {this?.experiment_id3}");

                //if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(feature_selection_type)} = {this?.feature_selection_type}");
                //if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(perf_selection_rule)} = {this?.perf_selection_rule}");
                if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(class_names)} = {string.Join(", ", this?.class_names ?? new List<(int class_id, string class_name)>())}");
                if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(kernel_parameter_search_methods)} = {string.Join(", ", this?.kernel_parameter_search_methods ?? new List<libsvm_caller.kernel_parameter_search_method>())}");
                if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(svm_types)} = {string.Join(", ", this?.svm_types ?? new List<libsvm_caller.libsvm_svm_type>())}");
                if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(kernels)} = {string.Join(", ", this?.kernels ?? new List<libsvm_caller.libsvm_kernel_type>())}");
                //if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(math_operations)} = {string.Join(", ", this?.math_operations ?? new List<math_operation>())}");
                if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(scaling_methods)} = {string.Join(", ", this?.scaling_methods ?? new List<cross_validation.scaling_method>())}");
                if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(prediction_methods)} = {string.Join(", ", this?.prediction_methods ?? new List<cross_validation.test_prediction_method>())}");
                //if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(example_instance_list)} = {string.Join(", ", example_instance_list?.SelectMany(a=>a.comment_columns.Select(b=>b.comment_header+"->"+b.comment_value).ToList()).ToList())}");

                if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(example_instance_list)} = {this?.example_instance_list?.GroupBy(a => a.class_id()).Select(b => (b.Key, b.Count())).ToList() ?? new List<(int Key, int)>()}");

                if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(weights)} = {string.Join(", ", weights == null ? new List<string>() : this?.weights?.Select(a => $"[{a.class_id} -> {a.weight}]").ToList())}");
                if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(randomisation_cv_folds)} = {this?.randomisation_cv_folds}");
                if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(outer_cv_folds)} = {this?.outer_cv_folds}");
                if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(outer_cv_folds_to_skip)} = {this?.outer_cv_folds_to_skip}");
                if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(inner_cv_folds)} = {this?.inner_cv_folds}");
                if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(cross_validation_metrics_class_list)} = {string.Join(", ", cross_validation_metrics_class_list ?? new List<int>())}");
                if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(cross_validation_metrics)} = {this?.cross_validation_metrics}");
               // if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_params)}.{nameof(max_tasks)} = {this?.max_tasks}");
            }
        }

        public run_svm_params()
        {
            // set_defaults(); 
        }

        public run_svm_params(run_svm_params run_svm_remote_params)
        {
            if (run_svm_remote_params == null)
            {
                return;
            }

            set_defaults();
            lock (property_lock)
            {
                this.feature_count = run_svm_remote_params.feature_count;
                this.group_count = run_svm_remote_params.group_count;
                this.run_remote = run_svm_remote_params.run_remote;
                this.outer_cv_random_seed = run_svm_remote_params.outer_cv_random_seed;
                this.random_skips = run_svm_remote_params.random_skips;
                this.return_predictions = run_svm_remote_params.return_predictions;
                this.return_performance = run_svm_remote_params.return_performance;
                this.return_meta_data = run_svm_remote_params.return_meta_data;
                this.return_roc_xy = run_svm_remote_params.return_roc_xy;
                this.return_pr_xy = run_svm_remote_params.return_pr_xy;
                //this.delete_model_file = run_svm_remote_params.delete_model_file;
                //this.delete_predict_file = run_svm_remote_params.delete_predict_file;
                //this.delete_test_file = run_svm_remote_params.delete_test_file;
                //this.delete_training_file = run_svm_remote_params.delete_training_file;
                this.output_threshold_adjustment_performance = run_svm_remote_params.output_threshold_adjustment_performance;
                this.class_names = run_svm_remote_params.class_names?.ToList() ?? null;
                this.kernel_parameter_search_methods = run_svm_remote_params.kernel_parameter_search_methods?.ToList() ?? null;
                this.svm_types = run_svm_remote_params.svm_types?.ToList() ?? null;
                this.kernels = run_svm_remote_params.kernels?.ToList() ?? null;
                this.scaling_methods = run_svm_remote_params.scaling_methods?.ToList() ?? null;
                this.prediction_methods = run_svm_remote_params.prediction_methods?.ToList() ?? null;
                this.training_resampling_methods = run_svm_remote_params.training_resampling_methods?.ToList() ?? null;
                this.example_instance_list = run_svm_remote_params.example_instance_list?.ToList() ?? null;
                this.weights = run_svm_remote_params.weights?.ToList() ?? null;
                this.randomisation_cv_folds = run_svm_remote_params.randomisation_cv_folds;
                this.outer_cv_folds = run_svm_remote_params.outer_cv_folds;
                this.outer_cv_folds_to_skip = run_svm_remote_params.outer_cv_folds_to_skip;
                this.inner_cv_folds = run_svm_remote_params.inner_cv_folds;
                this.cross_validation_metrics_class_list = run_svm_remote_params.cross_validation_metrics_class_list?.ToList() ?? null;
                this.cross_validation_metrics = run_svm_remote_params.cross_validation_metrics;
             //   this.max_tasks = run_svm_remote_params.max_tasks;
            }
        }

        public static string serialise_json(run_svm_params run_svm_params)
        {
            lock (run_svm_params.property_lock)
            {
                var json_settings = new JsonSerializerSettings() { PreserveReferencesHandling = PreserveReferencesHandling.All, };
                var serialised_run_svm_params = JsonConvert.SerializeObject(run_svm_params, json_settings);
                return serialised_run_svm_params;
            }
        }

        public static run_svm_params deserialise(string serialized_json)
        {
            var json_settings = new JsonSerializerSettings() { PreserveReferencesHandling = PreserveReferencesHandling.All };
            run_svm_params deserialised_run_svm_params = null;

            try
            {
                deserialised_run_svm_params = JsonConvert.DeserializeObject<run_svm_params>(serialized_json, json_settings);

            }
            catch (Exception e)
            {
                deserialised_run_svm_params = null;


                program.WriteLineException(e, nameof(deserialise),"", true, ConsoleColor.Yellow);

            }

            return deserialised_run_svm_params;
        }
    }

}