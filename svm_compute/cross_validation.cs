using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Accord.Math;
using Newtonsoft.Json;
using svm_compute;

namespace svm_compute
{
    public partial class cross_validation
    {


        public static run_svm_return run_svm(run_svm_params run_svm_remote_params, CancellationToken cancellation_token, libsvm_caller.svm_implementation inner_cv_svm_implementation, libsvm_caller.svm_implementation outer_cv_svm_implementation)
        {
            //var svm_implementation = libsvm_caller.svm_implementation.thundersvm_cpu;//.thundersvm_gpu;

            //var x = 100 / nested_n_folds;
            // n_fold = 1 = 100/0 test na tests
            // n_fold = 2 = 50/50 with 2 tests
            // n_fold = 3 = 67/33 with 3 tests
            // n_fold = 4 = 75/25 with 4 tests
            // n_fold = 5 = 80/20 with 5 tests
            // n_fold = 6 = 83/17 with 6 tests
            // n_fold = 7 = 86/14 with 7 tests

            // copy parameters to ensure no external modification is possible / copy to avoid accidental overwrites
            run_svm_remote_params = new run_svm_params(run_svm_remote_params);

            // start: load cache

            // serialise
            var svm_request_serialised_json = "";

            do
            {
                svm_request_serialised_json = run_svm_params.serialise_json(run_svm_remote_params);

                if (string.IsNullOrEmpty(svm_request_serialised_json))
                {
                    if (program.write_console_log) program.WriteLine($"{nameof(run_svm)}(): {nameof(svm_request_serialised_json)} is empty.", true, ConsoleColor.Red);

                    var delay = new TimeSpan(0, 0, 30);
                    Task.Delay(delay).Wait();
                }

            } while (string.IsNullOrEmpty(svm_request_serialised_json));

            var cache1 = cross_validation_remote.get_cached_response(svm_request_serialised_json);

            if (!string.IsNullOrWhiteSpace(cache1))
            {
                var r = cross_validation.run_svm_return.deserialise(cache1);

                if (r != null && r.run_svm_return_data != null && r.run_svm_return_data.Count > 0)
                {
                    return r;
                }
            }

            // end: load cache

            if (run_svm_remote_params.run_remote)
            {
                var result = cross_validation_remote.send_run_svm_request(run_svm_remote_params, cancellation_token);
                return result;
            }

            var print = false;
            if (print)
            {
                run_svm_remote_params.print();
            }


            if (run_svm_remote_params.example_instance_list == null || run_svm_remote_params.example_instance_list.Count == 0)
            {
                throw new Exception("Error: No data fit model to.");
            }

            

            var class_example_instance_list = run_svm_remote_params.example_instance_list.GroupBy(a => a.class_id()).Select(a => (class_id: a.Key, list: a.ToList())).ToList();
            //var class_sizes = run_svm_remote_params.example_instance_list.GroupBy(a => a.class_id()).Select(a => (class_id: a.Key, instances: a.Count(), features: a.First().feature_list.Count - 1)).OrderBy(a => a.class_id).ToList();
            //var smallest_class_length = class_example_instance_list.Min(a => a.list.Count);
            //var largest_class_length = class_example_instance_list.Max(a => a.list.Count);
            //var class_ids = class_example_instance_list.Select(a => a.class_id).ToList();
            //var cross_class_overlap = new List<(int class_id, int overlap)>();
            //var same_class_overlap = new List<(int class_id, int overlap)>();
            //cross_class_overlap = null;
            //same_class_overlap = null;
            var bal_train_tasks = new List<Task<List<run_svm_return_item>>>();

            var max_tasks = (int?)0;

            if (max_tasks == null || max_tasks == 0)
            {
                max_tasks = Environment.ProcessorCount * 10;
            }

            if (max_tasks < 0)
            {
                max_tasks = Environment.ProcessorCount * Math.Abs(max_tasks.Value) * 10;
            }



            // reset random seed here 
            var classes_randoms = class_example_instance_list.Select(a => new Random(run_svm_remote_params.outer_cv_random_seed)).ToList();

            // skip ahead to a future cv for distribution of cv across multiple instances
            for (var classes_index = 0; classes_index < class_example_instance_list.Count; classes_index++)
            {
                for (var random_skip_index = 0; random_skip_index < run_svm_remote_params.random_skips; random_skip_index++)
                {
                    class_example_instance_list[classes_index].list.shuffle(classes_randoms[classes_index]);
                }
            }

            // use 'randomisation_cv_folds' to randomise the dataset (must be done at least 1 time)...

            for (var randomisation_cv_index = 0; randomisation_cv_index <= run_svm_remote_params.randomisation_cv_folds; randomisation_cv_index++)
            {
                // support for when randomisation_cv_index = 0
                if (randomisation_cv_index > 0 & randomisation_cv_index == run_svm_remote_params.randomisation_cv_folds)
                {
                    continue;
                }

                if (run_svm_remote_params.randomisation_cv_folds != 0) // && randomisation_cv_index != 0)
                {
                    // randomise each class (with its own random instance)
                    for (var classes_index = 0; classes_index < class_example_instance_list.Count; classes_index++)
                    {
                        class_example_instance_list[classes_index].list.shuffle(classes_randoms[classes_index]);

                    }
                }

                // use 'outer_cv_folds' to split dataset into folds (i.e. 4/5 training, 1/5 testing)
                // plus: support for when outer_cv_folds_to_skip > 0
                for (var outer_cv_index = 0; outer_cv_index < (run_svm_remote_params.outer_cv_folds - run_svm_remote_params.outer_cv_folds_to_skip); outer_cv_index++)
                {

                    
                    if (program.write_console_log) program.WriteLine($@"{nameof(run_svm)}(): outer_cv_index={(outer_cv_index + 1)}/{run_svm_remote_params.outer_cv_folds}");
                    

                    var outer_cv_fold_size_pct1 = (double)1 / (double)run_svm_remote_params.outer_cv_folds;
                    var outer_cv_test_pct = 1d * outer_cv_fold_size_pct1;
                    var outer_cv_training_pct = 1d - outer_cv_test_pct;

                    var randomise_outer_cv = false;

                    if (randomise_outer_cv)
                    {
                        // randomise order
                        for (var classes_index = 0; classes_index < class_example_instance_list.Count; classes_index++)
                        {
                            class_example_instance_list[classes_index].list.shuffle(classes_randoms[classes_index]);

                        }
                    }
                    else
                    {
                        // rotate order
                        for (var classes_index = 0; classes_index < class_example_instance_list.Count; classes_index++)
                        {
                            var offset = (int)Math.Round(class_example_instance_list[classes_index].list.Count * outer_cv_test_pct);
                            class_example_instance_list[classes_index] = (class_example_instance_list[classes_index].class_id, class_example_instance_list[classes_index].list.RotateLeft(offset));

                        }
                    }



                    //foreach (var train_test_split in train_test_splits)
                    //{
                    
                    //if (program.write_console_log) program.WriteLine($@"{nameof(run_svm)}({token}): train_test_split={train_test_split}/{train_test_splits.Count}");
                    

                    foreach (var training_resampling_method in run_svm_remote_params.training_resampling_methods)
                    {
                        
                        //if (program.write_console_log) program.WriteLine($@"{nameof(run_svm)}({run_svm_remote_params.experiment_id1},{run_svm_remote_params.experiment_id2},{run_svm_remote_params.experiment_id3}): training_resampling_method={(training_resampling_method)}/{run_svm_remote_params.training_resampling_methods.Count}");
                        if (program.write_console_log) program.WriteLine($@"{nameof(run_svm)}(): training_resampling_method={(training_resampling_method)}/{run_svm_remote_params.training_resampling_methods.Count}");
                        

                        var l_class_example_instance_list = class_example_instance_list.Select(a => a.list.ToList()).ToList();
                        var are_classes_balanced = l_class_example_instance_list.Select(a => a.Count).Distinct().Count() == 1;

                        var downsample_training_data = false;
                        var upsample_training_data = false;

                        switch (training_resampling_method)
                        {
                            case cross_validation.resampling_method.do_not_resample:
                                break;

                            case cross_validation.resampling_method.downsample_with_full_dataset:
                                //run with imbalanced training dataset pool (e.g. make all training instances available for random selection)
                                downsample_training_data = true;
                                break;

                            case cross_validation.resampling_method.upsample_random_selection:
                                upsample_training_data = true;
                                break;

                            default:
                                throw new Exception("invalid imbalanced method");
                        }

                        // TODO: is test set scaled correctly?



                        // unbalanced: take (e.g. 20%) of all classes for testing -- this maintains the natural class composition balance
                        var test_set_imbalanced = l_class_example_instance_list.Select((a, i) => a.Take((int)Math.Round(a.Count * outer_cv_test_pct)).ToList()).ToList();

                        // balanced: take e.g. the lowest 65% value of all classes for testing
                        var test_set_downsample_balanced = test_set_imbalanced.Select(a => a.Take(test_set_imbalanced.Min(b => b.Count)).ToList()).ToList();



                        List<List<example_instance>> test_set_upsample_balanced = null;
                        if (run_svm_remote_params.prediction_methods.Contains(cross_validation.test_prediction_method.test_set_unnatural_upsample_balanced))
                        {
                            var test_upsample_random = new Random(1);
                            var max_class_testing_examples = test_set_imbalanced.Max(a => a.Count);

                            test_set_upsample_balanced = test_set_imbalanced.Select(a =>
                            {
                                var list = a.ToList();
                                while (list.Count < max_class_testing_examples)
                                {
                                    var copy_index = test_upsample_random.Next(0, a.Count - 1);
                                    list.Add(a[copy_index]);
                                }

                                return list;
                            }).ToList();
                        }


                        //var unused_set_imbalanced = l_class_example_instance_list.Select((a, i) => a.Except(test_set_imbalanced[i]).ToList()).ToList();

                        // take all non-test instances (e.g. 35%) for unbal training (this is not currently done, future use)
                        var training_set_imbalanced = l_class_example_instance_list.Select((a, i) => a.Except(test_set_imbalanced[i]).Take((int)Math.Round(a.Count * outer_cv_training_pct)).ToList()).ToList();

                        // get unbal scaling params (not class specific)
                        var training_set_imbalanced_scaling_params = example_instance.get_scaling_params(training_set_imbalanced.SelectMany(a => a).ToList());

                        // get balanced training params
                        var training_set_downsample_balanced = training_set_imbalanced.Select(a => a.Take(training_set_imbalanced.Min(b => b.Count)).ToList()).ToList();

                        var training_set_downsample_balanced_scaling_params = example_instance.get_scaling_params(training_set_downsample_balanced.SelectMany(a => a).ToList());




                        List<List<example_instance>> training_set_upsample_balanced = null;
                        List<(int fid, double fv_min, double fv_max)> training_set_upsample_balanced_scaling_params = null;
                        if (upsample_training_data)
                        {
                            // todo: upsample SMOTE


                            var train_upsample_random = new Random(1);
                            var max_class_training_examples = training_set_imbalanced.Max(a => a.Count);

                            training_set_upsample_balanced = training_set_imbalanced.Select(a =>
                            {
                                var list = a.ToList();
                                while (list.Count < max_class_training_examples)
                                {
                                    var copy_index = train_upsample_random.Next(0, a.Count - 1);
                                    list.Add(a[copy_index]);
                                }

                                return list;
                            }).ToList();

                            training_set_upsample_balanced_scaling_params = example_instance.get_scaling_params(training_set_upsample_balanced.SelectMany(a => a).ToList());
                        }

                        List<List<example_instance>> training_set;// = downsample ? training_set_downsample_balanced : training_set_imbalanced;
                        List<(int fid, double fv_min, double fv_max)> scaling_params;// = downsample ? training_set_downsample_balanced_scaling_params : training_set_imbalanced_scaling_params;

                        if (downsample_training_data)
                        {
                            training_set = training_set_downsample_balanced;
                            scaling_params = training_set_downsample_balanced_scaling_params;

                        }
                        else if (upsample_training_data)
                        {
                            training_set = training_set_upsample_balanced;
                            scaling_params = training_set_upsample_balanced_scaling_params;
                        }
                        else
                        {
                            training_set = training_set_imbalanced;
                            scaling_params = training_set_imbalanced_scaling_params;
                        }

                        //var bal_training_final_model_set = l_class_example_instance_list.Select(a => a.Take(smallest_class_length).ToList()).ToList();
                        //var final_scaling_params = example_instance.get_scaling_params(bal_training_final_model_set.SelectMany(a => a).ToList());

                        // train test overlap | class overlap


                        //var overlap_train_test = training_set_downsample_balanced.SelectMany(a => a).Count(a => test_set_imbalanced.SelectMany(b => b).Any(b => example_instance.compare_examples(a, b) >= (double)0.99));

                        foreach (var svm_type in run_svm_remote_params.svm_types)
                        {
                            
                            if (program.write_console_log) program.WriteLine($@"{nameof(run_svm)}(): svm_type={svm_type} / {run_svm_remote_params.svm_types.Count}");
                            

                            if (cancellation_token.IsCancellationRequested)
                            {
                                return null;
                            }

                            foreach (var kernel in run_svm_remote_params.kernels)
                            {
                                
                                if (program.write_console_log) program.WriteLine($@"{nameof(run_svm)}(): kernel={kernel} / {run_svm_remote_params.kernels.Count}");
                                

                                if (cancellation_token.IsCancellationRequested)
                                {
                                    return null;
                                }

                                foreach (var kernel_parameter_search_method in run_svm_remote_params.kernel_parameter_search_methods)
                                {
                                    
                                    if (program.write_console_log) program.WriteLine($@"{nameof(run_svm)}(): kernel_parameter_search_method={kernel_parameter_search_method} / {run_svm_remote_params.kernel_parameter_search_methods.Count}");
                                    

                                    if (cancellation_token.IsCancellationRequested)
                                    {
                                        return null;
                                    }

                                    //foreach (var math_operation in run_svm_remote_params.math_operations)
                                    //{
                                    //    
                                    //    if (program.write_console_log) program.WriteLine($@"{nameof(run_svm)}({run_svm_remote_params.experiment_id1},{run_svm_remote_params.experiment_id2},{run_svm_remote_params.experiment_id3}): math_operation={(math_operation)} / {run_svm_remote_params.math_operations.Count}");
                                    //    

                                    foreach (var scaling_method in run_svm_remote_params.scaling_methods)
                                    {
                                        
                                        if (program.write_console_log) program.WriteLine($@"{nameof(run_svm)}(): scaling_method={(scaling_method)} / {run_svm_remote_params.scaling_methods.Count}");
                                        

                                        if (cancellation_token.IsCancellationRequested)
                                        {
                                            return null;
                                        }

                                        var l_randomisation_cv_index = randomisation_cv_index;
                                        var l_outer_cv_index = outer_cv_index;
                                        //var l_train_test_split = train_test_split;
                                        var l_training_resampling_method = training_resampling_method;
                                        var l_svm_type = svm_type;
                                        var l_kernel = kernel;
                                        //var l_math_operation = math_operation;
                                        var l_scaling_method = scaling_method;
                                        var l_kernel_parameter_search_method = kernel_parameter_search_method;
                                        var l_class_sizes = l_class_example_instance_list.Select(a => (a.First().class_id(), a.Count)).Distinct().ToList();





                                        var task = Task.Run(() =>
                                        {
                                            if (cancellation_token.IsCancellationRequested)
                                            {
                                                return null;
                                            }
                                            //var task_result = new List<(string testing_set_name, List<performance_measure.confusion_matrix> confusion_matrices, List<performance_measure.prediction> prediction_list)>();

                                            //train

                                            var training_result = run_svm_train_subfunc(
                                                inner_cv_svm_implementation,
                                                outer_cv_svm_implementation, 
                                                program.inner_cv_probability_estimates,
                                                program.outer_cv_probability_estimates,
                                                program.inner_cv_shrinking_heuristics,
                                                program.outer_cv_shrinking_heuristics,
                                                cancellation_token, 
                                                nameof(training_set), 
                                                l_kernel_parameter_search_method,
                                                run_svm_remote_params.weights,
                                                l_svm_type,
                                                l_kernel, 
                                                l_training_resampling_method, 
                                                l_randomisation_cv_index,
                                                l_outer_cv_index, 
                                                l_scaling_method, 
                                                training_set, 
                                                scaling_params, 
                                                run_svm_remote_params.inner_cv_folds, 
                                                run_svm_remote_params.cross_validation_metrics_class_list,
                                                run_svm_remote_params.cross_validation_metrics,
                                                run_svm_remote_params.cost, run_svm_remote_params.gamma, run_svm_remote_params.epsilon, run_svm_remote_params.coef0, run_svm_remote_params.degree);

                                            //test
                                            var test_sets = new List<(string dataset_name, List<List<example_instance>> dataset)>();

                                            foreach (var prediction_method in run_svm_remote_params.prediction_methods)
                                            {
                                                // for testing on the training set
                                                if (prediction_method == cross_validation.test_prediction_method.training_set_unnatural_downsample_balanced)
                                                {
                                                    if (training_set_downsample_balanced != null && training_set_downsample_balanced.Count > 0)
                                                    {
                                                        test_sets.Add((nameof(cross_validation.test_prediction_method.training_set_unnatural_downsample_balanced), training_set_downsample_balanced));
                                                    }
                                                    else throw new Exception();
                                                }
                                                // for testing on the training set
                                                else if (prediction_method == cross_validation.test_prediction_method.training_set_unnatural_upsample_balanced)
                                                {
                                                    if (training_set_upsample_balanced != null && training_set_upsample_balanced.Count > 0)
                                                    {
                                                        test_sets.Add((nameof(cross_validation.test_prediction_method.training_set_unnatural_upsample_balanced), training_set_upsample_balanced));
                                                    }
                                                    else throw new Exception();
                                                }

                                                else if (prediction_method == cross_validation.test_prediction_method.test_set_unnatural_downsample_balanced)
                                                {
                                                    if (test_set_downsample_balanced != null && test_set_downsample_balanced.Count > 0)
                                                    {
                                                        test_sets.Add((nameof(cross_validation.test_prediction_method.test_set_unnatural_downsample_balanced), test_set_downsample_balanced));
                                                    }
                                                    else throw new Exception();
                                                }

                                                else if (prediction_method == cross_validation.test_prediction_method.test_set_unnatural_upsample_balanced)
                                                {
                                                    if (test_set_upsample_balanced != null && test_set_upsample_balanced.Count > 0)
                                                    {
                                                        test_sets.Add((nameof(cross_validation.test_prediction_method.test_set_unnatural_upsample_balanced), test_set_upsample_balanced));
                                                    }
                                                    else throw new Exception();
                                                }

                                                else if (prediction_method == cross_validation.test_prediction_method.test_set_natural_imbalanced)
                                                {
                                                    if (test_set_imbalanced != null && test_set_imbalanced.Count > 0)
                                                    {
                                                        test_sets.Add((nameof(cross_validation.test_prediction_method.test_set_natural_imbalanced), test_set_imbalanced));
                                                    }
                                                    else throw new Exception();
                                                }



                                                else //if (prediction_method == prediction_method.predict_final_model_training_downsample_balanced)
                                                {
                                                    // train & test on final data set with final scaling
                                                    throw new NotImplementedException(nameof(prediction_method)); //.predict_final_model_training_downsample_balanced));
                                                }
                                            }

                                            //update_eta(1, nameof(Main), $"{starter_features_name}.{source}.{group}.{member}.{perspective}");


                                            if (test_sets.Count > 0)
                                            {
                                                // predict using the model training on all ~ 4/5 of the training data
                                                var predict_results = run_svm_outer_cv_predict_subfunc1(
                                                    inner_cv_svm_implementation,
                                                    outer_cv_svm_implementation, 
                                                    program.inner_cv_probability_estimates,
                                                    program.outer_cv_probability_estimates,
                                                    cancellation_token,
                                                    training_result.libsvm_cv_perf,
                                                    run_svm_remote_params.feature_count,
                                                    run_svm_remote_params.group_count,
                                                    training_result.duration_grid_search,
                                                    training_result.duration_nm_search, 
                                                    training_result.duration_training,
                                                    run_svm_remote_params.output_threshold_adjustment_performance,
                                                    run_svm_remote_params.class_names, 
                                                    training_result.train_filename, 
                                                    training_result.train_comments_filename,
                                                    training_result.model_filename, 
                                                    nameof(training_set),
                                                    l_kernel_parameter_search_method, 
                                                    l_svm_type,
                                                    l_kernel, 
                                                    l_training_resampling_method,
                                                    training_result.cost,
                                                    training_result.gamma, 
                                                    training_result.coef0, 
                                                    training_result.degree,
                                                    training_result.cv_rate,
                                                    run_svm_remote_params.inner_cv_folds, 
                                                    l_randomisation_cv_index, 
                                                    l_outer_cv_index, 
                                                    run_svm_remote_params.randomisation_cv_folds, 
                                                    run_svm_remote_params.outer_cv_folds, 
                                                    l_scaling_method, 
                                                    l_class_sizes,
                                                    run_svm_remote_params.weights, 
                                                    training_set, 
                                                    test_sets, 
                                                    scaling_params);

                                                var predict_results2 = new List<run_svm_return_item>();

                                                // remove unrequired data from the returned result to save transmissison bandwidth / encoding/decoding cpu time
                                                for (var i = 0; i < predict_results.Count; i++)
                                                {
                                                    predict_results2.Add(new run_svm_return_item()
                                                    {
                                                        testing_set_name = predict_results[i].testing_set_name,
                                                        confusion_matrices = run_svm_remote_params.return_performance ? predict_results[i].confusion_matrices : null,
                                                        prediction_list = run_svm_remote_params.return_predictions ? predict_results[i].prediction_list : null,
                                                        prediction_meta_data = run_svm_remote_params.return_meta_data ? predict_results[i].prediction_meta_data : null
                                                    });

                                                    if (run_svm_remote_params.return_performance && predict_results2.Any(a => a.confusion_matrices != null))
                                                    {
                                                        if (!run_svm_remote_params.return_roc_xy)
                                                        {
                                                            predict_results2.ForEach(a => a.confusion_matrices.ForEach(b =>
                                                            {
                                                                b.roc_xy_str_all = "";
                                                                b.roc_xy_str_11p = "";
                                                            }));
                                                        }

                                                        if (!run_svm_remote_params.return_pr_xy)
                                                        {
                                                            predict_results2.ForEach(a => a.confusion_matrices.ForEach(b =>
                                                            {
                                                                b.pr_xy_str_all = "";
                                                                b.pr_xy_str_11p = "";
                                                                b.pri_xy_str_all = "";
                                                                b.pri_xy_str_11p = "";
                                                            }));
                                                        }
                                                    }
                                                }

                                                if (cancellation_token.IsCancellationRequested)
                                                {
                                                    return null;
                                                }

                                                return predict_results2;
                                            }

                                            return new List<run_svm_return_item>();
                                        }, cancellation_token);

                                        bal_train_tasks.Add(task);

                                        var incomplete_bal_train_tasks = bal_train_tasks.Where(a => !a.IsCompleted).ToList();

                                        while (max_tasks > 0 && incomplete_bal_train_tasks.Count >= max_tasks && !cancellation_token.IsCancellationRequested)
                                        {
                                            if (program.write_console_log) program.WriteLine($@"run_svm(): Task.WaitAny(incomplete_bal_train_tasks.ToArray<Task>(), cancellation_token);", true, ConsoleColor.Cyan);
                                            try
                                            {
                                                Task.WaitAny(incomplete_bal_train_tasks.ToArray<Task>(), cancellation_token);
                                            }
                                            catch (Exception e)
                                            {
                                                program.WriteLineException(e,nameof(run_svm),"", true, ConsoleColor.Red);
                                            }

                                            incomplete_bal_train_tasks = bal_train_tasks.Where(a => !a.IsCompleted).ToList();
                                        }

                                        var yield = false;


                                        if (yield)
                                        {
                                            try
                                            {
                                                if (!cancellation_token.IsCancellationRequested)
                                                {
                                                    var delay = new TimeSpan(0, 0, 0, 1);

                                                    if (program.write_console_log) program.WriteLine($@"run_svm(): Task.Delay({delay.ToString()}, cancellation_token).Wait(cancellation_token);", true, ConsoleColor.Red);
                                                    Task.Delay(delay, cancellation_token).Wait(cancellation_token);
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                program.WriteLineException(e, nameof(run_svm),"", true, ConsoleColor.DarkGray);

                                            }
                                        }
                                    }

                                }
                            }
                        }
                    }
                }
                //}
            }

            
            if (program.write_console_log) program.WriteLine($@"run_svm(): Task.WaitAll(bal_train_tasks.ToArray<Task>(), cancellation_token);", true, ConsoleColor.Cyan);

            try
            {
                Task.WaitAll(bal_train_tasks.ToArray<Task>(), cancellation_token);
            }
            catch (Exception e)
            {
                program.WriteLineException(e, nameof(run_svm));
            }

            var all_results = bal_train_tasks.SelectMany(a => a.Result).ToList();

            var svm_return = new run_svm_return() { run_svm_return_data = all_results };

            if (svm_return != null && svm_return.run_svm_return_data != null && svm_return.run_svm_return_data.Count > 0)
            {
                var svm_response_serialised_json = cross_validation.run_svm_return.serialise(svm_return);
                cross_validation_remote.cache_request_response(svm_request_serialised_json, svm_response_serialised_json);
            }

            if (cancellation_token.IsCancellationRequested)
            {
                return null;
            }

            return svm_return;
        }

        public static void save_svm_return(string save_filename_performance, string save_filename_predictions, run_svm_return svm_return)
        {
            if (svm_return == null || svm_return.run_svm_return_data == null || svm_return.run_svm_return_data.Count == 0)
            {
                if (program.write_console_log) program.WriteLine($@"save_svm_return(): Warning: empty svm_return parameter.");
                return;
            }

            if (!String.IsNullOrWhiteSpace(save_filename_performance))
            {
                var run_svm_return_data = svm_return.run_svm_return_data.SelectMany(a => a.confusion_matrices.Select(b => b.ToString()).ToList()).ToList();

                if (run_svm_return_data != null && run_svm_return_data.Count > 0)
                {
                    if (!File.Exists(save_filename_performance) || new FileInfo(save_filename_performance).Length == 0)
                    {
                        program.WriteAllLines(save_filename_performance, new List<string> { performance_measure.confusion_matrix.csv_header });
                    }

                    program.AppendAllLines(save_filename_performance, run_svm_return_data);
                }
            }

            if (!String.IsNullOrWhiteSpace(save_filename_predictions))
            {
                var run_svm_return_data = svm_return.run_svm_return_data.Select(a => (a.prediction_list, a.prediction_meta_data)).ToList();

                if (run_svm_return_data != null && run_svm_return_data.Count > 0)
                {
                    save_instance_specific_classification_probability_data(save_filename_predictions, run_svm_return_data);
                }
            }
        }


        public static void save_instance_specific_classification_probability_data(

            string save_filename_predictions,
            List<(List<performance_measure.prediction> prediction_list, List<(string key, string value)> prediction_meta_data)> prediction_data,
            bool append = true)
        {

            var print_params = true;

            if (print_params)
            {
                var param_list = new List<(string key, string value)>() {(nameof(save_filename_predictions), save_filename_predictions?.ToString()), (nameof(prediction_data), prediction_data?.ToString()), (nameof(append), append.ToString()),};

                if (program.write_console_log) program.WriteLine($@"{nameof(save_instance_specific_classification_probability_data)}({String.Join(", ", param_list.Select(a => $"{a.key}=\"{a.value}\"").ToList())});");
            }

