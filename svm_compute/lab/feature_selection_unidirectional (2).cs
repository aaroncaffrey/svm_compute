using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Accord;
using Newtonsoft.Json;

namespace svm_compute
{
    public class feature_selection_unidirectional
    {
        /* Unidirectional feature selection algorithm.
         *
         * Parameters:
         *
         *  Just rank the features or continue to perform feature selection...
         *
         *  Cross-validation parameters: svm type, svm kernel, random ordering total, outer cv total, inner cv total, ...
         *
         *  Base features to start with
         *
         *  Pool of available features to select
         *
         *  Attempt mutations (random replacements) at end of algorithm
         *
         *  Direction - backwards, forwards, both, one then the other, etc.
         *
         *  Select by whole interrelated features sets/groups or individual features
         *
         *  Performance ranking - overall, performance per feature, performance per group, etc.
         *
         *  Reorder the available features for selection at each iteration of ranking - asc, desc, random, unchanged
         *
         *  Keep features 'safe/good' or 'unsafe/bad' after max insertions/removals
         *
         *  Min features to go backwards for.
         *  Max features to go forwards for.
         *
         *  Max features to add per iteration from the rankings.
         *  Max features to remove per iteration from the rankings.
         *
         *  Max features to add together for new ranking.
         *  Max features to remove together for new ranking.
         *
         */

        public enum feature_selection_types : int
        {
            none,

            //forwards_rank,// this has been replaced by changing num iterations = 1
            //backwards_rank,// this too

            forwards,
            backwards,

            forwards_and_backwards,
            backwards_and_forwards,

            forwards_then_backwards,
            backwards_then_forwards,

            forwards_then_backwards_repeated_until_convergence,
            backwards_then_forwards_repeated_until_convergence
        }

        public enum perf_selection_rules : int
        {
            none,

            best_score,

            best_ppf_overall,
            best_ppg_overall,

            best_ppf_change,
            best_ppg_change,

            best_average_of_score_and_ppf,
            best_average_of_score_and_ppg,

            best_average_of_score_and_ppf_normalised,
            best_average_of_score_and_ppg_normalised
        }

        public enum feature_selection_combinators : int
        {
            none,
            individual_features,
            feature_sets
        }

        public enum reordering_rules : int
        {
            none,
            random_order,
            order_perf_asc,
            order_perf_desc,
        }

        public class feature_set
        {
            public string source;
            public string dimension;
            public string category;
            public string alphabet;
            public string group;

            public int set_id;

            public List<feature_set_member> set_members;

            public static string csv_header = string.Join(",", new object[]
            {
                nameof(category),
                nameof(dimension),
                nameof(alphabet),
                nameof(source),
                nameof(@group),
                nameof(set_id),
            });

            public override string ToString()
            {
                return string.Join(",", new object[]
                {

                    category,
                    dimension,
                    alphabet,
                    source,
                    @group,
                    set_id,
                });
            }
        }

        public class feature_set_member
        {
            public int fid;
            public int set_id;
            public string source;
            public string dimension;
            public string category;
            public string alphabet;
            public string group;
            public string member;
            public string perspective;
            public int alphabet_id;
            public int dimension_id;
            public int category_id;
            public int source_id;
            public int group_id;
            public int member_id;
            public int perspective_id;

            public static string csv_header = string.Join(",", new object[] {
                    nameof(fid),
                    nameof(set_id),
                    nameof(source),
                    nameof(dimension),
                    nameof(category),
                    nameof(alphabet),
                    nameof(group),
                    nameof(member),
                    nameof(perspective),
                    nameof(alphabet_id),
                    nameof(dimension_id),
                    nameof(category_id),
                    nameof(source_id),
                    nameof(group_id),
                    nameof(member_id),
                    nameof(perspective_id),
                }
            );
            public override string ToString()
            {
                return string.Join(",", new object[]
                {
                    fid,
                    set_id,
                    source,
                    dimension,
                    category,
                    alphabet,
                    group,
                    member,
                    perspective,
                    alphabet_id,
                    dimension_id,
                    category_id,
                    source_id,
                    group_id,
                    member_id,
                    perspective_id,
                });


            }

        }

        public static string csv_header = string.Join(",", new object[]
        {
            nameof(feature_selection_type),
             nameof(perf_selection_rule),
             //nameof(reorder_input_each_iteration),
             nameof(backwards_max_features_to_combine_per_iteration),
             nameof(forwards_max_features_to_combine_per_iteration),
             nameof(backwards_max_features_to_remove_per_iteration),
             nameof(forwards_max_features_to_add_per_iteration),
             nameof(backwards_min_features),
             nameof(forwards_max_features),
             nameof(backwards_max_feature_removal_attempts),
             nameof(forwards_max_features_insertion_attempts),
             nameof(random_baseline),
             nameof(margin_of_error),
             nameof(max_tasks),
             nameof(feature_selection_combinator),
             nameof(feature_selection_performance_metrics),
             nameof(feature_selection_performance_classes),
             nameof(backwards_finished),
             nameof(forwards_finished),

             //nameof(backwards_baseline_score),

             //nameof(forwards_baseline_score),

             nameof(baseline_score),

             nameof(baseline_recalculation_required),
             nameof(backwards_score_improved),
             nameof(forwards_score_improved),
             nameof(score_improved),
             nameof(direction_change),
             nameof(consecutive_iterations_with_improvement),
             nameof(consecutive_iterations_without_improvement),
             nameof(consecutive_forwards_iterations),
             nameof(consecutive_forwards_iterations_with_improvement),
             nameof(consecutive_forwards_iterations_without_improvement),
             nameof(consecutive_backwards_iterations),
             nameof(consecutive_backwards_iterations_with_improvement),
             nameof(consecutive_backwards_iterations_without_improvement),
             nameof(unidirectional_convergence_reached),
             nameof(iteration_loop),
             nameof(iteration_select),
             nameof(iteration_recalculate),
             nameof(iteration_select_max),
             nameof(go_backwards),
             nameof(go_forwards),
             nameof(can_go_backwards),
             nameof(can_go_forwards),
             nameof(forwards_max_features_reached),
             nameof(backwards_min_features_reached),
             //nameof(first),
             //nameof(store_last_iteration),
             //nameof(store_history_log),

             nameof(external_task_id),
             nameof(external_forwards_task_id),
             nameof(external_backwards_task_id),


             nameof(dataset_instance_list),
             nameof(features_input_untouched),
             nameof(features_input),
             nameof(base_features),
             nameof(features_selected),
             nameof(features_backwards_buffer),
             nameof(features_forwards_buffer),
             nameof(backwards_selection_feature_rankings),
             nameof(backwards_selection_feature_rankings_taken_for_removal),
             nameof(backwards_selection_feature_rankings_could_have_taken_for_removal),
             nameof(backwards_selection_feature_rankings_bad_features),
             nameof(backwards_selection_feature_rankings_good_features),
             //nameof(backwards_selection_feature_rankings_last_iteration),
             //nameof(backwards_selection_feature_rankings_taken_for_removal_last_iteration),
             //nameof(backwards_selection_feature_rankings_could_have_taken_for_removal_last_iteration),
             //nameof(backwards_selection_feature_rankings_bad_features_last_iteration),
             //nameof(backwards_selection_feature_rankings_good_features_last_iteration),

             nameof(forwards_selection_feature_rankings),
             nameof(forwards_selection_feature_rankings_taken_for_insertion),
             nameof(forwards_selection_feature_rankings_could_have_taken_for_insertion),
             nameof(forwards_selection_feature_rankings_bad_features),
             nameof(forwards_selection_feature_rankings_good_features),
             //nameof(forwards_selection_feature_rankings_last_iteration),
             //nameof(forwards_selection_feature_rankings_taken_for_insertion_last_iteration),
             //nameof(forwards_selection_feature_rankings_could_have_taken_for_insertion_last_iteration),
             //nameof(forwards_selection_feature_rankings_bad_features_last_iteration),
             //nameof(forwards_selection_feature_rankings_good_features_last_iteration),
             //nameof(feature_importance_first_iteration),
             //nameof(feature_importance_average),
             //nameof(feature_importance_actual),
             nameof(forwards_feature_bad_feature_log),
             nameof(forwards_feature_bad_feature_permanent),
             nameof(forwards_feature_good_feature_log),
             nameof(forwards_feature_good_feature_permanent),
             nameof(backwards_feature_bad_feature_log),
             nameof(backwards_feature_bad_feature_permanent),
             nameof(backwards_feature_good_feature_log),
             nameof(backwards_feature_good_feature_permanent),
        });
        public override string ToString()
        {
            return string.Join(",", new object[]
            {

                feature_selection_type,
                perf_selection_rule,
                //reorder_input_each_iteration,
                backwards_max_features_to_combine_per_iteration,
                forwards_max_features_to_combine_per_iteration,
                backwards_max_features_to_remove_per_iteration,
                forwards_max_features_to_add_per_iteration,
                backwards_min_features,
                forwards_max_features,
                backwards_max_feature_removal_attempts,
                forwards_max_features_insertion_attempts,
                random_baseline,
                margin_of_error,
                max_tasks,
                feature_selection_combinator,
                feature_selection_performance_metrics.ToString().Replace(",",";"),
                string.Join(";", feature_selection_performance_classes?.Select(a=>a.ToString()).ToList() ?? new List<string>()),
                Convert.ToInt16(backwards_finished),
                Convert.ToInt16(forwards_finished),

                //backwards_baseline_score.score_after,

                //forwards_baseline_score.score_after,

                baseline_score.score_after,

                Convert.ToInt16(baseline_recalculation_required),
                Convert.ToInt16(backwards_score_improved),
                Convert.ToInt16(forwards_score_improved),
                Convert.ToInt16(score_improved),
                Convert.ToInt16(direction_change),
                consecutive_iterations_with_improvement,
                consecutive_iterations_without_improvement,
                consecutive_forwards_iterations,
                consecutive_forwards_iterations_with_improvement,
                consecutive_forwards_iterations_without_improvement,
                consecutive_backwards_iterations,
                consecutive_backwards_iterations_with_improvement,
                consecutive_backwards_iterations_without_improvement,
                Convert.ToInt16(unidirectional_convergence_reached),
                iteration_loop,
                iteration_select,
                iteration_recalculate,
                iteration_select_max,
                Convert.ToInt16(go_backwards),
                Convert.ToInt16(go_forwards),
                Convert.ToInt16(can_go_backwards),
                Convert.ToInt16(can_go_forwards),
                Convert.ToInt16(forwards_max_features_reached),
                Convert.ToInt16(backwards_min_features_reached),
                //Convert.ToInt16(first),
                //Convert.ToInt16(store_last_iteration),
                //Convert.ToInt16(store_history_log),

                external_task_id,
                external_forwards_task_id,
                external_backwards_task_id,

                dataset_instance_list?.Count??0,
                features_input_untouched?.Count??0,
                features_input?.Count??0,
                base_features?.Count??0,
                features_selected?.Count??0,
                features_backwards_buffer?.Count??0,
                features_forwards_buffer?.Count??0,
                backwards_selection_feature_rankings?.Count??0,
                backwards_selection_feature_rankings_taken_for_removal?.Count??0,
                backwards_selection_feature_rankings_could_have_taken_for_removal?.Count??0,
                backwards_selection_feature_rankings_bad_features?.Count??0,
                backwards_selection_feature_rankings_good_features?.Count??0,
                //backwards_selection_feature_rankings_last_iteration?.Count??0,
                //backwards_selection_feature_rankings_taken_for_removal_last_iteration?.Count??0,
                //backwards_selection_feature_rankings_could_have_taken_for_removal_last_iteration?.Count??0,
                //backwards_selection_feature_rankings_bad_features_last_iteration?.Count??0,
                //backwards_selection_feature_rankings_good_features_last_iteration?.Count??0,

                forwards_selection_feature_rankings?.Count??0,
                forwards_selection_feature_rankings_taken_for_insertion?.Count??0,
                forwards_selection_feature_rankings_could_have_taken_for_insertion?.Count??0,
                forwards_selection_feature_rankings_bad_features?.Count??0,
                forwards_selection_feature_rankings_good_features?.Count??0,
                //forwards_selection_feature_rankings_last_iteration?.Count??0,
                //forwards_selection_feature_rankings_taken_for_insertion_last_iteration?.Count??0,
                //forwards_selection_feature_rankings_could_have_taken_for_insertion_last_iteration?.Count??0,
                //forwards_selection_feature_rankings_bad_features_last_iteration?.Count??0,
                //forwards_selection_feature_rankings_good_features_last_iteration?.Count??0,
                //feature_importance_first_iteration?.Count??0,
                //feature_importance_average?.Count??0,
                //feature_importance_actual?.Count??0,
                forwards_feature_bad_feature_log?.Count??0,
                forwards_feature_bad_feature_permanent?.Count??0,
                forwards_feature_good_feature_log?.Count??0,
                forwards_feature_good_feature_permanent?.Count??0,
                backwards_feature_bad_feature_log?.Count??0,
                backwards_feature_bad_feature_permanent?.Count??0,
                backwards_feature_good_feature_log?.Count??0,
                backwards_feature_good_feature_permanent?.Count??0,

            });
        }

