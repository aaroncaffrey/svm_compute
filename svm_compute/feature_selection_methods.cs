using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using svm_compute;

namespace svm_compute
{
    public class feature_selection_methods
    {
        public static List<(int priority, string starter_features_name, bool leave_one_feature_out, string source, string group, string member, string perspective, List<int> selected_fid_list)> combine_feature_sets(List<(int fid, string source, string group, string member, string perspective)> dataset_headers, List<(string name, string source, List<(string name, string group, List<(string name, string member, List<(string name, string perspective)> list)> list)> list)> dataset_headers_grouped, List<(string starter_features_name, List<int> fid_list)> starter_features)
        {

            var print_params = true;

            if (print_params)
            {
                var param_list = new List<(string key, string value)>() {(nameof(dataset_headers), dataset_headers.ToString()), (nameof(dataset_headers_grouped), dataset_headers_grouped.ToString()), (nameof(starter_features), starter_features.ToString()),};

                if (program.write_console_log) program.WriteLine($@"{nameof(combine_feature_sets)}({string.Join(", ", param_list.Select(a => $"{a.key}=\"{a.value}\"").ToList())});");
            }


            var feature_combinations_list = new List<(int priority, string starter_features_name, bool leave_one_feature_out, string source, string group, string member, string perspective, List<int> selected_fid_list)>();

            //starter_features.ForEach(a => feature_combinations_list.Add((0, a.starter_features_name, false, "*","*","*","*", a.fid_list)));


            if (program.write_console_log) program.WriteLine($@"{nameof(combine_feature_sets)}: Finding feature sets...");

            //var patterns = new List<(string source, string group, string member, string perspective)>()
            //{
            //    (source:"*",group:"*",member:"*",perspective:"*"),
            //};

            var all_features_with_the_same_source = false;
            var all_features_with_the_same_LOO_source = false;
            var all_features_with_the_same_source_and_group = true; // this
            var all_features_with_the_same_source_and_LOO_group = false;
            var all_features_with_the_same_source_and_group_and_member = false;
            var all_features_with_the_same_source_and_group_and_LOO_member = false;
            var all_features_with_the_same_source_and_group_and_member_and_perspective = false;
            var all_features_with_the_same_source_and_group_and_member_and_LOO_perspective = false;
            var all_features_with_the_same_source_and_group_and_perspective = false; //true; // this
            var all_features_with_the_same_source_and_group_and_LOO_perspective = false; //true; // this

            //var matches = new List<string>() {"00000", "00001", "00010", "00011", "00100", "00101", "00110", "00111", "01000", "01001", "01010", "01011", "01100", "01101", "01110", "01111", "10000", "10001", "10010", "10011", "10100", "10101", "10110", "10111", "11000", "11001", "11010", "11011", "11100", "11101", "11110", "11111",};

            //var source_list = dataset_headers.Select(a => a.source).Distinct().ToList();
            //var group_list = dataset_headers.Select(a => a.group).Distinct().ToList();
            //var memeber_list = dataset_headers.Select(a => a.member).Distinct().ToList();
            //var perspective_list = dataset_headers.Select(a => a.perspective).Distinct().ToList();


            //foreach (var match in matches)
            //{
            //    var match_source = match[0] == '1';
            //    var match_group = match[1] == '1';
            //    var match_member = match[2] == '1';
            //    var match_perspective = match[3] == '1';
            //    var match_loo = match[4] == '1';


            //    var perspective_match_fid_list = dataset_headers.Where(a => a.source == source.source && a.@group == @group.@group && a.member == member.member && a.perspective == perspective.perspective).Select(a => (int)a.fid).ToList();

            //}

            foreach (var source in dataset_headers_grouped) // i.e. main
            {
                if (all_features_with_the_same_source)
                {
                    // add all fids in 'source'
                    var source_match_fid_list = dataset_headers.Where(a => a.source == source.source).Select(a => (int)a.fid).ToList();

                    starter_features.ForEach(a =>
                    {

                        var list = new List<int>();
                        list.AddRange(source_match_fid_list);
                        list.AddRange(a.fid_list);
                        feature_combinations_list.Add((1, a.starter_features_name, false, source.source, "*", "*", "*", list));
                    });

                    //if (source_fid_list.Count <= 1) continue;
                }

                if (all_features_with_the_same_LOO_source)
                {
                    // add all fids in 'source'
                    var source_loo_fid_list = dataset_headers.Where(a => a.source != source.source).Select(a => (int)a.fid).ToList();

                    starter_features.ForEach(a =>
                    {

                        var list = new List<int>();
                        list.AddRange(source_loo_fid_list);
                        list.AddRange(a.fid_list);
                        feature_combinations_list.Add((11, a.starter_features_name, true, source.source, "*", "*", "*", list));
                    });

                    //if (source_fid_list.Count <= 1) continue;
                }

                foreach (var group in source.list) // i.e. aa_distribution
                {
                    if (all_features_with_the_same_source_and_group)
                    {
                        // add all fids in 'source' and 'group'

                        var group_match_fid_list = dataset_headers.Where(a => a.source == source.source && a.@group == @group.@group).Select(a => (int)a.fid).ToList();

                        // add this level by itself
                        //feature_combinations_list.Add((@group.group, group_fid_list));

                        // add this level with all sets of starter features
                        starter_features.ForEach(a =>
                        {
                            var list = new List<int>();
                            list.AddRange(group_match_fid_list);
                            list.AddRange(a.fid_list);
                            feature_combinations_list.Add((2, a.starter_features_name, false, source.source, group.group, "*", "*", list));
                        });

                        //if (group_fid_list.Count <= 1) continue;
                    }

                    if (all_features_with_the_same_source_and_LOO_group)
                    {
                        // add all fids in 'source' and 'group'

                        var group_loo_fid_list = dataset_headers.Where(a => a.source == source.source && a.@group != @group.@group).Select(a => (int)a.fid).ToList();

                        // add this level by itself
                        //feature_combinations_list.Add((@group.name, group_fid_list));

                        // add this level with all sets of starter features
                        starter_features.ForEach(a =>
                        {
                            var list = new List<int>();
                            list.AddRange(group_loo_fid_list);
                            list.AddRange(a.fid_list);
                            feature_combinations_list.Add((12, a.starter_features_name, true, source.source, group.group, "*", "*", list));
                        });

                        //if (group_fid_list.Count <= 1) continue;
                    }

                    // add whole group with each perspective
                    var perspetives_in_group = group.list.SelectMany(member => member.list.Select(perspective => perspective.perspective).ToList()).Distinct().ToList();
                    foreach (var perspective in perspetives_in_group)
                    {
                        if (all_features_with_the_same_source_and_group_and_perspective)
                        {
                            var group_perspective_match_fid_list = dataset_headers.Where(a => a.source == source.source && a.@group == @group.@group && a.perspective == perspective).Select(a => (int)a.fid).ToList();

                            starter_features.ForEach(a =>
                            {
                                var list = new List<int>();
                                list.AddRange(group_perspective_match_fid_list);
                                list.AddRange(a.fid_list);
                                feature_combinations_list.Add((3, a.starter_features_name, false, /*$"{a.starter_features_name}-{@group.group}-{perspective}", */ source.source, group.group, "*", perspective, list));
                            });
                        }

                        if (all_features_with_the_same_source_and_group_and_LOO_perspective)
                        {
                            var group_perspective_loo_fid_list = dataset_headers.Where(a => a.source == source.source && a.@group == @group.@group && a.perspective != perspective).Select(a => (int)a.fid).ToList();

                            starter_features.ForEach(a =>
                            {
                                var list = new List<int>();
                                list.AddRange(group_perspective_loo_fid_list);
                                list.AddRange(a.fid_list);
                                feature_combinations_list.Add((13, a.starter_features_name, true, source.source, group.group, "*", perspective, list));
                            });
                        }
                    }

                    foreach (var member in @group.list) // i.e. A
                    {
                        if (all_features_with_the_same_source_and_group_and_member)
                        {
                            var member_match_fid_list = dataset_headers.Where(a => a.source == source.source && a.@group == @group.@group && a.member == member.member).Select(a => (int)a.fid).ToList();
                            //feature_combinations_list.Add((member.name, member_fid_list));

                            starter_features.ForEach(a =>
                            {
                                var list = new List<int>();
                                list.AddRange(member_match_fid_list);
                                list.AddRange(a.fid_list);
                                feature_combinations_list.Add((4, a.starter_features_name, false, source.source, group.group, member.member, "*", list));
                            });

                            //if (member_fid_list.Count <= 1) continue;
                        }

                        if (all_features_with_the_same_source_and_group_and_LOO_member)
                        {
                            var member_loo_fid_list = dataset_headers.Where(a => a.source == source.source && a.@group == @group.@group && a.member != member.member).Select(a => (int)a.fid).ToList();
                            //feature_combinations_list.Add((member.name, member_fid_list));

                            starter_features.ForEach(a =>
                            {
                                var list = new List<int>();
                                list.AddRange(member_loo_fid_list);
                                list.AddRange(a.fid_list);
                                feature_combinations_list.Add((14, a.starter_features_name, true, source.source, group.group, member.member, "*", list));
                            });

                            //if (member_fid_list.Count <= 1) continue;
                        }


                        foreach (var perspective in member.list) // i.e. Mean
                        {
                            if (all_features_with_the_same_source_and_group_and_member_and_perspective)
                            {
                                //if (!perspective_filter.Contains(perspective.perspective)) continue;

                                // add perspective by itself
                                var perspective_match_fid_list = dataset_headers.Where(a => a.source == source.source && a.@group == @group.@group && a.member == member.member && a.perspective == perspective.perspective).Select(a => (int)a.fid).ToList();
                                //feature_combinations_list.Add((perspective.name, perspective_fid_list));

                                starter_features.ForEach(a =>
                                {
                                    var list = new List<int>();
                                    list.AddRange(perspective_match_fid_list);
                                    list.AddRange(a.fid_list);
                                    feature_combinations_list.Add((5, a.starter_features_name, false, source.source, group.group, member.member, perspective.perspective, list));
                                });
                            }

                            if (all_features_with_the_same_source_and_group_and_member_and_LOO_perspective)
                            {
                                // add all perspectives except this itself
                                var perspective_loo_fid_list = dataset_headers.Where(a => a.source == source.source && a.@group == @group.@group && a.member == member.member && a.perspective != perspective.perspective).Select(a => (int)a.fid).ToList();
                                //feature_combinations_list.Add(($"except_{perspective.name}", perspective_fid_list2));

                                starter_features.ForEach(a =>
                                {
                                    var list = new List<int>();
                                    list.AddRange(perspective_loo_fid_list);
                                    list.AddRange(a.fid_list);
                                    feature_combinations_list.Add((15, a.starter_features_name, true, source.source, group.group, member.member, perspective.perspective, list));
                                });
                            }
                        }
                    }
                }


            }

            return feature_combinations_list;


        }

        private static string calc_individual_group_feature_set_performance(libsvm_caller.svm_implementation inner_cv_svm_implementation, libsvm_caller.svm_implementation outer_cv_svm_implementation, List<(string cluster_name, string dimension, string alphabet, string group_name, int num_features)> feature_group_clusters, string classifier_dimensions, List<(int fid, string source, string group, string member, string perspective)> dataset_headers, string[] remove_perspectives, int class_id_feature_index, List<List<int>> sequences_done, int min_overall_features, int max_overall_features, List<(List<(string comment_header, string comment_value)> comment_columns, List<(int fid, double fv)> feature_data)> dataset_instance_list, string experiment_id1, bool output_threshold_adjustment_performance, List<(int class_id, string class_name)> class_names, string save_filename_performance, string save_filename_predictions, List<libsvm_caller.kernel_parameter_search_method> kernel_parameter_search_methods, List<libsvm_caller.libsvm_svm_type> svm_types, List<libsvm_caller.libsvm_kernel_type> kernels, /*List<(double train, double unused, double test)>*/ /*train_test_splits,*/ List<cross_validation.math_operation> math_operations, List<cross_validation.scaling_method> scaling_methods, List<cross_validation.test_prediction_method> test_prediction_methods, List<cross_validation.resampling_method> resampling_methods, int randomisation_cv_folds, int outer_cv_folds, int inner_cv_folds, List<int> cross_validation_metrics_class_list, performance_measure.confusion_matrix.cross_validation_metrics cross_validation_metrics, int? fs_perf_class, List<IGrouping<string, (string cluster_name, string dimension, string alphabet, string group_name, int num_features)>> cluster_groups_1d_and_3d, string save_filename_cluster_performance)
        {
            if (program.write_console_log) program.WriteLine($@"{nameof(calc_individual_group_feature_set_performance)}()");

            var outer_cv_random_seed = 1;
            var random_skips = 0;

            //string experiment_id1="";
            //string experiment_id2="";
            //string experiment_id3="";
            foreach (var x in feature_group_clusters)
            {
                //experiment_id2 = x.group_name;

                if (program.write_console_log) program.WriteLine($@"Classifying " + x.group_name);

                // get the group names
                var selected_feature_group_names = new string[] { x.group_name };

                classifier_dimensions = x.dimension + "_" + "single_feature";

                // get all matching "group names" feature sets (including all perspectives)
                var selected_feature_headers = dataset_headers.Where(a => //source_feature_names.Contains(a.source) && 
                    selected_feature_group_names.Contains(a.@group)).ToList();

                // filter out unwanted perspectives
                selected_feature_headers = selected_feature_headers.Where(a => !remove_perspectives.Any(b => a.perspective.Contains(b))).ToList();

                // get fids
                var fids = selected_feature_headers.Select(a => a.fid).OrderBy(a => a).ToList();
                fids.Insert(0, class_id_feature_index);

                if (sequences_done.Any(a => a.SequenceEqual(fids))) continue;

                sequences_done.Add(fids);

                if (fids.Count < min_overall_features || fids.Count > max_overall_features)
                {
                    if (program.write_console_log) program.WriteLine($@"Too many overall features, skipping.");
                    continue;
                }

                // copy the instances with only the fids selected
                var feature_limited_example_instance_list = dataset_instance_list.Select((instance, row_index) => new example_instance(row_index, fids.Select(fid => (fid: fid, fv: instance.feature_data.Count > fid && instance.feature_data[fid].fid == fid ? instance.feature_data[fid].fv : instance.feature_data.First(a => a.fid == fid).fv)).ToList(), instance.comment_columns)).ToList();


                var run_svm_remote_params = new run_svm_params()
                {
                    outer_cv_random_seed = outer_cv_random_seed,
                    random_skips = random_skips,
                    //experiment_id1 = experiment_id1,
                    //experiment_id2 = experiment_id2,
                    //experiment_id3 = experiment_id3,
                    

                    output_threshold_adjustment_performance = output_threshold_adjustment_performance,
                    class_names = class_names,
                    kernel_parameter_search_methods = kernel_parameter_search_methods,
                    svm_types = svm_types,
                    kernels = kernels,

                    //math_operations = math_operations,
                    scaling_methods = scaling_methods,
                    prediction_methods = test_prediction_methods,
                    training_resampling_methods = resampling_methods,
                    example_instance_list = feature_limited_example_instance_list,
                    randomisation_cv_folds = randomisation_cv_folds,
                    outer_cv_folds = outer_cv_folds,
                    inner_cv_folds = inner_cv_folds,
                    cross_validation_metrics_class_list = cross_validation_metrics_class_list,
                    cross_validation_metrics = cross_validation_metrics,
                    //feature_selection_type = feature_selection_unidirectional.feature_selection_types.none,
                    //perf_selection_rule = feature_selection_unidirectional.perf_selection_rules.none,
                    return_performance = true,
                    weights = null,
                    //max_tasks = -1,
                    return_predictions = true,
                    return_meta_data = true

                    //false,
                    //true,
                    //false,
                };

                // run svm
                run_svm_remote_params = new run_svm_params(run_svm_remote_params);

                var cts = new CancellationTokenSource();
                var cancellation_token = cts.Token;

                var run_svm_result = cross_validation.run_svm(run_svm_remote_params, cancellation_token, inner_cv_svm_implementation, outer_cv_svm_implementation);

                cross_validation.save_svm_return(save_filename_performance, save_filename_predictions, run_svm_result);

                var cms = new List<performance_measure.confusion_matrix>();

                foreach (var run_svm_result_group in run_svm_result.run_svm_return_data.GroupBy(a => a.testing_set_name))
                {
                    //var all_features_average_perf = performance_measure.confusion_matrix.Average(run_svm_result,);

                    var group_prediction_list = run_svm_result_group.SelectMany(a => a.prediction_list).ToList();
                    var group_cm_list = run_svm_result_group.SelectMany(a => a.confusion_matrices).ToList();

                    var group_perf = group_cm_list.Where(a => fs_perf_class == null || a.class_id == fs_perf_class.Value).Select(a => a).ToList();

                    cms.AddRange(group_perf);
                }

                var cms_average = performance_measure.confusion_matrix.Average1(cms);


                var selected_feature_group_names_str = string.Join(",", selected_feature_group_names) + (new string(',', cluster_groups_1d_and_3d.Count - 1));
                var output = new List<string>();

                foreach (var cm_average in cms_average)
                {
                    var y = classifier_dimensions + "," + selected_feature_group_names_str + "," + cm_average;

                    output.Add(y);
                }

                var append = true;
                if (append)
                {
                    program.AppendAllLines(save_filename_cluster_performance, output);
                }
                else
                {
                    program.WriteAllLines(save_filename_cluster_performance, output);
                }
            }

            return classifier_dimensions;
        }

        private static void run_feature_set