            var prediction_list = prediction_data.SelectMany(a => a.prediction_list).ToList();
            var class_ids = prediction_list.SelectMany(a => a.probability_estimates.Select(b => b.class_id).ToList()).Distinct().OrderBy(a => a).ToList();

            var meta_keys = prediction_data.SelectMany(a => a.prediction_meta_data.Select(b => "param_" + b.key).ToList()).Distinct().ToList();

            var header = new List<string>
            {
                "prediction_index",
                "actual_class",
                "predicted_class",
                "correctness_score",
                "correctness_text",
                "confidence_actual",
                "confidence_predicted",
                "probability_actual_class",
                "probability_predicted_class"
            };

            header.InsertRange(0, meta_keys);

            header.AddRange(class_ids.Select(a => "confidence_class_" + (a > 0 ? "+" : "") + a).ToList());

            header.AddRange(class_ids.Select(a => "probability_class_" + (a > 0 ? "+" : "") + a).ToList());

            var data = new List<string>();

            for (var pd_index = 0; pd_index < prediction_data.Count; pd_index++)
            {
                var pd = prediction_data[pd_index];
                var predictions = pd.prediction_list;
                var meta = pd.prediction_meta_data;

                var meta_values = meta.Select(a => a.value).ToList();


                for (var ai = 0; ai < predictions.Count; ai++)
                {
                    var a = predictions[ai];

                    var prob_actual_class = a.probability_estimates.First(b => b.class_id == a.actual_class).probability_estimate;
                    var prob_predicted_class = a.probability_estimates.First(b => b.class_id == a.default_predicted_class).probability_estimate;

                    var line = new List<string>()
                    {
                        a.prediction_index.ToString(),
                        a.actual_class.ToString(),
                        a.default_predicted_class.ToString(),
                        (a.actual_class == a.default_predicted_class ? "1" : "0"),
                        (a.actual_class == a.default_predicted_class ? "Correct" : "Incorrect"),
                        ((prob_actual_class - 0.5) * 2).ToString(), // reality: how good the classifier really is
                        ((prob_predicted_class - 0.5) * 2).ToString(), // theory: how good the classifier 'thinks' it is
                        prob_actual_class.ToString(),
                        prob_predicted_class.ToString()
                    };

                    line.InsertRange(0, meta_values);

                    var comments = a.comment.Split(';').Select(b =>
                    {
                        var x = b.Split(',');
                        var z = (comment_header: "meta_" + x[0], comment_value: x[1]);

                        return z;
                    }).ToList();

                    if (pd_index == 0 && ai == 0)
                    {
                        header.InsertRange(0, comments.Select(b => b.comment_header).ToList());
                    }

                    line.InsertRange(0, comments.Select(b => b.comment_value).ToList());

                    foreach (var class_id in class_ids)
                    {
                        var class_confidence = (a.probability_estimates.First(b => b.class_id == class_id).probability_estimate - 0.5) * 2;

                        line.Add(class_confidence.ToString());
                    }

                    foreach (var class_id in class_ids)
                    {
                        var class_prob = a.probability_estimates.First(b => b.class_id == class_id).probability_estimate;

                        line.Add(class_prob.ToString());
                    }

                    var joined = String.Join(",", line);

                    data.Add(joined);
                }
            }