        public (string name, object value, string type)[] AsArray()
        {

            var result = new List<(string name, object value, string type)>
            {
              (name:   nameof(feature_selection_type)                                                                       ,value:  feature_selection_type,                                                                          type:   feature_selection_type.GetType().Name ?? "null"                                                                                            ),
              (name:   nameof(perf_selection_rule)                                                                          ,value:  perf_selection_rule,                                                                             type:   perf_selection_rule.GetType().Name ?? "null"                                                                                               ),
              //(name:   nameof(reorder_input_each_iteration)                                                                 ,value:  reorder_input_each_iteration,                                                                    type:   reorder_input_each_iteration.GetType().Name ?? "null"                                                                                               ),
              (name:   nameof(backwards_max_features_to_combine_per_iteration)                                              ,value:  backwards_max_features_to_combine_per_iteration,                                                 type:   backwards_max_features_to_combine_per_iteration.GetType().Name ?? "null"                                                                   ),
              (name:   nameof(forwards_max_features_to_combine_per_iteration)                                               ,value:  forwards_max_features_to_combine_per_iteration,                                                  type:   forwards_max_features_to_combine_per_iteration.GetType().Name ?? "null"                                                                    ),
              (name:   nameof(backwards_max_features_to_remove_per_iteration)                                               ,value:  backwards_max_features_to_remove_per_iteration,                                                  type:   backwards_max_features_to_remove_per_iteration.GetType().Name ?? "null"                                                                    ),
              (name:   nameof(forwards_max_features_to_add_per_iteration)                                                   ,value:  forwards_max_features_to_add_per_iteration,                                                      type:   forwards_max_features_to_add_per_iteration.GetType().Name ?? "null"                                                                        ),
              (name:   nameof(backwards_min_features)                                                                       ,value:  backwards_min_features,                                                                          type:   backwards_min_features.GetType().Name ?? "null"                                                                                            ),
              (name:   nameof(forwards_max_features)                                                                        ,value:  forwards_max_features,                                                                           type:   forwards_max_features.GetType().Name ?? "null"                                                                                             ),
              (name:   nameof(backwards_max_feature_removal_attempts)                                                       ,value:  backwards_max_feature_removal_attempts,                                                          type:   backwards_max_feature_removal_attempts.GetType().Name ?? "null"                                                                            ),
              (name:   nameof(forwards_max_features_insertion_attempts)                                                     ,value:  forwards_max_features_insertion_attempts,                                                        type:   forwards_max_features_insertion_attempts.GetType().Name ?? "null"                                                                          ),
              (name:   nameof(random_baseline)                                                                              ,value:  random_baseline,                                                                                 type:   random_baseline.GetType().Name ?? "null"                                                                                                   ),
              (name:   nameof(margin_of_error)                                                                              ,value:  margin_of_error,                                                                                 type:   margin_of_error.GetType().Name ?? "null"                                                                                                   ),
              (name:   nameof(max_tasks)                                                                                    ,value:  max_tasks,                                                                                       type:   max_tasks.GetType().Name ?? "null"                                                                                                         ),
              (name:   nameof(feature_selection_combinator)                                                                 ,value:  feature_selection_combinator,                                                                    type:   feature_selection_combinator.GetType().Name ?? "null"                                                                                      ),
              (name:   nameof(feature_selection_performance_metrics)                                                        ,value:  feature_selection_performance_metrics.ToString().Replace(",",";"),                type:   feature_selection_performance_metrics.GetType().Name ?? "null"                                                                             ),
              (name:   nameof(feature_selection_performance_classes)                                                        ,value:  feature_selection_performance_classes,                                                           type:   feature_selection_performance_classes?.GetType().Name ?? "null"                                                                            ),
              (name:   nameof(backwards_finished)                                                                           ,value:  backwards_finished,                                                                              type:   backwards_finished.GetType().Name ?? "null"                                                                                                ),
              (name:   nameof(forwards_finished)                                                                            ,value:  forwards_finished,                                                                               type:   forwards_finished.GetType().Name ?? "null"                                                                                                 ),

              //(name:   nameof(backwards_baseline_score)                                                                   ,value:  backwards_baseline_score,                                                                        type:   backwards_baseline_score.GetType().Name ?? "null"                                                                                          ),

              //(name:   nameof(forwards_baseline_score)                                                                    ,value:  forwards_baseline_score,                                                                         type:   forwards_baseline_score.GetType().Name ?? "null"                                                                                           ),

              (name:   nameof(baseline_score)                                                                               ,value:  baseline_score,                                                                                  type:   baseline_score.GetType().Name ?? "null"                                                                                                    ),

              (name:   nameof(baseline_recalculation_required)                                                              ,value:  baseline_recalculation_required,                                                                 type:   baseline_recalculation_required.GetType().Name ?? "null"                                                                                   ),
              (name:   nameof(backwards_score_improved)                                                                     ,value:  backwards_score_improved,                                                                        type:   backwards_score_improved.GetType().Name ?? "null"                                                                                          ),
              (name:   nameof(forwards_score_improved)                                                                      ,value:  forwards_score_improved,                                                                         type:   forwards_score_improved.GetType().Name ?? "null"                                                                                           ),
              (name:   nameof(score_improved)                                                                               ,value:  score_improved,                                                                                  type:   score_improved.GetType().Name ?? "null"                                                                                                    ),
              (name:   nameof(direction_change)                                                                             ,value:  direction_change,                                                                                type:   direction_change.GetType().Name ?? "null"                                                                                                  ),
              (name:   nameof(consecutive_iterations_with_improvement)                                                      ,value:  consecutive_iterations_with_improvement,                                                         type:   consecutive_iterations_with_improvement.GetType().Name ?? "null"                                                                           ),
              (name:   nameof(consecutive_iterations_without_improvement)                                                   ,value:  consecutive_iterations_without_improvement,                                                      type:   consecutive_iterations_without_improvement.GetType().Name ?? "null"                                                                        ),
              (name:   nameof(consecutive_forwards_iterations)                                                              ,value:  consecutive_forwards_iterations,                                                                 type:   consecutive_forwards_iterations.GetType().Name ?? "null"                                                                                   ),
              (name:   nameof(consecutive_forwards_iterations_with_improvement)                                             ,value:  consecutive_forwards_iterations_with_improvement,                                                type:   consecutive_forwards_iterations_with_improvement.GetType().Name ?? "null"                                                                  ),
              (name:   nameof(consecutive_forwards_iterations_without_improvement)                                          ,value:  consecutive_forwards_iterations_without_improvement,                                             type:   consecutive_forwards_iterations_without_improvement.GetType().Name ?? "null"                                                               ),
              (name:   nameof(consecutive_backwards_iterations)                                                             ,value:  consecutive_backwards_iterations,                                                                type:   consecutive_backwards_iterations.GetType().Name ?? "null"                                                                                  ),
              (name:   nameof(consecutive_backwards_iterations_with_improvement)                                            ,value:  consecutive_backwards_iterations_with_improvement,                                               type:   consecutive_backwards_iterations_with_improvement.GetType().Name ?? "null"                                                                 ),
              (name:   nameof(consecutive_backwards_iterations_without_improvement)                                         ,value:  consecutive_backwards_iterations_without_improvement,                                            type:   consecutive_backwards_iterations_without_improvement.GetType().Name ?? "null"                                                              ),
              (name:   nameof(unidirectional_convergence_reached)                                                           ,value:  unidirectional_convergence_reached,                                                              type:   unidirectional_convergence_reached.GetType().Name ?? "null"                                                                                ),
              (name:   nameof(iteration_loop)                                                                               ,value:  iteration_loop,                                                                                  type:   iteration_loop.GetType().Name ?? "null"                                                                                                    ),
              (name:   nameof(iteration_select)                                                                             ,value:  iteration_select,                                                                                type:   iteration_select.GetType().Name ?? "null"                                                                                                  ),
              (name:   nameof(iteration_recalculate)                                                                        ,value:  iteration_recalculate,                                                                           type:   iteration_recalculate.GetType().Name ?? "null"                                                                                             ),
              (name:   nameof(iteration_select_max)                                                                         ,value:  iteration_select_max,                                                                            type:   iteration_select_max.GetType().Name ?? "null"                                                                                              ),
              (name:   nameof(go_backwards)                                                                                 ,value:  go_backwards,                                                                                    type:   go_backwards.GetType().Name ?? "null"                                                                                                      ),
              (name:   nameof(go_forwards)                                                                                  ,value:  go_forwards,                                                                                     type:   go_forwards.GetType().Name ?? "null"                                                                                                       ),
              (name:   nameof(can_go_backwards)                                                                             ,value:  can_go_backwards,                                                                                type:   can_go_backwards.GetType().Name ?? "null"                                                                                                  ),
              (name:   nameof(can_go_forwards)                                                                              ,value:  can_go_forwards,                                                                                 type:   can_go_forwards.GetType().Name ?? "null"                                                                                                   ),
              (name:   nameof(forwards_max_features_reached)                                                                ,value:  forwards_max_features_reached,                                                                   type:   forwards_max_features_reached.GetType().Name ?? "null"                                                                                     ),
              (name:   nameof(backwards_min_features_reached)                                                               ,value:  backwards_min_features_reached,                                                                  type:   backwards_min_features_reached.GetType().Name ?? "null"                                                                                    ),
              //(name:   nameof(first)                                                                                        ,value:  first,                                                                                           type:   first.GetType().Name ?? "null"                                                                                                             ),
              //(name:   nameof(store_last_iteration)                                                                         ,value:  store_last_iteration,                                                                            type:   store_last_iteration.GetType().Name ?? "null"                                                                                              ),
              //(name:   nameof(store_history_log)                                                                            ,value:  store_history_log,                                                                               type:   store_history_log.GetType().Name ?? "null"                                                                                                 ),

              (name:   nameof(external_task_id)                                                                             ,value:  external_task_id,                                                                                type:   external_task_id.GetType().Name ?? "null"                                                                                                  ),
              (name:   nameof(external_forwards_task_id)                                                                    ,value:  external_forwards_task_id,                                                                       type:   external_forwards_task_id.GetType().Name ?? "null"                                                                                         ),
              (name:   nameof(external_backwards_task_id)                                                                   ,value:  external_backwards_task_id,                                                                      type:   external_backwards_task_id.GetType().Name ?? "null"                                                                                        ),
              (name:   nameof(sw_method)                                                                                    ,value:  sw_method.ElapsedMilliseconds,                                                                   type:   sw_method?.GetType().Name ?? "null"                                                                                                        ),

              (name:   nameof(dataset_instance_list)                                                               +".Count",value:  dataset_instance_list?.Count ?? 0,                                                               type:   dataset_instance_list?.GetType().Name ?? "null"                                                                                            ),
              (name:   nameof(features_input_untouched)                                                            +".Count",value:  features_input_untouched?.Count ?? 0,                                                            type:   features_input_untouched?.GetType().Name ?? "null"                                                                                         ),
              (name:   nameof(features_input)                                                                      +".Count",value:  features_input?.Count ?? 0,                                                                      type:   features_input?.GetType().Name ?? "null"                                                                                                   ),
              (name:   nameof(base_features)                                                                       +".Count",value:  base_features?.Count ?? 0,                                                                       type:   base_features?.GetType().Name ?? "null"                                                                                                    ),
              (name:   nameof(features_selected)                                                                   +".Count",value:  features_selected?.Count ?? 0,                                                                   type:   features_selected?.GetType().Name ?? "null"                                                                                                ),
              (name:   nameof(features_backwards_buffer)                                                           +".Count",value:  features_backwards_buffer?.Count ?? 0,                                                           type:   features_backwards_buffer?.GetType().Name ?? "null"                                                                                        ),
              (name:   nameof(features_forwards_buffer)                                                            +".Count",value:  features_forwards_buffer?.Count ?? 0,                                                            type:   features_forwards_buffer?.GetType().Name ?? "null"                                                                                         ),
              (name:   nameof(backwards_selection_feature_rankings)                                                +".Count",value:  backwards_selection_feature_rankings?.Count ?? 0,                                                type:   backwards_selection_feature_rankings?.GetType().Name ?? "null"                                                                             ),
              (name:   nameof(backwards_selection_feature_rankings_taken_for_removal)                              +".Count",value:  backwards_selection_feature_rankings_taken_for_removal?.Count ?? 0,                              type:   backwards_selection_feature_rankings_taken_for_removal?.GetType().Name ?? "null"                                                           ),
              (name:   nameof(backwards_selection_feature_rankings_could_have_taken_for_removal)                   +".Count",value:  backwards_selection_feature_rankings_could_have_taken_for_removal?.Count ?? 0,                   type:   backwards_selection_feature_rankings_could_have_taken_for_removal?.GetType().Name ?? "null"                                                ),
              (name:   nameof(backwards_selection_feature_rankings_bad_features)                                   +".Count",value:  backwards_selection_feature_rankings_bad_features?.Count ?? 0,                                   type:   backwards_selection_feature_rankings_bad_features?.GetType().Name ?? "null"                                                                ),
              (name:   nameof(backwards_selection_feature_rankings_good_features)                                  +".Count",value:  backwards_selection_feature_rankings_good_features?.Count ?? 0,                                  type:   backwards_selection_feature_rankings_good_features?.GetType().Name ?? "null"                                                               ),
              //(name:   nameof(backwards_selection_feature_rankings_last_iteration)                                 +".Count",value:  backwards_selection_feature_rankings_last_iteration?.Count ?? 0,                                 type:   backwards_selection_feature_rankings_last_iteration?.GetType().Name ?? "null"                                                              ),
              //(name:   nameof(backwards_selection_feature_rankings_taken_for_removal_last_iteration)               +".Count",value:  backwards_selection_feature_rankings_taken_for_removal_last_iteration?.Count ?? 0,               type:   backwards_selection_feature_rankings_taken_for_removal_last_iteration?.GetType().Name ?? "null"                                            ),
              //(name:   nameof(backwards_selection_feature_rankings_could_have_taken_for_removal_last_iteration)    +".Count",value:  backwards_selection_feature_rankings_could_have_taken_for_removal_last_iteration?.Count ?? 0,    type:   backwards_selection_feature_rankings_could_have_taken_for_removal_last_iteration?.GetType().Name ?? "null"                                 ),
              //(name:   nameof(backwards_selection_feature_rankings_bad_features_last_iteration)                    +".Count",value:  backwards_selection_feature_rankings_bad_features_last_iteration?.Count ?? 0,                    type:   backwards_selection_feature_rankings_bad_features_last_iteration?.GetType().Name ?? "null"                                                 ),
              //(name:   nameof(backwards_selection_feature_rankings_good_features_last_iteration)                   +".Count",value:  backwards_selection_feature_rankings_good_features_last_iteration?.Count ?? 0,                   type:   backwards_selection_feature_rankings_good_features_last_iteration?.GetType().Name ?? "null"                                                ),
              //(name:   nameof(backwards_selection_feature_rankings_history_log)                                    +".Count",value:  backwards_selection_feature_rankings_history_log?.Count ?? 0,                                    type:   backwards_selection_feature_rankings_history_log?.GetType().Name ?? "null"                                                                 ),
              //(name:   nameof(backwards_selection_feature_rankings_taken_for_removal_history_log)                  +".Count",value:  backwards_selection_feature_rankings_taken_for_removal_history_log?.Count ?? 0,                  type:   backwards_selection_feature_rankings_taken_for_removal_history_log?.GetType().Name ?? "null"                                               ),
              //(name:   nameof(backwards_selection_feature_rankings_could_have_taken_for_removal_history_log)       +".Count",value:  backwards_selection_feature_rankings_could_have_taken_for_removal_history_log?.Count ?? 0,       type:   backwards_selection_feature_rankings_could_have_taken_for_removal_history_log?.GetType().Name ?? "null"                                    ),
              //(name:   nameof(backwards_selection_feature_rankings_bad_features_history_log)                       +".Count",value:  backwards_selection_feature_rankings_bad_features_history_log?.Count ?? 0,                       type:   backwards_selection_feature_rankings_bad_features_history_log?.GetType().Name ?? "null"                                                    ),
              //(name:   nameof(backwards_selection_feature_rankings_good_features_history_log)                      +".Count",value:  backwards_selection_feature_rankings_good_features_history_log?.Count ?? 0,                      type:   backwards_selection_feature_rankings_good_features_history_log?.GetType().Name ?? "null"                                                   ),
              (name:   nameof(forwards_selection_feature_rankings)                                                 +".Count",value:  forwards_selection_feature_rankings?.Count ?? 0,                                                 type:   forwards_selection_feature_rankings?.GetType().Name ?? "null"                                                                              ),
              (name:   nameof(forwards_selection_feature_rankings_taken_for_insertion)                             +".Count",value:  forwards_selection_feature_rankings_taken_for_insertion?.Count ?? 0,                             type:   forwards_selection_feature_rankings_taken_for_insertion?.GetType().Name ?? "null"                                                          ),
              (name:   nameof(forwards_selection_feature_rankings_could_have_taken_for_insertion)                  +".Count",value:  forwards_selection_feature_rankings_could_have_taken_for_insertion?.Count ?? 0,                  type:   forwards_selection_feature_rankings_could_have_taken_for_insertion?.GetType().Name ?? "null"                                               ),
              (name:   nameof(forwards_selection_feature_rankings_bad_features)                                    +".Count",value:  forwards_selection_feature_rankings_bad_features?.Count ?? 0,                                    type:   forwards_selection_feature_rankings_bad_features?.GetType().Name ?? "null"                                                                 ),
              (name:   nameof(forwards_selection_feature_rankings_good_features)                                   +".Count",value:  forwards_selection_feature_rankings_good_features?.Count ?? 0,                                   type:   forwards_selection_feature_rankings_good_features?.GetType().Name ?? "null"                                                                ),
              //(name:   nameof(forwards_selection_feature_rankings_last_iteration)                                  +".Count",value:  forwards_selection_feature_rankings_last_iteration?.Count ?? 0,                                  type:   forwards_selection_feature_rankings_last_iteration?.GetType().Name ?? "null"                                                               ),
              //(name:   nameof(forwards_selection_feature_rankings_taken_for_insertion_last_iteration)              +".Count",value:  forwards_selection_feature_rankings_taken_for_insertion_last_iteration?.Count ?? 0,              type:   forwards_selection_feature_rankings_taken_for_insertion_last_iteration?.GetType().Name ?? "null"                                           ),
              //(name:   nameof(forwards_selection_feature_rankings_could_have_taken_for_insertion_last_iteration)   +".Count",value:  forwards_selection_feature_rankings_could_have_taken_for_insertion_last_iteration?.Count ?? 0,   type:   forwards_selection_feature_rankings_could_have_taken_for_insertion_last_iteration?.GetType().Name ?? "null"                                ),
              //(name:   nameof(forwards_selection_feature_rankings_bad_features_last_iteration)                     +".Count",value:  forwards_selection_feature_rankings_bad_features_last_iteration?.Count ?? 0,                     type:   forwards_selection_feature_rankings_bad_features_last_iteration?.GetType().Name ?? "null"                                                  ),
              //(name:   nameof(forwards_selection_feature_rankings_good_features_last_iteration)                    +".Count",value:  forwards_selection_feature_rankings_good_features_last_iteration?.Count ?? 0,                    type:   forwards_selection_feature_rankings_good_features_last_iteration?.GetType().Name ?? "null"                                                 ),
              //(name:   nameof(forwards_selection_feature_rankings_history_log)                                     +".Count",value:  forwards_selection_feature_rankings_history_log?.Count ?? 0,                                     type:   forwards_selection_feature_rankings_history_log?.GetType().Name ?? "null"                                                                  ),
              //(name:   nameof(forwards_selection_feature_rankings_taken_for_insertion_history_log)                 +".Count",value:  forwards_selection_feature_rankings_taken_for_insertion_history_log?.Count ?? 0,                 type:   forwards_selection_feature_rankings_taken_for_insertion_history_log?.GetType().Name ?? "null"                                              ),
              //(name:   nameof(forwards_selection_feature_rankings_could_have_taken_for_insertion_history_log)      +".Count",value:  forwards_selection_feature_rankings_could_have_taken_for_insertion_history_log?.Count ?? 0,      type:   forwards_selection_feature_rankings_could_have_taken_for_insertion_history_log?.GetType().Name ?? "null"                                   ),
              //(name:   nameof(forwards_selection_feature_rankings_bad_features_history_log)                        +".Count",value:  forwards_selection_feature_rankings_bad_features_history_log?.Count ?? 0,                        type:   forwards_selection_feature_rankings_bad_features_history_log?.GetType().Name ?? "null"                                                     ),
              //(name:   nameof(forwards_selection_feature_rankings_good_features_history_log)                       +".Count",value:  forwards_selection_feature_rankings_good_features_history_log?.Count ?? 0,                       type:   forwards_selection_feature_rankings_good_features_history_log?.GetType().Name ?? "null"                                                    ),
              //(name:   nameof(feature_importance_first_iteration)                                                  +".Count",value:  feature_importance_first_iteration?.Count ?? 0,                                                  type:   feature_importance_first_iteration?.GetType().Name ?? "null"                                                                               ),
              //(name:   nameof(feature_importance_average)                                                          +".Count",value:  feature_importance_average?.Count ?? 0,                                                          type:   feature_importance_average?.GetType().Name ?? "null"                                                                                       ),
              //(name:   nameof(feature_importance_actual)                                                           +".Count",value:  feature_importance_actual?.Count ?? 0,                                                           type:   feature_importance_actual?.GetType().Name ?? "null"                                                                                        ),
              (name:   nameof(forwards_feature_bad_feature_log)                                                    +".Count",value:  forwards_feature_bad_feature_log?.Count ?? 0,                                                    type:   forwards_feature_bad_feature_log?.GetType().Name ?? "null"                                                                                 ),
              (name:   nameof(forwards_feature_bad_feature_permanent)                                              +".Count",value:  forwards_feature_bad_feature_permanent?.Count ?? 0,                                              type:   forwards_feature_bad_feature_permanent?.GetType().Name ?? "null"                                                                           ),
              (name:   nameof(forwards_feature_good_feature_log)                                                   +".Count",value:  forwards_feature_good_feature_log?.Count ?? 0,                                                   type:   forwards_feature_good_feature_log?.GetType().Name ?? "null"                                                                                ),
              (name:   nameof(forwards_feature_good_feature_permanent)                                             +".Count",value:  forwards_feature_good_feature_permanent?.Count ?? 0,                                             type:   forwards_feature_good_feature_permanent?.GetType().Name ?? "null"                                                                          ),
              (name:   nameof(backwards_feature_bad_feature_log)                                                   +".Count",value:  backwards_feature_bad_feature_log?.Count ?? 0,                                                   type:   backwards_feature_bad_feature_log?.GetType().Name ?? "null"                                                                                ),
              (name:   nameof(backwards_feature_bad_feature_permanent)                                             +".Count",value:  backwards_feature_bad_feature_permanent?.Count ?? 0,                                             type:   backwards_feature_bad_feature_permanent?.GetType().Name ?? "null"                                                                          ),
              (name:   nameof(backwards_feature_good_feature_log)                                                  +".Count",value:  backwards_feature_good_feature_log?.Count ?? 0,                                                  type:   backwards_feature_good_feature_log?.GetType().Name ?? "null"                                                                               ),
              (name:   nameof(backwards_feature_good_feature_permanent)                                            +".Count",value:  backwards_feature_good_feature_permanent?.Count ?? 0,                                            type:   backwards_feature_good_feature_permanent?.GetType().Name ?? "null"                                                                         ),
            };

            var h = score_metrics.csv_header.Split(',');
            //var b1 = backwards_baseline_score.ToString().Split(',');
            //var b2 = forwards_baseline_score.ToString().Split(',');
            var b3 = baseline_score.ToString().Split(',');

            for (var i = 0; i < h.Length; i++)
            {
                //result.Add(("backwards_" + h[i], b1[i], b1[i].GetType().Name));
                //result.Add(("forwards_" + h[i], b2[i], b2[i].GetType().Name));
                result.Add(("base_" + h[i], b3[i], b3[i].GetType().Name));

            }

            //if (!store_last_iteration)
            //{
            //    result = result.Where(a => !a.name.EndsWith("last_iteration")).ToList();

            //}

            //if (!store_history_log)
            //{
            //    result = result.Where(a => !a.name.EndsWith("history_log")).ToList();
            //}

            return result.ToArray();
        }

        public static object save_task_lock = new object();

        public void save_task(iterative_task result)
        {
            if (result==null)return;
            
            program.GC_Collection();

            Directory.CreateDirectory(this.results_output_folder);


            var save_task_data = true;

            if (save_task_data)
            {
                var h = iterative_task.csv_header + "," + performance_measure.confusion_matrix.csv_header;
                var x = result.ToString();

                var save_all_cm = true;
                var save_all_cm_average = true;

                if (save_all_cm)
                {
                    var all_cm_filename = Path.Combine(this.results_output_folder, $"{start_date_time}_all_cm.csv");//_{result.iteration_loop}_{result.iteration_recalculate}_{result.iteration_select}.csv");

                    var all_cm_data = result.all_cm.Select(a => $"{x},{a.ToString()}").ToList();

                    lock (save_task_lock)
                    {
                        if (!File.Exists(all_cm_filename) || new FileInfo(all_cm_filename).Length == 0)
                        {
                            var written_h = false;
                            do
                            {
                                try
                                {
                                    File.AppendAllLines(all_cm_filename, new string[] {h});
                                    written_h = true;
                                }
                                catch (Exception e)
                                {
                                    program.WriteLine($"save_task(): {e.ToString()}", true, ConsoleColor.Red);
                                    written_h = false;
                                }
                            } while (!written_h);
                        }

                        var written_d = false;

                        do
                        {
                            try
                            {
                                File.AppendAllLines(all_cm_filename, all_cm_data);
                                written_d = true;
                            }
                            catch (Exception e)
                            {
                                program.WriteLine($"save_task(): {e.ToString()}", true, ConsoleColor.Red);
                                written_d = false;
                            }
                        } while (!written_d);
                    }
                }

                if (save_all_cm_average)
                {
                    var all_cm_average_filename = Path.Combine(this.results_output_folder, $"{start_date_time}_all_cm_average.csv");//_{result.iteration_loop}_{result.iteration_recalculate}_{result.iteration_select}.csv");

                    var all_cm_average_data = result.all_cm_average.Select(a => $"{x},{a.ToString()}").ToList();

                    lock (save_task_lock)
                    {
                        if (!File.Exists(all_cm_average_filename) || new FileInfo(all_cm_average_filename).Length == 0)
                        {
                            var written_h = false;
                            do
                            {
                                try
                                {
                                    File.AppendAllLines(all_cm_average_filename, new string[] { h });
                                    written_h = true;
                                }
                                catch (Exception e)
                                {
                                    program.WriteLine($"save_task(): {e.ToString()}", true, ConsoleColor.Red);
                                    written_h = false;
                                }
                            } while (!written_h);
                        }

                        var written_d = false;

                        do
                        {
                            try
                            {
                                File.AppendAllLines(all_cm_average_filename, all_cm_average_data);
                                written_d = true;
                            }
                            catch (Exception e)
                            {
                                program.WriteLine($"save_task(): {e.ToString()}", true, ConsoleColor.Red);
                                written_d = false;
                            }
                        } while (!written_d);
                    }
                }

            }
        }

        public static object save_iteration_parameters_lock = new object();

