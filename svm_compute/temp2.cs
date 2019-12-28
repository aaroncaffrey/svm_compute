using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace svm_compute
{
    public  class temp2
    {
        //if (program.write_console_log) program.WriteLine($@"{nameof(feature_selection_unidirectional)}(...);");

        //var file_dt_timestamp = DateTime.Now.ToString($"yyMMdd_HHmmss_fff");
        //if (program.write_console_log) program.WriteLine($@"features_input = {features_input.Count}");
        //if (program.write_console_log) program.WriteLine($@"file_dt = {file_dt_timestamp}");
        //if (program.write_console_log) program.WriteLine($@"feature_selection_type = {feature_selection_type.ToString()}");
        //if (program.write_console_log) program.WriteLine($@"fs_selection_rule = {fs_perf_selection_rule.ToString()}");
        //if (program.write_console_log) program.WriteLine($@"max_features_to_add_per_iteration = {forwards_max_features_to_add_per_iteration.ToString()}");
        //if (program.write_console_log) program.WriteLine($@"max_features_to_remove_per_iteration = {backwards_max_features_to_remove_per_iteration.ToString()}");
        //if (program.write_console_log) program.WriteLine($@"max_tasks = {max_tasks.ToString()}");

        //var global_best_save_filename = Path.Combine($@"{program.root_folder}", $@"output", $@"fs_winners_all_time_{file_dt_timestamp}_{iteration_num}.csv");
        //var local_best_save_filename = Path.Combine($@"{program.root_folder}", $@"output", $@"fs_winners_iteration_{file_dt_timestamp}_{iteration_num}.csv");
        //var save_iteration_filename_performance = Path.Combine($@"{program.root_folder}", $@"output", $@"svm_iteration_performance_{file_dt_timestamp}_{iteration_num}.csv");
        //var save_iteration_filename_predictions = Path.Combine($@"{program.root_folder}", $@"output", $@"svm_iteration_predictions_{file_dt_timestamp}_{iteration_num}.csv");
        //var save_filename_best_performance_all = Path.Combine($@"{program.root_folder}", $@"output", $@"svm_winners_performance_{file_dt_timestamp}.csv");
        //var save_filename_best_predictions_all = Path.Combine($@"{program.root_folder}", $@"output", $@"svm_winners_predictions_{file_dt_timestamp}.csv");
        //var save_lock = new object();


        //var experiment_id1 = "FS_" + iteration_num;
        //var experiment_id2 = string.Join("|", l_feature_group.cluster_name, l_feature_group.alphabet, l_feature_group.neighbourhood_name, l_feature_group.group_name, l_feature_group.num_features);

        //var task_backwards_rank = classify(internal_features_selected_exclusive);
        //
        //var run_svm_remote_params = new run_svm_params()
        //{
        //    feature_selection_type = internal_feature_selection_type,
        //    perf_selection_rule = fs_perf_selection_rule,
        //    max_tasks = max_tasks,
        //    example_instance_list = null,
        //    training_resampling_methods = null,
        //    class_names = null,
        //    return_performance = true,
        //    weights = null,
        //    return_predictions = false,
        //    return_meta_data = false,
        //    cross_validation_metrics = cross_validation_metrics.Accuracy,
        //    outer_cv_folds = 2,
        //    cross_validation_metrics_class_list = null,
        //    experiment_id1 = experiment_id1,
        //    experiment_id2 = experiment_id2,
        //    inner_cv_folds = 2,
        //    kernel_parameter_search_methods = null,
        //    kernels = null,
        //    math_operations = null,
        //    outer_cv_random_seed = 1,
        //    output_threshold_adjustment_performance = false,
        //    prediction_methods = null,
        //    random_skips = 0,
        //    randomisation_cv_folds = 0,
        //    scaling_methods = null,
        //    svm_types = null,
        //};
        //
        //run_svm_remote_params = new run_svm_params(run_svm_remote_params);
        //
        //var run_svm_result = run_svm(run_svm_remote_params);


        ///------------------------ old -------------------
        ///

        //if (program.write_console_log) program.WriteLine($@"feature_group_clusters.Count = {feature_group_list.Count}");

        //var global_best_features_ffs_bfs_ranking_performance_task_result_list_lock = new object();
        //var global_best_features_ffs_bfs_ranking_performance_task_result_list = new List<ffs_bfs_ranking_performance_task_result>();

        //var selected_features_ffs_bfs_ranking_performance_task_result_list_lock = new object();
        //var selected_features_ffs_bfs_ranking_performance_task_result_list = new List<ffs_bfs_ranking_performance_task_result>();

        //ffs_bfs_ranking_performance_task_result last_best_ffs_bfs_ranking_performance_task_result = null;

        //    var selected_features_names = selected_features_ffs_bfs_ranking_performance_task_result_list.Select(a => (a.ffs_bfs_feature_id.neighbourhood_name, a.ffs_bfs_feature_id.group_name)).ToList();
        //foreach (var x_feature_group in feature_group_list)


        //          var selected_features_all_time = new List<(string neighbourhood, string group)>();


        //if (verbose) if (program.write_console_log) program.WriteLine($@"[i{l_iteration_num}] [t{l_task_id}] [en{experiment_id2}] [ei{experiment_id1}] [ci{l_feature_group_cluster_index}] Running...");
        //if (verbose) if (program.write_console_log) program.WriteLine($@"[i{l_iteration_num}] [t{l_task_id}] [en{experiment_id2}] [ei{experiment_id1}] [ci{l_feature_group_cluster_index}] Classifying for initial scores with all features enabled ({string.Join("; ", selected_features_all_time.Select(a => $"{a.neighbourhood}.{a.group}").ToList())})");
        //if (verbose) if (program.write_console_log) program.WriteLine($@"[i{l_iteration_num}] [t{l_task_id}] [en{experiment_id2}] [ei{experiment_id1}] [ci{l_feature_group_cluster_index}] Classifying with group ""{l_feature_group.group_name}"" {(direction == feature_selection_type.forwards ? "ADDED" : "")}{(direction == feature_selection_type.backwards ? "REMOVED" : "")} with selected features ({string.Join("; ", selected_features_ffs_bfs_ranking_performance_task_result_list.Select(a => $"{a.neighbourhood_name}.{a.group_name}").ToList())})");
        //if (verbose) if (program.write_console_log) program.WriteLine($@"[i{l_iteration_num}] [t{l_task_id}] [en{experiment_id2}] [ei{experiment_id1}] [ci{l_feature_group_cluster_index}] Task exiting.  Result = default.");
        //if (verbose) if (program.write_console_log) program.WriteLine($@"[i{iteration_num}] Finished: Score not improved by {(fs_algorithm_direction == feature_selection_type.forwards ? "ADDING" : "")}{(fs_algorithm_direction == feature_selection_type.backwards ? "REMOVING" : "")}{(fs_algorithm_direction == feature_selection_type.forwards ? "ADDING or REMOVING" : "")} features.");
        //if (verbose) if (program.write_console_log) program.WriteLine($@"[i{iteration_num}] Finished: No new feature set results to compare.");
        //if (verbose) if (program.write_console_log) program.WriteLine($@"[i{iteration_num}] Finished: Task.WaitAll()");
        //if (verbose) if (program.write_console_log) program.WriteLine($@"[i{l_iteration_num}] [t{l_task_id}] [en{experiment_id2}] [ei{experiment_id1}] [ci{l_feature_group_cluster_index}] Task exiting.  Result = {result}  ({result.direction}, group = {result.ffs_bfs_feature_id.group_name}, num_features_added = {result.num_features_added}, num_features_total = {result.num_features_total}, score = {result.score_overall}, score_ppf_added = {result.score_ppf_added}, score_ppf_overall = {result.score_ppf_overall}, score_improvement = {result.score_added}, score_ppf_improvement = {result.score_ppf_added_improvement}, score_ppf_overall= {result.score_ppf_overall_improvement}, this_algorithm = {result.duration_algorithm / 1000 / 60}m, this_iteration = {result.duration_iteration / 1000 / 60}m, this_task = {result.duration_task / 1000 / 60}m).");
        //if (verbose) if (program.write_console_log) program.WriteLine($@"[i{l_iteration_num}] [t{l_task_id}] [en{experiment_id2}] [ei{experiment_id1}] [ci{l_feature_group_cluster_index}] Task exiting.  Result = default.");
        //if (verbose) if (program.write_console_log) program.WriteLine($@"[i{iteration_num}] best_forward_features_rankings = {best_forward_features_rankings}, is_score_improved = {is_score_improved}");
        //if (verbose) if (program.write_console_log) program.WriteLine($@"[i{iteration_num}] score_improvement = {score_improvement}");
        //if (verbose) if (program.write_console_log) program.WriteLine($@"[i{iteration_num}] score_ppf_added_improvement = {score_ppf_added_improvement}");
        //if (verbose) if (program.write_console_log) program.WriteLine($@"[i{iteration_num}] score_ppf_overall_improvement = {score_ppf_overall_improvement}");
        //if (verbose) if (program.write_console_log) program.WriteLine($@"[i{iteration_best_ranked_features.iteration_num}] [t{iteration_best_ranked_features.task_id}] [en{iteration_best_ranked_features.experiment_id2}] [ei{iteration_best_ranked_features.experiment_id1}] Main Result = {iteration_best_ranked_features}  ({iteration_best_ranked_features.direction}, group = {iteration_best_ranked_features.group_name}, num_features_added = {iteration_best_ranked_features.num_features_added}, num_features_total = {iteration_best_ranked_features.num_features_total}, score = {iteration_best_ranked_features.score_overall}, score_ppf_added = {iteration_best_ranked_features.score_ppf_added}, score_ppf_overall = {iteration_best_ranked_features.score_ppf_overall}, score_improvement = {iteration_best_ranked_features.score_added}, score_ppf_improvement = {iteration_best_ranked_features.score_ppf_added_improvement}, score_ppf_overall= {iteration_best_ranked_features.score_ppf_overall_improvement}, this_algorithm = {iteration_best_ranked_features.duration_algorithm / 1000 / 60}m, this_iteration = {iteration_best_ranked_features.duration_iteration / 1000 / 60}m, this_task = {iteration_best_ranked_features.duration_task / 1000 / 60}m).");


        ////// get all matching "group names" feature sets (including all sources, members, & perspectives)
        ////var selected_feature_headers = dataset_headers.Where(a => selected_features_all_time.Contains((a.source, a.@group))).ToList();

        ////// filter out unwanted sources, groups, members, perspectives
        ////selected_feature_headers = selected_feature_headers.Where(a => !remove_sources.Any(b => a.source.ToUpperInvariant().Contains(b.ToUpperInvariant()))).ToList();
        ////selected_feature_headers = selected_feature_headers.Where(a => !remove_groups.Any(b => a.group.ToUpperInvariant().Contains(b.ToUpperInvariant()))).ToList();
        ////selected_feature_headers = selected_feature_headers.Where(a => !remove_members.Any(b => a.member.ToUpperInvariant().Contains(b.ToUpperInvariant()))).ToList();
        ////selected_feature_headers = selected_feature_headers.Where(a => !remove_perspectives.Any(b => a.perspective.ToUpperInvariant().Contains(b.ToUpperInvariant()))).ToList();

        ////// remove duplicates
        ////selected_feature_headers = selected_feature_headers.Distinct().ToList();


        ////// get fids
        ////var fids = selected_feature_headers.Select(a => a.fid).OrderBy(a => a).ToList();
        ////fids.Insert(0, class_id_feature_index);
        ////fids = fids.Distinct().ToList();

        ////var num_features_total = fids.Count - 1;

        ////var num_features_added = num_features_total - (last_best_ffs_bfs_ranking_performance_task_result != null ? last_best_ffs_bfs_ranking_performance_task_result.num_features_total : 0);

        ////if (num_features_total <= 0)// || (num_features_added <= 0 && direction == feature_selection_type.forwards) || (num_features_added >= 0 && direction == feature_selection_type.backwards))
        ////{
        ////    return default;
        ////}
        ///

        //// initialise run_svm_return to default value
        //var run_svm_result = new run_svm_return() { x = new List<(string testing_set_name, List<performance_measure.confusion_matrix> confusion_matrices, List<performance_measure.prediction> prediction_list, List<(string key, string value)> prediction_meta_data)>() };

        //// run svm
        //run_svm_result = run_svm_remote(outer_cv_random_seed, 0, false, true, false, experiment_id1, experiment_id2, fs_starting_features, direction, fs_algorithm_direction, fs_selection_rule, output_threshold_adjustment_performance, class_names, kernel_parameter_search_methods, svm_types, kernels, /*train_test_splits,*/ math_operations, scaling_methods, test_prediction_methods, resampling_methods, feature_limited_example_instance_list, null, randomisation_cv_folds, outer_cv_folds, inner_cv_folds, cross_validation_metrics_class_list, cross_validation_metrics, null);

        //// save raw run_svm results to file
        //lock (save_lock)
        //{// this should combine all results in the same file, so lock is required to avoid corruption
        //    save_svm_return(save_iteration_filename_performance, null/*save_iteration_filename_predictions*/, run_svm_result);
        //}



        //var best_data_header = string.Join(",", new string[]
        //{
        //                nameof(ffs_bfs_ranking_performance_task_result.iteration_num),
        //                nameof(ffs_bfs_ranking_performance_task_result.direction),
        //                nameof(ffs_bfs_ranking_performance_task_result.ffs_bfs_feature_id.neighbourhood_name),
        //                nameof(ffs_bfs_ranking_performance_task_result.ffs_bfs_feature_id.group_name),
        //                nameof(ffs_bfs_ranking_performance_task_result.num_features_added),
        //                nameof(ffs_bfs_ranking_performance_task_result.num_features_total),
        //                nameof(ffs_bfs_ranking_performance_task_result.score_overall),
        //                nameof(ffs_bfs_ranking_performance_task_result.score_added),
        //                nameof(ffs_bfs_ranking_performance_task_result.score_ppf_added),
        //                nameof(ffs_bfs_ranking_performance_task_result.score_ppf_overall),
        //                nameof(ffs_bfs_ranking_performance_task_result.score_ppf_added_improvement),
        //                nameof(ffs_bfs_ranking_performance_task_result.score_ppf_overall_improvement),
        //                nameof(ffs_bfs_ranking_performance_task_result.duration_algorithm),
        //                nameof(ffs_bfs_ranking_performance_task_result.duration_iteration),
        //                nameof(ffs_bfs_ranking_performance_task_result.duration_task),
        //});

        //var local_best_data = new List<string>();
        //local_best_data.Add(best_data_header);
        //local_best_data.AddRange(selected_features_ffs_bfs_ranking_performance_task_result_list.Select(a => string.Join(",", new string[]
        //{
        //                a.iteration_num.ToString(),
        //                a.direction.ToString(),
        //                a.ffs_bfs_feature_id.neighbourhood_name.ToString(),
        //                a.ffs_bfs_feature_id.group_name.ToString(),
        //                a.num_features_added.ToString(),
        //                a.num_features_total.ToString(),
        //                a.score_overall.ToString(),
        //                a.score_added.ToString(),
        //                a.score_ppf_added.ToString(),
        //                a.score_ppf_overall.ToString(),
        //                a.score_ppf_added_improvement.ToString(),
        //                a.score_ppf_overall_improvement.ToString(),
        //                a.duration_algorithm.ToString(),
        //                a.duration_iteration.ToString(),
        //                a.duration_task.ToString(),
        //})));

        //var global_best_data = new List<string>();
        //global_best_data.Add(best_data_header);
        //global_best_data.AddRange(global_best_feature_sets.Select(a => string.Join(",", new string[] { a.iteration_num.ToString(), a.direction.ToString(), a.ffs_bfs_feature_id.neighbourhood_name, a.ffs_bfs_feature_id.group_name, a.num_features_added.ToString(), a.num_features_total.ToString(), a.score_overall.ToString(), score_improvement.ToString(), a.score_ppf_added.ToString(), a.score_ppf_overall.ToString(), score_ppf_added_improvement.ToString(), score_ppf_overall_improvement.ToString(), })));

        //lock (save_lock)
        //{
        //    Directory.CreateDirectory(Path.GetDirectoryName(local_best_save_filename));
        //    File.WriteAllLines(local_best_save_filename, local_best_data);

        //    Directory.CreateDirectory(Path.GetDirectoryName(global_best_save_filename));
        //    File.WriteAllLines(global_best_save_filename, global_best_data);

        //    save_svm_return(save_filename_best_performance_all, null /*save_filename_best_predictions_all*/, iteration_best_ranked_features.run_svm_return);
        //}

        //last_best_ffs_bfs_ranking_performance_task_result = iteration_best_ranked_features;

        ////////////////////////////
        ///
        /// // copy the instances with only the fids selected
        //    var feature_limited_example_instance_list = dataset_instance_list.Select((instance, row_index) => new example_instance(row_index, fids.Select(fid => (fid: fid, fv: instance.feature_data.First(a => a.fid == fid).fv)).ToList(), instance.comment_columns)).ToList();



        //    // initialise run_svm_return to default value
        //    var run_svm_result = new run_svm_return() { x = new List<(string testing_set_name, List<performance_measure.confusion_matrix> confusion_matrices, List<performance_measure.prediction> prediction_list, List<(string key, string value)> prediction_meta_data)>() };

        //    // run svm
        //    run_svm_result = run_svm_remote(outer_cv_random_seed, 0, false, true, false, experiment_id1, experiment_id2, fs_starting_features, direction, fs_algorithm_direction, fs_selection_rule, output_threshold_adjustment_performance, class_names, kernel_parameter_search_methods, svm_types, kernels, /*train_test_splits,*/ math_operations, scaling_methods, test_prediction_methods, resampling_methods, feature_limited_example_instance_list, null, randomisation_cv_folds, outer_cv_folds, inner_cv_folds, cross_validation_metrics_class_list, cross_validation_metrics, null);

        //    // save raw run_svm results to file
        //    lock (save_lock)
        //    {// this should combine all results in the same file, so lock is required to avoid corruption
        //        save_svm_return(save_iteration_filename_performance, null/*save_iteration_filename_predictions*/, run_svm_result);
        //    }

        //    var performance_measure_confusion_matrices = new List<performance_measure.confusion_matrix>();

        //    for (var i = 0; i < performance_measure_confusion_matrices.Count; i++)
        //    {
        //        performance_measure_confusion_matrices[i].experiment_id1 = "FS_" + l_iteration_num + "";
        //        //cms_average[i].experiment_id2 = "" + iteration_num + "";
        //    }

        //    foreach (var run_svm_result_group in run_svm_result.x.GroupBy(a => a.testing_set_name))
        //    {
        //        var group_prediction_list = run_svm_result_group.Where(a => a.prediction_list != null).SelectMany(a => a.prediction_list).ToList();

        //        var group_cm_list = run_svm_result_group.Where(a => a.confusion_matrices != null).SelectMany(a => a.confusion_matrices).ToList();

        //        var group_perf = group_cm_list.Where(a => a != null && (fs_perf_class == null || a.class_id == fs_perf_class.Value)).Select(a => a).ToList();

        //        performance_measure_confusion_matrices.AddRange(group_perf);
        //    }

        //    var cms_average = performance_measure.confusion_matrix.Average(performance_measure_confusion_matrices);

        //    for (var i = 0; i < cms_average.Count; i++)
        //    {
        //        cms_average[i].experiment_id1 = "Average_" + l_iteration_num + "";
        //    }

        //    var score_overall = fix_double(cms_average.First(a => a.class_id == +1 && (a.prediction_threshold == null || a.prediction_threshold == -1)).F1S);
        //    var score_added = fix_double(score_overall - (last_best_ffs_bfs_ranking_performance_task_result?.score_overall ?? 0));

        //    var score_ppf_overall = fix_double(num_features_total == 0 ? 0d : (double)score_overall / (double)num_features_total);
        //    var score_ppf_added = fix_double(num_features_added == 0 ? 0d : (double)(score_overall - (last_best_ffs_bfs_ranking_performance_task_result?.score_overall ?? 0)) / (double)num_features_added); //num_features is larger, has best+additonal

        //    var score_ppf_overall_improvement = fix_double(score_ppf_overall - (last_best_ffs_bfs_ranking_performance_task_result?.score_ppf_overall ?? 0)); // does it make sense to do this?
        //    var score_ppf_added_improvement = fix_double(score_ppf_added - (last_best_ffs_bfs_ranking_performance_task_result?.score_ppf_added ?? 0)); // does it make sense to do this?



        //    var result = new ffs_bfs_ranking_performance_task_result()
        //    {
        //        iteration_num = l_iteration_num,
        //        direction = direction,

        //        ffs_bfs_feature_id = new ffs_bfs_feature_id()
        //        {
        //            neighbourhood_name = l_feature_group.neighbourhood_name,
        //            group_name = l_feature_group.group_name
        //        },

        //        num_features_added = num_features_added,
        //        num_features_total = num_features_total,

        //        score_overall = score_overall,
        //        score_ppf_added = score_ppf_added,
        //        score_ppf_overall = score_ppf_overall,

        //        score_ppf_added_improvement = score_ppf_added_improvement,
        //        score_added = score_added,
        //        score_ppf_overall_improvement = score_ppf_overall_improvement,

        //        run_svm_return = run_svm_result,
        //        average_cms = cms_average,

        //        duration_algorithm = duration_algorithm,
        //        duration_iteration = duration_iteration,
        //        duration_task = duration_task,
        //        task_id = l_task_id,
        //        experiment_id1 = experiment_id1,
        //        experiment_id2 = experiment_id2,
        //    };


        //    return result;
        //});

        ////////////////////////////////////////
        /// var forward_features_rankings = iteration_feature_rankings.Where(a => a.direction == feature_selection_type.forwards).ToList();
        //var backwards_features_rankings = iteration_feature_rankings.Where(a => a.direction == feature_selection_type.backwards).ToList();
        //var best_forward_features_rankings = new List<ffs_bfs_ranking_performance_task_result>();
        //var best_backwards_features_rankings = new List<ffs_bfs_ranking_performance_task_result>();

        //if (forward_features_rankings.Count > 0)
        //{
        //    // order by MCC_PPF - suitable for forward, initial backwards, ... what about backwards?
        //    if (fs_selection_rule == fs_perf_selection_rule.best_ppf)
        //    {
        //        forward_features_rankings = forward_features_rankings.OrderByDescending(a => a.score_ppf_added).ThenBy(a => a.num_features_added).ToList();
        //    }
        //    else if (fs_selection_rule == fs_perf_selection_rule.best_score)
        //    {
        //        forward_features_rankings = forward_features_rankings.OrderByDescending(a => a.score_overall).ThenByDescending(a => a.score_ppf_added).ThenBy(a => a.num_features_added).ToList();
        //    }
        //    else if (fs_selection_rule == fs_perf_selection_rule.best_average_score_ppf)
        //    {
        //        forward_features_rankings = forward_features_rankings.OrderByDescending(a => new double[] { a.score_overall, a.score_ppf_added }.Average()).ThenBy(a => a.num_features_added).ToList();
        //    }
        //    else if (fs_selection_rule == fs_perf_selection_rule.best_average_score_ppf_normalised)
        //    {

        //        var scores = forward_features_rankings.Select(a => a.score_overall).ToList();
        //        var score_added_ppfs = forward_features_rankings.Select(a => a.score_ppf_added).ToList();

        //        forward_features_rankings = forward_features_rankings.OrderByDescending(a => new double[] { example_instance.scale_value(a.score_overall, scores.Min(), scores.Max()), example_instance.scale_value(a.score_ppf_added, score_added_ppfs.Min(), score_added_ppfs.Max()), }.Average()).ThenBy(a => a.num_features_added).ToList();
        //    }

        //    // limit to cases where score >= best_score, then select by MCC
        //    best_forward_features_rankings = forward_features_rankings.Where(a => a.score_overall > (last_best_ffs_bfs_ranking_performance_task_result?.score_overall ?? 0)).Take(num_features_to_add_per_iteration).ToList();
        //}

        //if (backwards_features_rankings.Count > 0)
        //{
        //    if (fs_selection_rule == fs_perf_selection_rule.best_ppf)
        //    {
        //        best_backwards_features_rankings = best_backwards_features_rankings.OrderByDescending(a => a.score_ppf_added).ThenBy(a => a.num_features_added).ToList();
        //    }
        //    else if (fs_selection_rule == fs_perf_selection_rule.best_score)
        //    {
        //        best_backwards_features_rankings = best_backwards_features_rankings.OrderByDescending(a => a.score_overall).ThenByDescending(a => a.score_ppf_added).ThenBy(a => a.num_features_added).ToList();
        //    }
        //    else if (fs_selection_rule == fs_perf_selection_rule.best_average_score_ppf)
        //    {
        //        best_backwards_features_rankings = best_backwards_features_rankings.OrderByDescending(a => new double[] { a.score_overall, a.score_ppf_added }.Average()).ThenBy(a => a.num_features_added).ToList();
        //    }
        //    else if (fs_selection_rule == fs_perf_selection_rule.best_average_score_ppf_normalised)
        //    {

        //        var scores = best_backwards_features_rankings.Select(a => a.score_overall).ToList();
        //        var score_added_ppfs = best_backwards_features_rankings.Select(a => a.score_ppf_added).ToList();

        //        best_backwards_features_rankings = best_backwards_features_rankings.OrderByDescending(a => new double[] { example_instance.scale_value(a.score_overall, scores.Min(), scores.Max()), example_instance.scale_value(a.score_ppf_added, score_added_ppfs.Min(), score_added_ppfs.Max()), }.Average()).ThenBy(a => a.num_features_added).ToList();
        //    }

        //    best_backwards_features_rankings = backwards_features_rankings.Where(a => a.score_overall > (last_best_ffs_bfs_ranking_performance_task_result?.score_overall ?? 0)).Take(num_features_to_remove_per_iteration).ToList();
        //}

        //if (best_forward_features_rankings.Count > 0 || best_backwards_features_rankings.Count > 0)
        //{
        //    var total_features_added_to_selection = best_forward_features_rankings.Count;
        //    var is_score_improved = best_forward_features_rankings.Count > 0;

        //    if (is_score_improved)
        //    {
        //        var score_improvement = fix_double(iteration_best_ranked_features.score_overall - (last_best_ffs_bfs_ranking_performance_task_result?.score_overall ?? 0));
        //        var score_ppf_added_improvement = fix_double(iteration_best_ranked_features.score_ppf_added - (last_best_ffs_bfs_ranking_performance_task_result?.score_ppf_added ?? 0));
        //        var score_ppf_overall_improvement = fix_double(iteration_best_ranked_features.score_ppf_overall - (last_best_ffs_bfs_ranking_performance_task_result?.score_ppf_overall ?? 0));
        //    }
        //}


        //----------- old other ----------------


        //var perform_outer_cv_locally = false;
        //
        //if (perform_outer_cv_locally)
        //{
        //    throw new NotImplementedException("todo: randomisation_cv_folds");
        //
        //    var outer_cv_tasks = new List<Task<run_svm_return>>();
        //
        //    for (var outer_cv_fold_index = 0; outer_cv_fold_index < outer_cv_folds; outer_cv_fold_index++)
        //    {
        //        var random_skips = outer_cv_fold_index;
        //        var external_outer_cv_folds = 1;
        //
        //        var outer_cv_task = Task.Run(() =>
        //        {
        //            var task_run_svm_result = run_svm_remote(outer_cv_random_seed, random_skips, false, true, false, experiment_id1, experiment_id2, fs_starting, direction, fs_algorithm_direction, fs_selection_rule, output_threshold_adjustment_performance, class_names, kernel_parameter_search_methods, svm_types, kernels, /*train_test_splits,*/ math_operations, scaling_methods, test_prediction_methods, resampling_methods, feature_limited_example_instance_list, null, randomisation_cv_folds, external_outer_cv_folds, inner_cv_folds, cross_validation_metrics_class_list, cross_validation_metrics, null);
        //            return task_run_svm_result;
        //        });
        //        outer_cv_tasks.Add(outer_cv_task);
        //
        //    }
        //
        //    Task.WaitAll(outer_cv_tasks.ToArray<Task>());
        //    var run_svm_result_list = outer_cv_tasks.Select(a => a.Result).SelectMany(a => a.x).ToList();
        //
        //
        //    foreach (var r in run_svm_result_list.GroupBy(a => a.testing_set_name).ToList())
        //    {
        //        var x = r.ToList();
        //
        //        if (x == null || x.Count == 0) continue;
        //
        //        var testing_set_name = r.Key;
        //        var prediction_list = x.SelectMany(a => a.prediction_list != null ? a.prediction_list : new List<performance_measure.prediction>()).ToList();
        //        var confusion_matrices = x.SelectMany(a => a.confusion_matrices != null ? a.confusion_matrices : new List<performance_measure.confusion_matrix>()).ToList();
        //        var prediction_meta_data = x.SelectMany(a => a.prediction_meta_data != null ? a.prediction_meta_data : new List<(string key, string value)>()).ToList();
        //
        //        run_svm_result.x.Add((testing_set_name, confusion_matrices, prediction_list, prediction_meta_data));
        //    }
        //}
        //else
        //{
        //    run_svm_result = run_svm_remote(outer_cv_random_seed, 0, false, true, false, experiment_id1, experiment_id2, fs_starting, direction, fs_algorithm_direction, fs_selection_rule, output_threshold_adjustment_performance, class_names, kernel_parameter_search_methods, svm_types, kernels, /*train_test_splits,*/ math_operations, scaling_methods, test_prediction_methods, resampling_methods, feature_limited_example_instance_list, null, randomisation_cv_folds, outer_cv_folds, inner_cv_folds, cross_validation_metrics_class_list, cross_validation_metrics, null);
        //}

        //public enum feature_set_modification_rule
        //{
        //    add_top_ranked_ignore_score,
        //}

        //------------


        // start: -- tempoary code --
        //var f = select_features(data_for_prediction_row_values, required_fids);
        //var fe = example_instance.feature_list_encode(f, scaling_method.no_scaling, null);
        //Directory.CreateDirectory(Path.GetDirectoryName(@"{program.root_folder}\bioinf\larks.txt"));
        //File.WriteAllLines(@"{program.root_folder}\bioinf\larks.txt", fe);
        // end: -- temporary code --

        // -------------------

        //public class ffs_bfs_ranking_performance_task_result
        //{
        //    public int iteration_num;
        //    public feature_selection_type feature_selection_type;
        //    public bool go_forwards;
        //    public bool go_backwards;

        //    public ffs_bfs_feature_id ffs_bfs_feature_id = new ffs_bfs_feature_id();

        //    public int num_features_added;
        //    public int num_features_total;

        //    public double score_added;
        //    public double score_overall;

        //    public double score_ppf_added;
        //    public double score_ppf_overall;
        //    public run_svm_return run_svm_return;
        //    public List<performance_measure.confusion_matrix> average_cms;

        //    public double score_ppf_added_improvement;
        //    public double score_ppf_overall_improvement;

        //    public long duration_algorithm;
        //    public long duration_iteration;
        //    public long duration_task;

        //    public int task_id;

        //    public string experiment_id1;
        //    public string experiment_id2;
        //}

        //public class ffs_bfs_feature_id
        //{
        //    public string neighbourhood_name;
        //    public string group_name;
        //}



        //var classify_all_groups_individually = false;
        //var random_method = false;
        //var sequential_method = false;
        //var calculate_correlation_matrix = false;
        //var forward_selection = true;
        //var backwards_selection = false;

        //var min_overall_features = 1;
        //var max_overall_features = 132 * 2;
        //var max_individual_group_features = max_overall_features;


        //            if (limit_for_testing)
        //        {
        //            feature_group_clusters.shuffle();
        //            feature_group_clusters = feature_group_clusters.Take(10).ToList();
        //}

        //// after removing perspectives, num_features could be lower
        //// feature_group_clusters = feature_group_clusters.Where(a => a.num_features >= min_individual_group_features && a.num_features <= max_individual_group_features).ToList();


        //var cluster_groups_1d_and_3d = feature_group_clusters.GroupBy(a => a.cluster_name).ToList();
        ////var cluster_groups1d = feature_group_clusters.Where(a => a.dimension == "1").GroupBy(a => a.cluster_name).ToList();
        ////var cluster_groups3d = feature_group_clusters.Where(a => a.dimension == "3").GroupBy(a => a.cluster_name).ToList();

        ////var performance = new List<(List<string> feature_groups, double perf)>();


        //var selected_features1d3d = cluster_groups_1d_and_3d.Select(a => a.First()).ToList(); //new(int cluster_id, string feature_1d_or_3d, string group, string num_features)[cluster_groups.Count];
        //var sequences_done = new List<List<int>>();


        ////save_results(save_filename_cluster_performance, new List<string> { "data_dimensions" + "," + string.Join(",", cluster_groups_1d_and_3d.Select(a => $"c_{a.Key}").ToList()) + "," + performance_measure.confusion_matrix.csv_header }, false);

        //int? fs_perf_class = null; // null for overall average over all classes

        //        //var source_feature_names = new[] { "main" };


        //        //var classifier_dimensions = "";

        //        if (calculate_correlation_matrix)
        //        {
        //            //calc_correl(feature_group_clusters, dataset_headers, remove_sources, remove_groups, remove_members, remove_perspectives, dataset_instance_list, file_dt);
        //        }

        //        if (forward_selection)
        //        {
        //            calc_ffs_bfs_performance(feature_group_clusters, dataset_headers, remove_sources, remove_groups, remove_members, remove_perspectives, class_id_feature_index, dataset_instance_list, output_threshold_adjustment_performance, class_names, kernel_parameter_search_methods, svm_types, kernels, /*train_test_splits,*/ math_operations, scaling_methods, test_prediction_methods, resampling_methods, randomisation_cv_folds, outer_cv_folds, inner_cv_folds, cross_validation_metrics_class_list, cross_validation_metrics, fs_perf_class);
        //        }

        //        if (classify_all_groups_individually)
        //        {
        //            //classifier_dimensions = calc_individual_group_feature_set_performance(feature_group_clusters, classifier_dimensions, dataset_headers, remove_perspectives, class_id_feature_index, sequences_done, min_overall_features, max_overall_features, dataset_instance_list, experiment_id1, output_threshold_adjustment_performance, class_names, save_filename_performance, save_filename_predictions, kernel_parameter_search_methods, svm_types, kernels, /*train_test_splits,*/ math_operations, scaling_methods, test_prediction_methods, resampling_methods, outer_cv_folds, inner_cv_folds, cross_validation_metrics_class_list, cross_validation_metrics, fs_perf_class, cluster_groups_1d_and_3d, save_filename_cluster_performance);
        //        }

        //        if (random_method)
        //        {
        //            //generate_random_feature_sets_calc_performance(classifier_dimensions, cluster_groups_1d_and_3d, selected_features1d3d, dataset_headers, remove_perspectives, class_id_feature_index, sequences_done, min_overall_features, max_overall_features, dataset_instance_list, experiment_id1, output_threshold_adjustment_performance, class_names, save_filename_performance, save_filename_predictions, kernel_parameter_search_methods, svm_types, kernels, /*train_test_splits,*/ math_operations, scaling_methods, test_prediction_methods, resampling_methods, outer_cv_folds, inner_cv_folds, cross_validation_metrics_class_list, cross_validation_metrics, fs_perf_class, save_filename_cluster_performance);
        //        }

        //        if (sequential_method)
        //        {
        //            //calc_sequential_method_performance(cluster_groups_1d_and_3d, max_individual_group_features, selected_features1d3d, dataset_headers, remove_perspectives, sequences_done, max_overall_features, dataset_instance_list, experiment_id1, output_threshold_adjustment_performance, class_names, save_filename_performance, save_filename_predictions, kernel_parameter_search_methods, svm_types, kernels, /*train_test_splits,*/ math_operations, scaling_methods, test_prediction_methods, resampling_methods, outer_cv_folds, inner_cv_folds, cross_validation_metrics_class_list, cross_validation_metrics, fs_perf_class, save_filename_cluster_performance);
        //        }

        //        //Task.WaitAll(tasks.ToArray<Task>());
    }
}