            if (append == false || !File.Exists(save_filename_predictions) || new FileInfo(save_filename_predictions).Length == 0)
            {
                data.Insert(0, String.Join(",", header));
            }

            if (append)
            {
                program.AppendAllLines(save_filename_predictions, data);
            }
            else
            {
                program.WriteAllLines(save_filename_predictions, data);
            }

        }


        public static (string duration_grid_search, string duration_nm_search, string duration_training, string train_filename, string train_comments_filename, string model_filename, string train_result, double? cost, double? gamma, double? epsilon, double? coef0, double? degree, double? cv_rate, libsvm_cv_perf libsvm_cv_perf) run_svm_train_subfunc(
            libsvm_caller.svm_implementation inner_cv_svm_implementation,
            libsvm_caller.svm_implementation outer_cv_svm_implementation,
            bool inner_cv_probability_estimates,
            bool outer_cv_probability_estimates,
            bool inner_cv_shrinking_heuristics,
            bool outer_cv_shrinking_heuristics,

            CancellationToken cancellation_token,

            string file_prefix,
            libsvm_caller.kernel_parameter_search_method kernel_parameter_search_method,
            List<(int class_id, double weight)> weights,

            libsvm_caller.libsvm_svm_type svm_type,
            libsvm_caller.libsvm_kernel_type kernel,
            cross_validation.resampling_method training_resampling_method,

            int randomisation_cv_index,
            int outer_cv_index,

            cross_validation.scaling_method scaling_method,
            List<List<example_instance>> training_set,
            List<(int fid, double min, double max)> training_scaling_params,
            int inner_cv_folds = 5,
            List<int> cross_validation_metric_class_list = null,
            performance_measure.confusion_matrix.cross_validation_metrics cross_validation_metrics = performance_measure.confusion_matrix.cross_validation_metrics.ACC,
            double? cost = null, double? gamma = null, double? epsilon = null, double? coef0 = null, double? degree = null
            )
        {
            if (cancellation_token.IsCancellationRequested)
            {
                return default;
            }

            var print_params = true;

            if (print_params)
            {
                var param_list = new List<(string key, string value)>()
                {
                    //(nameof(delete_training_file), delete_training_file.ToString()),
                    (nameof(file_prefix), file_prefix),
                    (nameof(kernel_parameter_search_method), kernel_parameter_search_method.ToString()),
                    (nameof(weights), weights?.ToString()),
                    (nameof(svm_type), svm_type.ToString()),
                    (nameof(kernel), kernel.ToString()),
                    (nameof(training_resampling_method), training_resampling_method.ToString()),
                    (nameof(randomisation_cv_index), randomisation_cv_index.ToString()),
                    (nameof(outer_cv_index), outer_cv_index.ToString()),
                    (nameof(scaling_method), scaling_method.ToString()),
                    (nameof(training_set), training_set?.ToString()),
                    (nameof(training_scaling_params), training_scaling_params?.ToString()),
                    (nameof(inner_cv_folds), inner_cv_folds.ToString()),
                    (nameof(cross_validation_metrics), cross_validation_metrics.ToString()),
                };

                if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_train_subfunc)}({String.Join(", ", param_list.Select(a => $"{a.key}=\"{a.value}\"").ToList())});");
            }


            var file_id = random_id_generator.get_unique_id();

            var inner_cv_folder = libsvm_caller.svm_implementation_folder(inner_cv_svm_implementation);
            var outer_cv_folder = libsvm_caller.svm_implementation_folder(outer_cv_svm_implementation);

            var is_temp_file = true;

            var inner_cv_filename_training = program.convert_path(Path.Combine(inner_cv_folder, $@"{file_id}.training"), is_temp_file);
            var outer_cv_filename_training = program.convert_path(Path.Combine(outer_cv_folder, $@"{file_id}.training"), is_temp_file);

            var outer_cv_filename_training_comments = program.convert_path(Path.Combine(outer_cv_folder, $@"{file_id}.training_comments"), is_temp_file);
            var outer_cv_filename_model = program.convert_path(Path.Combine(outer_cv_folder, $@"{file_id}.model"), is_temp_file);
            var training_data_list = example_instance.feature_list_encode(outer_cv_index, training_set.SelectMany(a => a).ToList(), scaling_method, scaling_method == cross_validation.scaling_method.no_scaling ? null : training_scaling_params);

            program.WriteAllLines(inner_cv_filename_training, training_data_list.data, is_temp_file);

            if (inner_cv_filename_training != outer_cv_filename_training)
            {
                program.WriteAllLines(outer_cv_filename_training, training_data_list.data, is_temp_file);
            }

            program.WriteAllLines(outer_cv_filename_training_comments, training_data_list.comments, is_temp_file);

            var echo = false;
            var echo_err = true;

            var point_max_time = program.point_max_time;
            var process_max_time = program.process_max_time;
            var failsafe_search = true;
            var failsafe_searches_left = 2;

            // find parameters through inner-cross-validation:
            (double? cost, double? gamma, double? epsilon, double? coef0, double? degree, double? cv_rate, libsvm_cv_perf libsvm_cv_perf) parameters = (cost, gamma, epsilon, coef0, degree, null, null);

            var duration_grid_search = "";
            var duration_nm_search = "";
            var duration_training = "";

            var inner_cv_train_stdout_file = "";
            var inner_cv_train_stderr_file = "";

            var outer_cv_train_stdout_file = "";
            var outer_cv_train_stderr_file = "";

            if (kernel_parameter_search_method == libsvm_caller.kernel_parameter_search_method.none)
            {
                
            }
            else
            {


                do
                {
                    if (cancellation_token.IsCancellationRequested)
                    {
                        return default;
                    }

                    failsafe_searches_left--;



                    if (kernel_parameter_search_method == libsvm_caller.kernel_parameter_search_method.grid_internal || kernel_parameter_search_method == libsvm_caller.kernel_parameter_search_method.grid_libsvm_python)
                    {
                        var time_taken = new Stopwatch();
                        time_taken.Start();


                        parameters = libsvm_caller.grid_parameter_search(inner_cv_svm_implementation, inner_cv_filename_training, inner_cv_train_stdout_file, inner_cv_train_stderr_file, kernel_parameter_search_method, weights, svm_type, kernel, cross_validation_metric_class_list, cross_validation_metrics, inner_cv_folds, inner_cv_probability_estimates, inner_cv_shrinking_heuristics, point_max_time, process_max_time, echo, echo_err);
                        time_taken.Stop();



                        duration_grid_search = time_taken.ElapsedMilliseconds.ToString();

                        if ((failsafe_search && failsafe_searches_left > 0) && (parameters.cost == null || parameters.cost == 0) && (parameters.gamma == null || parameters.gamma == 0) && (parameters.epsilon == null || parameters.epsilon == 0) && (parameters.coef0 == null || parameters.coef0 == 0) && (parameters.degree == null || parameters.degree == 0))
                        {
                            kernel_parameter_search_method = libsvm_caller.kernel_parameter_search_method.nelder_mead;
                            continue;
                        }

                        break;

                    }
                    else if (kernel_parameter_search_method == libsvm_caller.kernel_parameter_search_method.nelder_mead)
                    {
                        var time_taken = new Stopwatch();
                        time_taken.Start();

                        //var use_modded_libsvm = libsvm_grid.should_use_modded_libsvm(cross_validation_metric_class_list, cross_validation_metrics);

                        var p = nelder_mead_params.search(inner_cv_svm_implementation, inner_cv_filename_training, inner_cv_train_stdout_file, inner_cv_train_stderr_file, svm_type, kernel, inner_cv_folds, inner_cv_probability_estimates, inner_cv_shrinking_heuristics, weights, point_max_time, process_max_time);
                        time_taken.Stop();

                        duration_nm_search = time_taken.ElapsedMilliseconds.ToString();

                        //parameters = (p.best_cost, p.best_gamma, p.best_epsilon, p.best_coef0, p.best_degree, p.best_rate, null);
                        parameters = (p.best_rate_container.best_cost, p.best_rate_container.best_gamma, p.best_rate_container.best_epsilon, p.best_rate_container.best_coef0, p.best_rate_container.best_degree, p.best_rate_container.best_rate, p.best_rate_container.best_libsvm_cv_perf);

                        if ((failsafe_search && failsafe_searches_left > 0) && (parameters.cost == null || parameters.cost == 0) && (parameters.gamma == null || parameters.gamma == 0) && (parameters.coef0 == null || parameters.coef0 == 0) && (parameters.degree == null || parameters.degree == 0))
                        {
                            kernel_parameter_search_method = libsvm_caller.kernel_parameter_search_method.grid_internal;
                            continue;
                        }

                        break;
                    }
                    else
                    {
                        throw new Exception();
                    }
                } while (failsafe_search && failsafe_searches_left > 0);

            }

            var training_cv_folds = 0;
            var training_max_time = (TimeSpan?)null;

            var time_taken_training = new Stopwatch();
            time_taken_training.Start();


            // use the parameters found to train a model (of the training set), which will later be used to perform predictions on the test set.
            // no max training time, since this is for training the model AFTER inner-cross-validation (i.e. not part of grid parameter search)
            var train_result = libsvm_caller.train(outer_cv_svm_implementation, outer_cv_filename_training, outer_cv_filename_model, outer_cv_train_stdout_file, outer_cv_train_stderr_file, parameters.cost, parameters.gamma, parameters.epsilon, parameters.coef0, parameters.degree, weights, svm_type, kernel, libsvm_caller.libsvm_cv_eval_methods.accuracy, training_cv_folds, outer_cv_probability_estimates, outer_cv_shrinking_heuristics, training_max_time, echo, echo_err);
            time_taken_training.Stop();

            duration_training = time_taken_training.ElapsedMilliseconds.ToString();


            if (inner_cv_filename_training != outer_cv_filename_training && !string.IsNullOrWhiteSpace(inner_cv_filename_training) && File.Exists(inner_cv_filename_training))
            {
                try
                {
                    File.Delete(inner_cv_filename_training);
                }
                catch (Exception e)
                {
                    program.WriteLineException(e, nameof(run_svm_train_subfunc));
                }
            }

            return (duration_grid_search, duration_nm_search, duration_training, outer_cv_filename_training, outer_cv_filename_training_comments, outer_cv_filename_model, train_result, parameters.cost, parameters.gamma, parameters.epsilon, parameters.coef0, parameters.degree, parameters.cv_rate, parameters.libsvm_cv_perf);

        }

        public class libsvm_cv_perf
        {
            public double v_precision;
            public double v_recall;
            public double v_fscore;
            public double v_bac;
            public double v_auc;
            public double v_accuracy;
            public double v_ap;
            public double v_cross_validation;

            public libsvm_cv_perf()
            {

            }

            public libsvm_cv_perf(List<string> libsvm_result_lines)
            {
                if (libsvm_result_lines == null || libsvm_result_lines.Count == 0) return;

                // Libsvm-Eval:
                //   Precision = 83.4862% (91/109)
                //   Recall = 75.8333% (91/120)
                //   F-score = 0.79476
                //   BAC = 0.819167
                //   AUC = 0.902056
                //   Accuracy = 82.5926% (223/270)
                //   AP = 0.887864
                //   Cross Validation = 75.8333%

                // Libsvm:
                //   Cross Validation Accuracy = 77.0833%

                // ThunderSVM:
                //   Cross Accuracy = 0.776042

                var v_precision_index = libsvm_result_lines.FindIndex(a => a.StartsWith("Precision = "));
                var v_recall_index = libsvm_result_lines.FindIndex(a => a.StartsWith("Recall = "));
                var v_fscore_index = libsvm_result_lines.FindIndex(a => a.StartsWith("F-score = "));
                var v_bac_index = libsvm_result_lines.FindIndex(a => a.StartsWith("BAC = "));
                var v_auc_index = libsvm_result_lines.FindIndex(a => a.StartsWith("AUC = "));
                var v_accuracy_index = libsvm_result_lines.FindIndex(a => a.StartsWith("Accuracy = "));
                var v_ap_index = libsvm_result_lines.FindIndex(a => a.StartsWith("AP = "));

                var v_cross_validation_index = libsvm_result_lines.FindIndex(a => a.StartsWith("Cross Validation = "));
                var v_libsvm_default_cross_validation_index = libsvm_result_lines.FindIndex(a => a.StartsWith("Cross Validation Accuracy = "));

                var v_thundersvm_default_cross_validation_index = libsvm_result_lines.FindIndex(a => a.StartsWith("Cross Accuracy = "));

                var v_precision_str = v_precision_index < 0 ? "" : libsvm_result_lines[v_precision_index].Split()[2];
                var v_recall_str = v_recall_index < 0 ? "" : libsvm_result_lines[v_recall_index].Split()[2];
                var v_fscore_str = v_fscore_index < 0 ? "" : libsvm_result_lines[v_fscore_index].Split()[2];
                var v_bac_str = v_bac_index < 0 ? "" : libsvm_result_lines[v_bac_index].Split()[2];
                var v_auc_str = v_auc_index < 0 ? "" : libsvm_result_lines[v_auc_index].Split()[2];
                var v_accuracy_str = v_accuracy_index < 0 ? "" : libsvm_result_lines[v_accuracy_index].Split()[2];
                var v_ap_str = v_ap_index < 0 ? "" : libsvm_result_lines[v_ap_index].Split()[2];
                var v_cross_validation_str = v_cross_validation_index < 0 ? "" : libsvm_result_lines[v_cross_validation_index].Split()[3];
                var v_libsvm_default_cross_validation_str = v_libsvm_default_cross_validation_index < 0 ? "" : libsvm_result_lines[v_libsvm_default_cross_validation_index].Split()[4];

                var v_thundersvm_default_cross_validation_str = v_thundersvm_default_cross_validation_index < 0 ? "" : libsvm_result_lines[v_thundersvm_default_cross_validation_index].Split()[3];

                this.v_precision = v_precision_index < 0 || string.IsNullOrWhiteSpace(v_precision_str) ? 0d : v_precision_str.Last() == '%' ? double.Parse(v_precision_str.Substring(0, v_precision_str.Length - 1)) / (double)100 : double.Parse(v_precision_str);
                this.v_recall = v_recall_index < 0 || string.IsNullOrWhiteSpace(v_recall_str) ? 0d : v_recall_str.Last() == '%' ? double.Parse(v_recall_str.Substring(0, v_recall_str.Length - 1)) / (double)100 : double.Parse(v_recall_str);
                this.v_fscore = v_fscore_index < 0 || string.IsNullOrWhiteSpace(v_fscore_str) ? 0d : v_fscore_str.Last() == '%' ? double.Parse(v_fscore_str.Substring(0, v_fscore_str.Length - 1)) / (double)100 : double.Parse(v_fscore_str);
                this.v_bac = v_bac_index < 0 || string.IsNullOrWhiteSpace(v_bac_str) ? 0d : v_bac_str.Last() == '%' ? double.Parse(v_bac_str.Substring(0, v_bac_str.Length - 1)) / (double)100 : double.Parse(v_bac_str);
                this.v_auc = v_auc_index < 0 || string.IsNullOrWhiteSpace(v_auc_str) ? 0d : v_auc_str.Last() == '%' ? double.Parse(v_auc_str.Substring(0, v_auc_str.Length - 1)) / (double)100 : double.Parse(v_auc_str);
                this.v_accuracy = v_accuracy_index < 0 || string.IsNullOrWhiteSpace(v_accuracy_str) ? 0d : v_accuracy_str.Last() == '%' ? double.Parse(v_accuracy_str.Substring(0, v_accuracy_str.Length - 1)) / (double)100 : double.Parse(v_accuracy_str);
                this.v_ap = v_ap_index < 0 || string.IsNullOrWhiteSpace(v_ap_str) ? 0d : v_ap_str.Last() == '%' ? double.Parse(v_ap_str.Substring(0, v_ap_str.Length - 1)) / (double)100 : double.Parse(v_ap_str);
                this.v_cross_validation = v_cross_validation_index < 0 || string.IsNullOrWhiteSpace(v_cross_validation_str) ? 0d : v_cross_validation_str.Last() == '%' ? double.Parse(v_cross_validation_str.Substring(0, v_cross_validation_str.Length - 1)) / (double)100 : double.Parse(v_cross_validation_str);

                // note: the default libsvm output and the modified eval outputs are slightly different
                if (this.v_cross_validation == 0)
                {
                    if (v_libsvm_default_cross_validation_index >= 0 && !string.IsNullOrWhiteSpace(v_libsvm_default_cross_validation_str))
                    {
                        this.v_cross_validation = v_libsvm_default_cross_validation_str.Last() == '%' ? double.Parse(v_libsvm_default_cross_validation_str.Substring(0, v_libsvm_default_cross_validation_str.Length - 1)) / (double)100 : double.Parse(v_libsvm_default_cross_validation_str);
                    }
                    else if (v_thundersvm_default_cross_validation_index >= 0 && !string.IsNullOrWhiteSpace(v_thundersvm_default_cross_validation_str))
                    {
                        this.v_cross_validation = v_thundersvm_default_cross_validation_str.Last() == '%' ? double.Parse(v_thundersvm_default_cross_validation_str.Substring(0, v_thundersvm_default_cross_validation_str.Length - 1)) / (double)100 : double.Parse(v_thundersvm_default_cross_validation_str);
                    }


                    if (this.v_accuracy == 0)
                    {
                        this.v_accuracy = this.v_cross_validation;
                    }
                }

                if (this.v_cross_validation == 0 && this.v_accuracy > 0) this.v_cross_validation = this.v_accuracy;
                else if (this.v_cross_validation == 0 && this.v_bac > 0) this.v_cross_validation = this.v_bac;
                else if (this.v_cross_validation == 0 && this.v_auc > 0) this.v_cross_validation = this.v_auc;
                else if (this.v_cross_validation == 0 && this.v_fscore > 0) this.v_cross_validation = this.v_fscore;
                else if (this.v_cross_validation == 0 && this.v_ap > 0) this.v_cross_validation = this.v_ap;
                else if (this.v_cross_validation == 0 && this.v_recall > 0) this.v_cross_validation = this.v_recall;
                else if (this.v_cross_validation == 0 && this.v_precision > 0) this.v_cross_validation = this.v_precision;

            }
        }

        public static List<(string testing_set_name, List<performance_measure.prediction> prediction_list)> run_svm_outer_cv_predict(
            libsvm_caller.svm_implementation inner_cv_svm_implementation,
            libsvm_caller.svm_implementation outer_cv_svm_implementation,
            bool inner_cv_probability_estimates,
            bool outer_cv_probability_estimates,
            CancellationToken cancellation_token,
            libsvm_cv_perf libsvm_cv_perf,


            
            string filename_training,
            string filename_training_comments,
            string filename_model,
            string file_prefix,
            libsvm_caller.libsvm_svm_type svm_type,
            libsvm_caller.libsvm_kernel_type kernel,
            cross_validation.resampling_method training_resampling_method,
            double? cost,
            double? gamma,
            double? coef0,
            double? degree,
            double? cv_rate,
            //(double train_pct, double unused_pct, double test_pct) train_test_split,
            int outer_cv_index,
            int randomisation_cv_folds,
            int outer_cv_folds,
            //cross_validation.math_operation l_math_operation,
            cross_validation.scaling_method scaling_method,
            List<(int class_id, int class_size)> class_sizes,
            List<List<example_instance>> training_set,
            List<(string dataset_name, List<List<example_instance>> dataset)> testing_set_list,
            List<(int fid, double min, double max)> scaling_params
        )
        {
            var print_params = true;

            if (print_params)
            {
                var param_list = new List<(string key, string value)>()
                {
                  
                    (nameof(filename_training), filename_training.ToString()),
                    (nameof(filename_training_comments), filename_training_comments.ToString()),
                    (nameof(filename_model), filename_model.ToString()),
                    (nameof(file_prefix), file_prefix.ToString()),
                    (nameof(svm_type), svm_type.ToString()),
                    (nameof(kernel), kernel.ToString()),
                    (nameof(training_resampling_method), training_resampling_method.ToString()),
                    (nameof(cost), cost.ToString()),
                    (nameof(gamma), gamma.ToString()),
                    (nameof(coef0), coef0.ToString()),
                    (nameof(degree), degree.ToString()),
                    (nameof(cv_rate), cv_rate.ToString()),
                    (nameof(outer_cv_index), outer_cv_index.ToString()),
                    (nameof(randomisation_cv_folds), randomisation_cv_folds.ToString()),
                    (nameof(outer_cv_folds), outer_cv_folds.ToString()),
                    (nameof(scaling_method), scaling_method.ToString()),
                    (nameof(class_sizes), class_sizes.ToString()),
                    (nameof(training_set), training_set.ToString()),
                    (nameof(testing_set_list), testing_set_list.ToString()),
                    (nameof(scaling_params), scaling_params.ToString()),
                };

                if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_outer_cv_predict)}({String.Join(", ", param_list.Select(a => $"{a.key}=\"{a.value}\"").ToList())});");
            }

            if (string.IsNullOrWhiteSpace(filename_model) || !File.Exists(filename_model) || new FileInfo(filename_model).Length == 0)
            {
                throw new Exception($@"Error: Model file not found: ""{filename_model}"".");
            }

            // balanced training code

            var result = new List<(string testing_set_name, List<performance_measure.prediction> prediction_list)>();

            for (var index = 0; index < testing_set_list.Count; index++)
            {
                if (cancellation_token.IsCancellationRequested)
                {
                    return default;
                }

                var (testing_set_name, testing_set) = testing_set_list[index];
                if (String.IsNullOrWhiteSpace(testing_set_name) || testing_set == null || testing_set.Count == 0) continue;

                var folder = libsvm_caller.svm_implementation_folder(outer_cv_svm_implementation);


                var is_temp_file = true;
                var filename_testing_data = program.convert_path(Path.Combine(folder, $"{Path.GetFileNameWithoutExtension(filename_model)}_{index}.testing.{testing_set_name}"), is_temp_file);
                var filename_testing_comments = program.convert_path(Path.Combine(folder, $"{Path.GetFileNameWithoutExtension(filename_model)}_{index}.testing_comments.{testing_set_name}"), is_temp_file);

                var filename_predict = program.convert_path(Path.Combine(folder, $"{Path.GetFileNameWithoutExtension(filename_model)}_{index}.testing.{testing_set_name}.predict"), is_temp_file);

                var testing_data_list = example_instance.feature_list_encode(outer_cv_index, testing_set.SelectMany(a => a).ToList(), scaling_method, scaling_method == cross_validation.scaling_method.no_scaling ? null : scaling_params);

                program.WriteAllLines(filename_testing_data, testing_data_list.data, is_temp_file);
                program.WriteAllLines(filename_testing_comments, testing_data_list.comments, is_temp_file);

                var predict_stdout_file = "";
                var predict_stderr_file = "";

                var predict_result = libsvm_caller.predict(outer_cv_svm_implementation, filename_testing_data, filename_model, filename_predict, outer_cv_probability_estimates, predict_stdout_file, predict_stderr_file);
                var predict_result_lines = predict_result.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                //var cv_r = new libsvm_cv_perf(predict_result_lines);


                var prediction_list = performance_measure.load_prediction_file_regression_values(filename_testing_data, filename_testing_comments, filename_predict);

                result.Add((testing_set_name, prediction_list));

                var delete_predict_file = true;
                var delete_test_file = true;

                if (delete_test_file && !string.IsNullOrWhiteSpace(filename_testing_data) && File.Exists(filename_testing_data))
                {
                    try
                    {
                        File.Delete(filename_testing_data);
                    }
                    catch (Exception e)
                    {
                        program.WriteLineException(e, nameof(run_svm_outer_cv_predict));
                    }
                }

                if (delete_test_file && !string.IsNullOrWhiteSpace(filename_testing_comments) && File.Exists(filename_testing_comments))
                {
                    try
                    {
                        File.Delete(filename_testing_comments);
                    }
                    catch (Exception e)
                    {
                        program.WriteLineException(e, nameof(run_svm_outer_cv_predict));
                    }
                }

                if (delete_predict_file && !string.IsNullOrWhiteSpace(filename_predict) && File.Exists(filename_predict))
                {
                    try
                    {
                        File.Delete(filename_predict);
                    }
                    catch (Exception e)
                    {
                        program.WriteLineException(e, nameof(run_svm_outer_cv_predict));
                    }

                }

                if (delete_predict_file && !string.IsNullOrWhiteSpace(predict_stdout_file) && File.Exists(predict_stdout_file))
                {
                    try
                    {
                        File.Delete(predict_stdout_file);
                    }
                    catch (Exception e)
                    {
                        program.WriteLineException(e, nameof(run_svm_outer_cv_predict));
                    }

                }

                if (delete_predict_file && !string.IsNullOrWhiteSpace(predict_stderr_file) && File.Exists(predict_stderr_file))
                {
                    try
                    {
                        File.Delete(predict_stderr_file);
                    }
                    catch (Exception e)
                    {
                        program.WriteLineException(e, nameof(run_svm_outer_cv_predict));
                    }

                }
            }

            var delete_training_file = true;
            var delete_model_file = true;

            if (delete_model_file && !string.IsNullOrWhiteSpace(filename_model) && File.Exists(filename_model))
            {
                try
                {
                    File.Delete(filename_model);
                }
                catch (Exception e)
                {
                    program.WriteLineException(e, nameof(run_svm_outer_cv_predict));
                }
            }

            if (delete_training_file)
            {
                //var filename_training_stdout = $"{filename_training}.stdout";
                //var filename_training_stderr = $"{filename_training}.stderr";

                if (delete_training_file && !string.IsNullOrWhiteSpace(filename_training) && File.Exists(filename_training))
                {
                    try
                    {
                        File.Delete(filename_training);
                    }
                    catch (Exception e)
                    {
                        program.WriteLineException(e, nameof(run_svm_outer_cv_predict));
                    }
                    
                }

                if (delete_training_file && !string.IsNullOrWhiteSpace(filename_training_comments) && File.Exists(filename_training_comments))
                {
                    try
                    {
                        File.Delete(filename_training_comments);
                    }
                    catch (Exception e)
                    {
                        program.WriteLineException(e, nameof(run_svm_outer_cv_predict));
                    }
                    
                }

                //if (delete_training_file && !string.IsNullOrWhiteSpace(filename_training) && File.Exists(filename_training_stdout))
                //{
                //    try
                //    {
                //        File.Delete(filename_training_stdout);
                //    }
                //    catch (Exception e)
                //    {
                //        program.WriteLineException(e,nameof(run_svm_outer_cv_predict));
                //    }
                //}

                //if (File.Exists(filename_training_stderr))
                //{
                //    try
                //    {
                //        File.Delete(filename_training_stderr);
                //    }
                //    catch (Exception e)
                //    {
                //        program.WriteLineException(e,nameof(run_svm_outer_cv_predict));
                //    }
                //}
            }



            return result;
        }

        public static List<(string testing_set_name, List<performance_measure.confusion_matrix> confusion_matrices, List<performance_measure.prediction> prediction_list, List<(string key, string value)> prediction_meta_data)> run_svm_outer_cv_predict_subfunc1(
            libsvm_caller.svm_implementation inner_cv_svm_implementation,
            libsvm_caller.svm_implementation outer_cv_svm_implementation,
            bool inner_cv_probability_estimates,
            bool outer_cv_probability_estimates,
            CancellationToken cancellation_token,
            libsvm_cv_perf libsvm_cv_perf,
            int feature_count,
            int group_count,
            string duration_grid_search,
            string duration_nm_search,
            string duration_training,
            bool output_threshold_adjustment_performance,
            List<(int class_id, string class_name)> class_names,
            
            string filename_training,
            string filename_training_comments,
            string filename_model,
            string file_prefix,
            libsvm_caller.kernel_parameter_search_method kernel_parameter_search_method,
            libsvm_caller.libsvm_svm_type svm_type,
            libsvm_caller.libsvm_kernel_type kernel,
            cross_validation.resampling_method training_resampling_method,
            double? cost,
            double? gamma,
            double? coef0,
            double? degree,
            double? cv_rate,
            int inner_cv_folds,
            int randomisation_cv_index,
            int outer_cv_index,
            int randomisation_cv_folds,
            int outer_cv_folds,
            cross_validation.scaling_method scaling_method,
            List<(int class_id, int class_size)> class_sizes,
            List<(int class_id, double class_weight)> class_weights,
            List<List<example_instance>> training_set,
            List<(string dataset_name, List<List<example_instance>> dataset)> testing_set_list,
            List<(int fid, double min, double max)> scaling_params
            )
        {
            var print_params = true;

            if (print_params)
            {
                var param_list = new List<(string key, string value)>()
                {
                    (nameof(output_threshold_adjustment_performance), output_threshold_adjustment_performance.ToString()),
                    (nameof(class_names), class_names?.ToString()),
             
                    (nameof(filename_model), filename_model?.ToString()),
                    (nameof(file_prefix), file_prefix?.ToString()),
                    (nameof(kernel_parameter_search_method), kernel_parameter_search_method.ToString()),
                    (nameof(svm_type), svm_type.ToString()),
                    (nameof(kernel), kernel.ToString()),
                    (nameof(training_resampling_method), training_resampling_method.ToString()),
                    (nameof(cost), cost?.ToString()),
                    (nameof(gamma), gamma?.ToString()),
                    (nameof(coef0), coef0?.ToString()),
                    (nameof(degree), degree?.ToString()),
                    (nameof(cv_rate), cv_rate?.ToString()),
                    (nameof(inner_cv_folds), inner_cv_folds.ToString()),
                    (nameof(outer_cv_index), outer_cv_index.ToString()),
                    (nameof(outer_cv_folds), outer_cv_folds.ToString()),
                    (nameof(scaling_method), scaling_method.ToString()),
                    (nameof(class_sizes), class_sizes?.ToString()),
                    (nameof(class_weights), class_weights?.ToString()),
                    (nameof(training_set), training_set?.ToString()),
                    (nameof(testing_set_list), testing_set_list?.ToString()),
                    (nameof(scaling_params), scaling_params?.ToString()),
                };

                if (program.write_console_log) program.WriteLine($@"{nameof(run_svm_outer_cv_predict_subfunc1)}({String.Join(", ", param_list.Select(a => $"{a.key}=\"{a.value}\"").ToList())});");
            }


            var time_taken = new Stopwatch();
            time_taken.Start();

            var prediction_lists = run_svm_outer_cv_predict(
                
                inner_cv_svm_implementation,
                outer_cv_svm_implementation,
                inner_cv_probability_estimates,
                outer_cv_probability_estimates,
                cancellation_token,
                libsvm_cv_perf,
                
                filename_training,
                filename_training_comments,
                filename_model,
                file_prefix,
                svm_type,
                kernel,
                training_resampling_method,
                cost,
                gamma,
                coef0,
                degree,
                cv_rate,
                outer_cv_index,
                randomisation_cv_folds,
                outer_cv_folds,
                scaling_method,
                class_sizes,
                training_set,
                testing_set_list,
                scaling_params);

            // balanced training code
            time_taken.Stop();

            var duration_prediction = time_taken.ElapsedMilliseconds.ToString();

            var features_included = string.Join(" ", training_set.First().First().feature_list.Skip(1).Select(e => e.fid).ToList());

            var training_size = training_set.Select(a => (class_id: a.First().class_id(), class_size: a.Count)).Distinct().ToList();

            var result = new List<(string testing_set_name, List<performance_measure.confusion_matrix> confusion_matrices, List<performance_measure.prediction> prediction_list, List<(string key, string value)> prediction_meta_data)>();

            var prediction_meta_data = new List<(string key, string value)>()
            {
                (nameof(inner_cv_folds),inner_cv_folds.ToString()),
                (nameof(outer_cv_index),outer_cv_index.ToString()),
                (nameof(randomisation_cv_folds),randomisation_cv_folds.ToString()),
                (nameof(outer_cv_folds),outer_cv_folds.ToString()),
                (nameof(scaling_method),scaling_method.ToString()),
                (nameof(feature_count),feature_count.ToString()),
                (nameof(group_count),group_count.ToString()),
                (nameof(features_included),features_included),
                (nameof(training_resampling_method),training_resampling_method.ToString()),
                (nameof(kernel_parameter_search_method),((int)kernel_parameter_search_method).ToString()),
                (nameof(svm_type),svm_type.ToString()),
                (nameof(kernel),kernel.ToString()),
                (nameof(cost),((double?)cost)?.ToString() ?? ""),
                (nameof(gamma),((double?)gamma)?.ToString() ?? ""),
                (nameof(coef0),((double?)coef0)?.ToString() ?? ""),
                (nameof(degree),((double?)degree)?.ToString() ?? ""),
                (nameof(cv_rate),((double?)cv_rate)?.ToString() ?? ""),
            };

            for (var index = 0; index < testing_set_list.Count; index++)
            {
                if (cancellation_token.IsCancellationRequested)
                {
                    return default;
                }

                var (testing_set_name, testing_set) = testing_set_list[index];

                if (String.IsNullOrWhiteSpace(testing_set_name) || testing_set == null || testing_set.Count == 0) continue;

                var prediction_list = prediction_lists.First(a => a.testing_set_name == testing_set_name);

                if (prediction_list.prediction_list == null || prediction_list.prediction_list.Count == 0)
                {
                    throw new ArgumentException(nameof(prediction_list.prediction_list));
                }

                var acc_predict_result_confusion_matrices = performance_measure.load_prediction_file(prediction_list.prediction_list, output_threshold_adjustment_performance);

                acc_predict_result_confusion_matrices.ForEach(a =>
                {
                    a.duration_training = duration_training;
                    a.duration_grid_search = duration_grid_search;
                    a.duration_nm_search = duration_nm_search;
                    a.duration_testing = duration_prediction;

                    a.class_weight = class_weights?.FirstOrDefault(b => a.class_id == b.class_id).class_weight;
                    a.class_name = class_names?.FirstOrDefault(b => a.class_id == b.class_id).class_name;

                    a.libsvm_cv = libsvm_cv_perf?.v_cross_validation ?? 0;
                    a.libsvm_cv_accuracy = libsvm_cv_perf?.v_accuracy ?? 0;
                    a.libsvm_cv_ap = libsvm_cv_perf?.v_ap ?? 0;
                    a.libsvm_cv_auc = libsvm_cv_perf?.v_auc ?? 0;
                    a.libsvm_cv_bac = libsvm_cv_perf?.v_bac ?? 0;
                    a.libsvm_cv_fscore = libsvm_cv_perf?.v_fscore ?? 0;
                    a.libsvm_cv_precision = libsvm_cv_perf?.v_precision ?? 0;
                    a.libsvm_cv_recall = libsvm_cv_perf?.v_recall ?? 0;

                    a.testing_set_name = prediction_list.testing_set_name;

                    a.inner_cv_folds = inner_cv_folds;
                    a.outer_cv_index = outer_cv_index;
                    a.randomisation_cv_index = randomisation_cv_index;
                    a.randomisation_cv_folds = randomisation_cv_folds;
                    a.outer_cv_folds = outer_cv_folds;
                    a.scaling_method = scaling_method.ToString();
                    a.class_size = class_sizes.First(b => b.class_id == a.class_id).class_size;
                    a.class_training_size = training_size.First(b => b.class_id == a.class_id).class_size;
                    a.feature_count = feature_count;
                    a.group_count = group_count;
                    a.features_included = features_included;
                    a.training_resampling_method = training_resampling_method.ToString();

                    a.svm_type = svm_type.ToString();
                    a.kernel = kernel.ToString();

                    a.kernel_parameter_search_method = kernel_parameter_search_method.ToString();
                    a.cost = (double?)cost;
                    a.gamma = (double?)gamma;
                    a.coef0 = (double?)coef0;
                    a.degree = (double?)degree;
                    

                    a.calculate_ppf();
                });

                result.Add((testing_set_name, acc_predict_result_confusion_matrices, prediction_list.prediction_list, prediction_meta_data));
            }

            return result;
        }

        public class run_svm_return_item
        {
            public string testing_set_name;
            public List<performance_measure.confusion_matrix> confusion_matrices;
            public List<performance_measure.prediction> prediction_list;
            public List<(string key, string value)> prediction_meta_data;
        }

        public class run_svm_return
        {
            public List<run_svm_return_item> run_svm_return_data;

            public static string serialise(run_svm_return svm_return)
            {
                var json_settings1 = new JsonSerializerSettings() { PreserveReferencesHandling = PreserveReferencesHandling.All };
                var request_svm_return_json_serialised = Newtonsoft.Json.JsonConvert.SerializeObject(svm_return, json_settings1);
                return request_svm_return_json_serialised;
            }

            public static run_svm_return deserialise(string serialised_run_svm_return)
            {
                run_svm_return response_svm_return = null;
                try
                {
                    var json_settings2 = new JsonSerializerSettings() { PreserveReferencesHandling = PreserveReferencesHandling.All };
                    response_svm_return = JsonConvert.DeserializeObject<cross_validation.run_svm_return>(serialised_run_svm_return, json_settings2);
                }
                catch (Exception)
                {
                    response_svm_return = null;
                }

                return response_svm_return;
            }
        }


        //public static readonly object save_file_lock = new object();

        //public static void save_results(string save_filename, List<string> results, bool append = true, int max_tries = Int32.MaxValue)
        //{
        //    try
        //    {
        //        Directory.CreateDirectory(Path.GetDirectoryName(save_filename));
        //    }
        //    catch (Exception e)
        //    {
        //        if (program.write_console_log) program.WriteLine($@"{nameof(save_results)}(): {e.Message}");
        //    }

        //    if (append)
        //    {
        //        program.AppendAllLines(save_filename, results);
        //    }
        //    else
        //    {
        //        program.WriteAllLines(save_filename, results);
        //    }
        //}


        public enum scaling_method : int
        {
            no_scaling = 0,
            scale_zero_to_plus_one = 1,
            scale_minus_one_to_plus_one = 2,
            square_root = 3
        }

        public enum resampling_method : int
        {
            do_not_resample = 0,
            downsample_with_full_dataset,
            upsample_random_selection,
            //downsample_with_minimised_dataset,
            //downsample_with_replacement,
            //upsample_repeat_data,
            //upsample_with_replacement,
        }

        public enum test_prediction_method : int
        {
            none = 0,

            test_set_unnatural_downsample_balanced,
            test_set_unnatural_upsample_balanced,
            test_set_natural_imbalanced,


            training_set_unnatural_downsample_balanced,
            training_set_unnatural_upsample_balanced,
            training_set_natural_imbalanced,

            external_set_for_prediction,

            final_model_training_downsample_balanced,
            final_model_training_imbalanced,
        }

        public enum math_operation : int
        {
            none,
            abs_method,
            acos_method,
            asin_method,
            atan_method,
            atan2_method,
            big_mul_method,
            ceiling_method,
            cos_method,
            cosh_method,
            exp_method,
            floor_method,
            log_method,
            log10_method,
            pow_method,
            round_method,
            sign_method,
            sin_method,
            sinh_method,
            sqrt_method,
            tan_method,
            tanh_method,
            truncate_method,
        }
    }
}