        public void save_iteration_parameters()
        {
            program.GC_Collection();

            //var rank_first = get_rank(rank_types.first_iteration, perf_selection_rule);
            //var rank_average = get_rank(rank_types.overall_average, perf_selection_rule);
            //var rank_last = get_rank(rank_types.last_test, perf_selection_rule);

            Directory.CreateDirectory(this.results_output_folder);

            var save_params = false;

            if (save_params)
            {
                // save csv of parameters
                var p = AsArray();
                var header = p.Select(a => a.name.Replace(",", ";")).ToList();
                var types = p.Select(a => a.type.Replace(",", ";")).ToList();
                var values = p.Select(a => a.value.ToString().Replace(",", ";")).ToList();
                var p_lines = new List<string>();
                p_lines.Add(string.Join(",", header));
                p_lines.Add(string.Join(",", types));
                p_lines.Add(string.Join(",", values));
                lock (save_iteration_parameters_lock)
                {
                    File.WriteAllLines(Path.Combine(this.results_output_folder, $"{start_date_time}_parameters_{iteration_loop}_{iteration_recalculate}_{iteration_select}.csv"), p_lines);
                }
            }

            var save_selected_features_cm_average = true;

            if (save_selected_features_cm_average)
            {
                var feat_header =
                    $@"feature_selection_type,perf_selection_rule,feature_selection_performance_metrics,feature_selection_combinator,experiment_id1,experiment_id2,experiment_id3,iteration_loop,alphabet,dimension,category,source,group,set_id,{performance_measure.confusion_matrix.csv_header}";

                {
                    var f_lines_cm_average = this.features_selected_average_confusion_matrices.SelectMany(a => a.cm_list
                        .Select(b =>
                            $@"{feature_selection_type.ToString().Replace(",", ";")},{perf_selection_rule.ToString().Replace(",", ";")},{feature_selection_performance_metrics.ToString().Replace(",", ";")},{feature_selection_combinator.ToString().Replace(",", ";")},{a.experiment_id1},{a.experiment_id2},{a.experiment_id3},{a.iteration},{a.feature_set.alphabet},{a.feature_set.dimension},{a.feature_set.category},{a.feature_set.source},{a.feature_set.@group},{a.feature_set.set_id},{b.ToString()}"
                        ).ToList()).ToList();

                    f_lines_cm_average.Insert(0, feat_header);

                    lock (save_iteration_parameters_lock)
                    {
                        File.WriteAllLines(
                            Path.Combine(this.results_output_folder,
                                $"{start_date_time}_selected_features_CM_average_{iteration_loop}_{iteration_recalculate}_{iteration_select}.csv"),
                            f_lines_cm_average);
                    }

                }
            }

            var save_selected_features_cm_all = true;
            if (save_selected_features_cm_all)

            {
                var feat_header =
                    $@"feature_selection_type,perf_selection_rule,feature_selection_performance_metrics,feature_selection_combinator,experiment_id1,experiment_id2,experiment_id3,iteration_loop,alphabet,dimension,category,source,group,set_id,{performance_measure.confusion_matrix.csv_header}";

                var f_lines_cm_all = this.features_selected_average_confusion_matrices.SelectMany(a => a.cm_all.Select(b =>
                    $@"{feature_selection_type.ToString().Replace(",", ";")},{perf_selection_rule.ToString().Replace(",", ";")},{feature_selection_performance_metrics.ToString().Replace(",", ";")},{feature_selection_combinator.ToString().Replace(",", ";")},{a.experiment_id1},{a.experiment_id2},{a.experiment_id3},{a.iteration},{a.feature_set.alphabet},{a.feature_set.dimension},{a.feature_set.category},{a.feature_set.source},{a.feature_set.@group},{a.feature_set.set_id},{b.ToString()}").ToList()).ToList();

                f_lines_cm_all.Insert(0, feat_header);

                lock (save_iteration_parameters_lock)
                {
                    File.WriteAllLines(Path.Combine(this.results_output_folder, $"{start_date_time}_selected_features_CM_all_{iteration_loop}_{iteration_recalculate}_{iteration_select}.csv"), f_lines_cm_all);
                }
            }


            var save_selected_features_list = true;

            if (save_selected_features_list)
            {
                // save csv of features
                var feat_header = $"feature_selection_type,perf_selection_rule,feature_selection_performance_metrics,feature_selection_combinator,source,group,set_id,fid,alphabet,dimension,category,source,group,member,perspective,alphabet_id,dimension_id,category_id,source_id,group_id,member_id,perspective_id";
                var feat_list = this.features_selected.SelectMany(a => a.set_members.Select(b => $"{feature_selection_type.ToString().Replace(",", ";")},{perf_selection_rule.ToString().Replace(",", ";")},{feature_selection_performance_metrics.ToString().Replace(",", ";")},{feature_selection_combinator.ToString().Replace(",", ";")},{a.source},{a.@group},{a.set_id},{b.fid},{b.alphabet},{b.dimension},{b.category},{b.source},{b.@group},{b.member},{b.perspective},{b.alphabet_id},{b.dimension_id},{b.category_id},{b.source_id},{b.@group_id},{b.member_id},{b.perspective_id}").ToList()).ToList();

                var f_lines = new List<string>();
                f_lines.Add(feat_header);
                f_lines.AddRange(feat_list);
                lock (save_iteration_parameters_lock)
                {
                    File.WriteAllLines(Path.Combine(this.results_output_folder, $"{start_date_time}_selected_features_{iteration_loop}_{iteration_recalculate}_{iteration_select}.csv"), f_lines);
                }
            }

            var save_ranks1 = true;

            if (save_ranks1)
            {
                save_ranks();
            }


            if (save_iteration_json)
            {
                // save json
                var json_serialised = "";

                try
                {
                    json_serialised = feature_selection_unidirectional.serialise_json(this);
                }
                catch (Exception e)
                {
                    program.WriteLine(e.ToString(), true, ConsoleColor.Red);
                }

                if (!string.IsNullOrEmpty(json_serialised))
                {
                    lock (save_iteration_parameters_lock)
                    {
                        try
                        {
                            File.WriteAllText(
                                Path.Combine(this.results_output_folder,
                                    $"{start_date_time}_data_json_{iteration_loop}_{iteration_recalculate}_{iteration_select}.json"),
                                json_serialised);
                        }
                        catch (Exception e)
                        {
                            program.WriteLine(e.ToString(), true, ConsoleColor.Red);
                        }

                    }
                }
            }
        }

        public bool save_iteration_json;

        public string results_output_folder;



        [JsonIgnore] public List<(List<(string comment_header, string comment_value)> comment_columns, List<(int fid, double fv)> feature_data)> dataset_instance_list;

        public List<feature_set> features_input_untouched;
        public List<feature_set> features_input;
        public List<feature_set> base_features;

        public cross_validation.run_svm_params run_svm_params;
        //public int rand_cv;
        //public int outer_cv;
        //public int inner_cv;


        public feature_selection_types feature_selection_type;
        public perf_selection_rules perf_selection_rule;

        public int backwards_max_features_to_combine_per_iteration;
        public int forwards_max_features_to_combine_per_iteration;

        public int backwards_max_features_to_remove_per_iteration;
        public int forwards_max_features_to_add_per_iteration;
        public int backwards_min_features;
        public int forwards_max_features;

        public int backwards_max_feature_removal_attempts;
        public int forwards_max_features_insertion_attempts;
        public double random_baseline;
        public double margin_of_error;
        public int max_tasks;
        public feature_selection_combinators feature_selection_combinator;

        public List<(int iteration, long ticks)> task_durations;

        public List<iterative_task> backwards_selection_feature_rankings;
        public List<iterative_task> backwards_selection_feature_rankings_taken_for_removal;
        public List<iterative_task> backwards_selection_feature_rankings_could_have_taken_for_removal;
        public List<iterative_task> backwards_selection_feature_rankings_bad_features;
        public List<iterative_task> backwards_selection_feature_rankings_good_features;

        //public List<iterative_task> backwards_selection_feature_rankings_last_iteration;
        //public List<iterative_task> backwards_selection_feature_rankings_taken_for_removal_last_iteration;
        //public List<iterative_task> backwards_selection_feature_rankings_could_have_taken_for_removal_last_iteration;
        //public List<iterative_task> backwards_selection_feature_rankings_bad_features_last_iteration;
        //public List<iterative_task> backwards_selection_feature_rankings_good_features_last_iteration;
        //public List<List<iterative_task>> backwards_selection_feature_rankings_history_log;
        //public List<List<iterative_task>> backwards_selection_feature_rankings_taken_for_removal_history_log;
        //public List<List<iterative_task>> backwards_selection_feature_rankings_could_have_taken_for_removal_history_log;
        //public List<List<iterative_task>> backwards_selection_feature_rankings_bad_features_history_log;
        //public List<List<iterative_task>> backwards_selection_feature_rankings_good_features_history_log;


        public List<iterative_task> forwards_selection_feature_rankings;
        public List<iterative_task> forwards_selection_feature_rankings_taken_for_insertion;
        public List<iterative_task> forwards_selection_feature_rankings_could_have_taken_for_insertion;
        public List<iterative_task> forwards_selection_feature_rankings_bad_features;
        public List<iterative_task> forwards_selection_feature_rankings_good_features;

        //public List<iterative_task> forwards_selection_feature_rankings_last_iteration;
        //public List<iterative_task> forwards_selection_feature_rankings_taken_for_insertion_last_iteration;
        //public List<iterative_task> forwards_selection_feature_rankings_could_have_taken_for_insertion_last_iteration;
        //public List<iterative_task> forwards_selection_feature_rankings_bad_features_last_iteration;
        //public List<iterative_task> forwards_selection_feature_rankings_good_features_last_iteration;
        //public List<List<iterative_task>> forwards_selection_feature_rankings_history_log;
        //public List<List<iterative_task>> forwards_selection_feature_rankings_taken_for_insertion_history_log;
        //public List<List<iterative_task>> forwards_selection_feature_rankings_could_have_taken_for_insertion_history_log;
        //public List<List<iterative_task>> forwards_selection_feature_rankings_bad_features_history_log;
        //public List<List<iterative_task>> forwards_selection_feature_rankings_good_features_history_log;

        public List<feature_set> features_selected;

        public List<(string experiment_id1, string experiment_id2, string experiment_id3, int iteration, char direction, feature_set feature_set, List<performance_measure.confusion_matrix> cm_list, List<performance_measure.confusion_matrix> cm_all)> features_selected_average_confusion_matrices;


        public List<feature_set> features_backwards_buffer;
        public List<feature_set> features_forwards_buffer;

        public performance_measure.confusion_matrix.cross_validation_metrics feature_selection_performance_metrics;
        public List<int> feature_selection_performance_classes;

        public bool backwards_finished;
        public bool forwards_finished;

        //public score_metrics previous_backwards_baseline_score;
        //public score_metrics previous_forwards_baseline_score;
        //public score_metrics previous_baseline_score;

        //public score_metrics backwards_baseline_score;
        //public score_metrics forwards_baseline_score;
        public score_metrics baseline_score;


        public bool baseline_recalculation_required;
        public bool backwards_score_improved;
        public bool forwards_score_improved;
        public bool score_improved;
        public bool direction_change;
        public int consecutive_iterations_with_improvement;
        public int consecutive_iterations_without_improvement;
        public int consecutive_forwards_iterations;
        public int consecutive_forwards_iterations_with_improvement;
        public int consecutive_forwards_iterations_without_improvement;
        public int consecutive_backwards_iterations;
        public int consecutive_backwards_iterations_with_improvement;
        public int consecutive_backwards_iterations_without_improvement;
        public bool unidirectional_convergence_reached;
        public int iteration_loop;
        public int iteration_select;
        public int iteration_recalculate;
        public int iteration_select_max;

        public bool go_backwards;
        public bool go_forwards;
        public bool can_go_backwards;
        public bool can_go_forwards;
        public bool forwards_max_features_reached;
        public bool backwards_min_features_reached;
        //public bool first;

        //public bool store_last_iteration;
        //public bool store_history_log;

        public int external_task_id;
        public int external_forwards_task_id;
        public int external_backwards_task_id;

        //public string experiment_id1;
        //public string experiment_id2;

        //public reordering_rules reorder_input_each_iteration;


        //List<((string source, string group) set_name, List<(int fid, string alphabet, string dimension, string category, string source, string group, string member, string perspective, int alphabet_id, int dimension_id, int category_id, int source_id, int group_id, int member_id, int perspective_id)> set_members)>

        // possible fields: group name, iterative_task data, score_metrics, confusion_matrix_average, confusion_matrix_all
        // need to know the change in score (- or +) ... 

        // The ranks show what (if any, positive or negative) improvement/change there is in the chosen classification performance metrics score
        // e.g. +0.2 ... -0.4... 
        // However, the rank may not tell the whole story, so the confusion matrix is also needed for comparison

        public enum rank_types : int
        {
            first_iteration,
            overall_average,
            last_test,
            all
        }

        public List<(List<(perf_selection_rules perf_rule, int rank)> perf_rule_ranks, string experiment_id1, string experiment_id2, string experiment_id3, char direction, feature_set feature_set, int iteration, score_metrics score_metrics, performance_measure.confusion_matrix cm_average, List<performance_measure.confusion_matrix> cm_all)> get_rank(rank_types rank_type)//, perf_selection_rules perf_selection_rule)
        {
            List<(string experiment_id1, string experiment_id2, string experiment_id3, char direction, feature_set feature_set, int iteration, score_metrics score_metrics, performance_measure.confusion_matrix cm_average, List<performance_measure.confusion_matrix> cm_all)> ranks = null;


            switch (rank_type)
            {
                case rank_types.first_iteration:

                    var first_iteration_id = ranking_data.OrderBy(a => a.iteration).First().iteration;

                    var first_ranks = ranking_data.Where(a => a.iteration == first_iteration_id).ToList();
                    //first_ranks = first_ranks.OrderByDescending(a => a.score_metrics.score_change).ToList();

                    ranks = first_ranks;

                    break;

                case rank_types.overall_average:

                    var g1 = ranking_data.GroupBy(a => a.feature_set.set_id).Select(a => (
                    experiment_id1: string.Join(";", a.Select(c => c.experiment_id1).Distinct().ToList()),
                    experiment_id2: string.Join(";", a.Select(c => c.experiment_id2).Distinct().ToList()),
                    experiment_id3: string.Join(";", a.Select(c => c.experiment_id3).Distinct().ToList()),
                    direction: a.First().direction, feature_set: a.First().feature_set, iteration: a.Select(b => b.iteration).Max(), score_metrics: score_metrics.Average(a.Select(b => b.score_metrics).ToList()), cm_average: performance_measure.confusion_matrix.Average2(a.Select(b => b.cm_average).ToList()), cm_all: a.SelectMany(b => b.cm_all).ToList())).ToList();
                    //g1 = g1.OrderByDescending(a => a.score_metrics.score_change).ToList();

                    ranks = g1;

                    break;

                case rank_types.last_test:

                    var g2 = ranking_data.GroupBy(a => a.feature_set.set_id).Select(a => a.OrderByDescending(b => b.iteration).Last()).ToList();
                    //g2 = g2.OrderByDescending(a => a.score_metrics.score_change).ToList();

                    ranks = g2;

                    break;

                case rank_types.all:

                    ranks = ranking_data;

                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(rank_type), rank_type, null);
            }

            List<(List<(perf_selection_rules perf_rule, int rank)> perf_rule_ranks, string experiment_id1, string experiment_id2, string experiment_id3, char direction, feature_set feature_set, int iteration, score_metrics score_metrics, performance_measure.confusion_matrix cm_average, List<performance_measure.confusion_matrix> cm_all)> ranks2 = null;
            ranks2 = ranks.Select(a => (new List<(perf_selection_rules perf_rule, int rank)>(), a.experiment_id1, a.experiment_id2, a.experiment_id3, a.direction, a.feature_set, a.iteration, a.score_metrics, a.cm_average, a.cm_all)).ToList();

