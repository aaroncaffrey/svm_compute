using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using svm_compute;

namespace svm_compute
{
    public class feature_correl
    {

        public static void calc_correl(List<(string cluster_name, string dimension, string alphabet, string group_name, int num_features)> feature_group_clusters, List<(int fid, string source, string group, string member, string perspective)> dataset_headers, string[] remove_sources, string[] remove_groups, string[] remove_members, string[] remove_perspectives, List<(List<(string comment_header, string comment_value)> comment_columns, List<(int fid, double fv)> feature_data)> dataset_instance_list, string file_dt)
        {
            program.WriteLine($@"{nameof(calc_correl)}()");
            var start_time = DateTime.Now;

            program.WriteLine($@"Calculating correlation matrix");
            var selected_feature_group_names_1d = feature_group_clusters.Where(a => a.dimension == "1").Select(a => a.group_name).Distinct().ToList();

            // get all matching "group names" feature sets (including all perspectives)
            var selected_feature_headers = dataset_headers.Where(a =>
                //source_feature_names.Contains(a.source) && 
                selected_feature_group_names_1d.Contains(a.@group)).ToList();

            // filter out unwanted perspectives
            selected_feature_headers = selected_feature_headers.Where(a => !remove_sources.Any(b => a.source.Contains(b))).ToList();
            selected_feature_headers = selected_feature_headers.Where(a => !remove_groups.Any(b => a.@group.Contains(b))).ToList();
            selected_feature_headers = selected_feature_headers.Where(a => !remove_members.Any(b => a.member.Contains(b))).ToList();
            selected_feature_headers = selected_feature_headers.Where(a => !remove_perspectives.Any(b => a.perspective.Contains(b))).ToList();

            // remove duplicates
            selected_feature_headers = selected_feature_headers.Distinct().ToList();

            // get fids -- fids are not in order -- and not consequtive
            var fids_names = selected_feature_headers.OrderBy(a => a.fid).Select(a => $"{a.source}.{a.@group}.{a.member}.{a.perspective}").ToList();
            var fids = selected_feature_headers.OrderBy(a => a.fid).Select(a => a.fid).ToList();

            //fids.Insert(0, class_id_feature_index);
            fids = fids.Distinct().ToList();

            var feature_data_fid_indexes = dataset_instance_list.First().feature_data.Select(a => a.fid).ToList();
            var fids_indexes = fids.Select(fid => feature_data_fid_indexes.IndexOf(fid)).ToList();


            var columns = new double[fids.Count - 1][];

            program.WriteLine($@"Calculating correlation matrix: getting column data: (columns: {columns.Length}, rows: {dataset_instance_list.Count})");

            for (var fid_index = 1; fid_index < fids.Count; fid_index++)
            {
                var column_index = fid_index - 1;

                var fid = fids[fid_index];
                var fid_feature_data_index = fids_indexes[fid_index];

                //var col = new double[dataset_instance_list.Count];

                var col = dataset_instance_list.Select(a => a.feature_data[fid_feature_data_index].fv).ToArray();

                columns[column_index] = col;
            }


            var column_pairs = columns.SelectMany((a, i) => columns.Select((b, j) => (i, j)).ToList()).Where(a => a.i < a.j).ToArray();

            var correls_p = new double[column_pairs.Length];
            var correls_s = new double[column_pairs.Length];
            var correls_k = new double[column_pairs.Length];

            program.WriteLine($@"Calculating correlation matrix: getting correlations for {column_pairs.Length} pairs");
            var correl_tasks = new List<Task>();
            var max_correl_tasks = Environment.ProcessorCount * 10;

            var sw_start_time = new Stopwatch();
            sw_start_time.Start();


            var inc_i = (int)Math.Ceiling((double)column_pairs.Length / (double)max_correl_tasks);
            var task_id = 0;
            for (var i = 0; i < column_pairs.Length; i += inc_i)
            {
                var l_this_i = i;
                var l_final_i = l_this_i + inc_i;
                var l_task_id = task_id;

                var task = Task.Run(() =>
                {
                    var c = 1;
                    var total = (l_final_i - l_this_i) + 1;
                    var current = 0;

                    for (var x1 = l_this_i; x1 < l_final_i && x1 < column_pairs.Length; x1++)
                    {
                        correls_p[x1] = MathNet.Numerics.Statistics.Correlation.Pearson(columns[column_pairs[x1].i], columns[column_pairs[x1].j]);
                        correls_s[x1] = MathNet.Numerics.Statistics.Correlation.Spearman(columns[column_pairs[x1].i], columns[column_pairs[x1].j]);
                        correls_k[x1] = kendall.kendall_tau(columns[column_pairs[x1].i], columns[column_pairs[x1].j]);

                        current++;
                        c++;
                        if (c == 1000)
                        {
                            c = 1;
                            var pct_done = (double)current / (double)total;



                            TimeSpan eta = TimeSpan.FromTicks(DateTime.Now.Subtract(start_time).Ticks * (total - (current)) / (current));


                            program.WriteLine($@"{l_task_id}: {pct_done}, eta: {eta.Days}d {eta.Hours}h {eta.Minutes}m {eta.Seconds}s");
                        }
                    }
                });

                correl_tasks.Add(task);
                task_id++;

                var incomplete_correl_tasks = correl_tasks.Where(a => !a.IsCompleted).ToList();

                while (max_correl_tasks>0 && incomplete_correl_tasks.Count >= max_correl_tasks)
                {
                    program.WriteLine($@"calc_correl(): Task.WaitAny(correl_tasks.ToArray<Task>());", true, ConsoleColor.Cyan);

                    Task.WaitAny(incomplete_correl_tasks.ToArray<Task>());

                    incomplete_correl_tasks = correl_tasks.Where(a => !a.IsCompleted).ToList();
                }
            }

            program.WriteLine($@"calc_correl(): Task.WaitAll(correl_tasks.ToArray<Task>());", true, ConsoleColor.Cyan);
            Task.WaitAll(correl_tasks.ToArray<Task>());


            program.WriteLine($@"Calculating correlation matrix: saving");

            var correl_out = column_pairs.Select((pair, index) => $"{fids_names[pair.i]},{pair.i},{fids_names[pair.j]},{pair.j},{correls_p[index]},{correls_s[index]},{correls_k[index]}").ToList();

            correl_out.Insert(0, string.Join(",", "Name X", "Index X", "Name Y", "Index Y", "Pearson", "Spearman", "Kendall"));

            var correl_filename = $@"c:\svm_compute\correl\correls_{file_dt}.csv";

            Directory.CreateDirectory(Path.GetDirectoryName(correl_filename));
            File.WriteAllLines(correl_filename, correl_out);
            //Task.WaitAll(correl_tasks.ToArray<Task>());
        }

    }
}
