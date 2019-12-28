using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace svm_compute
{
    class temp3
    {
        /*
                                           var bag_randoms = bal_training_set.Select(a => new Random(bag_cv_repeatability_seed)).ToList();
                                           //var bags = new List<(int bag_id, List<List<example_instance>> bag, List<List<example_instance>> out_of_bag)>();

                                           for (var bootstrap_bag_index = 0; bootstrap_bag_index < number_of_bags; bootstrap_bag_index++) // set number_of_bags to 0 to skip boostrapping
                                           {
                                                //if (program.write_console_log) program.WriteLine($@"{nameof(run_svm)}({feature_set_index}): bootstrap_bag={(bootstrap_bag_index + 1)}/{number_of_bags}");

                                               var bag_training_set = bal_training_set.Select(a => a.ToList()).ToList();

                                               for (var training_bag_index = 0; training_bag_index < bag_training_set.Count; training_bag_index++)
                                               {
                                                   bag_training_set[training_bag_index].shuffle(bag_randoms[training_bag_index]);
                                               }

                                               bag_training_set = bag_training_set.Select(a => a.Take((int)Math.Floor(a.Count * size_of_bags_pct)).ToList()).ToList();
                                               var out_of_bag = bal_training_set.Select((a, i) => a.Except(bag_training_set[i]).ToList()).ToList();
                                               //bags.Add((bootstrap_bag_index, training_bag, out_of_bag));

                                               var l_bag_index = bootstrap_bag_index;

                                               var bag_scaling_params = bal_scaling_params;

                                               var bag_task = Task.Run(() =>
                                               {
                                                   var task_result = new List<(string name, List<performance_measure.prediction> prediction_list)>();

                                                   //train
                                                   var bag_training_result = run_svm_train_subfunc(feature_set_index, label_training_balanced, l_kernel, l_imbalanced_method, l_bag_index, l_training_size_pct, l_testing_size_pct, l_nested_cv_repeat_test, l_scaled, bag_training_set, bag_scaling_params);

                                                   //test
                                                   var bag_test_full_bal_training = true;
                                                   var bag_test_full_unbal_training = true;
                                                   var bag_test_in_bag = true;
                                                   var bag_test_out_of_bag = true;

                                                   var bag_test_sets = new List<(string dataset_name, List<List<example_instance>> dataset)>();

                                                   if (bag_test_full_bal_training)
                                                   {
                                                       bag_test_sets.Add((nameof(bal_training_set), bal_training_set));
                                                   }

                                                   if (bag_test_full_unbal_training)
                                                   {
                                                       bag_test_sets.Add((nameof(unbal_training_set), unbal_training_set));
                                                   }

                                                   if (bag_test_in_bag)
                                                   {
                                                       bag_test_sets.Add((nameof(bag_training_set), bag_training_set));
                                                   }

                                                   if (bag_test_out_of_bag)
                                                   {
                                                       bag_test_sets.Add((nameof(out_of_bag), out_of_bag));
                                                   }

                                                   var predict_result = run_svm_predict_subfunc2(feature_set_index, label_training_balanced, l_kernel, l_imbalanced_method, l_bag_index, bag_training_result.cost, bag_training_result.gamma, bag_training_result.cv_rate, l_training_size_pct, l_testing_size_pct, l_nested_cv_repeat_test, cross_class_overlap, same_class_overlap, overlap_train_test, feature_set_index, feature_set_name, nested_cv_repeats, l_scaled, l_class_sizes, bag_training_set, bag_test_sets, bag_scaling_params);
                                                   predict_result.ForEach(a => task_result.Add((a.testing_set_name, a.prediction_list)));

                                                   return task_result;
                                               });

                                               bag_tasks.Add(bag_task);
                                           }

                                           Task.WaitAll(bag_tasks.ToArray<Task>());
                                           var bag_results = bag_tasks.SelectMany(a => a.Result).GroupBy(a => a.name).Select(a =>
                                               (test_set_name: a.Key, a.SelectMany(b => b.prediction_list)
                                                   .GroupBy(c => c.instance_index).Select(c =>
                                                       (instance_index: c.Key, c.SelectMany(d => d.probability_estimates)
                                                           .ToList())).ToList())).ToList();

                                           //if (program.write_console_log) program.WriteLine();
                                           // todo: test full training set, test training examples used, test out of bag

                                           */
    }
}