            foreach (perf_selection_rules perf_select_rule1 in Enum.GetValues(typeof(perf_selection_rules)))
            {
                switch (perf_select_rule1)
                {
                    case perf_selection_rules.none:
                        continue;

                    case perf_selection_rules.best_score:
                        ranks2 = ranks2.OrderByDescending(a => a.score_metrics.score_after).ThenByDescending(a => a.score_metrics.score_change).ThenBy(a => a.score_metrics.total_features_change).ToList();
                        break;

                    case perf_selection_rules.best_ppf_overall:
                        ranks2 = ranks2.OrderByDescending(a => a.score_metrics.score_ppf_change).ThenBy(a => a.score_metrics.total_features_change).ToList();
                        break;

                    case perf_selection_rules.best_ppg_overall:
                        ranks2 = ranks2.OrderByDescending(a => a.score_metrics.score_ppg_change).ThenBy(a => a.score_metrics.total_features_change).ToList();
                        break;

                    case perf_selection_rules.best_ppf_change:
                        ranks2 = ranks2.OrderByDescending(a => a.score_metrics.score_change_ppf).ThenBy(a => a.score_metrics.total_features_change).ToList();
                        break;

                    case perf_selection_rules.best_ppg_change:
                        ranks2 = ranks2.OrderByDescending(a => a.score_metrics.score_change_ppg).ThenBy(a => a.score_metrics.total_features_change).ToList();
                        break;

                    case perf_selection_rules.best_average_of_score_and_ppf:
                        ranks2 = ranks2.OrderByDescending(a => new double[] { a.score_metrics.score_after, a.score_metrics.score_ppf_change }.Average()).ThenBy(a => a.score_metrics.total_features_change).ToList();
                        break;

                    case perf_selection_rules.best_average_of_score_and_ppg:
                        ranks2 = ranks2.OrderByDescending(a => new double[] { a.score_metrics.score_after, a.score_metrics.score_ppg_change }.Average()).ThenBy(a => a.score_metrics.total_features_change).ToList();
                        break;

                    case perf_selection_rules.best_average_of_score_and_ppf_normalised:
                        var scores1 = ranks.Select(a => a.score_metrics.score_after).ToList();
                        var score_added_ppfs = ranks.Select(a => a.score_metrics.score_ppf_change).ToList();

                        ranks2 = ranks2.OrderByDescending(a => new double[] { scale_value(a.score_metrics.score_after, scores1.Min(), scores1.Max()), scale_value(a.score_metrics.score_ppf_change, score_added_ppfs.Min(), score_added_ppfs.Max()), }.Average()).ThenBy(a => a.score_metrics.total_features_change).ToList();
                        break;

                    case perf_selection_rules.best_average_of_score_and_ppg_normalised:
                        var scores2 = ranks.Select(a => a.score_metrics.score_after).ToList();
                        var score_added_ppgs = ranks.Select(a => a.score_metrics.score_ppg_change).ToList();

                        ranks2 = ranks2.OrderByDescending(a => new double[] { scale_value(a.score_metrics.score_after, scores2.Min(), scores2.Max()), scale_value(a.score_metrics.score_ppg_change, score_added_ppgs.Min(), score_added_ppgs.Max()), }.Average()).ThenBy(a => a.score_metrics.total_features_change).ToList();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                for (var r = 0; r < ranks2.Count; r++)
                {
                    var rk = ranks2[r];
                    rk.perf_rule_ranks.Add((perf_rule: perf_select_rule1, r));
                }
            }

            // send ranks back in the order of 'perf_selection_rule' (different from 'perf_select_rule')
            switch (perf_selection_rule)
            {
                case perf_selection_rules.best_score:
                    ranks2 = ranks2.OrderByDescending(a => a.score_metrics.score_after).ThenByDescending(a => a.score_metrics.score_change).ThenBy(a => a.score_metrics.total_features_change).ToList();
                    break;

                case perf_selection_rules.best_ppf_overall:
                    ranks2 = ranks2.OrderByDescending(a => a.score_metrics.score_ppf_change).ThenBy(a => a.score_metrics.total_features_change).ToList();
                    break;

                case perf_selection_rules.best_ppg_overall:
                    ranks2 = ranks2.OrderByDescending(a => a.score_metrics.score_ppg_change).ThenBy(a => a.score_metrics.total_features_change).ToList();
                    break;

                case perf_selection_rules.best_ppf_change:
                    ranks2 = ranks2.OrderByDescending(a => a.score_metrics.score_change_ppf).ThenBy(a => a.score_metrics.total_features_change).ToList();
                    break;

                case perf_selection_rules.best_ppg_change:
                    ranks2 = ranks2.OrderByDescending(a => a.score_metrics.score_change_ppg).ThenBy(a => a.score_metrics.total_features_change).ToList();
                    break;

                case perf_selection_rules.best_average_of_score_and_ppf:
                    ranks2 = ranks2.OrderByDescending(a => new double[] { a.score_metrics.score_after, a.score_metrics.score_ppf_change }.Average()).ThenBy(a => a.score_metrics.total_features_change).ToList();
                    break;

                case perf_selection_rules.best_average_of_score_and_ppg:
                    ranks2 = ranks2.OrderByDescending(a => new double[] { a.score_metrics.score_after, a.score_metrics.score_ppg_change }.Average()).ThenBy(a => a.score_metrics.total_features_change).ToList();
                    break;

                case perf_selection_rules.best_average_of_score_and_ppf_normalised:
                    var scores1 = ranks.Select(a => a.score_metrics.score_after).ToList();
                    var score_added_ppfs = ranks.Select(a => a.score_metrics.score_ppf_change).ToList();

                    ranks2 = ranks2.OrderByDescending(a => new double[] { scale_value(a.score_metrics.score_after, scores1.Min(), scores1.Max()), scale_value(a.score_metrics.score_ppf_change, score_added_ppfs.Min(), score_added_ppfs.Max()), }.Average()).ThenBy(a => a.score_metrics.total_features_change).ToList();
                    break;

                case perf_selection_rules.best_average_of_score_and_ppg_normalised:
                    var scores2 = ranks.Select(a => a.score_metrics.score_after).ToList();
                    var score_added_ppgs = ranks.Select(a => a.score_metrics.score_ppg_change).ToList();

                    ranks2 = ranks2.OrderByDescending(a => new double[] { scale_value(a.score_metrics.score_after, scores2.Min(), scores2.Max()), scale_value(a.score_metrics.score_ppg_change, score_added_ppgs.Min(), score_added_ppgs.Max()), }.Average()).ThenBy(a => a.score_metrics.total_features_change).ToList();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return ranks2;
        }




        public void save_ranks()
        {
            Directory.CreateDirectory(results_output_folder);

            var p = string.Join(",", Enum.GetNames(typeof(perf_selection_rules)).Where(a => a != perf_selection_rules.none.ToString()).Select(a => a == perf_selection_rule.ToString() ? a + "*" : a).ToList());

            var cm_average_csv_header = $@"{p},CM,feature_selection_type,perf_selection_rule,feature_selection_performance_metrics,feature_selection_combinator,direction,iteration,is_selected,alphabet,category,dimension,source,group,set_id,{score_metrics.csv_header},{performance_measure.confusion_matrix.csv_header}";



            foreach (rank_types rank_type in Enum.GetValues(typeof(rank_types)))
            {
                var ranks2 = get_rank(rank_type);//, perf_select_rule);

                var rank_cm_average_csv = ranks2.Select((a, i) => $@"{string.Join(",", a.perf_rule_ranks.Select(c => c.rank).ToList())},0,{feature_selection_type.ToString().Replace(",", ";")},{perf_selection_rule.ToString().Replace(",", ";")},{feature_selection_performance_metrics.ToString().Replace(",", ";")},{feature_selection_combinator.ToString().Replace(",", ";")},{a.direction},{a.iteration},{features_selected.Any(c => c.set_id == a.feature_set.set_id)},{a.feature_set.alphabet},{a.feature_set.category},{a.feature_set.dimension},{a.feature_set.source},{a.feature_set.@group},{a.feature_set.set_id},{a.score_metrics.ToString()},{a.cm_average.ToString()}").ToList();

                rank_cm_average_csv.Insert(0, cm_average_csv_header);

                lock (save_iteration_parameters_lock)
                {
                    File.WriteAllLines(Path.Combine(results_output_folder, $@"{start_date_time}_rank_cm_average_{rank_type.ToString()}_{iteration_loop}_{iteration_recalculate}_{iteration_select}.csv"), rank_cm_average_csv);
                }

                var rank_cm_all_csv = ranks2.SelectMany((a, i) => a.cm_all.Select((b, j) => $@"{string.Join(", ", a.perf_rule_ranks.Select(c => c.rank).ToList())},{j.ToString()},{feature_selection_type.ToString().Replace(",", ";")},{perf_selection_rule.ToString().Replace(",", ";")},{feature_selection_performance_metrics.ToString().Replace(",", ";")},{feature_selection_combinator.ToString().Replace(",", ";")},{a.direction},{a.iteration},{features_selected.Any(c => c.set_id == a.feature_set.set_id)},{a.feature_set.alphabet},{a.feature_set.category},{a.feature_set.dimension},{a.feature_set.source},{a.feature_set.@group},{a.feature_set.set_id},{a.score_metrics.ToString()},{b.ToString()}").ToList()).ToList();

                rank_cm_all_csv.Insert(0, cm_average_csv_header);

                lock (save_iteration_parameters_lock)
                {
                    File.WriteAllLines(Path.Combine(results_output_folder, $@"{start_date_time}_rank_cm_all_{rank_type.ToString()}_{iteration_loop}_{iteration_recalculate}_{iteration_select}.csv"), rank_cm_all_csv);
                }

                program.WriteLine($"{iteration_loop} :::{feature_selection_type}: {rank_type.ToString()} iteration ranks:");

                for (var index = 0; index < ranks2.Count; index++)
                {
                    var a = ranks2[index];

                    program.WriteLine($"{iteration_loop} :{rank_type.ToString()}: {index} -> {a.feature_set.set_id}.{a.feature_set.source}.{a.feature_set.@group} -> {a.score_metrics.ToText()}");
                }
            }


        }

        public List<(string experiment_id1, string experiment_id2, string experiment_id3, char direction, feature_set feature_set, int iteration, score_metrics score_metrics, performance_measure.confusion_matrix cm_average, List<performance_measure.confusion_matrix> cm_all)> ranking_data;
        //public List<(feature_set feature_set, double score_change, confusion_matrix)> feature_importance_first_iteration;
        //public List<(feature_set feature_set, double score_change, confusion_matrix)> feature_importance_moving_average;
        //public List<(feature_set feature_set, double score_change, confusion_matrix)> feature_importance_overall_average;
        //public List<(feature_set feature_set, double score_change, confusion_matrix)> feature_importance_current_iteration;
        //public List<(feature_set feature_set, double score_change, confusion_matrix)> feature_importance_last_iteration_ranked;

        // try to add feature N times... if no performance improvement on insertion of N times, mark feature as bad
        public List<(feature_set feature_set, int bad_feature_iteration_count)> forwards_feature_bad_feature_log;
        public List<feature_set> forwards_feature_bad_feature_permanent;
        public List<(feature_set feature_set, int good_feature_iteration_count)> forwards_feature_good_feature_log;
        public List<feature_set> forwards_feature_good_feature_permanent;

        // try to remove feature N times... if no performance improvement on removal of N times, mark feature as good
        public List<(feature_set feature_set, int bad_feature_iteration_count)> backwards_feature_bad_feature_log;
        public List<feature_set> backwards_feature_bad_feature_permanent;
        public List<(feature_set feature_set, int good_feature_iteration_count)> backwards_feature_good_feature_log;
        public List<feature_set> backwards_feature_good_feature_permanent;

        public Stopwatch sw_method;

        public List<(int iteration, string description)> text_log;
        public object text_log_lock;

        public string start_date_time;

        //public void add_text_log(int iteration, string text)
        //{
        //    lock (text_log_lock)
        //    {
        //        program.WriteLine($@"iteration: {iteration}.  {text}");

        //        text_log.Add((iteration, text));

        //        var filename = Path.Combine(this.results_output_folder, $"{start_date_time}_log_{iteration}.txt");

        //        File.AppendAllLines(filename, new[] { text });
        //    }
        //}

        public void set_defaults()
        {
            this.start_date_time = program.program_start_time;// DateTime.Now.ToString().Replace(":", "-").Replace("/", "-").Replace(" ", "_");

            this.save_iteration_json = false;

            this.results_output_folder = "";

            //experiment_id1 = "";
            //experiment_id2 = "";

            //this.reorder_input_each_iteration = reordering_rules.none;
            this.text_log_lock = new object();
            this.sw_method = new Stopwatch();
            this.text_log = new List<(int iteration, string description)>();

            //this.program.WriteLine(-1, "Set default parameters");


            this.dataset_instance_list = null;

            this.run_svm_params = new cross_validation.run_svm_params();
            //rand_cv = 1;
            //outer_cv = 5;
            //inner_cv = 5;

            this.task_durations = new List<(int iteration, long ticks)>();

            this.ranking_data = new List<(string experiment_id1, string experiment_id2, string experiment_id3, char direction, feature_set feature_set, int iteration, score_metrics score_metrics, performance_measure.confusion_matrix cm_average, List<performance_measure.confusion_matrix> cm_all)>();

            this.features_input_untouched = new List<feature_set>();
            this.features_input = new List<feature_set>();

            this.base_features = new List<feature_set>();
            this.feature_selection_type = feature_selection_types.none;
            this.perf_selection_rule = perf_selection_rules.none;

            this.backwards_max_features_to_combine_per_iteration = 1;
            this.forwards_max_features_to_combine_per_iteration = 1;

            this.backwards_max_features_to_remove_per_iteration = 1;
            this.forwards_max_features_to_add_per_iteration = 1;
            this.backwards_min_features = 0;
            this.forwards_max_features = 0;

            this.backwards_max_feature_removal_attempts = 0;
            this.forwards_max_features_insertion_attempts = 0;
            this.random_baseline = 0.2;
            this.margin_of_error = 0.0001;
            this.max_tasks = -1;
            this.feature_selection_combinator = feature_selection_combinators.feature_sets;

            this.backwards_selection_feature_rankings = new List<iterative_task>();
            this.backwards_selection_feature_rankings_taken_for_removal = new List<iterative_task>();
            this.backwards_selection_feature_rankings_could_have_taken_for_removal = new List<iterative_task>();
            this.backwards_selection_feature_rankings_bad_features = new List<iterative_task>();
            this.backwards_selection_feature_rankings_good_features = new List<iterative_task>();
            //this.backwards_selection_feature_rankings_last_iteration = new List<iterative_task>();
            //this.backwards_selection_feature_rankings_taken_for_removal_last_iteration = new List<iterative_task>();
            //this.backwards_selection_feature_rankings_could_have_taken_for_removal_last_iteration = new List<iterative_task>();
            //this.backwards_selection_feature_rankings_bad_features_last_iteration = new List<iterative_task>();
            //this.backwards_selection_feature_rankings_good_features_last_iteration = new List<iterative_task>();
            //this.backwards_selection_feature_rankings_history_log = new List<List<iterative_task>>();
            //this.backwards_selection_feature_rankings_taken_for_removal_history_log = new List<List<iterative_task>>();
            //this.backwards_selection_feature_rankings_could_have_taken_for_removal_history_log = new List<List<iterative_task>>();
            //this.backwards_selection_feature_rankings_bad_features_history_log = new List<List<iterative_task>>();
            //this.backwards_selection_feature_rankings_good_features_history_log = new List<List<iterative_task>>();

            this.forwards_selection_feature_rankings = new List<iterative_task>();
            this.forwards_selection_feature_rankings_taken_for_insertion = new List<iterative_task>();
            this.forwards_selection_feature_rankings_could_have_taken_for_insertion = new List<iterative_task>();
            this.forwards_selection_feature_rankings_bad_features = new List<iterative_task>();
            this.forwards_selection_feature_rankings_good_features = new List<iterative_task>();
            //this.forwards_selection_feature_rankings_last_iteration = new List<iterative_task>();
            //this.forwards_selection_feature_rankings_taken_for_insertion_last_iteration = new List<iterative_task>();
            //this.forwards_selection_feature_rankings_could_have_taken_for_insertion_last_iteration = new List<iterative_task>();
            //this.forwards_selection_feature_rankings_bad_features_last_iteration = new List<iterative_task>();
            //this.forwards_selection_feature_rankings_good_features_last_iteration = new List<iterative_task>();
            //this.forwards_selection_feature_rankings_history_log = new List<List<iterative_task>>();
            //this.forwards_selection_feature_rankings_taken_for_insertion_history_log = new List<List<iterative_task>>();
            //this.forwards_selection_feature_rankings_could_have_taken_for_insertion_history_log = new List<List<iterative_task>>();
            //this.forwards_selection_feature_rankings_bad_features_history_log = new List<List<iterative_task>>();
            //this.forwards_selection_feature_rankings_good_features_history_log = new List<List<iterative_task>>();

            this.features_selected = new List<feature_set>();
            this.features_selected_average_confusion_matrices = new List<(string experiment_id1, string experiment_id2, string experiment_id3, int iteration, char direction, feature_set feature_set, List<performance_measure.confusion_matrix> cm_list, List<performance_measure.confusion_matrix> cm_all)>();

            this.features_backwards_buffer = new List<feature_set>();
            this.features_forwards_buffer = new List<feature_set>();

            this.feature_selection_performance_metrics = performance_measure.confusion_matrix.cross_validation_metrics.MCC | performance_measure.confusion_matrix.cross_validation_metrics.API_All;
            this.feature_selection_performance_classes = new List<int>() { /*-1,*/ +1 };

            this.backwards_finished = false;
            this.forwards_finished = false;


            //this.backwards_baseline_score = new score_metrics();
            //this.forwards_baseline_score = new score_metrics();
            this.baseline_score = new score_metrics();


            this.baseline_recalculation_required = false;
            this.backwards_score_improved = false;
            this.forwards_score_improved = false;
            this.score_improved = false;
            this.direction_change = false;
            this.consecutive_iterations_with_improvement = 0;
            this.consecutive_iterations_without_improvement = 0;
            this.consecutive_forwards_iterations = 0;
            this.consecutive_forwards_iterations_with_improvement = 0;
            this.consecutive_forwards_iterations_without_improvement = 0;
            this.consecutive_backwards_iterations = 0;
            this.consecutive_backwards_iterations_with_improvement = 0;
            this.consecutive_backwards_iterations_without_improvement = 0;
            this.unidirectional_convergence_reached = false;
            this.iteration_loop = 0;
            this.iteration_select = 0;
            this.iteration_recalculate = 0;
            this.iteration_select_max = 0;

            this.go_backwards = false;
            this.go_forwards = false;
            this.can_go_backwards = false;
            this.can_go_forwards = false;
            this.forwards_max_features_reached = false;
            this.backwards_min_features_reached = false;
            //this.first = true;

            //this.store_last_iteration = false;
            //this.store_history_log = false;

            this.external_task_id = 0;
            this.external_forwards_task_id = 0;
            this.external_backwards_task_id = 0;

            //List<((string source, string group) set_name, List<(int fid, string alphabet, string dimension, string category, string source, string group, string member, string perspective, int alphabet_id, int dimension_id, int category_id, int source_id, int group_id, int member_id, int perspective_id)> set_members)>
            //this.feature_importance_first_iteration = new List<(feature_set feature_set, double perf)>();
            //this.feature_importance_average = new List<(feature_set feature_set, double perf)>();
            //this.feature_importance_actual = new List<(feature_set feature_set, double perf)>();

            // try to add feature N times... if no performance improvement on insertion of N times, mark feature as bad
            this.forwards_feature_bad_feature_log = new List<(feature_set feature_set, int bad_feature_iteration_count)>();
            this.forwards_feature_bad_feature_permanent = new List<feature_set>();
            this.forwards_feature_good_feature_log = new List<(feature_set feature_set, int good_feature_iteration_count)>();
            this.forwards_feature_good_feature_permanent = new List<feature_set>();

            // try to remove feature N times... if no performance improvement on removal of N times, mark feature as good
            this.backwards_feature_bad_feature_log = new List<(feature_set feature_set, int bad_feature_iteration_count)>();
            this.backwards_feature_bad_feature_permanent = new List<feature_set>();
            this.backwards_feature_good_feature_log = new List<(feature_set feature_set, int good_feature_iteration_count)>();
            this.backwards_feature_good_feature_permanent = new List<feature_set>();
        }


        public static string serialise_json(feature_selection_unidirectional feature_selection_unidirectional)
        {
            program.GC_Collection();

            var json_settings = new JsonSerializerSettings() { PreserveReferencesHandling = PreserveReferencesHandling.All };

            var feature_selection_unidirectional_serialised = "";

            try
            {
                feature_selection_unidirectional_serialised = Newtonsoft.Json.JsonConvert.SerializeObject(feature_selection_unidirectional, json_settings);
            }
            catch (Exception e)
            {
                feature_selection_unidirectional_serialised = null;
                program.WriteLine(e.ToString(), true, ConsoleColor.Yellow);

            }

            return feature_selection_unidirectional_serialised;
        }

        public static feature_selection_unidirectional deserialise_json(string serialized_json)
        {
            program.GC_Collection();

            var json_settings = new JsonSerializerSettings() { PreserveReferencesHandling = PreserveReferencesHandling.All };

            feature_selection_unidirectional feature_selection_unidirectional_deserialised = null;

            try
            {
                feature_selection_unidirectional_deserialised = Newtonsoft.Json.JsonConvert.DeserializeObject<feature_selection_unidirectional>(serialized_json, json_settings);
            }
            catch (Exception e)
            {
                feature_selection_unidirectional_deserialised = null;
                program.WriteLine(e.ToString(), true, ConsoleColor.Yellow);

            }

            return feature_selection_unidirectional_deserialised;
        }

        //List<(int fid, string alphabet, string dimension, string category, string source, string group, string member, string perspective, int alphabet_id, int dimension_id, int category_id, int source_id, int group_id, int member_id, int perspective_id)>

        public feature_selection_unidirectional()
        {

        }

        public feature_selection_unidirectional(
            string results_output_folder,
            cross_validation.run_svm_params run_svm_params,

            List<(List<(string comment_header, string comment_value)> comment_columns, List<(int fid, double fv)> feature_data)> dataset_instance_list,
            List<feature_set> features_input,
            List<feature_set> base_features = null,
            feature_selection_combinators feature_selection_combinator = feature_selection_combinators.feature_sets,
            feature_selection_types feature_selection_type = feature_selection_types.forwards,
            perf_selection_rules perf_selection_rule = perf_selection_rules.best_score,
            //reordering_rules reorder_input_each_iteration = reordering_rules.none,
            performance_measure.confusion_matrix.cross_validation_metrics feature_selection_performance_metrics = performance_measure.confusion_matrix.cross_validation_metrics.MCC | performance_measure.confusion_matrix.cross_validation_metrics.API_All,
            List<int> feature_selection_performance_classes = null,
            int iteration_select_max = -1,
            int backwards_max_features_to_combine_per_iteration = 1,
            int forwards_max_features_to_combine_per_iteration = 1,
            int backwards_max_features_to_remove_per_iteration = 1,
            int forwards_max_features_to_add_per_iteration = 1,
            int backwards_min_features = 0,
            int forwards_max_features = 0,
            int backwards_max_feature_removal_attempts = 0,
            int forwards_max_features_insertion_attempts = 0,
            double random_baseline = 0.2,
            double margin_of_error = 0.0001,
            int max_tasks = -1)
        {
            Directory.CreateDirectory(results_output_folder);

            set_defaults();



            //program.WriteLine(-1, "Setting algorithm parameters:");

            this.run_svm_params = run_svm_params;

            this.results_output_folder = results_output_folder;

            this.features_selected = new List<feature_set>();


            this.dataset_instance_list = dataset_instance_list?.ToList() ?? null;

            this.features_input_untouched = features_input?.ToList() ?? null;
            this.features_input = features_input?.ToList() ?? null;

            //this.reorder_input_each_iteration = reorder_input_each_iteration;

            this.base_features = base_features?.ToList() ?? null;

            this.backwards_max_features_to_combine_per_iteration = backwards_max_features_to_combine_per_iteration;
            this.forwards_max_features_to_combine_per_iteration = forwards_max_features_to_combine_per_iteration;

            this.feature_selection_performance_metrics = feature_selection_performance_metrics;
            this.feature_selection_performance_classes = feature_selection_performance_classes;

            this.feature_selection_combinator = feature_selection_combinator;
            this.feature_selection_type = feature_selection_type;
            this.perf_selection_rule = perf_selection_rule;
            this.iteration_select_max = iteration_select_max;
            this.backwards_max_features_to_remove_per_iteration = backwards_max_features_to_remove_per_iteration;
            this.forwards_max_features_to_add_per_iteration = forwards_max_features_to_add_per_iteration;
            this.backwards_min_features = backwards_min_features;
            this.forwards_max_features = forwards_max_features;
            this.backwards_max_feature_removal_attempts = backwards_max_feature_removal_attempts;
            this.backwards_max_feature_removal_attempts = backwards_max_feature_removal_attempts;
            this.forwards_max_features_insertion_attempts = forwards_max_features_insertion_attempts;
            this.random_baseline = random_baseline;
            this.margin_of_error = margin_of_error;
            this.max_tasks = max_tasks;

            if (this.features_input == null || this.features_input.Count == 0)
            {
                return;
            }

            if (this.feature_selection_type == feature_selection_types.none)
            {
                return;
            }

            if (this.perf_selection_rule == perf_selection_rules.none)
            {
                return;
            }

            if (this.feature_selection_combinator == feature_selection_combinators.none)
            {
                return;
            }

            if (this.feature_selection_combinator == feature_selection_combinators.individual_features)
            {
                //var set_id = 0;
                //this.base_features = this.base_features.SelectMany((a, i) =>
                //{
                //    return a.members.Select((b, j) => new feature_set_id() { set_id = ++set_id, members = new List<feature_set_member>() { new feature_set_member() { external_feature_id = b.external_feature_id, member_name = b.member_name, internal_member_id = b.internal_member_id, set = null } }, set_length = 1, set_name = a.set_name }).ToList();
                //}).ToList();

                //List<(int fid, string alphabet, string dimension, string category, string source, string group, string member, string perspective, int alphabet_id, int dimension_id, int category_id, int source_id, int group_id, int member_id, int perspective_id)>
                if (this.features_input != null && this.features_input.Count > 0)
                {
                    this.features_input = this.features_input.SelectMany((a, i) => a.set_members.Select((b, j) => new feature_set()
                    {
                        @group = a.@group,
                        source = a.source,
                        set_id = b.fid,
                        set_members = new List<feature_set_member>(){new feature_set_member()
                        {
                            alphabet_id = b.alphabet_id,
                            perspective = b.perspective,
                            fid = b.fid,
                            member = b.member,
                            dimension = b.dimension,
                            @group = b.@group,
                            source = b.source,
                            category = b.category,
                            dimension_id = b.dimension_id,
                            category_id = b.category_id,
                            alphabet = b.alphabet,
                            source_id = b.source_id,
                            perspective_id = b.perspective_id,
                            group_id = b.group_id,
                            member_id = b.member_id
                        }}
                    }).ToList()).ToList();

                }

                if (this.base_features != null && this.base_features.Count > 0)
                {
                    this.base_features = this.base_features.SelectMany((a, i) => a.set_members.Select((b, j) => new feature_set()
                    {
                        @group = a.@group,
                        source = a.source,
                        set_id = b.fid,
                        set_members = new List<feature_set_member>(){new feature_set_member()
                        {
                            alphabet_id = b.alphabet_id,
                            perspective = b.perspective,
                            fid = b.fid,
                            member = b.member,
                            dimension = b.dimension,
                            @group = b.@group,
                            source = b.source,
                            category = b.category,
                            dimension_id = b.dimension_id,
                            category_id = b.category_id,
                            alphabet = b.alphabet,
                            source_id = b.source_id,
                            perspective_id = b.perspective_id,
                            group_id = b.group_id,
                            member_id = b.member_id
                        }}
                    }).ToList()).ToList();


                }
            }

            if (this.max_tasks < 0)
            {
                this.max_tasks = Math.Abs(this.max_tasks) * Environment.ProcessorCount * 10;
            }

            if (this.feature_selection_type == feature_selection_types.backwards || this.feature_selection_type == feature_selection_types.backwards_and_forwards || this.feature_selection_type == feature_selection_types.backwards_then_forwards || this.feature_selection_type == feature_selection_types.backwards_then_forwards_repeated_until_convergence)
            {
                this.features_selected = this.features_input.ToList();
                this.baseline_recalculation_required = true;
            }

            if (this.base_features != null && this.base_features.Count > 0)
            {
                this.features_selected = this.features_selected.Union(this.base_features).ToList();
                this.baseline_recalculation_required = true;
            }

            if (this.features_selected != null && this.features_selected.Count > 0)
            {
                this.features_selected_average_confusion_matrices.AddRange(features_selected.Select(a => ("base features", "", "", -1, '_', a, new List<performance_measure.confusion_matrix>() { new performance_measure.confusion_matrix() { } }, new List<performance_measure.confusion_matrix>() { new performance_measure.confusion_matrix() { } })).ToList());
            }

        }

        public class score_metrics
        {
            public performance_measure.confusion_matrix.cross_validation_metrics score_metric;

            public double score_before;
            public double score_after;
            public double score_change;
            public double score_change_ppf;
            public double score_change_ppg;
            public double score_ppf_before;
            public double score_ppf_after;
            public double score_ppf_change;
            public double score_ppg_before;
            public double score_ppg_after;
            public double score_ppg_change;

            public int total_groups_before;
            public int total_groups_after;
            public int total_groups_change;

            public int total_features_before;
            public int total_features_after;
            public int total_features_change;


            public score_metrics()
            {

            }

            public score_metrics(score_metrics score_metrics, bool negate_change = false)
            {
                this.score_metric = score_metrics.score_metric;

                this.score_before = score_metrics.score_before;
                this.score_after = score_metrics.score_after;
                this.score_change = score_metrics.score_change;
                this.score_change_ppf = score_metrics.score_change_ppf;
                this.score_change_ppg = score_metrics.score_change_ppg;
                this.score_ppf_before = score_metrics.score_ppf_before;
                this.score_ppf_after = score_metrics.score_ppf_after;
                this.score_ppf_change = score_metrics.score_ppf_change;
                this.score_ppg_before = score_metrics.score_ppg_before;
                this.score_ppg_after = score_metrics.score_ppg_after;
                this.score_ppg_change = score_metrics.score_ppg_change;

                this.total_groups_before = score_metrics.total_groups_before;
                this.total_groups_after = score_metrics.total_groups_after;
                this.total_groups_change = score_metrics.total_groups_change;

                this.total_features_before = score_metrics.total_features_before;
                this.total_features_after = score_metrics.total_features_after;
                this.total_features_change = score_metrics.total_features_change;

                if (negate_change)
                {

                    this.score_change = -this.score_change;
                    this.score_change_ppf = -this.score_change_ppf;
                    this.score_change_ppg = -this.score_change_ppg;
                    this.score_ppf_change = -this.score_ppf_change;
                    this.score_ppg_change = -this.score_ppg_change;

                    this.score_after = this.score_after + (this.score_change * 2);
                    this.score_ppf_after = this.score_ppf_after + (this.score_ppf_change * 2);
                    this.score_ppg_after = this.score_ppg_after + (this.score_ppg_change * 2);


                    this.total_features_change = -this.total_features_change;
                    this.total_groups_change = -this.total_groups_change;

                    var v1 = this.total_features_before;
                    var v2 = this.total_features_after;
                    var w1 = this.total_groups_before;
                    var w2 = this.total_groups_after;

                    this.total_features_before = v2;
                    this.total_features_after = v1;


                    this.total_groups_before = w2;
                    this.total_groups_after = w1;

                    //calc_change(score_metric, score_before, score_after, total_features_before, total_features_after, total_groups_before, total_groups_after); // required ??
                }
            }

            public static score_metrics Average(List<score_metrics> score_metrics_list)
            {
                if (score_metrics_list == null || score_metrics_list.Count == 0)
                {
                    return new score_metrics();
                }

                var result = new score_metrics()
                {
                    score_metric = score_metrics_list.First().score_metric,
                    score_before = score_metrics_list.Average(a => a.score_before),
                    score_after = score_metrics_list.Average(a => a.score_after),
                    score_change = score_metrics_list.Average(a => a.score_change),
                    score_change_ppf = score_metrics_list.Average(a => a.score_change_ppf),
                    score_change_ppg = score_metrics_list.Average(a => a.score_change_ppg),
                    score_ppf_before = score_metrics_list.Average(a => a.score_ppf_before),
                    score_ppf_after = score_metrics_list.Average(a => a.score_ppf_after),
                    score_ppf_change = score_metrics_list.Average(a => a.score_ppf_change),
                    score_ppg_before = score_metrics_list.Average(a => a.score_ppg_before),
                    score_ppg_after = score_metrics_list.Average(a => a.score_ppg_after),
                    score_ppg_change = score_metrics_list.Average(a => a.score_ppg_change),
                    total_groups_before = (int)score_metrics_list.Average(a => a.total_groups_before),
                    total_groups_after = (int)score_metrics_list.Average(a => a.total_groups_after),
                    total_groups_change = (int)score_metrics_list.Average(a => a.total_groups_change),
                    total_features_before = (int)score_metrics_list.Average(a => a.total_features_before),
                    total_features_after = (int)score_metrics_list.Average(a => a.total_features_after),
                    total_features_change = (int)score_metrics_list.Average(a => a.total_features_change),
                };

                return result;
            }

            public void calc_change(performance_measure.confusion_matrix.cross_validation_metrics score_metric, double old_score, double new_score, int total_features_before, int total_features_after, int total_groups_before, int total_groups_after)
            {
                this.score_metric = score_metric;
                this.score_before = old_score;
                this.score_after = new_score;
                this.total_features_before = total_features_before;
                this.total_features_after = total_features_after;
                this.total_groups_before = total_groups_before;
                this.total_groups_after = total_groups_after;

                this.total_features_change = this.total_features_after - this.total_features_before;
                this.total_groups_change = this.total_groups_after - this.total_groups_before;

                this.score_ppf_before = this.total_features_before != 0 ? (double)this.score_before / (double)this.total_features_before : 0;
                this.score_ppf_after = this.total_features_after != 0 ? (double)this.score_after / (double)this.total_features_after : 0;

                this.score_ppg_before = this.total_groups_before != 0 ? (double)this.score_before / (double)this.total_groups_before : 0;
                this.score_ppg_after = this.total_groups_after != 0 ? (double)this.score_after / (double)this.total_groups_after : 0;

                this.score_change = this.score_after - this.score_before;
                this.score_ppf_change = this.score_ppf_after - this.score_ppf_before;
                this.score_ppg_change = this.score_ppg_after - this.score_ppg_before;

                this.total_features_change = this.total_features_after - this.total_features_before;
                this.total_groups_change = this.total_groups_after - this.total_groups_before;

                this.score_change_ppf = this.total_features_change != 0 ? (double)this.score_change / (double)this.total_features_change : 0;
                this.score_change_ppg = this.total_groups_change != 0 ? (double)this.score_change / (double)this.total_groups_change : 0;
            }

            public override string ToString()
            {
                return string.Join(",", new object[]
                {
                    score_metric.ToString().Replace(",",";"),

                    score_before,
                    score_after,
                    score_change,

                    score_change_ppf,
                    score_change_ppg,

                    score_ppf_before,
                    score_ppf_after,
                    score_ppf_change,

                    score_ppg_before,
                    score_ppg_after,
                    score_ppg_change,

                    total_groups_before,
                    total_groups_after,
                    total_groups_change,

                    total_features_before,
                    total_features_after,
                    total_features_change,
            });
            }

            public string ToText()
            {
                var h = csv_header.Split(',');
                var d = ToString().Split(',');

                var x = string.Join("; ", h.Select((a, i) => h[i] + "=" + d[i]).ToList());

                return x;
            }

            public static string csv_header = string.Join(",",
       new string[]
            {
                nameof(score_metric),

                nameof(score_before),
                nameof(score_after),
                nameof(score_change),

                nameof(score_change_ppf),
                nameof(score_change_ppg),

                nameof(score_ppf_before),
                nameof(score_ppf_after),
                nameof(score_ppf_change),

                nameof(score_ppg_before),
                nameof(score_ppg_after),
                nameof(score_ppg_change),

                nameof(total_groups_before),
                nameof(total_groups_after),
                nameof(total_groups_change),

                nameof(total_features_before),
                nameof(total_features_after),
                nameof(total_features_change),
            });
        }

        public class iterative_task
        {
            public List<feature_set> feature_set_list;

            public string experiment_id1;
            public string experiment_id2;
            public string experiment_id3;

            public int task_id;
            public int loop_index;
            public int iteration_loop;
            public int iteration_select;
            public int iteration_recalculate;

            public long duration_method;
            public long duration_iteration;
            public long duration_task;
            public long duration_index;

            public char direction;
            public string metrics;

            
            public score_metrics baseline_score;

            public List<performance_measure.confusion_matrix> all_cm;
            public List<performance_measure.confusion_matrix> all_cm_average;

            public performance_measure.confusion_matrix rank_cm;

            public void calc_change(performance_measure.confusion_matrix.cross_validation_metrics score_metric, double old_score, double new_score, int total_features_before, int total_features_after, int total_groups_before, int total_groups_after)
            {
                baseline_score = new score_metrics();
                baseline_score.calc_change(score_metric, old_score, new_score, total_features_before, total_features_after, total_groups_before, total_groups_after);
            }

            public override string ToString()
            {
                var r = string.Join(",", new object[]
                {
                    experiment_id1,
                    experiment_id2,
                    experiment_id3,
                    task_id ,
                    loop_index,
                    iteration_loop ,
                    iteration_select ,
                    iteration_recalculate ,
                    direction,
                    metrics,
                    duration_method ,
                    duration_iteration ,
                    duration_index,
                    duration_task ,
                }.Select(a => a?.ToString().Replace(",", ";") ?? "").ToList());

                r = $"{r},{baseline_score.ToString()}";

                return r;
            }

            public static readonly string csv_header = string.Join(",", new object[]
                {
                    nameof(experiment_id1),
                    nameof(experiment_id2),
                    nameof(experiment_id3),
                    nameof(task_id ),
                    nameof(loop_index ),
                    nameof(iteration_loop ),
                    nameof(iteration_select ),
                    nameof(iteration_recalculate ),
                    nameof(direction),
                    nameof(metrics),
                    nameof(duration_method ),
                    nameof(duration_iteration ),
                    nameof(duration_index ),
                    nameof(duration_task ),

                    feature_selection_unidirectional.score_metrics.csv_header,
                });//.Select(a => a?.ToString().Replace(",", ";") ?? "").ToList());


            public (string name, object value)[] AsArray()
            {
                var result = new List<(string name, object value)>()
                {

                  (nameof(experiment_id1                    ),     experiment_id1                          ),
                  (nameof(experiment_id2                    ),     experiment_id2                          ),
                  (nameof(experiment_id3                    ),     experiment_id3                          ),

                  (nameof(task_id                    ),     task_id                          ),
                  (nameof(loop_index                 ),     loop_index                       ),
                  (nameof(iteration_loop             ),     iteration_loop                   ),
                  (nameof(iteration_select           ),     iteration_select                 ),
                  (nameof(iteration_recalculate      ),     iteration_recalculate            ),
                  (nameof(direction      ),     direction            ),
                  (nameof(metrics      ),     metrics            ),
                  (nameof(duration_method            ),     duration_method                  ),
                  (nameof(duration_iteration         ),     duration_iteration               ),
                  (nameof(duration_task              ),     duration_task                    ),

                };

                var h = feature_selection_unidirectional.score_metrics.csv_header.Split(',');
                var d = baseline_score.ToString().Split(',');

                for (var i = 0; i < h.Length; i++)
                {
                    result.Add((h[i], d[i]));
                }

                return result.ToArray();
            }


        }

        public static double fix_double(double value)
        {
            // the doubles must be compatible with libsvm which is written in C (C and C# have different min/max values for double)
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

        public static double scale_value(double value, double min, double max)
        {
            var x = fix_double(value - min);
            var y = fix_double(max - min);

            if (x == 0) return 0;
            if (y == 0) return value;

            var scaled = x / y;

            scaled = fix_double(scaled);

            return scaled;
        }


        public cross_validation.run_svm_return classify(List<feature_set> feature_set_list, string experiment_id1, string experiment_id2, string experiment_id3)
        {

            // make sure features are in alphabetical/consistant order so that result file cache can be used
            feature_set_list = feature_set_list.OrderBy(a => a.dimension).ThenBy(a => a.category).ThenBy(a => a.alphabet).ThenBy(a => a.source).ThenBy(a => a.group).ToList();

            var group_count = feature_set_list.Count;
            var feature_count = feature_set_list.Sum(a => a.set_members.Count);
            // get features by their unique id
            var fids = feature_set_list.SelectMany(a => a.set_members.Select(b => b.fid).ToList()).OrderBy(a => a).Distinct().ToList();

            // add class_id
            fids.Insert(0, 0);

            var feature_limited_example_instance_list = dataset_instance_list.Select((instance, row_index) => new example_instance(row_index, fids.Select(fid => (fid: fid, fv: instance.feature_data.First(a => a.fid == fid).fv)).ToList(), instance.comment_columns)).ToList();

            //var cP = feature_limited_example_instance_list.Select(a => a.feature_list.First(b => b.fid == 0).fv).GroupBy(a => a).Select(a => (a.Key, a.Count())).ToList();

            // setup svm parameters
            var classify_run_svm_params = new cross_validation.run_svm_params(this.run_svm_params)
            {
                example_instance_list = feature_limited_example_instance_list,

                return_performance = true,
                return_predictions = false,
                return_meta_data = false,
                return_roc_xy = false,
                return_pr_xy = false,

                max_tasks = -1,

                group_count = group_count,
                feature_count = feature_count
            };


            // run_svm will be called for each iteration of forwards or backwards

            var cts = new CancellationTokenSource();

            var svm_return = cross_validation.run_svm(classify_run_svm_params, cts.Token);


            var save_filename_performance = "";
            var save_filename_predictions = "";

            if (!string.IsNullOrWhiteSpace(save_filename_performance) || !string.IsNullOrWhiteSpace(save_filename_predictions))
            {
                // 4. save svm prediction/performance results to file - used for debugging - the performance results are already saved elsewhere
                cross_validation.save_svm_return(save_filename_performance: save_filename_performance, save_filename_predictions: save_filename_predictions, svm_return);
            }

            return svm_return;
        }

        //public static (feature_selection_unidirectional fs, List<feature_set> selected_features, double performance) run_unidirectional_feature_selection(List<(List<(string comment_header, string comment_value)> comment_columns, List<(int fid, double fv)> feature_data)> dataset_instance_list, List<feature_set> features_input, List<feature_set> base_features = null, feature_selection_combinators feature_selection_combinator = feature_selection_combinators.feature_sets, feature_selection_types feature_selection_type = feature_selection_types.forwards, perf_selection_rules perf_selection_rule = perf_selection_rules.best_score, int iteration_select_max = -1, int backwards_max_features_to_remove_per_iteration = 1, int forwards_max_features_to_add_per_iteration = 1, int backwards_min_features = 0, int forwards_max_features = 0, int backwards_max_feature_removal_attempts = 0, int forwards_max_features_insertion_attempts = 0, double random_baseline = 0.2, double margin_of_error = 0.0001, int max_tasks = -1)
        //{
        //    var x = new feature_selection_unidirectional(dataset_instance_list, features_input, base_features, feature_selection_combinator, feature_selection_type, perf_selection_rule, iteration_select_max, backwards_max_features_to_remove_per_iteration, forwards_max_features_to_add_per_iteration, backwards_min_features, forwards_max_features, backwards_max_feature_removal_attempts, forwards_max_features_insertion_attempts, random_baseline, margin_of_error, max_tasks);
        //    var y = x.run_unidirectional_feature_selection();
        //    return (x, y.selected_features, y.performance);
        //}

        //public void save_task_result_as_csv(List<performance_measure.confusion_matrix> svm_cms, iterative_task result, string global_params)
        //{
        //    lock (save_iterative_data_lock)
        //    {
        //        var dt = DateTime.Now.ToString();
        //
        //        var str_data = svm_cms.Select(a => string.Join(",", dt, global_params, result.ToString(), a.ToString())).ToList();
        //
        //        var csv_filename = Path.Combine(this.results_output_folder, $@"{start_date_time}_results_csv_{iteration_loop}.csv");
        //
        //        if (!File.Exists(csv_filename) || new FileInfo(csv_filename).Length == 0)
        //        {
        //            var str_header = string.Join(",", "date_time", csv_header, iterative_task.csv_header, performance_measure.confusion_matrix.csv_header);
        //            str_data.Insert(0, str_header);
        //        }
        //
        //        File.AppendAllLines(csv_filename, str_data);
        //    }
        //}

        public static readonly object tasks_lock = new object();

        public (List<feature_set> selected_features, score_metrics performance) run_unidirectional_feature_selection()
        {
            if (feature_selection_type == feature_selection_types.none)
            {
                return default;
            }

            sw_method.Start();

            do
            {
                program.GC_Collection();

                var sw_iteration = new Stopwatch();
                sw_iteration.Start();

                iteration_loop++;

                program.WriteLine($@"");
                program.WriteLine($@"------------------------------------------------------------------------------------------------------------------");
                program.WriteLine($@"{iteration_loop} Starting: Iteration #: {iteration_loop} with ranking score: {baseline_score}.  Algorithm: {feature_selection_type}.");
                program.WriteLine($@"{iteration_loop} Starting: Selected features at start; {features_selected.Count} feature groups; {features_selected.Sum(a => a.set_members.Count)} feature members;");
                program.WriteLine($@"{iteration_loop} Starting: Selected features groups at start; [{string.Join(", ", features_selected.Select(a => $"{a.source}.{a.@group}").ToList())}]");
                program.WriteLine($@"{iteration_loop} Starting: Selected features groups and members at start; [{string.Join(", ", features_selected.SelectMany(a => a.set_members.Select(b => $"{b.dimension}.{b.alphabet}.{b.category}.{b.source}.{b.@group}.{b.member}.{b.perspective}").ToList()).ToList())}]");


                //can_go_forwards = (features_input.Count - features_selected.Count) > 0;
                //can_go_backwards = (features_selected.Count > 0);
                can_go_forwards = features_input.Count - ( /*base_features.Count +*/ backwards_feature_bad_feature_permanent.Count + forwards_feature_bad_feature_permanent.Count + features_selected.Count) > 0;
                can_go_backwards = features_selected.Count - (backwards_feature_good_feature_permanent.Count + forwards_feature_good_feature_permanent.Count) > 0;

                backwards_min_features_reached = (backwards_min_features > 0 && features_selected.Count <= backwards_min_features);
                forwards_max_features_reached = (forwards_max_features > 0 && features_selected.Count >= forwards_max_features);

                go_forwards = feature_selection_type == feature_selection_types.forwards || (feature_selection_type == feature_selection_types.forwards_and_backwards || feature_selection_type == feature_selection_types.backwards_and_forwards) || (!forwards_finished && feature_selection_type == feature_selection_types.forwards_then_backwards) || (backwards_finished && feature_selection_type == feature_selection_types.backwards_then_forwards) || (!unidirectional_convergence_reached && (!forwards_finished && feature_selection_type == feature_selection_types.forwards_then_backwards_repeated_until_convergence)) || (!unidirectional_convergence_reached && (backwards_finished && feature_selection_type == feature_selection_types.backwards_then_forwards_repeated_until_convergence));
                go_backwards = feature_selection_type == feature_selection_types.backwards || (feature_selection_type == feature_selection_types.forwards_and_backwards || feature_selection_type == feature_selection_types.backwards_and_forwards) || (!backwards_finished && feature_selection_type == feature_selection_types.backwards_then_forwards) || (forwards_finished && feature_selection_type == feature_selection_types.forwards_then_backwards) || (!unidirectional_convergence_reached && (!backwards_finished && feature_selection_type == feature_selection_types.backwards_then_forwards_repeated_until_convergence)) || (!unidirectional_convergence_reached && (forwards_finished && feature_selection_type == feature_selection_types.forwards_then_backwards_repeated_until_convergence));

                program.WriteLine($@"{iteration_loop} Search directions: can_go_forwards={can_go_forwards}; go_forwards={go_forwards}; can_go_backward={can_go_backwards}; go_backwards={go_backwards}; ");

                if (baseline_recalculation_required)
                {
                    var features_selected_str = string.Join("|", features_selected.Select(a => a.ToString().Replace(",", ";")).ToList());
                    var features_selected_total_group = features_selected.Count;
                    var features_selected_total_features = features_selected.Sum(a => a.set_members.Count);

                    program.WriteLine($@"{iteration_loop} Baseline ranking: Recalculating baseline rank:");
                    program.WriteLine($@"{iteration_loop} Baseline ranking: features: groups: {features_selected_total_group} features: {features_selected_total_features}. List: {features_selected_str}.");
                    iteration_recalculate++;

                    baseline_recalculation_required = false;

                    var prev_baseline_score = baseline_score;

                    var internal_experiment_id1 = $@"Iteration #{iteration_loop}".Replace(",", ";");
                    var internal_experiment_id2 = $@"Baseline Calc #{iteration_recalculate}".Replace(",", ";");
                    var internal_experiment_id3 = $@"{features_selected_str}".Replace(",", ";");

                    var svm_return = classify(features_selected, internal_experiment_id1, internal_experiment_id2, internal_experiment_id3);




                    var svm_return_data = svm_return.run_svm_return_data;
                    var svm_pred = svm_return_data.Where(a => a.prediction_list != null && a.prediction_list.Count > 0).SelectMany(a => a.prediction_list).ToList();
                    var svm_meta = svm_return_data.Where(a => a.prediction_meta_data != null && a.prediction_meta_data.Count > 0).SelectMany(a => a.prediction_meta_data).ToList();

                    var svm_cms_all = svm_return_data.Where(a => a.confusion_matrices != null && a.confusion_matrices.Count > 0).SelectMany(a => a.confusion_matrices).ToList();
                    var svm_cms_all_average = svm_cms_all.GroupBy(a => (a.testing_set_name, a.class_id)).Select(a => performance_measure.confusion_matrix.Average2(a.ToList())).ToList();

                    var svm_cms_perf_classes = svm_cms_all.Where(a => feature_selection_performance_classes == null || feature_selection_performance_classes.Count == 0 || feature_selection_performance_classes.Contains(a.class_id.Value)).ToList();
                    var svm_cms_perf_classes_average = performance_measure.confusion_matrix.Average2(svm_cms_perf_classes);
                    var feature_selection_metrics_average = svm_cms_perf_classes_average.get_values(feature_selection_performance_metrics).Average();

                    baseline_score = new score_metrics();
                    baseline_score.calc_change(feature_selection_performance_metrics, prev_baseline_score.score_after, feature_selection_metrics_average, prev_baseline_score.total_features_after, features_selected_total_features, prev_baseline_score.total_groups_after, features_selected_total_group);

                    var result = new iterative_task()
                    {
                        experiment_id1 = internal_experiment_id1,
                        experiment_id2 = internal_experiment_id2,
                        experiment_id3 = internal_experiment_id3,

                        baseline_score = baseline_score,
                        all_cm = svm_cms_all,
                        all_cm_average = svm_cms_all_average,
                        rank_cm = svm_cms_perf_classes_average,
                        direction = 'Z',
                        metrics = feature_selection_performance_metrics.ToString().Replace(",", ";"),
                        feature_set_list = features_selected,
                        loop_index = -1,
                        iteration_loop = iteration_loop,
                        iteration_select = iteration_select,
                        iteration_recalculate = iteration_recalculate,
                        task_id = -1,
                        duration_method = sw_method.ElapsedTicks,
                        duration_iteration = sw_iteration.ElapsedTicks,
                        duration_index = 0,
                        duration_task = 0,
                    };

                    var output_average_cm_base_recalc = (new List<iterative_task>() { result }).SelectMany(a => a.feature_set_list.Select(b => (a.experiment_id1, a.experiment_id2, a.experiment_id3, iteration: a.iteration_loop, direction: a.direction, feature_set: b, average_cm_list: new List<performance_measure.confusion_matrix>() { a.rank_cm }, all_cm_list: a.all_cm)).ToList()).ToList();
                    features_selected_average_confusion_matrices.AddRange(output_average_cm_base_recalc);

                    save_task(result);

                    var x1 = (new List<iterative_task>() { result }).SelectMany(a => a.feature_set_list.Select(b => (experiment_id1: a.experiment_id1, experiment_id2: a.experiment_id2, experiment_id3: a.experiment_id3, direction: a.direction, feature_set: b, iteration: a.iteration_select, score_metrics: new score_metrics(a.baseline_score, false), cm_average: a.rank_cm, cm_all: a.all_cm)).ToList()).ToList();
                    ranking_data.AddRange(x1);

                    score_improved = (baseline_score.score_after > baseline_score.score_before);

                    

                    program.WriteLine($@"{iteration_loop} Baseline ranking: New baseline rank... Previous rank: {baseline_score.score_before}.  New rank: {baseline_score.score_after}.  Change: {baseline_score.score_change}.  Improved: {score_improved}.");

                    program.WriteLine($@"{iteration_loop} Baseline ranking: Selection Classes Average Performance: {string.Join("; ", svm_cms_perf_classes_average?.get_value_strings().Select(a => $"{a.name}={a.value};").ToList())}");
                    svm_cms_all.ForEach(x => program.WriteLine($@"{iteration_loop} Baseline ranking: All Class Performance: {string.Join("; ", x?.get_value_strings().Select(a => $"{a.name}={a.value};").ToList())}"));

                }
                else //if (!baseline_recalculation_required)
                {
                    iteration_select++;
                    direction_change = false;
                    score_improved = false;
                    forwards_score_improved = false;
                    backwards_score_improved = false;

                    if ((go_forwards && can_go_forwards && !forwards_max_features_reached) || (go_backwards && can_go_backwards && !backwards_min_features_reached))
                    {


                        var tasks = new List<Task<iterative_task>>();
                        var backwards_tasks = new List<Task<iterative_task>>();
                        var forwards_tasks = new List<Task<iterative_task>>();

                        var final_backwards_iteration_index = 0;
                        var final_forwards_iteration_index = 0;

                        var backwards_num_features_already_selected = -1;
                        var forwards_num_features_not_selected = -1;

                        for (var features_input_index = 0; features_input_index < features_input.Count; features_input_index++)
                        {
                            var iteration_feature_set = features_input[features_input_index];
                            var feature_already_selected = features_selected.Any(a => a.set_id == iteration_feature_set.set_id);

                            if (feature_already_selected)
                            {
                                backwards_num_features_already_selected++;
                                final_backwards_iteration_index = features_input_index;
                            }
                            else
                            {
                                forwards_num_features_not_selected++;
                                final_forwards_iteration_index = features_input_index;
                            }
                        }

                        // backwards and forwards
                        for (var features_input_index = 0; features_input_index < features_input.Count; features_input_index++)
                        {
                            program.GC_Collection();

                            var stopwatch_index = new Stopwatch();
                            stopwatch_index.Start();

                            program.WriteLine($@"{iteration_loop} Loop: features_input_index={(features_input_index + 1)}/{features_input.Count}");

                            var iteration_feature_set = features_input[features_input_index];

                            if (iteration_feature_set == null || iteration_feature_set.set_members == null || iteration_feature_set.set_members.Count == 0) continue;

                            
                            // backwards bad = performance gain by removal of this feature
                            var backwards_bad_feature = backwards_feature_bad_feature_permanent?.Any(bad_feature => bad_feature.set_id == iteration_feature_set.set_id) ?? false;

                            // backwards good = no performance gain by removal of this feature
                            var backwards_good_feature = backwards_feature_good_feature_permanent?.Any(good_feature => good_feature.set_id == iteration_feature_set.set_id) ?? false;

                            // forwards bad = no performance gain by insertion of this feature
                            var forwards_bad_feature = forwards_feature_bad_feature_permanent?.Any(bad_feature => bad_feature.set_id == iteration_feature_set.set_id) ?? false;

                            // forwards good = performance gain by insertion of this feature
                            var forwards_good_feature = forwards_feature_good_feature_permanent?.Any(good_feature => good_feature.set_id == iteration_feature_set.set_id) ?? false;

                            if (features_backwards_buffer == null || features_backwards_buffer.Count == 0) features_backwards_buffer = new List<feature_set>();
                            if (features_forwards_buffer == null || features_forwards_buffer.Count == 0) features_forwards_buffer = new List<feature_set>();

                            var feature_already_selected = features_selected.Any(a => a.set_id == iteration_feature_set.set_id);

                            if (feature_already_selected) // if feature already selected, go backwards if requested/possible
                            {
                                if (go_backwards && can_go_backwards && !backwards_min_features_reached)
                                {

                                    if (backwards_good_feature || forwards_good_feature)
                                    {
                                        // marked as good feature; do not remove...

                                        program.WriteLine($@"{iteration_loop} Loop: Backwards skipping feature set: [{iteration_feature_set.source}.{iteration_feature_set.@group}]");

                                        if (features_input_index != final_backwards_iteration_index) continue;
                                    }
                                    else
                                    {
                                        features_backwards_buffer.Add(iteration_feature_set);
                                    }

                                    if ((features_input_index == final_backwards_iteration_index && features_backwards_buffer.Count > 0) || (features_selected.Count - features_backwards_buffer.Count <= backwards_min_features && features_backwards_buffer.Count > 0) || features_backwards_buffer.Count >= backwards_max_features_to_combine_per_iteration)
                                    {
                                        // remove unwanted features from selected_features
                                        var internal_b_features_selected_exclusive = features_selected.Where(a => features_backwards_buffer.All(b => a.set_id != b.set_id)).ToList();
                                        var internal_b_features_backwards_buffer = features_backwards_buffer.ToList();
                                        var internal_b_features_backwards_buffer_str = string.Join("|", internal_b_features_backwards_buffer.Select(a => a.ToString().Replace(",", ";")).ToList());

                                        features_backwards_buffer = new List<feature_set>();

                                        var internal_b_features_selected_exclusive_str = string.Join("|", internal_b_features_selected_exclusive.Select(a => a.ToString().Replace(",", ";")).ToList());
                                        var internal_b_total_groups_after = internal_b_features_selected_exclusive.Count;
                                        var internal_b_total_features_after = internal_b_features_selected_exclusive.Sum(a => a.set_members.Count);



                                        var internal_b_total_groups_before = features_selected.Count;
                                        var internal_b_total_features_before = features_selected.Sum(a => a.set_members.Count);

                                        var internal_b_task_id = external_task_id++;
                                        var internal_b_backwards_task_id = external_backwards_task_id++;
                                        var internal_b_iteration_loop = iteration_loop;
                                        var internal_b_iteration_select = iteration_select;
                                        var internal_b_iteration_recalculate = iteration_recalculate;

                                        var internal_b_feature_selection_combinator = feature_selection_combinator;
                                        var internal_b_perf_selection_rule = perf_selection_rule;
                                        var internal_b_feature_selection_type = feature_selection_type;

                                        //var internal_iteration_feature_set = iteration_feature_set;
                                        var internal_b_sw_method = sw_method;
                                        var internal_b_sw_iteration = sw_iteration;
                                        var internal_b_sw_index = stopwatch_index;
                                        var internal_b_baseline_score = baseline_score;

                                        var internal_b_feature_selection_performance_metrics = feature_selection_performance_metrics;
                                        var internal_b_feature_selection_performance_classes = feature_selection_performance_classes.ToList();

                                        //var internal_file_tag = $@"[{internal_feature_selection_type}]_[{internal_feature_selection_combinator}]_[{internal_perf_selection_rule}]_[{rand_cv}]_[{outer_cv}]_[{inner_cv}]_[{internal_task_id}]_[{internal_iteration_loop}]_[{iteration_select}]_[{iteration_recalculate}]";


                                        //var internal_experiment_id = $@"Iteration #{iteration_loop}. Backwards.";

                                        //var internal_experiment_name = $@"Backwards {internal_features_selected_exclusive_total_groups}g {internal_features_selected_exclusive_total_features}f: {internal_features_selected_exclusive_str}";
                                        //var internal_experiment_name = $@"{internal_features_selected_exclusive_str}";

                                        var internal_b_experiment_id1 = $@"{internal_b_features_backwards_buffer_str}".Replace(",", ";");
                                        var internal_b_experiment_id2 = $@"{internal_b_features_selected_exclusive_str}".Replace(",", ";");
                                        var internal_b_experiment_id3 = $@"".Replace(",", ";");

                                        // tag = remove / add feature ... feature name / feature id ... 

                                        var internal_b_global_params = this.ToString();
                                        var internal_b_loop_index = features_input_index;


                                        var backwards_task = Task.Run(() =>
                                        {
                                            var task_sw_task = new Stopwatch();
                                            task_sw_task.Start();

                                            // feature(s) already selected; remove
                                            //var x = internal_features_selected_exclusive.SelectMany(a => a.members.Select(b => b.member_name).ToList()).ToList();

                                            var svm_return = classify(internal_b_features_selected_exclusive, internal_b_experiment_id1, internal_b_experiment_id2, internal_b_experiment_id3);


                                            if (svm_return == null || svm_return.run_svm_return_data == null || svm_return.run_svm_return_data.Count == 0)
                                            {
                                                program.WriteLine($@"{internal_b_iteration_loop} Task: Error: svm return is empty");
                                                return null;
                                            }
                                            else
                                            {
                                                var svm_return_data = svm_return.run_svm_return_data;
                                                var svm_pred = svm_return_data.Where(a => a.prediction_list != null && a.prediction_list.Count > 0).SelectMany(a => a.prediction_list).ToList();
                                                var svm_meta = svm_return_data.Where(a => a.prediction_meta_data != null && a.prediction_meta_data.Count > 0).SelectMany(a => a.prediction_meta_data).ToList();

                                                var svm_cms_all = svm_return_data.Where(a => a.confusion_matrices != null && a.confusion_matrices.Count > 0).SelectMany(a => a.confusion_matrices).ToList();
                                                var svm_cms_all_average = svm_cms_all.GroupBy(a => (a.testing_set_name, a.class_id)).Select(a => performance_measure.confusion_matrix.Average2(a.ToList())).ToList();

                                                var svm_cms_perf_classes = svm_cms_all.Where(a => internal_b_feature_selection_performance_classes == null || internal_b_feature_selection_performance_classes.Count == 0 || internal_b_feature_selection_performance_classes.Contains(a.class_id.Value)).ToList();
                                                var svm_cms_perf_classes_average = performance_measure.confusion_matrix.Average2(svm_cms_perf_classes);
                                                var feature_selection_metrics_average = svm_cms_perf_classes_average.get_values(internal_b_feature_selection_performance_metrics).Average();

                                                var result = new iterative_task()
                                                {
                                                    experiment_id1 = internal_b_experiment_id1,
                                                    experiment_id2 = internal_b_experiment_id2,
                                                    experiment_id3 = internal_b_experiment_id3,

                                                    baseline_score = new score_metrics(),
                                                    all_cm = svm_cms_all,
                                                    all_cm_average = svm_cms_all_average,
                                                    rank_cm = svm_cms_perf_classes_average,
                                                    direction = 'B',
                                                    metrics = internal_b_feature_selection_performance_metrics.ToString().Replace(",", ";"),
                                                    feature_set_list = internal_b_features_backwards_buffer,
                                                    loop_index = internal_b_loop_index,
                                                    iteration_loop = internal_b_iteration_loop,
                                                    iteration_select = internal_b_iteration_select,
                                                    iteration_recalculate = internal_b_iteration_recalculate,
                                                    task_id = internal_b_task_id,
                                                    duration_method = internal_b_sw_method.ElapsedTicks,
                                                    duration_iteration = internal_b_sw_iteration.ElapsedTicks,
                                                    duration_index = internal_b_sw_index.ElapsedTicks,
                                                    duration_task = task_sw_task.ElapsedTicks,
                                                };

                                                result.calc_change(internal_b_feature_selection_performance_metrics, baseline_score.score_after, feature_selection_metrics_average, internal_b_total_features_before, internal_b_total_features_after, internal_b_total_groups_before, internal_b_total_groups_after);

                                                //save_task_result_as_csv(svm_cms_all, result, internal_b_global_params);

                                                save_task(result);

                                                program.WriteLine($@"{internal_b_iteration_loop} Task: Backwards ranking: Task: {string.Join("; ", result.AsArray().Select(a => $"{a.name}={a.value};").ToList())}");
                                                program.WriteLine($@"{internal_b_iteration_loop} Task: Backwards ranking: Groups: {internal_b_features_backwards_buffer.Count}; Features: {internal_b_features_backwards_buffer.Sum(a => a.set_members.Count)}; [{string.Join("; ", internal_b_features_backwards_buffer.Select(c => $"{c.source}.{c.@group}").ToList())}] -> {feature_selection_metrics_average}");
                                                program.WriteLine($@"{internal_b_iteration_loop} Task: Forwards ranking: Selection Classes Average Performance: {string.Join("; ", svm_cms_perf_classes_average?.get_value_strings().Select(a => $"{a.name}={a.value};").ToList())}");


                                                svm_cms_all.ForEach(x => program.WriteLine($@"{internal_b_iteration_loop} Task: Forwards ranking: All Class Performance: {string.Join("; ", x?.get_value_strings().Select(a => $"{a.name}={a.value};").ToList())}"));



                                                lock (tasks_lock)
                                                {
                                                    var dur_this_iteration = tasks.Where(a => a.IsCompleted).Select(a => a.Result.duration_task).ToList();
                                                    if (dur_this_iteration.Count > 0) program.WriteLine($"{internal_b_iteration_loop} Projected time: iteration using current: Min: {TimeSpan.FromTicks(dur_this_iteration.Min() * features_input.Count)}, Average: {TimeSpan.FromTicks((long)dur_this_iteration.Average() * features_input.Count)}, Max: {TimeSpan.FromTicks(dur_this_iteration.Max() * features_input.Count)}", true, ConsoleColor.Cyan);
                                                }

                                                var dur_last_iteration = task_durations.Where(a => a.iteration == internal_b_iteration_loop - 1).Select(a => a.ticks).ToList();
                                                var dur_all_iteration = task_durations.Select(a => a.ticks).ToList();


                                                if (dur_last_iteration.Count > 0) program.WriteLine($"{internal_b_iteration_loop} Projected time: iteration using last: Min: {TimeSpan.FromTicks(dur_last_iteration.Min() * features_input.Count)}, Average: {TimeSpan.FromTicks((long)dur_last_iteration.Average() * features_input.Count)}, Max: {TimeSpan.FromTicks(dur_last_iteration.Max() * features_input.Count)}", true, ConsoleColor.Cyan);
                                                if (dur_all_iteration.Count > 0) program.WriteLine($"{internal_b_iteration_loop} Projected time: iteration using all: Min: {TimeSpan.FromTicks(dur_all_iteration.Min() * features_input.Count)}, Average: {TimeSpan.FromTicks((long)dur_all_iteration.Average() * features_input.Count)}, Max: {TimeSpan.FromTicks(dur_all_iteration.Max() * features_input.Count)}", true, ConsoleColor.Cyan);


                                                return result;
                                            }
                                        });

                                        lock (tasks_lock)
                                        {
                                            backwards_tasks.Add(backwards_task);
                                            tasks.Add(backwards_task);
                                        }

                                        if (max_tasks > 0 && tasks != null && tasks.Count > 0)
                                        {
                                            var incomplete_tasks = tasks.Where(a => !a.IsCompleted).ToList();

                                            while (incomplete_tasks != null && incomplete_tasks.Count > 0 && incomplete_tasks.Count >= max_tasks)
                                            {
                                                program.WriteLine($@"{iteration_loop} Loop: {nameof(run_unidirectional_feature_selection)}(): Task.WaitAny(incomplete_tasks.ToArray<Task>());",true, ConsoleColor.Cyan);
                                                try
                                                {
                                                    Task.WaitAny(incomplete_tasks.ToArray<Task>());
                                                }
                                                catch (Exception e)
                                                {
                                                    program.WriteLine($@"{iteration_loop} Loop: {nameof(run_unidirectional_feature_selection)}(): {e.ToString()}", true, ConsoleColor.Cyan);
                                                }

                                                incomplete_tasks = tasks.Where(a => !a.IsCompleted).ToList();
                                            }
                                        }
                                    }
                                }
                            }
                            else // if feature *not* already selected, go forwards if requested/possible
                            {
                                if (go_forwards && can_go_forwards && !forwards_max_features_reached)
                                {
                                    if (backwards_bad_feature || forwards_bad_feature)
                                    {
                                        // marked as bad feature; do not insert...

                                        program.WriteLine($@"{iteration_loop} Loop: Forwards skipping feature set: [{iteration_feature_set.source}.{iteration_feature_set.@group}]");


                                        if (features_input_index != final_forwards_iteration_index) continue;
                                    }
                                    else
                                    {
                                        features_forwards_buffer.Add(iteration_feature_set);
                                    }

                                    if ((features_input_index == final_forwards_iteration_index && features_forwards_buffer.Count > 0) || (features_selected.Count + features_forwards_buffer.Count >= forwards_max_features && features_forwards_buffer.Count > 0) || features_forwards_buffer.Count >= forwards_max_features_to_combine_per_iteration)
                                    {
                                        // add prospective features to selected_features
                                        var internal_f_features_selected_inclusive = features_selected.Union(features_forwards_buffer).ToList();
                                        var internal_f_features_forwards_buffer = features_forwards_buffer.ToList();
                                        var internal_f_features_forwards_buffer_str = string.Join("|", internal_f_features_forwards_buffer.Select(a => a.ToString().Replace(",", ";")).ToList());
                                        features_forwards_buffer = new List<feature_set>();

                                        var internal_f_features_selected_inclusive_str = string.Join("|", internal_f_features_selected_inclusive.Select(a => a.ToString().Replace(",", ";")).ToList());
                                        var internal_f_total_groups_after = internal_f_features_selected_inclusive.Count;
                                        var internal_f_total_features_after = internal_f_features_selected_inclusive.Sum(a => a.set_members.Count);

                                        var internal_f_total_groups_before = features_selected.Count;
                                        var internal_f_total_features_before = features_selected.Sum(a => a.set_members.Count);

                                        var internal_f_task_id = external_task_id++;
                                        var internal_f_forwards_task_id = external_forwards_task_id++;
                                        var internal_f_iteration_loop = iteration_loop;
                                        var internal_f_iteration_select = iteration_select;
                                        var internal_f_iteration_recalculate = iteration_recalculate;

                                        var internal_f_feature_selection_combinator = feature_selection_combinator;
                                        var internal_f_perf_selection_rule = perf_selection_rule;
                                        var internal_f_feature_selection_type = feature_selection_type;
                                        //var internal_iteration_feature_set = iteration_feature_set;
                                        var internal_f_sw_method = sw_method;
                                        var internal_f_sw_iteration = sw_iteration;
                                        var internal_f_sw_index = stopwatch_index;
                                        var internal_f_baseline_score = baseline_score;

                                        var internal_f_experiment_id1 = $@"{internal_f_features_forwards_buffer_str}".Replace(",", ";"); ;
                                        var internal_f_experiment_id2 = $@"{internal_f_features_selected_inclusive_str}".Replace(",", ";"); ;
                                        var internal_f_experiment_id3 = $@"".Replace(",", ";"); ;


                                        var internal_f_global_params = this.ToString();

                                        var internal_f_loop_index = features_input_index;

                                        var internal_f_feature_selection_performance_metrics = feature_selection_performance_metrics;
                                        var internal_f_feature_selection_performance_classes = feature_selection_performance_classes.ToList();


                                        //var internal_file_tag = $@"[{internal_feature_selection_type}]_[{internal_feature_selection_combinator}]_[{internal_perf_selection_rule}]_[{rand_cv}]_[{outer_cv}]_[{inner_cv}]_[{internal_task_id}]_[{internal_iteration_loop}]_[{iteration_select}]_[{iteration_recalculate}]";

                                        var forwards_task = Task.Run(() =>
                                        {
                                            var task_sw_task = new Stopwatch();
                                            task_sw_task.Start();

                                            // feature(s) not selected; add
                                            // var x = internal_features_selected_inclusive.SelectMany(a => a.members.Select(b => b.member_name).ToList()).ToList();

                                            var svm_return = classify(internal_f_features_selected_inclusive, internal_f_experiment_id1, internal_f_experiment_id2, internal_f_experiment_id3);

                                            if (svm_return == null || svm_return.run_svm_return_data == null || svm_return.run_svm_return_data.Count == 0)
                                            {
                                                program.WriteLine($"{internal_f_iteration_loop} Loop: Error: svm return is empty");
                                                return null;
                                            }
                                            else
                                            {
                                                var svm_return_data = svm_return.run_svm_return_data;
                                                var svm_pred = svm_return_data.Where(a => a.prediction_list != null && a.prediction_list.Count > 0).SelectMany(a => a.prediction_list).ToList();
                                                var svm_meta = svm_return_data.Where(a => a.prediction_meta_data != null && a.prediction_meta_data.Count > 0).SelectMany(a => a.prediction_meta_data).ToList();
                                                //var svm_cms = svm_return_data.Where(a => a.confusion_matrices != null && a.confusion_matrices.Count > 0).SelectMany(a => a.confusion_matrices).ToList();
                                                //var svm_cms_classes = svm_cms.Where(a => internal_f_feature_selection_performance_classes == null || internal_f_feature_selection_performance_classes.Count == 0 || internal_f_feature_selection_performance_classes.Contains(a.class_id.Value)).GroupBy(a => (testing_set_name: a.testing_set_name, class_id: a.class_id.Value)).Select(a => (class_id: a.Key.class_id, testing_set_name: a.Key.testing_set_name, cm: a.ToList())).ToList();
                                                //var svm_fs_perf = svm_cms_classes.Select(a => a.cm.Select(b => b.get_values(internal_f_feature_selection_performance_metrics).Average()).Average()).Average();
                                                //var svm_all_metrics_perf = performance_measure.confusion_matrix.Average(svm_cms_classes.SelectMany(a => a.cm).ToList()).FirstOrDefault();
                                                //var task_forwards_rank = svm_fs_perf; //classify(internal_features_selected_inclusive, "", "");


                                                var svm_cms_all = svm_return_data.Where(a => a.confusion_matrices != null && a.confusion_matrices.Count > 0).SelectMany(a => a.confusion_matrices).ToList();
                                                var svm_cms_all_average = svm_cms_all.GroupBy(a => (a.testing_set_name, a.class_id)).Select(a => performance_measure.confusion_matrix.Average2(a.ToList())).ToList();

                                                var svm_cms_perf_classes = svm_cms_all.Where(a => internal_f_feature_selection_performance_classes == null || internal_f_feature_selection_performance_classes.Count == 0 || internal_f_feature_selection_performance_classes.Contains(a.class_id.Value)).ToList();
                                                var svm_cms_perf_classes_average = performance_measure.confusion_matrix.Average2(svm_cms_perf_classes);
                                                var feature_selection_metrics_average = svm_cms_perf_classes_average.get_values(internal_f_feature_selection_performance_metrics).Average();

                                                var result = new iterative_task()
                                                {
                                                    experiment_id1 = internal_f_experiment_id1,
                                                    experiment_id2 = internal_f_experiment_id2,
                                                    experiment_id3 = internal_f_experiment_id3,

                                                    baseline_score = new score_metrics(),
                                                    all_cm = svm_cms_all,
                                                    all_cm_average = svm_cms_all_average,
                                                    rank_cm = svm_cms_perf_classes_average,
                                                    direction = 'F',
                                                    metrics = internal_f_feature_selection_performance_metrics.ToString().Replace(",", ";"),
                                                    feature_set_list = internal_f_features_forwards_buffer,
                                                    loop_index = internal_f_loop_index,
                                                    iteration_loop = internal_f_iteration_loop,
                                                    iteration_select = internal_f_iteration_select,
                                                    iteration_recalculate = internal_f_iteration_recalculate,
                                                    task_id = internal_f_task_id,
                                                    duration_method = internal_f_sw_method.ElapsedTicks,
                                                    duration_iteration = internal_f_sw_iteration.ElapsedTicks,
                                                    duration_index = internal_f_sw_index.ElapsedTicks,
                                                    duration_task = task_sw_task.ElapsedTicks,
                                                };

                                                result.calc_change(internal_f_feature_selection_performance_metrics, baseline_score.score_after, feature_selection_metrics_average, internal_f_total_features_before, internal_f_total_features_after, internal_f_total_groups_before, internal_f_total_groups_after);

                                                //save_task_result_as_csv(svm_cms_all, result, internal_f_global_params);

                                                save_task(result);

                                                program.WriteLine($@"{internal_f_iteration_loop} Task: Forwards ranking: Task: {string.Join("; ", result.AsArray().Select(a => $"{a.name}={a.value};").ToList())}");
                                                program.WriteLine($@"{internal_f_iteration_loop} Task: Forwards ranking: Groups: {internal_f_features_forwards_buffer.Count}; Features: {internal_f_features_forwards_buffer.Sum(a => a.set_members.Count)}; [{string.Join("; ", internal_f_features_forwards_buffer.Select(c => $"{c.source}.{c.@group}").ToList())}] -> {feature_selection_metrics_average}");
                                                program.WriteLine($@"{internal_f_iteration_loop} Task: Forwards ranking: Selection Classes Average Performance: {string.Join("; ", svm_cms_perf_classes_average?.get_value_strings().Select(a => $"{a.name}={a.value};").ToList())}");
                                                svm_cms_all.ForEach(x => program.WriteLine($@"{internal_f_iteration_loop} Task: Forwards ranking: All Class Performance: {string.Join("; ", x?.get_value_strings().Select(a => $"{a.name}={a.value};").ToList())}"));


                                                

                                                lock (tasks_lock)
                                                {
                                                    var dur_this_iteration = tasks.Where(a => a.IsCompleted).Select(a => a.Result.duration_task).ToList();
                                                    if (dur_this_iteration.Count > 0) program.WriteLine($"{internal_f_iteration_loop} Projected time: iteration using current: Min: {TimeSpan.FromTicks(dur_this_iteration.Min() * features_input.Count)}, Average: {TimeSpan.FromTicks((long)dur_this_iteration.Average() * features_input.Count)}, Max: {TimeSpan.FromTicks(dur_this_iteration.Max() * features_input.Count)}", true, ConsoleColor.Cyan);
                                                }

                                                var dur_last_iteration = task_durations.Where(a => a.iteration == internal_f_iteration_loop - 1).Select(a => a.ticks).ToList();
                                                var dur_all_iteration = task_durations.Select(a => a.ticks).ToList();


                                                if (dur_last_iteration.Count > 0) program.WriteLine($"{internal_f_iteration_loop} Projected time: iteration using last: Min: {TimeSpan.FromTicks(dur_last_iteration.Min() * features_input.Count)}, Average: {TimeSpan.FromTicks((long)dur_last_iteration.Average() * features_input.Count)}, Max: {TimeSpan.FromTicks(dur_last_iteration.Max() * features_input.Count)}", true, ConsoleColor.Cyan);
                                                if (dur_all_iteration.Count > 0) program.WriteLine($"{internal_f_iteration_loop} Projected time: iteration using all: Min: {TimeSpan.FromTicks(dur_all_iteration.Min() * features_input.Count)}, Average: {TimeSpan.FromTicks((long)dur_all_iteration.Average() * features_input.Count)}, Max: {TimeSpan.FromTicks(dur_all_iteration.Max() * features_input.Count)}", true, ConsoleColor.Cyan);




                                                return result;
                                            }
                                        });

                                        lock (tasks_lock)
                                        {
                                            forwards_tasks.Add(forwards_task);
                                            tasks.Add(forwards_task);
                                        }

                                        if (max_tasks > 0 && tasks != null && tasks.Count > 0)
                                        {
                                            var incomplete_tasks = tasks.Where(a => !a.IsCompleted).ToList();

                                            while (incomplete_tasks != null && incomplete_tasks.Count > 0 && incomplete_tasks.Count >= max_tasks)
                                            {
                                                program.WriteLine(
                                                    $@"{iteration_loop} Loop: {nameof(run_unidirectional_feature_selection)}(): Task.WaitAny(incomplete_tasks.ToArray<Task>());",
                                                    true, ConsoleColor.Cyan);
                                                try
                                                {
                                                    Task.WaitAny(incomplete_tasks.ToArray<Task>());
                                                }
                                                catch (Exception e)
                                                {
                                                    program.WriteLine($@"{iteration_loop} Loop: {nameof(run_unidirectional_feature_selection)}(): {e.ToString()}", true, ConsoleColor.Cyan);
                                                }

                                                incomplete_tasks = tasks.Where(a => !a.IsCompleted).ToList();
                                            }
                                        }
                                    }
                                }
                            }

                            if (max_tasks > 0 && tasks != null && tasks.Count > 0)
                            {
                                var incomplete_tasks = tasks.Where(a => !a.IsCompleted).ToList();

                                while (incomplete_tasks != null && incomplete_tasks.Count > 0 && incomplete_tasks.Count >= max_tasks)
                                {
                                    program.WriteLine(
                                        $@"{iteration_loop} Loop: {nameof(run_unidirectional_feature_selection)}(): Task.WaitAny(incomplete_tasks.ToArray<Task>());",
                                        true, ConsoleColor.Cyan);
                                    try
                                    {
                                        Task.WaitAny(incomplete_tasks.ToArray<Task>());
                                    }
                                    catch (Exception e)
                                    {
                                        program.WriteLine($@"{iteration_loop} Loop: {nameof(run_unidirectional_feature_selection)}(): {e.ToString()}", true, ConsoleColor.Cyan);
                                    }

                                    incomplete_tasks = tasks.Where(a => !a.IsCompleted).ToList();
                                }
                            }
                        }

                        if (go_backwards && can_go_backwards && !backwards_min_features_reached)
                        {
                            program.WriteLine($@"{iteration_loop} Loop: {nameof(run_unidirectional_feature_selection)}(): Task.WaitAll(backwards_tasks.ToArray<Task>());", true, ConsoleColor.Cyan);
                            Task.WaitAll(backwards_tasks.ToArray<Task>());
                            task_durations.AddRange(backwards_tasks.Select(a => (a.Result.iteration_loop, a.Result.duration_task)).ToList());

                            backwards_selection_feature_rankings.AddRange(backwards_tasks.Select(a => a.Result).ToList());

                            var x = backwards_selection_feature_rankings.SelectMany(a => a.feature_set_list.Select(b => (experiment_id1: a.experiment_id1, experiment_id2: a.experiment_id2, experiment_id3: a.experiment_id3, direction: a.direction, feature_set: b, iteration: a.iteration_select, score_metrics: new score_metrics(a.baseline_score, true), cm_average: a.rank_cm, cm_all: a.all_cm)).ToList()).ToList();
                            ranking_data.AddRange(x);

                            switch (perf_selection_rule)
                            {
                                case perf_selection_rules.best_score:
                                    backwards_selection_feature_rankings = backwards_selection_feature_rankings.OrderByDescending(a => a.baseline_score.score_after).ThenByDescending(a => a.baseline_score.score_change).ThenByDescending(a => a.baseline_score.total_features_change).ToList();
                                    break;

                                case perf_selection_rules.best_ppf_overall:
                                    backwards_selection_feature_rankings = backwards_selection_feature_rankings.OrderByDescending(a => a.baseline_score.score_ppf_change).ThenByDescending(a => a.baseline_score.total_features_change).ToList();
                                    break;

                                case perf_selection_rules.best_ppg_overall:
                                    backwards_selection_feature_rankings = backwards_selection_feature_rankings.OrderByDescending(a => a.baseline_score.score_ppg_change).ThenByDescending(a => a.baseline_score.total_features_change).ToList();
                                    break;

                                case perf_selection_rules.best_ppf_change:
                                    backwards_selection_feature_rankings = backwards_selection_feature_rankings.OrderByDescending(a => a.baseline_score.score_change_ppf).ThenByDescending(a => a.baseline_score.total_features_change).ToList();
                                    break;

                                case perf_selection_rules.best_ppg_change:
                                    backwards_selection_feature_rankings = backwards_selection_feature_rankings.OrderByDescending(a => a.baseline_score.score_change_ppg).ThenByDescending(a => a.baseline_score.total_features_change).ToList();
                                    break;

                                case perf_selection_rules.best_average_of_score_and_ppf:
                                    backwards_selection_feature_rankings = backwards_selection_feature_rankings.OrderByDescending(a => new double[] { a.baseline_score.score_after, a.baseline_score.score_ppf_change }.Average()).ThenByDescending(a => a.baseline_score.total_features_change).ToList();
                                    break;

                                case perf_selection_rules.best_average_of_score_and_ppg:
                                    backwards_selection_feature_rankings = backwards_selection_feature_rankings.OrderByDescending(a => new double[] { a.baseline_score.score_after, a.baseline_score.score_ppg_change }.Average()).ThenByDescending(a => a.baseline_score.total_features_change).ToList();
                                    break;

                                case perf_selection_rules.best_average_of_score_and_ppf_normalised:
                                    var scores1 = backwards_selection_feature_rankings.Select(a => a.baseline_score.score_after).ToList();
                                    var score_added_ppfs = backwards_selection_feature_rankings.Select(a => a.baseline_score.score_ppf_change).ToList();

                                    backwards_selection_feature_rankings = backwards_selection_feature_rankings.OrderByDescending(a => new double[] { scale_value(a.baseline_score.score_after, scores1.Min(), scores1.Max()), scale_value(a.baseline_score.score_ppf_change, score_added_ppfs.Min(), score_added_ppfs.Max()), }.Average()).ThenByDescending(a => a.baseline_score.total_features_change).ToList();
                                    break;

                                case perf_selection_rules.best_average_of_score_and_ppg_normalised:
                                    var scores2 = backwards_selection_feature_rankings.Select(a => a.baseline_score.score_after).ToList();
                                    var score_added_ppgs = backwards_selection_feature_rankings.Select(a => a.baseline_score.score_ppg_change).ToList();

                                    backwards_selection_feature_rankings = backwards_selection_feature_rankings.OrderByDescending(a => new double[] { scale_value(a.baseline_score.score_after, scores2.Min(), scores2.Max()), scale_value(a.baseline_score.score_ppg_change, score_added_ppgs.Min(), score_added_ppgs.Max()), }.Average()).ThenByDescending(a => a.baseline_score.total_features_change).ToList();
                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }




                            // note: more than or equals because performance should go up when bad features are removed
                            backwards_selection_feature_rankings_bad_features = backwards_selection_feature_rankings.Where(a => a.baseline_score.score_after >= (baseline_score.score_after - margin_of_error))/*.OrderByDescending(a => a.baseline_score_after)*/.ToList();
                            backwards_selection_feature_rankings_taken_for_removal = backwards_selection_feature_rankings_bad_features.Take(backwards_max_features_to_remove_per_iteration).ToList();
                            backwards_selection_feature_rankings_could_have_taken_for_removal = backwards_selection_feature_rankings_bad_features.Skip(backwards_max_features_to_remove_per_iteration).ToList();
                            backwards_selection_feature_rankings_good_features = backwards_selection_feature_rankings.Except(backwards_selection_feature_rankings_bad_features).ToList();

                            // filter 'feature_importance_actual' to remove any backwards_selection_feature_rankings (note: object id may not match, so use set_id property instead...)
                            //feature_importance_actual = feature_importance_actual.Where(feature_importance_actual_item => backwards_selection_feature_rankings.All(b => b.feature_set_list.All(c => c.set_id != feature_importance_actual_item.feature_set.set_id))).ToList();

                            // insert into 'feature_importance_actual' the 'backwards_selection_feature_rankings'
                            //feature_importance_actual.AddRange(backwards_selection_feature_rankings.SelectMany(a => a.feature_set_list.Select(b => (feature_set: b, perf: -a.baseline_score.score_change)).ToList()).ToList());

                            // 
                            //feature_importance_average.AddRange(backwards_selection_feature_rankings.SelectMany(a => a.feature_set_list.Select(b => (feature_set: b, perf: -a.baseline_score.score_change)).ToList()).ToList());


                            if (backwards_max_feature_removal_attempts > 0)
                            {
                                backwards_selection_feature_rankings_bad_features.ForEach(bad_feature_list =>

                                bad_feature_list.feature_set_list.ForEach(bad_feature =>
                                {
                                    {
                                        var bad_feature_index = backwards_feature_bad_feature_log.FindIndex(a => a.feature_set.set_id == bad_feature.set_id);

                                        if (bad_feature_index > -1)
                                        {
                                            var bad_feature_iteration_count = backwards_feature_bad_feature_log[bad_feature_index].bad_feature_iteration_count;
                                            bad_feature_iteration_count++;
                                            if (bad_feature_iteration_count >= backwards_max_feature_removal_attempts)
                                            {
                                                backwards_feature_bad_feature_permanent.Add(bad_feature);
                                                backwards_feature_bad_feature_log.RemoveAt(bad_feature_index);
                                            }
                                            else
                                            {
                                                backwards_feature_bad_feature_log[bad_feature_index] = (bad_feature, bad_feature_iteration_count);
                                            }
                                        }
                                        else
                                        {
                                            backwards_feature_bad_feature_log.Add((bad_feature, 1));
                                        }
                                    }
                                }));

                                backwards_selection_feature_rankings_good_features.ForEach(good_feature_list =>

                                    good_feature_list.feature_set_list.ForEach(good_feature =>
                                    {
                                        var good_feature_index = backwards_feature_good_feature_log.FindIndex(a => a.feature_set.set_id == good_feature.set_id);

                                        if (good_feature_index > -1)
                                        {
                                            var good_feature_iteration_count = backwards_feature_good_feature_log[good_feature_index].good_feature_iteration_count;
                                            good_feature_iteration_count++;
                                            if (good_feature_iteration_count >= backwards_max_feature_removal_attempts)
                                            {
                                                backwards_feature_good_feature_permanent.Add(good_feature);
                                                backwards_feature_good_feature_log.RemoveAt(good_feature_index);
                                            }
                                            else
                                            {
                                                backwards_feature_good_feature_log[good_feature_index] = (good_feature, good_feature_iteration_count);
                                            }
                                        }
                                        else
                                        {
                                            backwards_feature_good_feature_log.Add((good_feature, 1));
                                        }
                                    }
                                ));
                            }

                            if (backwards_selection_feature_rankings_taken_for_removal != null && backwards_selection_feature_rankings_taken_for_removal.Count > 0)
                            {
                                backwards_score_improved = true;
                                score_improved = true;



                                var backwards_features_taken_names = backwards_selection_feature_rankings_taken_for_removal.SelectMany(a => a.feature_set_list).ToList();


                                var output_average_cm_backwards = backwards_selection_feature_rankings_taken_for_removal.SelectMany(a => a.feature_set_list.Select(b => (experiment_id1: a.experiment_id1, experiment_id2: a.experiment_id2, experiment_id3: a.experiment_id3, iteration: a.iteration_loop, direction: a.direction, feature_set: b, average_cm_list: new List<performance_measure.confusion_matrix>() { a.rank_cm }, all_cm_list: a.all_cm)).ToList()).ToList();
                                features_selected_average_confusion_matrices.AddRange(output_average_cm_backwards);





                                features_selected = features_selected.Except(backwards_features_taken_names).ToList();

                                program.WriteLine($@"{iteration_loop} Loop: Backwards removed features: {backwards_features_taken_names.Count} -> [{string.Join(", ", backwards_features_taken_names.Select(b => $"{b.source}.{b.@group}").ToList())}]");

                                consecutive_backwards_iterations++;
                                consecutive_backwards_iterations_with_improvement++;
                                consecutive_backwards_iterations_without_improvement = 0;
                            }
                            else
                            {
                                consecutive_backwards_iterations++;
                                consecutive_backwards_iterations_with_improvement = 0;
                                consecutive_backwards_iterations_without_improvement++;
                            }

                            if (backwards_selection_feature_rankings != null && backwards_selection_feature_rankings.Count > 0)
                            {
                                program.WriteLine($@"{iteration_loop} Loop: Backwards set: {backwards_selection_feature_rankings.Count} -> [{string.Join(", ", backwards_selection_feature_rankings.SelectMany(c => c.feature_set_list.Select(b => $"{b.source}.{b.@group}").ToList()).ToList())}]");
                                program.WriteLine($@"{iteration_loop} Loop: Backwards set taken: {backwards_selection_feature_rankings_taken_for_removal.Count} -> [{string.Join(", ", backwards_selection_feature_rankings_taken_for_removal.SelectMany(c => c.feature_set_list.Select(b => $"{b.source}.{b.@group}").ToList()).ToList())}]");
                            }
                        }
                        else
                        {
                            consecutive_backwards_iterations = 0;
                        }

                        if (go_forwards && can_go_forwards && !forwards_max_features_reached)
                        {
                            program.WriteLine($@"{iteration_loop} Loop: {nameof(run_unidirectional_feature_selection)}(): Task.WaitAll(forwards_tasks.ToArray<Task>());", true, ConsoleColor.Cyan);
                            Task.WaitAll(forwards_tasks.ToArray<Task>());
                            task_durations.AddRange(forwards_tasks.Select(a => (a.Result.iteration_loop, a.Result.duration_task)).ToList());

                            forwards_selection_feature_rankings.AddRange(forwards_tasks.Select(a => a.Result).ToList());

                            // reorder 'forwards_selection_feature_rankings' so that the best features are at the top of the list

                            var x = forwards_selection_feature_rankings.SelectMany(a => a.feature_set_list.Select(b => (experiment_id1: a.experiment_id1, experiment_id2: a.experiment_id2, experiment_id3: a.experiment_id3, direction: a.direction, feature_set: b, iteration: a.iteration_select, score_metrics: new score_metrics(a.baseline_score, false), cm_average: a.rank_cm, cm_all: a.all_cm)).ToList()).ToList();
                            ranking_data.AddRange(x);

                            switch (perf_selection_rule)
                            {
                                case perf_selection_rules.best_score:
                                    forwards_selection_feature_rankings = forwards_selection_feature_rankings.OrderByDescending(a => a.baseline_score.score_after).ThenByDescending(a => a.baseline_score.score_change).ThenBy(a => a.baseline_score.total_features_change).ToList();
                                    break;

                                case perf_selection_rules.best_ppf_overall:
                                    forwards_selection_feature_rankings = forwards_selection_feature_rankings.OrderByDescending(a => a.baseline_score.score_ppf_change).ThenBy(a => a.baseline_score.total_features_change).ToList();
                                    break;

                                case perf_selection_rules.best_ppg_overall:
                                    forwards_selection_feature_rankings = forwards_selection_feature_rankings.OrderByDescending(a => a.baseline_score.score_ppg_change).ThenBy(a => a.baseline_score.total_features_change).ToList();
                                    break;

                                case perf_selection_rules.best_ppf_change:
                                    forwards_selection_feature_rankings = forwards_selection_feature_rankings.OrderByDescending(a => a.baseline_score.score_change_ppf).ThenBy(a => a.baseline_score.total_features_change).ToList();
                                    break;

                                case perf_selection_rules.best_ppg_change:
                                    forwards_selection_feature_rankings = forwards_selection_feature_rankings.OrderByDescending(a => a.baseline_score.score_change_ppg).ThenBy(a => a.baseline_score.total_features_change).ToList();
                                    break;

                                case perf_selection_rules.best_average_of_score_and_ppf:
                                    forwards_selection_feature_rankings = forwards_selection_feature_rankings.OrderByDescending(a => new double[] { a.baseline_score.score_after, a.baseline_score.score_ppf_change }.Average()).ThenBy(a => a.baseline_score.total_features_change).ToList();
                                    break;

                                case perf_selection_rules.best_average_of_score_and_ppg:
                                    forwards_selection_feature_rankings = forwards_selection_feature_rankings.OrderByDescending(a => new double[] { a.baseline_score.score_after, a.baseline_score.score_ppg_change }.Average()).ThenBy(a => a.baseline_score.total_features_change).ToList();
                                    break;

                                case perf_selection_rules.best_average_of_score_and_ppf_normalised:
                                    var scores1 = forwards_selection_feature_rankings.Select(a => a.baseline_score.score_after).ToList();
                                    var score_added_ppfs = forwards_selection_feature_rankings.Select(a => a.baseline_score.score_ppf_change).ToList();

                                    forwards_selection_feature_rankings = forwards_selection_feature_rankings.OrderByDescending(a => new double[] { scale_value(a.baseline_score.score_after, scores1.Min(), scores1.Max()), scale_value(a.baseline_score.score_ppf_change, score_added_ppfs.Min(), score_added_ppfs.Max()), }.Average()).ThenBy(a => a.baseline_score.total_features_change).ToList();
                                    break;

                                case perf_selection_rules.best_average_of_score_and_ppg_normalised:
                                    var scores2 = forwards_selection_feature_rankings.Select(a => a.baseline_score.score_after).ToList();
                                    var score_added_ppgs = forwards_selection_feature_rankings.Select(a => a.baseline_score.score_ppg_change).ToList();

                                    forwards_selection_feature_rankings = forwards_selection_feature_rankings.OrderByDescending(a => new double[] { scale_value(a.baseline_score.score_after, scores2.Min(), scores2.Max()), scale_value(a.baseline_score.score_ppg_change, score_added_ppgs.Min(), score_added_ppgs.Max()), }.Average()).ThenBy(a => a.baseline_score.total_features_change).ToList();
                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }


                            forwards_selection_feature_rankings_good_features = forwards_selection_feature_rankings.Where(a => a.baseline_score.score_after > random_baseline && a.baseline_score.score_after > (baseline_score.score_after + margin_of_error)) /*.OrderByDescending(a => a.baseline_score_after)*/.ToList();

                            forwards_selection_feature_rankings_taken_for_insertion = forwards_selection_feature_rankings_good_features.Take(forwards_max_features_to_add_per_iteration).ToList();
                            forwards_selection_feature_rankings_could_have_taken_for_insertion = forwards_selection_feature_rankings_good_features.Skip(forwards_max_features_to_add_per_iteration).ToList();
                            forwards_selection_feature_rankings_bad_features = forwards_selection_feature_rankings.Except(forwards_selection_feature_rankings_good_features).ToList();

                            //features_forwards_buffer

                            // previous version... adding only 1 feature per task
                            //feature_importance_actual = feature_importance_actual.Where(a => forwards_selection_feature_rankings.All(b => b.feature_set.set_id != a.feature_set.set_id)).ToList();
                            //feature_importance_actual.AddRange(forwards_selection_feature_rankings.Select(a => (a.feature_set, (baseline_score - a.baseline_score_after))).ToList());
                            //feature_importance_average.AddRange(forwards_selection_feature_rankings.Select(a => (a.feature_set, (baseline_score - a.baseline_score_after))).ToList());

                            // actual = current/real time rank ??? or what ?

                            // feature_importance_actual = the real time up to date result
                            //feature_importance_actual = feature_importance_actual.Where(a => forwards_selection_feature_rankings.All(b => b.feature_set_list.All(c => c.set_id != a.feature_set.set_id))).ToList();
                            //feature_importance_actual.AddRange(forwards_selection_feature_rankings.SelectMany(a => a.feature_set_list.Select(b => (feature_set: b, perf: a.baseline_score.score_change)).ToList()).ToList());

                            // feature_importance_average = the average over all iterations
                            //feature_importance_average.AddRange(forwards_selection_feature_rankings.SelectMany(a => a.feature_set_list.Select(b => (feature_set: b, perf: a.baseline_score.score_change)).ToList()).ToList());

                            if (forwards_max_features_insertion_attempts > 0)
                            {
                                forwards_selection_feature_rankings_bad_features.ForEach(bad_feature_list =>
                                {
                                    bad_feature_list.feature_set_list.ForEach(bad_feature =>
                                    {
                                        var bad_feature_index = forwards_feature_bad_feature_log.FindIndex(a => a.feature_set.set_id == bad_feature.set_id);
                                        //var bad_feature_indexes = forwards_feature_bad_feature_log.FindIndex(a => a.feature_set.set_id == bad_feature.feature_set.set_id);

                                        if (bad_feature_index > -1)
                                        {
                                            var bad_feature_iteration_count = forwards_feature_bad_feature_log[bad_feature_index].bad_feature_iteration_count;
                                            bad_feature_iteration_count++;
                                            forwards_feature_bad_feature_log[bad_feature_index] = (bad_feature, bad_feature_iteration_count);

                                            if (bad_feature_iteration_count >= forwards_max_features_insertion_attempts)
                                            {
                                                forwards_feature_bad_feature_permanent.Add(bad_feature);
                                                forwards_feature_bad_feature_log.RemoveAt(bad_feature_index);
                                            }
                                            else
                                            {
                                                forwards_feature_bad_feature_log[bad_feature_index] = (bad_feature, bad_feature_iteration_count);
                                            }
                                        }
                                        else
                                        {
                                            forwards_feature_bad_feature_log.Add((bad_feature, 1));
                                        }
                                    });
                                });

                                forwards_selection_feature_rankings_good_features.ForEach(good_feature_list =>
                                good_feature_list.feature_set_list.ForEach(good_feature =>
                                {
                                    {
                                        var good_feature_index = forwards_feature_good_feature_log.FindIndex(a => a.feature_set.set_id == good_feature.set_id);

                                        if (good_feature_index > -1)
                                        {
                                            var good_feature_iteration_count = forwards_feature_good_feature_log[good_feature_index].good_feature_iteration_count;
                                            good_feature_iteration_count++;
                                            forwards_feature_good_feature_log[good_feature_index] = (good_feature, good_feature_iteration_count);

                                            if (good_feature_iteration_count >= forwards_max_features_insertion_attempts)
                                            {
                                                forwards_feature_good_feature_permanent.Add(good_feature);
                                                forwards_feature_good_feature_log.RemoveAt(good_feature_index);
                                            }
                                            else
                                            {
                                                forwards_feature_good_feature_log[good_feature_index] = (good_feature, good_feature_iteration_count);
                                            }
                                        }
                                        else
                                        {
                                            forwards_feature_good_feature_log.Add((good_feature, 1));
                                        }
                                    }
                                }));
                            }

                            if (forwards_selection_feature_rankings_taken_for_insertion != null && forwards_selection_feature_rankings_taken_for_insertion.Count > 0)
                            {
                                forwards_score_improved = true;
                                score_improved = true;

                                var forwards_features_taken_feature_sets = forwards_selection_feature_rankings_taken_for_insertion.SelectMany(a => a.feature_set_list).ToList();


                                var output_average_cm_forwards = forwards_selection_feature_rankings_taken_for_insertion.SelectMany(a => a.feature_set_list.Select(b => (a.experiment_id1, a.experiment_id2, a.experiment_id3, iteration: a.iteration_loop, direction: a.direction, feature_set: b, average_cm_list: new List<performance_measure.confusion_matrix>() { a.rank_cm }, all_cm_list: a.all_cm)).ToList()).ToList();
                                features_selected_average_confusion_matrices.AddRange(output_average_cm_forwards);


                                //var forwards_features_taken_feature_sets_cm = forwards_selection_feature_rankings_taken_for_insertion.SelectMany(a => (a.feature_set_list, a.all_cm)).ToList();

                                features_selected.AddRange(forwards_features_taken_feature_sets);

                                program.WriteLine($@"{iteration_loop} Loop: Forwards added features: {forwards_features_taken_feature_sets.Count} -> [{string.Join(", ", forwards_features_taken_feature_sets.Select(a => a.source + "." + a.@group).ToList())}]");

                                consecutive_forwards_iterations++;
                                consecutive_forwards_iterations_with_improvement++;
                                consecutive_forwards_iterations_without_improvement = 0;
                            }
                            else
                            {
                                consecutive_forwards_iterations++;
                                consecutive_forwards_iterations_with_improvement = 0;
                                consecutive_forwards_iterations_without_improvement++;
                            }

                            if (forwards_selection_feature_rankings != null && forwards_selection_feature_rankings.Count > 0)
                            {
                                program.WriteLine($@"{iteration_loop} Loop: Forwards set: {forwards_selection_feature_rankings.Count} -> [{string.Join(", ", forwards_selection_feature_rankings)}]");
                                program.WriteLine($@"{iteration_loop} Loop: Forwards set taken: {forwards_selection_feature_rankings_taken_for_insertion.Count} -> [{string.Join(", ", forwards_selection_feature_rankings_taken_for_insertion.SelectMany(a => a.feature_set_list.Select(b => b.source + "." + b.@group).ToList()).ToList())}]");
                            }
                        }
                        else
                        {
                            consecutive_forwards_iterations = 0;
                        }

                        //if (first)
                        //{
                        //    // note: 'first' is also needed further down in the code so not set to false here.
                        //
                        //    // store information from the first iteration (either forwards or backwards) for feature ranking information
                        //
                        //    if (backwards_selection_feature_rankings != null && backwards_selection_feature_rankings.Count > 0)
                        //    {
                        //        feature_importance_first_iteration.AddRange(backwards_selection_feature_rankings.OrderBy(a => a.baseline_score.score_after).SelectMany(a => a.feature_set_list.Select(b => (feature_set: b, perf: -a.baseline_score.score_change)).ToList()).ToList());
                        //    }
                        //
                        //    if (forwards_selection_feature_rankings != null && forwards_selection_feature_rankings.Count > 0)
                        //    {
                        //        feature_importance_first_iteration.AddRange(forwards_selection_feature_rankings.OrderByDescending(a => a.baseline_score.score_after).SelectMany(a => a.feature_set_list.Select(b => (feature_set: b, perf: a.baseline_score.score_change)).ToList()).ToList());
                        //    }
                        //
                        //    feature_importance_first_iteration = feature_importance_first_iteration.OrderByDescending(a => a.perf).ToList();
                        //
                        //
                        //}

                        // optimise for the case where only 1 feature is added/removed
                        if (forwards_selection_feature_rankings_taken_for_insertion.Count == 1 && backwards_selection_feature_rankings_taken_for_removal.Count == 0)
                        {
                            var forwards_baseline_score = forwards_selection_feature_rankings_taken_for_insertion.First().baseline_score;
                            baseline_score = forwards_baseline_score;
                            baseline_recalculation_required = false;
                        }
                        else if (forwards_selection_feature_rankings_taken_for_insertion.Count == 0 && backwards_selection_feature_rankings_taken_for_removal.Count == 1)
                        {
                            var backwards_baseline_score = backwards_selection_feature_rankings_taken_for_removal.First().baseline_score;
                            baseline_score = backwards_baseline_score;
                            baseline_recalculation_required = false;
                        }
                        else if (forwards_selection_feature_rankings_taken_for_insertion.Count > 0 || backwards_selection_feature_rankings_taken_for_removal.Count > 0)
                        {
                            baseline_recalculation_required = true;
                        }
                        else
                        {
                            baseline_recalculation_required = false;
                        }

                        if (!score_improved)
                        {
                            consecutive_iterations_with_improvement = 0;
                            consecutive_iterations_without_improvement++;
                        }
                        else
                        {
                            consecutive_iterations_with_improvement++;
                            consecutive_iterations_without_improvement = 0;
                        }

                        backwards_min_features_reached = (backwards_min_features > 0 && features_selected.Count <= backwards_min_features);
                        forwards_max_features_reached = (forwards_max_features > 0 && features_selected.Count >= forwards_max_features);

                        // change search direction if no score improvement + for convergence search
                        if ((!score_improved || forwards_max_features_reached) && !forwards_finished && go_forwards && (feature_selection_type == feature_selection_types.forwards_then_backwards || feature_selection_type == feature_selection_types.forwards_then_backwards_repeated_until_convergence))
                        {
                            forwards_finished = true;
                            direction_change = true;
                            if (feature_selection_type == feature_selection_types.forwards_then_backwards_repeated_until_convergence)
                            {
                                backwards_finished = false;
                            }
                        }
                        else if ((!score_improved || backwards_min_features_reached) && !backwards_finished && go_backwards && (feature_selection_type == feature_selection_types.backwards_then_forwards || feature_selection_type == feature_selection_types.backwards_then_forwards_repeated_until_convergence))
                        {
                            backwards_finished = true;
                            direction_change = true;
                            if (feature_selection_type == feature_selection_types.backwards_then_forwards_repeated_until_convergence)
                            {
                                forwards_finished = false;
                            }
                        }

                        if (consecutive_iterations_without_improvement >= 2)
                        {
                            unidirectional_convergence_reached = true;
                            backwards_finished = true;
                            forwards_finished = true;
                            score_improved = false;
                            direction_change = false;
                        }
                    }

                    //if (store_last_iteration)
                    //{
                    //    backwards_selection_feature_rankings_last_iteration = backwards_selection_feature_rankings;
                    //    backwards_selection_feature_rankings_taken_for_removal_last_iteration = backwards_selection_feature_rankings_taken_for_removal;
                    //    backwards_selection_feature_rankings_could_have_taken_for_removal_last_iteration = backwards_selection_feature_rankings_could_have_taken_for_removal;
                    //    backwards_selection_feature_rankings_good_features_last_iteration = backwards_selection_feature_rankings_good_features;
                    //    backwards_selection_feature_rankings_bad_features_last_iteration = backwards_selection_feature_rankings_bad_features;
                    //
                    //    forwards_selection_feature_rankings_last_iteration = forwards_selection_feature_rankings;
                    //    forwards_selection_feature_rankings_taken_for_insertion_last_iteration = forwards_selection_feature_rankings_taken_for_insertion;
                    //    forwards_selection_feature_rankings_could_have_taken_for_insertion_last_iteration = forwards_selection_feature_rankings_could_have_taken_for_insertion;
                    //    forwards_selection_feature_rankings_good_features_last_iteration = forwards_selection_feature_rankings_good_features;
                    //    forwards_selection_feature_rankings_bad_features_last_iteration = forwards_selection_feature_rankings_bad_features;
                    //}

                    //if (store_history_log)
                    //{
                    //    backwards_selection_feature_rankings_history_log.Add(backwards_selection_feature_rankings);
                    //    backwards_selection_feature_rankings_taken_for_removal_history_log.Add(backwards_selection_feature_rankings_taken_for_removal);
                    //    backwards_selection_feature_rankings_could_have_taken_for_removal_history_log.Add(backwards_selection_feature_rankings_could_have_taken_for_removal);
                    //    backwards_selection_feature_rankings_good_features_history_log.Add(backwards_selection_feature_rankings_good_features);
                    //    backwards_selection_feature_rankings_bad_features_history_log.Add(backwards_selection_feature_rankings_bad_features);
                    //
                    //    forwards_selection_feature_rankings_history_log.Add(forwards_selection_feature_rankings);
                    //    forwards_selection_feature_rankings_taken_for_insertion_history_log.Add(forwards_selection_feature_rankings_taken_for_insertion);
                    //    forwards_selection_feature_rankings_could_have_taken_for_insertion_history_log.Add(forwards_selection_feature_rankings_could_have_taken_for_insertion);
                    //    forwards_selection_feature_rankings_good_features_history_log.Add(forwards_selection_feature_rankings_good_features);
                    //    forwards_selection_feature_rankings_bad_features_history_log.Add(forwards_selection_feature_rankings_bad_features);
                    //}

                    backwards_selection_feature_rankings = new List<iterative_task>();
                    backwards_selection_feature_rankings_taken_for_removal = new List<iterative_task>();
                    backwards_selection_feature_rankings_could_have_taken_for_removal = new List<iterative_task>();
                    backwards_selection_feature_rankings_good_features = new List<iterative_task>();
                    backwards_selection_feature_rankings_bad_features = new List<iterative_task>();

                    forwards_selection_feature_rankings = new List<iterative_task>();
                    forwards_selection_feature_rankings_taken_for_insertion = new List<iterative_task>();
                    forwards_selection_feature_rankings_could_have_taken_for_insertion = new List<iterative_task>();
                    forwards_selection_feature_rankings_good_features = new List<iterative_task>();
                    forwards_selection_feature_rankings_bad_features = new List<iterative_task>();

                    can_go_forwards = features_input.Count - ( /*base_features.Count +*/ backwards_feature_bad_feature_permanent.Count + forwards_feature_bad_feature_permanent.Count + features_selected.Count) > 0;
                    can_go_backwards = features_selected.Count - (backwards_feature_good_feature_permanent.Count + forwards_feature_good_feature_permanent.Count) > 0;
                }

                program.WriteLine($@"{iteration_loop} End of iteration: Selected features at end: {features_selected.Count} [{string.Join(", ", features_selected.Select(a => a.source + "." + a.@group))}] -> {baseline_score}");
                program.WriteLine($@"{iteration_loop} End of iteration: Ranking status at end: score_improved={score_improved}, forwards_score_improved={forwards_score_improved}, backwards_score_improved={backwards_score_improved}, direction_change={direction_change}, forwards_max_features_reached={forwards_max_features_reached}, forwards_finished={forwards_finished}, can_go_forwards={can_go_forwards}, backwards_min_features_reached={backwards_min_features_reached}, backwards_finished={backwards_finished}, can_go_backwards={can_go_backwards}, unidirectional_convergence_reached={unidirectional_convergence_reached}");

                //////////////////////////
                // ranking
                //

                //if (first)
                //{
                //    first = false;
                //    feature_importance_first_iteration = feature_importance_first_iteration.OrderByDescending(a => a.perf).ToList();
                //}

                //// perform downward averaging
                //feature_importance_average = feature_importance_average.GroupBy(a => a.feature_set.set_id).Select(a => (feature_set: a.First(b => b.feature_set.set_id == a.Key).feature_set, perf: a.Select(b => b.perf).Average())).OrderByDescending(a => a.perf).ToList();

                //feature_importance_actual = feature_importance_actual.OrderByDescending(a => a.perf).ToList();

                //
                //
                //////////////////////////

                //switch (reorder_input_each_iteration)
                //{
                //    case reordering_rules.random_order:
                //        features_input.shuffle();
                //        break;
                //
                //    case reordering_rules.order_perf_asc:
                //        features_input = features_input.OrderBy(a => a).ToList();
                //        break;
                //
                //    case reordering_rules.order_perf_desc:
                //        features_input = features_input.OrderByDescending(a => a).ToList();
                //        break;
                //}


                //var serialized = serialise_json(this).with_refs;
                //var serialized_len = serialized.Length;
                //var deserialized = deserialise_json(serialized);

                save_iteration_parameters();

            } while (baseline_recalculation_required || ((iteration_select < iteration_select_max || iteration_select_max < 0) && ((score_improved && (can_go_backwards || can_go_forwards)) || ((go_backwards && direction_change && can_go_forwards) || (go_forwards && direction_change && can_go_backwards)))));

            // todo: what if baseline needs recalculation? e.g. on the iteration_select condition


            //save_ranks();


            return (features_selected, baseline_score);
        }

    }
}