(
            libsvm_caller.svm_implementation inner_cv_svm_implementation,
            libsvm_caller.svm_implementation outer_cv_svm_implementation,
            bool return_predictions,
    bool return_performance,
    bool return_meta_data,
    bool output_threshold_adjustment_performance,
    List<(int class_id, string class_name)> class_names,
    int feature_set_index,
    int feature_set_count,
    List<libsvm_caller.kernel_parameter_search_method> kernel_parameter_search_methods,
    List<libsvm_caller.libsvm_svm_type> svm_types,
    List<libsvm_caller.libsvm_kernel_type> kernels,
    /*List<(double train, double unused, double test)>*/ /*train_test_splits,*/
    List<cross_validation.math_operation> math_operations,
    List<cross_validation.scaling_method> scaling_methods,
    List<cross_validation.test_prediction_method> test_prediction_methods,
    List<cross_validation.resampling_method> resampling_methods,
    List<(List<(string comment_header, string comment_value)> comment_columns, List<(int fid, double fv)> feature_data)> dataset_instance_list,
    List<(List<(string comment_header, string comment_value)> comment_columns, List<(int fid, double fv)> feature_data)> novel_dataset_instance_list,
    (int priority, string starter_features_name, bool leave_one_feature_out, string source, string group, string member, string perspective, List<int> selected_fid_list) fs,

    string save_filename_performance,
    string save_filename_predictions,
    List<(int class_id, double weight)> weights,
    int randomisation_cv_folds = 1,
    int outer_cv_folds = 5,
    int inner_cv_folds = 5,
    List<int> cross_validation_metrics_class_list = null,
    performance_measure.confusion_matrix.cross_validation_metrics cross_validation_metrics = performance_measure.confusion_matrix.cross_validation_metrics.ACC
    )
        {
            var print_params = true;

            if (print_params)
            {
                var param_list = new List<(string key, string value)>()
                {
                    (nameof(output_threshold_adjustment_performance), output_threshold_adjustment_performance.ToString()),
                    (nameof(class_names), class_names?.ToString()),
                    (nameof(feature_set_index), feature_set_index.ToString()),
                    (nameof(feature_set_count), feature_set_count.ToString()),
                    (nameof(kernel_parameter_search_methods), kernel_parameter_search_methods?.ToString()),
                    (nameof(svm_types), svm_types?.ToString()),
                    (nameof(kernels), kernels?.ToString()),
                    (nameof(math_operations), math_operations?.ToString()),
                    (nameof(scaling_methods), scaling_methods?.ToString()),
                    (nameof(test_prediction_methods), test_prediction_methods?.ToString()),
                    (nameof(resampling_methods), resampling_methods?.ToString()),
                    (nameof(dataset_instance_list), dataset_instance_list?.ToString()),
                    (nameof(novel_dataset_instance_list), novel_dataset_instance_list?.ToString()),
                    (nameof(fs), fs.ToString()),
                    (nameof(randomisation_cv_folds), randomisation_cv_folds.ToString()),
                    (nameof(outer_cv_folds), outer_cv_folds.ToString()),
                    (nameof(save_filename_performance), save_filename_performance),
                    (nameof(save_filename_predictions), save_filename_predictions),
                    (nameof(inner_cv_folds), inner_cv_folds.ToString()),
                    (nameof(cross_validation_metrics_class_list), cross_validation_metrics_class_list?.ToString()),
                    (nameof(cross_validation_metrics), cross_validation_metrics.ToString()),
                };

                if (program.write_console_log) program.WriteLine($@"{nameof(run_feature_set)}({string.Join(", ", param_list.Select(a => $"{a.key}=\"{a.value}\"").ToList())});");
            }

            var outer_cv_random_seed = 1;
            var random_skips = 0;


            //var experiment_id1 = "";
            //var experiment_id2 = "";
            //var experiment_id3 = "";
            
            //if (program.write_console_log) program.WriteLine($@"{nameof(Main)}: feature combination set (default: {fs.starter_features_name}) {(fs.leave_one_feature_out ? "LOFO" : "")} \"{fs.source}.{fs.group}.{fs.member}.{fs.perspective}\" (priority: {fs.priority}) {(feature_set_index + 1)} / {feature_set_count}");
            int? fs_perf_class = null; // null for overall average over all classes
            double? all_f1 = null;

            var fids = fs.selected_fid_list;

            var test_all_features = true;

            if (test_all_features)
            {
                var features = dataset_instance_list.Select((instance, row_index) => new example_instance(row_index, fids.Select(fid => (fid: fid, fv: instance.feature_data.Count > fid && instance.feature_data[fid].fid == fid ? instance.feature_data[fid].fv : instance.feature_data.First(a => a.fid == fid).fv)).ToList(), instance.comment_columns)).ToList();

                var prediction_data_features = novel_dataset_instance_list.Select((instance, row_index) => new example_instance(-row_index - 1, fids.Select(fid => (fid: fid, fv: instance.feature_data.Count > fid && instance.feature_data[fid].fid == fid ? instance.feature_data[fid].fv : instance.feature_data.First(a => a.fid == fid).fv)).ToList(), instance.comment_columns)).ToList();

                var run_svm_remote_params = new run_svm_params()
                {
                    outer_cv_random_seed = outer_cv_random_seed,
                    random_skips = random_skips,
                    return_predictions = return_predictions,
                    return_performance = return_performance,
                    return_meta_data = return_meta_data,
                    //experiment_id1 = experiment_id1,
                    //experiment_id2 = experiment_id2,
                    //experiment_id3 = experiment_id3,
                    
                    //feature_selection_type = feature_selection_unidirectional.feature_selection_types.none,
                    //perf_selection_rule = feature_selection_unidirectional.perf_selection_rules.none,
                    output_threshold_adjustment_performance = output_threshold_adjustment_performance,
                    class_names = class_names,
                    kernel_parameter_search_methods = kernel_parameter_search_methods,
                    svm_types = svm_types,
                    kernels = kernels,
                    //math_operations = math_operations,
                    scaling_methods = scaling_methods,
                    prediction_methods = test_prediction_methods,
                    weights = weights,
                    randomisation_cv_folds = randomisation_cv_folds,
                    outer_cv_folds = outer_cv_folds,
                    inner_cv_folds = inner_cv_folds,
                    cross_validation_metrics_class_list = cross_validation_metrics_class_list,
                    cross_validation_metrics = cross_validation_metrics,
                    training_resampling_methods = resampling_methods,
                    //max_tasks = -1,
                    example_instance_list = features,
                };

                run_svm_remote_params = new run_svm_params(run_svm_remote_params);

                var cts = new CancellationTokenSource();
                var run_svm_result = cross_validation.run_svm(run_svm_remote_params, cts.Token, inner_cv_svm_implementation, outer_cv_svm_implementation);
                //save_svm_return(save_filename_performance, save_filename_predictions, run_svm_result);

                var f1s = new List<double>();

                foreach (var group in run_svm_result.run_svm_return_data.GroupBy(a => a.testing_set_name))
                {
                    //var all_features_average_perf = performance_measure.confusion_matrix.Average(run_svm_result,);

                    var group_prediction_list = group.SelectMany(a => a.prediction_list).ToList();
                    var group_cm_list = group.SelectMany(a => a.confusion_matrices).ToList();

                    var group_f1 = group_cm_list.Where(a => fs_perf_class == null || a.class_id == fs_perf_class.Value).Select(a => a.F1S).Average();

                    f1s.Add(group_f1);
                }

                all_f1 = f1s.Average();
            }

            var backwards_feature_selection = false;

            var forwards_feature_selection = false;

            if (forwards_feature_selection && fids.Count > 2)
            {
                //FFS: fid tested: 66, highest perf 0.67981897978096, best_f1: 0.69734610559927, all_f1:
                //FFS: best number of features: 3(removed: 17): 0, 61, 67, 76
                var better_perf_found = false;

                var fs_nested_cv_repeats = 5;

                var fs_fids = new List<int>() { 0 };

                var best_f1 = 0d;

                do //for (var num_features = 1; num_features <= fids.Count; num_features++)
                {
                    better_perf_found = false;
                    var fs_cm = new List<(int included_fid, double f1)>();


                    //if (program.write_console_log) program.WriteLine($@"FFS: forwards feature selection on {fids.Count - 1} features: {string.Join(", ", fids)}");

                    foreach (var fid_to_include in fids)
                    {
                        if (fid_to_include == 0) continue;

                        if (fs_fids.Contains(fid_to_include)) continue;

                        //if (program.write_console_log) program.WriteLine($@"FFS: testing {fs_fids.Count - 1} features with add one feature in (fid: {fid_to_include}) - {(fs_fids.IndexOf(fid_to_include) + 1)} / {fs_fids.Count}");


                        fs_fids.Add(fid_to_include);
                        fs_fids = fs_fids.OrderBy(a => a).ToList();


                        var fs_features = dataset_instance_list.Select((instance, row_index) => new example_instance(row_index, fs_fids.Select(fid => (fid: fid, fv: instance.feature_data.Count > fid && instance.feature_data[fid].fid == fid ? instance.feature_data[fid].fv : instance.feature_data.First(a => a.fid == fid).fv)).ToList(), instance.comment_columns)).ToList();

                        var fs_prediction_data_features = novel_dataset_instance_list.Select((instance, row_index) => new example_instance(-row_index - 1, fs_fids.Select(fid => (fid: fid, fv: instance.feature_data.Count > fid && instance.feature_data[fid].fid == fid ? instance.feature_data[fid].fv : instance.feature_data.First(a => a.fid == fid).fv)).ToList(), instance.comment_columns)).ToList();

                        var fs_run_svm_remote_params = new run_svm_params()
                        {
                            outer_cv_random_seed = outer_cv_random_seed,
                            random_skips = random_skips,
                            return_predictions = return_predictions,
                            return_performance = return_performance,
                            return_meta_data = return_meta_data,
                            //experiment_id1 = experiment_id1,
                            //experiment_id2 = experiment_id2,
                            //experiment_id3 = experiment_id3,
                            
                            //feature_selection_type = feature_selection_unidirectional.feature_selection_types.none,
                            //perf_selection_rule = feature_selection_unidirectional.perf_selection_rules.none,
                            output_threshold_adjustment_performance = output_threshold_adjustment_performance,
                            class_names = class_names,
                            kernel_parameter_search_methods = kernel_parameter_search_methods,
                            svm_types = svm_types,
                            kernels = kernels,
                            /*train_test_splits,*/
                            //math_operations = math_operations,
                            scaling_methods = scaling_methods,
                            prediction_methods = test_prediction_methods,
                            training_resampling_methods = resampling_methods,
                            example_instance_list = fs_features,
                            //fs_prediction_data_features,
                            weights = weights,
                            randomisation_cv_folds = randomisation_cv_folds,

                            inner_cv_folds = inner_cv_folds,
                            cross_validation_metrics_class_list = cross_validation_metrics_class_list,
                            cross_validation_metrics = cross_validation_metrics,
                            outer_cv_folds = fs_nested_cv_repeats,
                            //max_tasks = -1
                        };

                        fs_run_svm_remote_params = new run_svm_params(fs_run_svm_remote_params);

                        var cts = new CancellationTokenSource();
                        var fs_run_svm_result = cross_validation.run_svm(fs_run_svm_remote_params,cts.Token, inner_cv_svm_implementation, outer_cv_svm_implementation);
                        //save_svm_return(save_filename_performance, save_filename_predictions, fs_run_svm_result);

                        var fs_f1s = new List<double>();

                        foreach (var group in fs_run_svm_result.run_svm_return_data.GroupBy(a => a.testing_set_name))
                        {
                            var group_prediction_list = group.SelectMany(a => a.prediction_list).ToList();
                            var group_cm_list = group.SelectMany(a => a.confusion_matrices).ToList();

                            var group_fs_f1 = group_cm_list.Where(a => fs_perf_class == null || a.class_id == fs_perf_class.Value).Select(a => a.F1S).Average();
                            fs_f1s.Add(group_fs_f1);

                        }

                        fs_cm.Add((fid_to_include, fs_f1s.Average()));

                        fs_fids.Remove(fid_to_include);
                    }


                    fs_cm = fs_cm.OrderByDescending(a => a.f1).ToList();
                    var highest_perf = fs_cm.First();
                    //if (program.write_console_log) program.WriteLine($@"FFS: fid tested: {highest_perf.included_fid}, highest perf {highest_perf.f1}, best_f1: {best_f1}, all_f1: {all_f1}");

                    if (highest_perf.f1 > best_f1)
                    {
                        //if (program.write_console_log) program.WriteLine($@"FFS: better performance found... adding feature {highest_perf.included_fid}");
                        better_perf_found = true;
                        fs_fids.Add(highest_perf.included_fid);
                        best_f1 = highest_perf.f1;
                    }


                } while (better_perf_found && fs_fids.Count < fids.Count);

                //if (program.write_console_log) program.WriteLine($@"FFS: best number of features: {fs_fids.Count - 1} (removed: {fids.Count - fs_fids.Count}): {string.Join(", ", fs_fids)}");

            }

            if (backwards_feature_selection && fids.Count > 2)
            {
                var fs_nested_cv_repeats = 5;

                var fs_fids = fs.selected_fid_list;

                var better_perf_found = false;

                var best_f1 = all_f1;

                do
                {
                    better_perf_found = false;
                    var fs_cm = new List<(int removed_fid, double f1)>();

                    //if (program.write_console_log) program.WriteLine($@"BFS: backwards feature selection on {fs_fids.Count - 1} features: {string.Join(", ", fs_fids)}");

                    foreach (var fid_to_remove in fs_fids)
                    {
                        if (fid_to_remove == 0) continue;

                        //if (program.write_console_log) program.WriteLine($@"BFS: testing {fs_fids.Count - 2} features with leave one feature out (fid: {fid_to_remove}) - {(fs_fids.IndexOf(fid_to_remove) + 1)} / {fs_fids.Count}");

                        var fs_features = dataset_instance_list.Select((instance, row_index) => new example_instance(row_index, fs_fids.Where(a => a != fid_to_remove).Select(fid => (fid: fid, fv: instance.feature_data.Count > fid && instance.feature_data[fid].fid == fid ? instance.feature_data[fid].fv : instance.feature_data.First(a => a.fid == fid).fv)).ToList(), instance.comment_columns)).ToList();

                        var fs_prediction_data_features = novel_dataset_instance_list.Select((instance, row_index) => new example_instance(-row_index - 1, fs_fids.Where(a => a != fid_to_remove).Select(fid => (fid: fid, fv: instance.feature_data.Count > fid && instance.feature_data[fid].fid == fid ? instance.feature_data[fid].fv : instance.feature_data.First(a => a.fid == fid).fv)).ToList(), instance.comment_columns)).ToList();

                        var fs_run_svm_remote_params = new run_svm_params()
                        {
                            outer_cv_random_seed = outer_cv_random_seed,
                            random_skips = random_skips,
                            return_predictions = return_predictions,
                            return_performance = return_performance,
                            return_meta_data = return_meta_data,
                            //experiment_id1 = experiment_id1,
                            //experiment_id2 = experiment_id2,
                            //experiment_id3 = experiment_id3,
                            


                            //feature_selection_type = feature_selection_unidirectional.feature_selection_types.none,
                            //perf_selection_rule = feature_selection_unidirectional.perf_selection_rules.none,
                            output_threshold_adjustment_performance = output_threshold_adjustment_performance,
                            class_names = class_names,
                            kernel_parameter_search_methods = kernel_parameter_search_methods,
                            svm_types = svm_types,
                            kernels = kernels,
                            /*train_test_splits,*/
                            //math_operations = math_operations,
                            scaling_methods = scaling_methods,
                            prediction_methods = test_prediction_methods,
                            training_resampling_methods = resampling_methods,
                            example_instance_list = fs_features,
                            //fs_prediction_data_features, 
                            weights = weights,
                            randomisation_cv_folds = randomisation_cv_folds,

                            inner_cv_folds = inner_cv_folds,
                            cross_validation_metrics_class_list = cross_validation_metrics_class_list,
                            cross_validation_metrics = cross_validation_metrics,
                            outer_cv_folds = fs_nested_cv_repeats,
                            //max_tasks = -1

                        };
                        fs_run_svm_remote_params = new run_svm_params(fs_run_svm_remote_params);

                        var cts = new CancellationTokenSource();

                        var fs_run_svm_result = cross_validation.run_svm(fs_run_svm_remote_params,cts.Token, inner_cv_svm_implementation, outer_cv_svm_implementation);
                        //save_svm_return(save_filename_performance, save_filename_predictions, fs_run_svm_result);

                        var fs_f1s = new List<double>();

                        foreach (var group in fs_run_svm_result.run_svm_return_data.GroupBy(a => a.testing_set_name))
                        {
                            var group_prediction_list = group.SelectMany(a => a.prediction_list).ToList();
                            var group_cm_list = group.SelectMany(a => a.confusion_matrices).ToList();

                            var group_fs_f1 = group_cm_list.Where(a => fs_perf_class == null || a.class_id == fs_perf_class.Value).Select(a => a.F1S).Average();
                            fs_f1s.Add(group_fs_f1);

                        }



                        fs_cm.Add((fid_to_remove, fs_f1s.Average()));
                    }

                    fs_cm = fs_cm.OrderByDescending(a => a.f1).ToList();

                    var highest_perf = fs_cm.First();
                    //if (program.write_console_log) program.WriteLine($@"BFS: fid tested: {highest_perf.removed_fid}, highest perf {highest_perf.f1}, best_f1: {best_f1}, all_f1: {all_f1}");


                    // first element is highest performance, so it has to be better than the best, otherwise feature selection failed
                    if (highest_perf.f1 > best_f1)
                    {
                        // if it is better with that feature removed, then permenently remove it, and repeat logic
                        //if (program.write_console_log) program.WriteLine($@"BFS: better performance found... removing feature {highest_perf.removed_fid}");
                        better_perf_found = true;
                        fs_fids.Remove(highest_perf.removed_fid);
                        best_f1 = highest_perf.f1;
                    }


                } while (better_perf_found && fs_fids.Count > 2);

                //if (program.write_console_log) program.WriteLine($@"BFS: best number of features: {fs_fids.Count - 1} (removed: {fids.Count - fs_fids.Count}): {string.Join(", ", fs_fids)}");

            }
        }


        public static void generate_random_feature_sets_calc_performance(libsvm_caller.svm_implementation inner_cv_svm_implementation, libsvm_caller.svm_implementation outer_cv_svm_implementation, string classifier_dimensions, List<IGrouping<string, (string cluster_name, string dimension, string alphabet, string group_name, int num_features)>> cluster_groups_1d_and_3d, List<(string cluster_name, string dimension, string alphabet, string group_name, int num_features)> selected_features1d3d, List<(int fid, string source, string group, string member, string perspective)> dataset_headers, string[] remove_perspectives, int class_id_feature_index, List<List<int>> sequences_done, int min_overall_features, int max_overall_features, List<(List<(string comment_header, string comment_value)> comment_columns, List<(int fid, double fv)> feature_data)> dataset_instance_list, string experiment_id1, bool output_threshold_adjustment_performance, List<(int class_id, string class_name)> class_names, string save_filename_performance, string save_filename_predictions, List<libsvm_caller.kernel_parameter_search_method> kernel_parameter_search_methods, List<libsvm_caller.libsvm_svm_type> svm_types, List<libsvm_caller.libsvm_kernel_type> kernels, /*List<(double train, double unused, double test)>*/ /*train_test_splits,*/ List<cross_validation.math_operation> math_operations, List<cross_validation.scaling_method> scaling_methods, List<cross_validation.test_prediction_method> test_prediction_methods, List<cross_validation.resampling_method> resampling_methods, int randomisation_cv_folds, int outer_cv_folds, int inner_cv_folds, List<int> cross_validation_metrics_class_list, performance_measure.confusion_matrix.cross_validation_metrics cross_validation_metrics, int? fs_perf_class, string save_filename_cluster_performance)
        {
            if (program.write_console_log) program.WriteLine($@"{nameof(generate_random_feature_sets_calc_performance)}()");

            var outer_cv_random_seed = 1;
            var random_skips = 0;


            //string experiment_id1 = "";
            //string experiment_id2 = "";
            //string experiment_id3 = "";

            var rand = new Random();

            var dimension = -1;

            if (program.write_console_log) program.WriteLine($@"Generating random classifiers.  Press ESC to stop.");
            do
            {
                dimension++; //0=1d, 1=3d, 2=1d3d
                if (dimension < 0 || dimension > 2) dimension = 0;

                if (dimension == 0) classifier_dimensions = "1";
                else if (dimension == 1) classifier_dimensions = "3";
                else if (dimension == 2) classifier_dimensions = "1d3d";


                if (program.write_console_log) program.WriteLine($@"Next");
                // select a random item (or empty) for each cluster
                for (var cluster_index = 0; cluster_index < cluster_groups_1d_and_3d.Count; cluster_index++)
                {
                    var cluster_group = cluster_groups_1d_and_3d[cluster_index].ToList();


                    var group_index = rand.Next(-1, cluster_group.Count - 1);

                    if (dimension == 0 && cluster_group.Any(a => a.dimension != "1")) group_index = -1;
                    if (dimension == 1 && cluster_group.Any(a => a.dimension != "3")) group_index = -1;


                    if (group_index == -1)
                    {
                        selected_features1d3d[cluster_index] = ("", "", "", "", 0);
                        continue;
                    }

                    var cluster_item = cluster_group[group_index];
                    selected_features1d3d[cluster_index] = cluster_item;


                    //if (cluster_item.num_features > max_individual_group_features)
                    //{
                    //cluster_index--;
                    //}
                }


                // get the group names
                var selected_feature_group_names = selected_features1d3d.Select(a => a.group_name).ToList();

                // get all matching "group names" feature sets (including all perspectives)
                var selected_feature_headers = dataset_headers.Where(a => //source_feature_names.Contains(a.source) && 
                    selected_feature_group_names.Contains(a.@group)).ToList();

                // filter out unwanted perspectives
                selected_feature_headers = selected_feature_headers.Where(a => !remove_perspectives.Any(b => a.perspective.Contains(b))).ToList();

                // get fids
                var fids = selected_feature_headers.Select(a => a.fid).OrderBy(a => a).ToList();
                fids.Insert(0, class_id_feature_index);

                if (sequences_done.Any(a => a.SequenceEqual(fids))) continue;

                sequences_done.Add(fids);

                if (fids.Count < min_overall_features || fids.Count > max_overall_features)
                {
                    if (program.write_console_log) program.WriteLine($@"Too many overall features, skipping.");
                    continue;
                }

                // copy the instances with only the fids selected
                var feature_limited_example_instance_list = dataset_instance_list.Select((instance, row_index) => new example_instance(row_index, fids.Select(fid => (fid: fid, fv: instance.feature_data.Count > fid && instance.feature_data[fid].fid == fid ? instance.feature_data[fid].fv : instance.feature_data.First(a => a.fid == fid).fv)).ToList(), instance.comment_columns)).ToList();

                var selected_feature_group_names_str = string.Join(",", selected_feature_group_names);

                //experiment_id2 = selected_feature_group_names_str.Replace(",", ";");

                // run svm params
                var run_svm_remote_params = new run_svm_params()
                {
                    outer_cv_random_seed = outer_cv_random_seed,
                    random_skips = random_skips,
                    //false, true, false,
                    //experiment_id1 = experiment_id1,
                    //experiment_id2 = experiment_id2,
                    //experiment_id3 = experiment_id3,
                    
                    //perf_selection_rule = feature_selection_unidirectional.perf_selection_rules.none,
                    output_threshold_adjustment_performance = output_threshold_adjustment_performance,
                    class_names = class_names,
                    kernel_parameter_search_methods = kernel_parameter_search_methods,
                    svm_types = svm_types,
                    kernels = kernels,
                    //math_operations = math_operations,
                    scaling_methods = scaling_methods,
                    prediction_methods = test_prediction_methods,
                    training_resampling_methods = resampling_methods,
                    example_instance_list = feature_limited_example_instance_list,
                    randomisation_cv_folds = randomisation_cv_folds,
                    outer_cv_folds = outer_cv_folds,
                    inner_cv_folds = inner_cv_folds,
                    cross_validation_metrics_class_list = cross_validation_metrics_class_list,
                    cross_validation_metrics = cross_validation_metrics,
                    //feature_selection_type = feature_selection_unidirectional.feature_selection_types.none,
                   // max_tasks = -1,
                    return_performance = true, // false?
                    weights = null,
                    return_predictions = true, // false?
                    return_meta_data = true, // false?


                };

                run_svm_remote_params = new run_svm_params(run_svm_remote_params);

                // run svm

                var cts = new CancellationTokenSource();
                var run_svm_result = cross_validation.run_svm(run_svm_remote_params,cts.Token, inner_cv_svm_implementation, outer_cv_svm_implementation);


                cross_validation.save_svm_return(save_filename_performance, save_filename_predictions, run_svm_result);

                var cms = new List<performance_measure.confusion_matrix>();

                foreach (var run_svm_result_group in run_svm_result.run_svm_return_data.GroupBy(a => a.testing_set_name))
                {
                    //var all_features_average_perf = performance_measure.confusion_matrix.Average(run_svm_result,);

                    var group_prediction_list = run_svm_result_group.SelectMany(a => a.prediction_list).ToList();
                    var group_cm_list = run_svm_result_group.SelectMany(a => a.confusion_matrices).ToList();

                    var group_perf = group_cm_list.Where(a => fs_perf_class == null || a.class_id == fs_perf_class.Value).Select(a => a).ToList();

                    cms.AddRange(group_perf);
                }

                var cms_average = performance_measure.confusion_matrix.Average1(cms);


                var output = new List<string>();

                foreach (var cm_average in cms_average)
                {
                    var x = classifier_dimensions + "," + selected_feature_group_names_str + "," + cm_average;

                    output.Add(x);
                }

                var append = true;
                if (append)
                {
                    program.AppendAllLines(save_filename_cluster_performance, output);
                }
                else
                {
                    program.WriteAllLines(save_filename_cluster_performance, output);
                }
                
            } while (!Console.KeyAvailable || Console.ReadKey(true).Key != ConsoleKey.Escape);
        }


        public static void calc_sequential_method_performance(libsvm_caller.svm_implementation inner_cv_svm_implementation, libsvm_caller.svm_implementation outer_cv_svm_implementation, List<IGrouping<string, (string cluster_name, string dimension, string alphabet, string group_name, int num_features)>> cluster_groups_1d_and_3d, int max_individual_group_features, List<(string cluster_name, string dimension, string alphabet, string group_name, int num_features)> selected_features1d3d, List<(int fid, string source, string group, string member, string perspective)> dataset_headers, string[] remove_perspectives, List<List<int>> sequences_done, int max_overall_features, List<(List<(string comment_header, string comment_value)> comment_columns, List<(int fid, double fv)> feature_data)> dataset_instance_list, string experiment_id1, bool output_threshold_adjustment_performance, List<(int class_id, string class_name)> class_names, string save_filename_performance, string save_filename_predictions, List<libsvm_caller.kernel_parameter_search_method> kernel_parameter_search_methods, List<libsvm_caller.libsvm_svm_type> svm_types, List<libsvm_caller.libsvm_kernel_type> kernels, /*List<(double train, double unused, double test)>*/ /*train_test_splits,*/ List<cross_validation.math_operation> math_operations, List<cross_validation.scaling_method> scaling_methods, List<cross_validation.test_prediction_method> test_prediction_methods, List<cross_validation.resampling_method> resampling_methods, int randomisation_cv_folds, int outer_cv_folds, int inner_cv_folds, List<int> cross_validation_metrics_class_list, performance_measure.confusion_matrix.cross_validation_metrics cross_validation_metrics, int? fs_perf_class, string save_filename_cluster_performance)
        {
            if (program.write_console_log) program.WriteLine($@"{nameof(calc_sequential_method_performance)}()");

            var outer_cv_random_seed = 1;
            var random_skips = 0;

            //string experiment_id1 = "";
            //string experiment_id2 = "";
            //string experiment_id3 = "";
            
            for (var cluster_index = 0; cluster_index < cluster_groups_1d_and_3d.Count; cluster_index++)
            {
                if (program.write_console_log) program.WriteLine(($"cluster_index={cluster_index}/{cluster_groups_1d_and_3d.Count}"));
                var cluster_group = cluster_groups_1d_and_3d[cluster_index].ToList();

                for (var group_index = 0; group_index < cluster_group.Count; group_index++)
                {
                    if (program.write_console_log) program.WriteLine(($"group_index={group_index}/{cluster_group.Count}"));


                    var cluster_item = cluster_group[group_index];

                    //var group_name = item.group;
                    var num_features = cluster_item.num_features;

                    if (num_features > max_individual_group_features)
                    {
                        if (program.write_console_log) program.WriteLine($@"Too many individual features, skipping.");
                        continue;
                    }

                    selected_features1d3d[cluster_index] = cluster_item;

                    var selected_feature_group_names = selected_features1d3d.Select(a => a.group_name).ToList();

                    // get all matching feature sets (including all perspectives)
                    var selected_feature_headers = dataset_headers.Where(a => //source_feature_names.Contains(a.source) && 
                        selected_feature_group_names.Contains(a.@group)).ToList();

                    // filter out unwanted perspectives
                    selected_feature_headers = selected_feature_headers.Where(a => !remove_perspectives.Any(b => a.perspective.Contains(b))).ToList();


                    // get fids
                    var fids = selected_feature_headers.Select(a => a.fid).OrderBy(a => a).ToList();

                    if (sequences_done.Any(a => a.SequenceEqual(fids))) continue;

                    sequences_done.Add(fids);

                    if (fids.Count > max_overall_features)
                    {
                        if (program.write_console_log) program.WriteLine($@"Too many overall features, skipping.");
                        continue;
                    }

                    // copy the instances with only the fids selected
                    var feature_limited_example_instance_list = dataset_instance_list.Select((instance, row_index) => new example_instance(row_index, fids.Select(fid => (fid: fid, fv: instance.feature_data.Count > fid && instance.feature_data[fid].fid == fid ? instance.feature_data[fid].fv : instance.feature_data.First(a => a.fid == fid).fv)).ToList(), instance.comment_columns)).ToList();

                    var selected_feature_group_names_str = string.Join(",", selected_feature_group_names);

                    //experiment_id2 = selected_feature_group_names_str.Replace(",", ";");

                    // run svm
                    var run_svm_remote_params = new run_svm_params()
                    {
                        outer_cv_random_seed = outer_cv_random_seed,
                        random_skips = random_skips,
                        //experiment_id1 = experiment_id1,
                        //experiment_id2 = experiment_id2,
                        //experiment_id3 = experiment_id3,
                        
                        //feature_selection_type = 0,

                        //perf_selection_rule = feature_selection_unidirectional.perf_selection_rules.none,
                        output_threshold_adjustment_performance = output_threshold_adjustment_performance,
                        class_names = class_names,
                        kernel_parameter_search_methods = kernel_parameter_search_methods,
                        svm_types = svm_types,
                        kernels = kernels,
                        //math_operations = math_operations,
                        scaling_methods = scaling_methods,
                        prediction_methods = test_prediction_methods,
                        training_resampling_methods = resampling_methods,
                        example_instance_list = feature_limited_example_instance_list,
                        randomisation_cv_folds = randomisation_cv_folds,
                        outer_cv_folds = outer_cv_folds,
                        inner_cv_folds = inner_cv_folds,
                        cross_validation_metrics_class_list = cross_validation_metrics_class_list,

                        cross_validation_metrics = cross_validation_metrics,
                        //max_tasks = -1,
                        weights = null,
                        return_meta_data = false, // false?true?
                        return_performance = true, // false?true?
                        return_predictions = false // false?true?
                    };

                    run_svm_remote_params = new run_svm_params(run_svm_remote_params);

                    var cts = new CancellationTokenSource();
                    var run_svm_result = cross_validation.run_svm(run_svm_remote_params,cts.Token, inner_cv_svm_implementation, outer_cv_svm_implementation);

                    cross_validation.save_svm_return(save_filename_performance, save_filename_predictions, run_svm_result);

                    var cms = new List<performance_measure.confusion_matrix>();

                    foreach (var run_svm_result_group in run_svm_result.run_svm_return_data.GroupBy(a => a.testing_set_name))
                    {
                        //var all_features_average_perf = performance_measure.confusion_matrix.Average(run_svm_result,);

                        var group_prediction_list = run_svm_result_group.SelectMany(a => a.prediction_list).ToList();
                        var group_cm_list = run_svm_result_group.SelectMany(a => a.confusion_matrices).ToList();

                        var group_perf = group_cm_list.Where(a => fs_perf_class == null || a.class_id == fs_perf_class.Value).Select(a => a).ToList();

                        cms.AddRange(group_perf);
                    }

                    var cms_average = performance_measure.confusion_matrix.Average1(cms);

                    //var selected_feature_group_names_str = string.Join(",", selected_feature_group_names);
                    var output = new List<string>();

                    foreach (var cm_average in cms_average)
                    {
                        var x = selected_feature_group_names_str + "," + cm_average;

                        output.Add(x);
                    }

                    var append = true;
                    if (append)
                    {
                        program.AppendAllLines(save_filename_cluster_performance, output);
                    }
                    else
                    {
                        program.WriteAllLines(save_filename_cluster_performance, output);
                    }
                    
                }
            }
        }

    }
}
