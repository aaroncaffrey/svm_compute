using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using svm_compute;

namespace svm_compute
{
    public class program
    {
        //public const string root_folder = @"c:\betastrands_dataset\";
        //public static bool test_mode = false;

        public static readonly object console_lock = new object();
        public static readonly object console_log_lock = new object();

        public static int log_line_number = 0;
        public static int log_suffix = 0;
        public static bool log_console_to_file = true;


        public static string program_start_time;

        private static readonly object gc_lock = new object();
        public static void GC_Collection(bool collect = false)
        {
            if (!collect) return;

            lock (gc_lock)
            {
                try
                {
                    GC.Collect();
                }
                catch (Exception e)
                {
                    program.WriteLine($"{nameof(GC_Collection)}(): " + e.ToString(), true, ConsoleColor.DarkGray);
                    throw;
                }
            }
        }



        public static void WriteLine(string text, bool timestamp = true, ConsoleColor foreground_cc = ConsoleColor.White, ConsoleColor background_cc = ConsoleColor.Black)
        {
            //#if DEBUG
            var dt = timestamp ? $"[{DateTime.Now}] " : "";

            var text2 = text;

            if (dt.Length + text2.Length > Console.WindowWidth)
            {
                text2 = text2.Substring(0, (Console.WindowWidth - dt.Length) - 4) + "...";
            }

            lock (program.console_lock)
            {
                var reset = false;

                if (foreground_cc != ConsoleColor.White)
                {
                    Console.ForegroundColor = foreground_cc;
                    reset = true;
                }

                if (background_cc != ConsoleColor.Black)
                {
                    Console.BackgroundColor = background_cc;
                    reset = true;
                }

                Console.WriteLine($"{dt}{text2}");

                if (reset)
                {
                    Console.ResetColor();
                }

                if (log_console_to_file)
                {
                    lock (console_log_lock)
                    {
                        log_line_number++;

                        if (log_line_number >= 1024 * 25)
                        {
                            log_suffix++;
                            log_line_number = 0;
                        }

                        try
                        {
                            File.AppendAllLines(
                                $@"c:\svm_compute\console_log\{program_start_time}\{program_start_time}_{master_or_compute}_console_log_{log_suffix}.log",
                                new string[] {text});
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }
            //#endif
        }

        public static void maximise_window()
        {
            extensions.show_window_console(extensions.show_window_options.SW_MAXIMIZE);
        }


        public static string master_or_compute;

        public static void Main(string[] args)
        {
            program_start_time = DateTime.Now.ToString().Replace(":", "-").Replace("/", "-").Replace(" ", "_");

            Directory.CreateDirectory($@"c:\svm_compute\input_dataset\");
            Directory.CreateDirectory($@"c:\svm_compute\hosts\");
            Directory.CreateDirectory($@"c:\svm_compute\cache\request\");
            Directory.CreateDirectory($@"c:\svm_compute\cache\response\");
            Directory.CreateDirectory($@"c:\svm_compute\console_log\{program_start_time}\");
            Directory.CreateDirectory($@"c:\svm_compute\tcp_log\{program_start_time}\");
            Directory.CreateDirectory($@"c:\svm_compute\charts\{program_start_time}\");
            Directory.CreateDirectory($@"c:\svm_compute\feature_selection\{program_start_time}\");

#if DEBUG
            program.WriteLine($@"Running in debug mode... press enter to continue.", true, ConsoleColor.Red);
            //Console.ReadLine();
#endif



            //var psi1 = new ProcessStartInfo() {FileName = @"C:\Windows\system32\notepad.exe"};

            //using (var pro = Process.Start(psi1))
            //{
            //    pro.PriorityClass = ProcessPriorityClass.High;
            //    pro.PriorityBoostEnabled = true;

            //    while (true)
            //    {
            //        var t = DateTime.Now-pro.StartTime;
            //        Console.WriteLine("Time Running: " + t);
            //        Console.WriteLine("PrivilegedProcessorTime: " + pro.PrivilegedProcessorTime);
            //        Console.WriteLine("UserProcessorTime: " + pro.UserProcessorTime);
            //        Console.WriteLine("TotalProcessorTime: " + pro.TotalProcessorTime);
            //        Task.Delay(new TimeSpan(0, 0, 1)).Wait();
            //    }
            //}

            //Console.ReadLine();
            //return;

            //var z = new feature_selection_unidirectional();
            //z.set_defaults();
            //var y = z.AsArrayStrings();
            //Console.WriteLine(string.Join("\r\n", y));
            //Console.ReadKey();
            //return;
            ////var pt = cross_validation_remote.make_system_perf_task();
            //var spt = system_perf.get_system_perf_task();

            //while (true)
            //{
            //    Console.WriteLine("Free cpu usage: " + system_perf.get_average_cpu_free(3));
            //    Console.WriteLine("Average ram usage: " + system_perf.get_average_ram_free_mb(3) + "Mb");
            //    Task.Delay(1000).Wait();
            //}
            //Console.ReadKey();
            //return;
            maximise_window();
            libsvm_caller.wait_libsvm();
            var this_exe = Environment.GetCommandLineArgs()[0];
            //var args_str = string.Join(" ", args);
            // leave commented: slave_hostnames = slave_hostnames.Where(a => !a.ToUpperInvariant().Contains(Dns.GetHostName().ToUpperInvariant())).ToArray();

            //var is_auto_update = true;

            // slave 
            //is_slave_unit = true;

            //if (args.Length == 0)
            //{
            //    args = new[]
            //    {
            //        "/master",
            //        "/run_slave",
            //        //"/slave",
            //        "/test",
            //        "/local"
            //    };
            //}

            //program.test_mode = args.Any(a => a == "/test");

            //var use_local_slave = args.Any(a => a == "/local");

            //if (use_local_slave)
            //{
            //    cross_validation_remote.slave_hostnames = new List<(string hostname, int port)>() { ("localhost", cross_validation_remote.default_server_port) };
            //}

            //var set_master = true;

            var is_master_unit = args.Any(a => a == "/master");
            var is_compute_unit = args.Any(a => a == "/compute");

            if (is_compute_unit) is_master_unit = false;

            if (!is_master_unit && !is_compute_unit) is_master_unit = true;

            if (is_master_unit) master_or_compute = "master";
            if (is_compute_unit) master_or_compute = "compute";
            if (is_master_unit && is_compute_unit) master_or_compute = "master_and_compute";

            //var do_run_slave = args.Any(a => a == "/run_slave");
            //var do_run_master = args.Any(a => a == "/run_master");

            //if (!is_slave_unit && !is_master_unit)
            //{
            //    is_master_unit = true;
            //    do_run_slave = true;
            //}

            //if (do_run_master)
            //{
            //    var psi = new ProcessStartInfo()
            //    {
            //        FileName = this_exe,
            //        Arguments = string.Join(" ", "/master", /*test_mode ? "/test" : "",*/ use_local_slave ? "/local" : "")
            //    };

            //    var p = Process.Start(psi);
            //}

            //var do_run_slave = false;

            //if (is_master_unit)
            //{
            //    do_run_slave = true;
            //}


            //if (do_run_slave)
            //{
            //    var psi = new ProcessStartInfo()
            //    {
            //        FileName = this_exe,
            //        //Arguments = string.Join(" ", "/slave", /*test_mode ? "/test" : "",*/ use_local_slave ? "/local" : "")
            //        Arguments = string.Join(" ", "/slave")
            //    };

            //    var p = Process.Start(psi);
            //}

            //var update_tasks = new List<Task>();
            //var slave_tasks = new List<Task>();
            //var master_tasks = new List<Task>();

            //var update_task_cancellation_source = new CancellationTokenSource();
            var slave_task_cancellation_source = new CancellationTokenSource();
            var master_task_cancellation_source = new CancellationTokenSource();



            if (is_compute_unit && is_master_unit) throw new Exception();
            if (!is_compute_unit && !is_master_unit) throw new Exception();


            if (is_compute_unit)
            {
                //if (is_auto_update)
                //{
                //    update_tasks.Add(Task.Run(() => { auto_update.auto_update_loop(this_exe, args_str, update_task_cancellation_source.Token); }, update_task_cancellation_source.Token));
                //}

                //slave_tasks.Add(Task.Run(() => { cross_validation_remote.compute_loop(slave_task_cancellation_source.Token); }, slave_task_cancellation_source.Token));

                cross_validation_remote.compute_loop(slave_task_cancellation_source.Token);

                //update_task_cancellation_source.Cancel();
                //Task.WaitAll(update_tasks.ToArray<Task>());
            }

            if (is_master_unit)
            {
                //if (!use_local_slave)
                //{

                //string[] xx;


                load_hosts();

                //var hosts_folder = $@"c:\svm_compute\hosts\";

                //                if (Directory.Exists(hosts_folder))
                //                {

                //                    var hosts_files = Directory.GetFiles(hosts_folder);
                //                    var host_files_data = hosts_files.SelectMany(a => File.ReadAllLines(a).Select(b => b.Trim()).ToArray()).Distinct().Where(a => !string.IsNullOrWhiteSpace(a)).Select(a => $@"{a}.kuds.kingston.ac.uk").ToArray();

                //                    cross_validation_remote.compute_unit_hostnames = host_files_data.Select(a => (a, cross_validation_remote.default_server_port)).ToList();

                //#if DEBUG
                //                    //cross_validation_remote.compute_unit_hostnames = new List<(string hostname, int port)>() { ( "127.0.0.1", 843) };
                //#endif
                //                }
                //                else
                //                {
                //                    program.WriteLine("Hosts folder not found - Continue?", true, ConsoleColor.Red);
                //                    Console.ReadLine();
                //                }



                //}

                //master_tasks.Add(Task.Run(() => { master_compute(master_task_cancellation_source.Token); }, master_task_cancellation_source.Token));

                master_compute(master_task_cancellation_source.Token);
            }

            //var all_tasks = new List<Task>().Union(master_tasks).Union(slave_tasks).Union(update_tasks).ToList();

            //program.WriteLine($@"Main(): Task.WaitAll(all_tasks.ToArray<Task>());", true, ConsoleColor.Cyan);
            //Task.WaitAll(all_tasks.ToArray<Task>());
        }

        public static void load_hosts()
        {
            var hosts_file = $@"c:\svm_compute\hosts\hosts.csv";

            try
            {
                if (File.Exists(hosts_file) && new FileInfo(hosts_file).Length > 0)
                {
                    lock (cross_validation_remote.compute_unit_hostnames_lock)
                    {
                        var hosts_lines = File.ReadAllLines(hosts_file).Skip(1).ToList();
                        var hosts_data = hosts_lines.Select(a =>
                        {
                            var x = a.Split(',').Select(b => b.Trim()).ToList();

                            return (hostname: x[0], port: int.Parse(x[1]),
                                enabled: string.Equals(x[2], "true", StringComparison.InvariantCultureIgnoreCase) ||
                                         x[2] == "1");
                        }).ToList();



                        cross_validation_remote.compute_unit_hostnames = hosts_data.Select(a => (a.hostname, a.port)).ToList();
                    }
                }
            }
            catch (Exception e)
            {
                program.WriteLine($@"load_hosts(): {e.ToString()}", true, ConsoleColor.Red);
            }

            //var hosts_folder = $@"c:\svm_compute\hosts\";

            //try
            //{
            //    if (Directory.Exists(hosts_folder))
            //    {

            //        var hosts_files = Directory.GetFiles(hosts_folder);

            //        if (hosts_files != null && hosts_files.Length > 0)
            //        {
            //            lock (cross_validation_remote.compute_unit_hostnames_lock)
            //            {
            //                var host_files_data = hosts_files.SelectMany(a => File.ReadAllLines(a).Select(b => b.Trim()).ToArray()).Distinct().Where(a => !string.IsNullOrWhiteSpace(a)).Select(a => $@"{a}.kuds.kingston.ac.uk").ToArray();

            //                cross_validation_remote.compute_unit_hostnames = host_files_data.Select(a => (a, cross_validation_remote.default_server_port)).ToList();
            //            }
            //        }

            //    }
            //}
            //catch (Exception e)
            //{
            //    program.WriteLine($@"load_hosts(): {e.ToString()}", true, ConsoleColor.Red);
            //}
            //finally
            //{

            //}
        }

        public void wildcard_search_feature_list(
        List<(int fid, string source, string group, string member, string perspective)> dataset_headers,
        List<(string name, string source, List<(string name, string group, List<(string name, string member, List<(string name, string perspective)> list)> list)> list)> dataset_headers_grouped)
        {
            //var search_matches = new List<(string source, string group, string member, string perspective)>();

            //search_matches.Add
            //((
            //    source: WildCardToRegular("main"),
            //    group: WildCardToRegular("*"),
            //    member: WildCardToRegular("*"),
            //    perspective: WildCardToRegular("*mean*")
            //));

            //program.WriteLine($@"{nameof(Main)}: {nameof(search_matches)}: {string.Join(", ", search_matches)}");   

            //// find features required
            //    if (search_matches != null && search_matches.Count > 0)
            //    {
            //        var required_dataset_headers = dataset_headers.Where(a => a.fid == 0 || search_matches.Any(sm => (Regex.IsMatch(a.source, sm.source)) && (Regex.IsMatch(a.group, sm.group)) && (Regex.IsMatch(a.member, sm.member)) && (Regex.IsMatch(a.perspective, sm.perspective)))).ToList();

            //        var required_fids = required_dataset_headers.Select(a => a.fid).ToList();


            //        if (required_fids != null && required_fids.Count > 0)
            //        {
            //            // remove unwanted features
            //            dataset_csv_files_headers_all = dataset_csv_files_headers_all.Select(a => a.Where((b, i) => required_fids.Contains(i)).ToList()).ToList();
            //            dataset_csv_files_headers = dataset_csv_files_headers.Where((a, i) => required_fids.Contains(i)).ToList();
            //            dataset_headers = dataset_headers.Where(a => required_fids.Contains(a.fid)).ToList();
            //            dataset_headers_grouped = dataset_headers.Where(a => a.source != "class_id").GroupBy(a => a.source).Select(a => (name: a.Key, source: a.Key, list: a.GroupBy(b => (b.source, b.group)).Select(c => (name: $"{c.Key.source}.{c.Key.group}", group: c.Key.group, list: c.GroupBy(d => (d.source, d.group, d.member)).Select(e => (name: $"{e.Key.source}.{e.Key.group}.{e.Key.member}", member: e.Key.member, list: e /*.GroupBy(f => (f.source, f.group, f.member, f.perspective))*/.Select(g => (name: $"{g. /*Key.*/source}.{g. /*Key.*/group}.{g. /*Key.*/member}.{g. /*Key.*/perspective}", perspective: g. /*Key.*/perspective)).ToList())).ToList())).ToList())).ToList();
            //            dataset_instance_list = dataset_instance_list.Select(a =>
            //            {
            //                a.feature_data = a.feature_data.Where(b => required_fids.Contains(b.fid)).ToList();
            //                return a;
            //            }).ToList();

            //            //if (novel_dataset_instance_list != null && novel_dataset_instance_list.Count > 0)
            //            //{
            //            //    novel_dataset_csv_files_headers_all = novel_dataset_csv_files_headers_all.Select(a => a.Where((b, i) => required_fids.Contains(i)).ToList()).ToList();
            //            //    novel_dataset_csv_files_headers = novel_dataset_csv_files_headers.Where((a, i) => required_fids.Contains(i)).ToList();
            //            //    novel_dataset_instance_list = novel_dataset_instance_list.Select(a =>
            //            //    {
            //            //        a.feature_data = a.feature_data.Where(b => required_fids.Contains(b.fid)).ToList();
            //            //        return a;
            //            //    }).ToList();
            //            //}
            //        }
            //    }
        }

        //public static List<(string cluster_name, string dimension, string alphabet, string source_name, string group_name, int num_features)> filter_features(List<(string cluster_name, string dimension, string alphabet, string source_name, string group_name, int num_features)> feature_group_clusters, bool aaindex_only = false)
        //{
        //    // List of items to REMOVE
        //    var remove_cluster_names = new string[]
        //    {
        //        // comment out which items to KEEP
        //        //"aaindex",
        //        "count_normal",
        //        "count_sqrt"
        //    };

        //    var remove_cluster_dimensions = new string[]
        //    {
        //        // comment out which items to KEEP
        //        //"1",
        //        "0","2","3", "4"
        //    };

        //    var remove_cluster_alphabets = new string[]
        //    {
        //        // comment out which items to KEEP
        //        //"hydrophobicity",
        //        //"pdbsum",
        //        //"physicochemical",
        //        //"uniprotkb", 

        //        //"normal",
        //        // "venn"
        //    };

        //    var remove_cluster_groups = new string[]
        //    {
        //        // comment out which items to KEEP
        //        "_min", "_max", "_range",
        //        //"_mode",
        //        //"_median",
        //        "count_normal", "count_sqrt"
        //    };

        //    var remove_sources = new string[]
        //    {
        //        // comment out which items to KEEP
        //        //"class_id",
        //        //"main",
        //        "neighbourhood_1d",
        //        "neighbourhood_3d"
        //    };

        //    var remove_groups = new string[]
        //    {
        //        // comment out which items to KEEP
        //        "_min", "_max", "_range", "_mode",
        //        //"_median",
        //        "count_normal", "count_sqrt"
        //    };

        //    var remove_members = new string[]
        //    {
        //        // comment out which items to KEEP

        //    };

        //    var remove_perspectives = new string[]
        //    {
        //        // comment out which items to KEEP

        //        //"mean_arithmetic",
        //        //"dev_standard",
        //        //"median_q2",
        //        "mode",


        //        "count", "count_zero_values", "count_non_zero_values", "count_distinct_values", "sum",
        //        "min", "max", "range", "mid_range", "variance",
        //        "skewness", "kurtosis", "interquartile_range", "median_q1",
        //        "median_q3", "sum_of_error", "sum_of_error_square",
        //        "mad_mean_arithmetic", "mad_median_q1", "mad_median_q2", "mad_median_q3", "mad_mode", "mad_mid_range"
        //    };

        //    // clusters: remove null, empty or white space cluster names, alphabets, or group names
        //    feature_group_clusters = feature_group_clusters.Where(a => !string.IsNullOrWhiteSpace(a.cluster_name) && !string.IsNullOrWhiteSpace(a.alphabet) && !string.IsNullOrWhiteSpace(a.group_name)).ToList();

        //    if (aaindex_only)
        //    {
        //        feature_group_clusters = feature_group_clusters.Where(a => String.Equals(a.cluster_name, "aaindex", StringComparison.InvariantCultureIgnoreCase)).ToList();
        //    }

        //    // clusters: remove any matching items from the filters
        //    if (remove_cluster_names != null && remove_cluster_names.Length > 0)
        //    {
        //        feature_group_clusters = feature_group_clusters.Where(a => !remove_cluster_names.Any(b => a.cluster_name.ToUpperInvariant().Contains(b.ToUpperInvariant()))).ToList();
        //    }

        //    if (remove_cluster_dimensions != null && remove_cluster_dimensions.Length > 0)
        //    {
        //        feature_group_clusters = feature_group_clusters.Where(a => !remove_cluster_dimensions.Any(b => a.dimension.ToUpperInvariant().Contains(b.ToUpperInvariant()))).ToList();
        //    }

        //    if (remove_cluster_alphabets != null && remove_cluster_alphabets.Length > 0)
        //    {
        //        feature_group_clusters = feature_group_clusters.Where(a => !remove_cluster_alphabets.Any(b => a.alphabet.ToUpperInvariant().Contains(b.ToUpperInvariant()))).ToList();
        //    }

        //    if (remove_cluster_groups != null && remove_cluster_groups.Length > 0)
        //    {
        //        feature_group_clusters = feature_group_clusters.Where(a => !remove_cluster_groups.Any(b => a.group_name.ToUpperInvariant().Contains(b.ToUpperInvariant()))).ToList();
        //    }

        //    if (remove_sources != null && remove_sources.Length > 0)
        //    {
        //        feature_group_clusters = feature_group_clusters.Where(a => !remove_sources.Any(b => a.source_name.ToUpperInvariant().Contains(b.ToUpperInvariant()))).ToList();
        //    }

        //    return feature_group_clusters;
        //}

        //public struct dataset_instance
        //{
        //    public int negative_class_id;
        //    public int positive_class_id;
        //    public List<(int class_id, string class_name)> class_names;
        //    public List<(string comment_header, string comment_value)> comment_columns;
        //    public List<(int fid, double fv)> feature_data;
        //}

        public static
            (
            List<(int fid, string alphabet, string dimension, string category, string source, string group, string member, string perspective, int alphabet_id, int dimension_id, int category_id, int source_id, int group_id, int member_id, int perspective_id)> dataset_headers,
            //List<(string name, string source, List<(string name, string group, List<(string name, string member, List<(string name, string perspective)> list)> list)> list)> dataset_headers_grouped,

            List<(int filename_index, int line_index, List<(string comment_header, string comment_value)> comment_columns)> dataset_comment_row_values,
            List<(List<(string comment_header, string comment_value)> comment_columns, List<(int fid, double fv)> feature_data)> dataset_instance_list
            //List<dataset_instance> dataset_instance_list
            )
            read_binary_dataset(
            bool output_feature_tree,
            int negative_class_id,
            int positive_class_id,
            List<(int class_id, string class_name)> class_names,
            //List<string> novel_dataset_csv_files = null,
            //List<string> novel_dataset_comment_csv_files = null,
            //List<string> novel_dataset_comment_csv_files = null,
            bool test_mode = false
            )
        {
            //$"{a.alphabet},{a.dimension},{a.category},{a.source},{a.@group},{a.member},{a.perspective}"


            var lock_table = new object();

            var table_alphabet = new List<string>();
            var table_dimension = new List<string>();
            var table_category = new List<string>();
            var table_source = new List<string>();
            var table_group = new List<string>();
            var table_member = new List<string>();
            var table_perspective = new List<string>();

            //var count_table_alphabet = new List<int>();
            //var count_table_dimension = new List<int>();
            //var count_table_category = new List<int>();
            //var count_table_source = new List<int>();
            //var count_table_group = new List<int>();
            //var count_table_member = new List<int>();
            //var count_table_perspective = new List<int>();

            // note: removed 'novel' related code temporarily as currently unneeded...

            // novel_dataset_csv_files:
            // @"{program.root_folder}\bioinf\e_larks_unknown_seq_only_1.txt"

            // novel_dataset_comment_csv_files:
            // @"{program.root_folder}\bioinf\c_standard_beta_strand_2",
            // @"{program.root_folder}\bioinf\c_dimorphics_2"
            // @"{program.root_folder}\bioinf\e_standard_beta_strand_full_protein_sequence_2",
            // @"{program.root_folder}\bioinf\e_dimorphics_full_protein_sequence_2"

            var dataset_csv_files = new List<string>()
            {
                Path.Combine($@"c:\svm_compute\input_dataset\", $@"f__[{class_names.First(a => a.class_id == positive_class_id).class_name}].txt"),
                Path.Combine($@"c:\svm_compute\input_dataset\", $@"f__[{class_names.First(a => a.class_id == negative_class_id).class_name}].txt"),
            };

            var dataset_comment_csv_files = new List<string>
            {
                Path.Combine($@"c:\svm_compute\input_dataset\", $@"c__[{class_names.First(a => a.class_id == positive_class_id).class_name}].txt"),
                Path.Combine($@"c:\svm_compute\input_dataset\", $@"c__[{class_names.First(a => a.class_id == negative_class_id).class_name}].txt"),
            };

            program.WriteLine($@"{nameof(read_binary_dataset)}.{nameof(output_feature_tree)} = {output_feature_tree}");
            program.WriteLine($@"{nameof(read_binary_dataset)}.{nameof(negative_class_id)} = {negative_class_id}");
            program.WriteLine($@"{nameof(read_binary_dataset)}.{nameof(positive_class_id)} = {positive_class_id}");
            program.WriteLine($@"{nameof(read_binary_dataset)}.{nameof(class_names)} = {string.Join(", ", class_names)}");
            program.WriteLine($@"{nameof(read_binary_dataset)}: {nameof(dataset_csv_files)}: {string.Join(", ", dataset_csv_files)}");
            program.WriteLine($@"{nameof(read_binary_dataset)}: {nameof(dataset_comment_csv_files)}: {string.Join(", ", dataset_comment_csv_files)}");
            //program.WriteLine($@"{nameof(read_binary_dataset)}: {nameof(novel_dataset_csv_files)}: {string.Join(", ", novel_dataset_csv_files)}");
            //program.WriteLine($@"{nameof(read_binary_dataset)}: {nameof(novel_dataset_comment_csv_files)}: {string.Join(", ", novel_dataset_comment_csv_files)}");

            // read standard CSV input file headers
            program.WriteLine($@"{nameof(read_binary_dataset)}: Reading non-novel dataset headers...");
            var dataset_csv_files_headers_all_files = dataset_csv_files.AsParallel().AsOrdered().Select(a => File.ReadLines(a).First().Split(',').ToList()).ToList();
            var dataset_csv_files_headers = dataset_csv_files_headers_all_files.First();

            // check standard CSV file headers match
            if (dataset_csv_files_headers_all_files.AsParallel().AsOrdered().Any(a => !a.SequenceEqual(dataset_csv_files_headers)))
            {
                throw new Exception();
            }

            // read standard COMMENT input file headers
            program.WriteLine($@"{nameof(read_binary_dataset)}: Reading known dataset comment headers...");
            var dataset_comment_csv_files_headers_all_files = dataset_comment_csv_files.AsParallel().AsOrdered().Select(a => File.ReadLines(a).First().Split(',').ToList()).ToList();
            var dataset_comment_csv_files_headers = dataset_comment_csv_files_headers_all_files.FirstOrDefault();


            //if (novel_dataset_csv_files != null && novel_dataset_csv_files.Count > 0)
            //{
            //    // read novel CSV input file headers
            //    program.WriteLine($@"{nameof(read_binary_dataset)}: Reading novel dataset headers...");
            //    List<List<string>> novel_dataset_csv_files_headers_all = null;
            //    List<string> novel_dataset_csv_files_headers = null;

            //    novel_dataset_csv_files_headers_all = novel_dataset_csv_files.Select(a => File.ReadLines(a).First().Split(',').ToList()).ToList();
            //    novel_dataset_csv_files_headers = novel_dataset_csv_files_headers_all.First();

            //    // check novel dataset classes have the same headers
            //    if (!novel_dataset_csv_files_headers.SequenceEqual(novel_dataset_csv_files_headers_all.SelectMany(a => a).Distinct()))
            //    {
            //        throw new Exception();
            //    }

            //    // check standard dataset and novel dataset have the same headers
            //    if (!dataset_csv_files_headers.SequenceEqual(novel_dataset_csv_files_headers))
            //    {
            //        throw new Exception();
            //    }

            //    // reading novel COMMENTS input file headers
            //    program.WriteLine($@"{nameof(read_binary_dataset)}: Reading novel dataset comment headers...");
            //    var novel_dataset_comment_csv_files_headers_all = novel_dataset_comment_csv_files.Select(a => File.ReadLines(a).First().Split(',').ToList()).ToList();
            //    var novel_dataset_comment_csv_files_headers = novel_dataset_comment_csv_files_headers_all.FirstOrDefault();
            //}

            var dataset_csv_files_headers_fix = dataset_csv_files_headers.AsParallel().AsOrdered().Select((a, i) => i % 7 == 0 ? dataset_csv_files_headers.Skip(i).Take(7).ToArray() : null).Where(a => a != null).ToArray();



            program.WriteLine($@"{nameof(read_binary_dataset)}: Parsing headers...");
            var dataset_headers = dataset_csv_files_headers_fix.AsParallel().AsOrdered().Select((b, i) =>
            {
                //var b = a.ToArray();//.Split('.').Select(c=>c.Trim()).ToArray();

                //$"{a.alphabet},{a.dimension},{a.category},{a.source},{a.@group},{a.member},{a.perspective}"


                var fid = i;

                var default_str = "default";

                var alphabet = b.Length >= 1 && !string.IsNullOrWhiteSpace(b[0]) ? b[0] : default_str;
                var dimension = b.Length >= 2 && !string.IsNullOrWhiteSpace(b[1]) ? b[1] : default_str;
                var category = b.Length >= 3 && !string.IsNullOrWhiteSpace(b[2]) ? b[2] : default_str;
                var source = b.Length >= 4 && !string.IsNullOrWhiteSpace(b[3]) ? b[3] : default_str;
                var group = b.Length >= 5 && !string.IsNullOrWhiteSpace(b[4]) ? b[4] : default_str;
                var member = b.Length >= 6 && !string.IsNullOrWhiteSpace(b[5]) ? b[5] : default_str;
                var perspective = b.Length >= 7 && !string.IsNullOrWhiteSpace(b[6]) ? b[6] : default_str;


                lock (lock_table)
                {
                    if (!table_alphabet.Contains(alphabet)) table_alphabet.Add(alphabet);
                    if (!table_dimension.Contains(dimension)) table_dimension.Add(dimension);
                    if (!table_category.Contains(category)) table_category.Add(category);
                    if (!table_source.Contains(source)) table_source.Add(source);
                    if (!table_group.Contains(group)) table_group.Add(group);
                    if (!table_member.Contains(member)) table_member.Add(member);
                    if (!table_perspective.Contains(perspective)) table_perspective.Add(perspective);
                }

                var alphabet_id = table_alphabet.LastIndexOf(alphabet);
                var dimension_id = table_dimension.LastIndexOf(dimension);
                var category_id = table_category.LastIndexOf(category);
                var source_id = table_source.LastIndexOf(source);
                var group_id = table_group.LastIndexOf(group);
                var member_id = table_member.LastIndexOf(member);
                var perspective_id = table_perspective.LastIndexOf(perspective);


                // set strings to immutable tabled copy to save memory
                alphabet = table_alphabet[alphabet_id];
                dimension = table_dimension[dimension_id];
                category = table_category[category_id];
                source = table_source[source_id];
                group = table_group[group_id];
                member = table_member[member_id];
                perspective = table_perspective[perspective_id];


                return (
                    fid: fid,

                    alphabet: alphabet,
                    dimension: dimension,
                    category: category,
                    source: source,
                    group: group,
                    member: member,
                    perspective: perspective,

                    alphabet_id: alphabet_id,
                    dimension_id: dimension_id,
                    category_id: category_id,
                    source_id: source_id,
                    group_id: group_id,
                    member_id: member_id,
                    perspective_id: perspective_id
                    );
            }).ToList();

            var total_features = dataset_headers.Count; // 

            if (total_features != dataset_headers.Count)
            {
                throw new Exception();
            }

            program.WriteLine($@"{nameof(read_binary_dataset)}: {nameof(total_features)}: {total_features}");
            program.WriteLine($@"{nameof(read_binary_dataset)}: {nameof(table_alphabet)}: {table_alphabet.Count}: {string.Join(", ", table_alphabet)}");
            program.WriteLine($@"{nameof(read_binary_dataset)}: {nameof(table_dimension)}: {table_dimension.Count}: {string.Join(", ", table_dimension)}");
            program.WriteLine($@"{nameof(read_binary_dataset)}: {nameof(table_category)}: {table_category.Count}: {string.Join(", ", table_category)}");
            program.WriteLine($@"{nameof(read_binary_dataset)}: {nameof(table_source)}: {table_source.Count}: {string.Join(", ", table_source)}");
            program.WriteLine($@"{nameof(read_binary_dataset)}: {nameof(table_group)}: {table_group.Count}: {string.Join(", ", table_group)}");
            program.WriteLine($@"{nameof(read_binary_dataset)}: {nameof(table_member)}: {table_member.Count}: {string.Join(", ", table_member)}");
            program.WriteLine($@"{nameof(read_binary_dataset)}: {nameof(table_perspective)}: {table_perspective.Count}: {string.Join(", ", table_perspective)}");
            program.WriteLine($@"");
            program.WriteLine($@"{nameof(read_binary_dataset)}: Grouping headers...");

            //List<(int fid, string alphabet, string dimension, string category, string source, string group, string member, string perspective, int alphabet_id, int dimension_id, int category_id, int source_id, int group_id, int member_id, int perspective_id)> ds_h = dataset_headers;



            if (output_feature_tree)
            {
                feature_tree.group_dataset_headers_all_levels(dataset_headers);
                //feature_tree.output_feature_tree(dataset_headers_grouped,true,true,false,false);
                //Console.ReadLine();
            }

            // header alredy removed; index 0 = class id; index 1..n = feature values; libsvm features start at index 1;
            program.WriteLine($@"{nameof(read_binary_dataset)}: Reading data...");

            List<(int filename_index, int line_index, List<(string comment_header, string comment_value)> comment_columns)> dataset_comment_row_values = null;
            List<(List<(string comment_header, string comment_value)> comment_columns, List<(int fid, double fv)> feature_data)> dataset_instance_list = null;

            //List<(int filename_index, int line_index, List<(string comment_header, string comment_value)> comment_columns)> novel_dataset_comment_row_values = null;
            //List<(List<(string comment_header, string comment_value)> comment_columns, List<(int fid, double fv)> feature_data)> novel_dataset_instance_list = null;

            if (test_mode)
            {
                var row_limit = 20;

                // load comment lines as key-value pairs.  note: these are variables associated with each example instance rather than specific features.
                dataset_comment_row_values = dataset_comment_csv_files.AsParallel().AsOrdered().SelectMany((filename, filename_index) => File.ReadAllLines(filename).Skip(1).Take(row_limit).Select((line, line_index) => (filename_index: filename_index, line_index: line_index, comment_columns: line.Split(',').Select((col, col_index) => (comment_header: dataset_comment_csv_files_headers[col_index], comment_value: col)).ToList())).ToList()).ToList();

                // filter out any '#' commented out key-value pairs 
                dataset_comment_row_values = dataset_comment_row_values.AsParallel().AsOrdered().Select(a => { a.comment_columns = a.comment_columns.Where(b => b.comment_header[0] != '#').ToList(); return a; }).ToList();

                // add header name?!
                dataset_instance_list = dataset_csv_files.AsParallel().AsOrdered().SelectMany((filename, filename_index) => File.ReadAllLines(filename).Skip(1).Take(row_limit).Select((line, line_index) => (comment_columns: dataset_comment_row_values.First(b => b.filename_index == filename_index && b.line_index == line_index).comment_columns, feature_data: line.Split(',').Select((column_value, fid) => (fid, fv: double_compat.fix_double(column_value))).ToList())).ToList()).ToList();
                //novel_dataset_comment_row_values = novel_dataset_comment_csv_files.SelectMany((filename, filename_index) => File.ReadAllLines(filename).Skip(1).Take(row_limit).Select((line, line_index) => (filename_index: filename_index, line_index: line_index, comment_columns: line.Split(',').Select((col, col_index) => (comment_header: novel_dataset_comment_csv_files_headers[col_index], comment_value: col)).ToList())).ToList()).ToList();
                //novel_dataset_instance_list = novel_dataset_csv_files.SelectMany((filename, filename_index) => File.ReadAllLines(filename).Skip(1).Take(row_limit).Select((line, line_index) => (comment_columns: novel_dataset_comment_row_values.First(b => b.filename_index == filename_index && b.line_index == line_index).comment_columns, feature_data: line.Split(',').Select((column_value, fid) => (fid, fv: double_compat.fix_double(column_value))).ToList())).ToList()).ToList();
            }
            else
            {
                // load comment lines as key-value pairs.  note: these are variables associated with each example instance rather than specific features.
                dataset_comment_row_values = dataset_comment_csv_files.AsParallel().AsOrdered().SelectMany((filename, filename_index) => File.ReadAllLines(filename).Skip(1).Select((line, line_index) =>

                    (filename_index: filename_index, line_index: line_index, comment_columns: line.Split(',').Select((col, col_index) =>
                    (comment_header: dataset_comment_csv_files_headers[col_index], comment_value: col)).ToList())).ToList()).ToList();

                // filter out any '#' commented out key-value pairs 
                dataset_comment_row_values = dataset_comment_row_values.AsParallel().AsOrdered().Select(a => { a.comment_columns = a.comment_columns.Where(b => b.comment_header[0] != '#').ToList(); return a; }).ToList();

                dataset_instance_list = dataset_csv_files.AsParallel().AsOrdered().SelectMany((filename, filename_index) => File.ReadAllLines(filename).Skip(1).Select((line, line_index) =>

                    (
                        comment_columns: dataset_comment_row_values.First(b => b.filename_index == filename_index && b.line_index == line_index).comment_columns,

                        feature_data: line.Split(',').Select((column_value, fid) => (fid, fv: double_compat.fix_double(column_value))).ToList()
                    )

                ).ToList()).ToList();


                //novel_dataset_comment_row_values = novel_dataset_comment_csv_files.SelectMany((filename, filename_index) => File.ReadAllLines(filename).Skip(1).Select((line, line_index) => (filename_index: filename_index, line_index: line_index, comment_columns: line.Split(',').Select((col, col_index) => (comment_header: novel_dataset_comment_csv_files_headers[col_index], comment_value: col)).ToList())).ToList()).ToList();
                //novel_dataset_instance_list = novel_dataset_csv_files.SelectMany((filename, filename_index) => File.ReadAllLines(filename).Skip(1).Select((line, line_index) => (comment_columns: novel_dataset_comment_row_values.First(b => b.filename_index == filename_index && b.line_index == line_index).comment_columns, feature_data: line.Split(',').Select((column_value, fid) => (fid, fv: double_compat.fix_double(column_value))).ToList())).ToList()).ToList();
            }

            if (dataset_comment_row_values.Count != dataset_instance_list.Count)
            {
                throw new Exception();
            }

            return (dataset_headers, /*dataset_headers_grouped,*/ dataset_comment_row_values, dataset_instance_list);
        }

        //public static List<(string source, string g)> group_dataset_headers_by_group()
        //{
        //
        //}




        //public static List<(string cluster_name, string dimension, string alphabet, string source_name, string group_name, int num_features)> read_clusters()
        //{
        //    var file_data = File.ReadAllLines(Path.Combine($@"{program.root_folder}", $@"", $@"cluster_group_list2.csv"));
        //    var header = File.ReadAllLines(Path.Combine($@"{program.root_folder}", $@"", $@"cluster_group_list2.csv")).First();
        //    var rows = file_data.Skip(1).ToList();
        //
        //    var feature_group_clusters = rows.Where(a => a.Replace(",", "").Trim().Length > 0).Select(a =>
        //    {
        //        var b = a.Split(',');
        //        var i = 0;
        //
        //        return (cluster_name: b[i++], dimension: b[i++], alphabet: b[i++], source_name: b[i++], group_name: b[i++], num_features: int.Parse(b[i++]));
        //
        //    }).OrderBy(a => a.dimension).ThenBy(a => a.cluster_name).ThenBy(a => a.num_features).ToList();
        //
        //    return feature_group_clusters;
        //}

        public static void master_compute(CancellationToken token)
        {
            //var cached = Directory.GetFiles(@"c:\svm_compute\cache\", "*.txt");
            //
            //foreach (var c in cached)
            //{
            //    Console.WriteLine(c);
            //
            //    var d = File.ReadAllText(c);
            //    Console.WriteLine(d);
            //    Console.WriteLine();
            //
            //    //var x = cross_validation.run_svm_params.deserialise(d);
            //    var y = cross_validation.run_svm_return.deserialise(d);
            //    y.run_svm_return_data.ForEach(a=>a.confusion_matrices.ForEach(b=>Console.WriteLine(b.ToString() + "\r\n\r\n")));
            //    //x.print();
            //
            //    Console.WriteLine();
            //    Console.WriteLine();
            //}

            //Console.ReadLine();
            //return;

            // order by least/most correlation
            // group average correlation


            // feature selection:
            //
            // 1 task per feature group(e.g. 100 tasks) (task is either forwards or backwards)
            // 1 iteration per feature group selection
            // 1 final set of features
            //
            //
            // 1 first ranking
            // 1 last ranking
            // 1 average ranking
            //
            //
            // each task has a cross - validation / svm result
            //
            //
            // save cv cm results of each iteration to a file?


            // read clusters
            // var clusters = read_clusters();

            // A: Rank all groups.  To rank instead of select, set parameter iteration_select_max = 1.
            // B: Feature selection.
            //$"{a.alphabet},{a.dimension},{a.category},{a.source},{a.@group},{a.member},{a.perspective}"

            //List<((string source, string group) set_name, List<(int fid, string alphabet, string dimension, string category, string source, string group, string member, string perspective, int alphabet_id, int dimension_id, int category_id, int source_id, int group_id, int member_id, int perspective_id)> set_members)>
            //var features_input = dataset_headers.GroupBy(a => (a.source_id, a.group_id)).Select(a => (set_name: (a.FirstOrDefault().source, a.FirstOrDefault().group), set_members: a.ToList())).ToList();


            //dataset_headers = dataset_headers.Where(a=>a.)

            //.AsParallel().AsOrdered()
            // todo: make sure still balanced, not imbalanced training...

            var program_stopwatch = new Stopwatch();
            program_stopwatch.Start();

            var output_feature_tree = true;
            var output_empty_features = false;
            //var output_threshold_adjustment_performance = false; // note: be careful about when 'output_threshold_adjustment_performance' is true/false (should be false for feature selection)

            //var file_dt = DateTime.Now.ToString($"yyMMdd_HHmmss_fff");
            //var save_filename_cluster_performance = Path.Combine($@"{program.root_folder}", $@"output\", $@"svm_cluster_perf_{file_dt}.csv");
            //var save_filename_performance = Path.Combine($@"{program.root_folder}", $@"output\", $@"svm_performance_{file_dt}.csv");
            //var save_filename_predictions = Path.Combine($@"{program.root_folder}", $@"output\", $@"svm_predictions_{file_dt}.csv");


            program.WriteLine($@"{nameof(Main)}: {nameof(output_feature_tree)}: {string.Join(", ", output_feature_tree)}");

            //var svm_params = new cross_validation.run_svm_params();
            //svm_params.set_defaults();
            //svm_params.randomisation_cv_folds = 1;
            //svm_params.outer_cv_folds = 5;
            //svm_params.inner_cv_folds = 5;
            //svm_params.output_threshold_adjustment_performance = output_threshold_adjustment_performance;


            //if (test_mode)
            //{
            //    svm_params.randomisation_cv_folds = 1;
            //    svm_params.outer_cv_folds = 1;
            //    svm_params.inner_cv_folds = 2;
            //    //svm_params.max_tasks = 1;
            //}

            // setup class labels and class names
            var negative_class_id = -1;
            var positive_class_id = +1;
            var class_names = new List<(int class_id, string class_name)>() { (negative_class_id, "standard_coil"), (positive_class_id, "dimorphic_coil") };


            // read dataset
            var (dataset_headers, dataset_comment_row_values, dataset_instance_list) = read_binary_dataset(false, negative_class_id, positive_class_id, class_names);

            // filter dataset to desired groups/features
            //dataset_headers = dataset_headers.Where(a => a.fid==0 || string.Equals(a.alphabet, "Normal", StringComparison.InvariantCultureIgnoreCase)).ToList();

            var filter_overall_and_normal = false;
            var filter_uniprotkb = false;

            if (filter_overall_and_normal)
            {
                dataset_headers = dataset_headers.Where(a =>
                    a.fid == 0 || string.IsNullOrWhiteSpace(a.alphabet) ||
                    string.Equals(a.alphabet, "Normal", StringComparison.InvariantCultureIgnoreCase) ||
                    string.Equals(a.alphabet, "Overall", StringComparison.InvariantCultureIgnoreCase)).ToList();
            }

            if (filter_uniprotkb)
            {
                dataset_headers = dataset_headers.Where(a => a.fid == 0 || string.Equals(a.alphabet, "UniProtKB", StringComparison.InvariantCultureIgnoreCase)).ToList();
            }

            var dataset_headers_fids = dataset_headers.AsParallel().AsOrdered().Select(a => a.fid).ToList();

            dataset_instance_list = dataset_instance_list.AsParallel().AsOrdered().Select(a => (a.comment_columns, a.feature_data.Where(b => dataset_headers_fids.Contains(b.fid)).ToList())).ToList();

            // output feature tree
            if (output_feature_tree)
            {
                var sw1 = new Stopwatch();
                sw1.Start();
                feature_tree.group_dataset_headers_all_levels(dataset_headers);
                sw1.Stop();
                program.WriteLine("master_compute(): feature_tree.group_dataset_headers_all_levels(dataset_headers): " + sw1.Elapsed.ToString());
            }

            // check dataset format
            check_class_id_feature(dataset_headers);
            check_rows_have_equal_features(dataset_instance_list);
            find_empty_features(output_empty_features, dataset_headers, dataset_instance_list);


            // group headers 
            var dataset_headers_grouped = dataset_headers.Skip(1).AsParallel().AsOrdered().GroupBy(a => (a.alphabet, a.category, a.dimension, a.source_id, a.group_id)).ToList();

            var features_input = dataset_headers_grouped.AsParallel().AsOrdered().Select((a, i) => new feature_selection_unidirectional.feature_set()
            {
                source = a.FirstOrDefault().source,
                dimension = a.FirstOrDefault().dimension,
                category = a.FirstOrDefault().category,
                alphabet = a.FirstOrDefault().alphabet,
                @group = a.FirstOrDefault().group,

                set_id = i,

                set_members = a.Select(b => new feature_selection_unidirectional.feature_set_member()
                {
                    fid = b.fid,
                    set_id = i,
                    alphabet = b.alphabet,
                    dimension = b.dimension,
                    category = b.category,
                    source = b.source,
                    @group = b.@group,
                    member = b.member,
                    perspective = b.perspective,

                    alphabet_id = b.alphabet_id,
                    dimension_id = b.dimension_id,
                    category_id = b.category_id,
                    source_id = b.source_id,
                    group_id = b.group_id,
                    member_id = b.member_id,
                    perspective_id = b.perspective_id,
                }).ToList()
            }).ToList();


            // 1. convert dataset to feature_set_id format
            //var features_input = dataset_headers.GroupBy(a => (a.source_id, a.group_id)).Select(a => new feature_selection_unidirectional.feature_set_id()
            //{
            //    set_name = $"{a.FirstOrDefault().source}.{a.FirstOrDefault().@group}",
            //    set_id = a.Key.group_id,
            //    set_length = a.Count(),
            //    members = a.Select((b, j) => new feature_selection_unidirectional.feature_set_member()
            //    {
            //        external_feature_id = b.fid,
            //        internal_member_id = j,
            //        member_name = $"{b.member}.{b.perspective}",
            //        set = null
            //    }).ToList()
            //}).ToList();

            // 2. run feature selection
            var rsp = new cross_validation.run_svm_params();
            rsp.set_defaults();

            rsp.run_remote = true;
            rsp.outer_cv_folds = 5;
            rsp.outer_cv_folds_to_skip = rsp.outer_cv_folds - 1; // previous value: 0
            rsp.randomisation_cv_folds = rsp.outer_cv_folds; // previous value: 1
            rsp.inner_cv_folds = 5;
            rsp.class_names = class_names;
            rsp.kernels = new List<libsvm_caller.libsvm_kernel_type>() { libsvm_caller.libsvm_kernel_type.linear };
            //rsp.output_threshold_adjustment_performance = output_threshold_adjustment_performance;

            // Alternative unused open ports: 3121 or 4155

            // Next: best_score

            var results_output_folder = $@"c:\svm_compute\feature_selection\{program.program_start_time}\";
            Directory.CreateDirectory(results_output_folder);

            var fsu = new feature_selection_unidirectional(
                results_output_folder: results_output_folder,
                run_svm_params: rsp,
                dataset_instance_list: dataset_instance_list,
                features_input: features_input,
                base_features: null,
                feature_selection_combinator: feature_selection_unidirectional.feature_selection_combinators.feature_sets,
                feature_selection_type: feature_selection_unidirectional.feature_selection_types.forwards,
                perf_selection_rule: feature_selection_unidirectional.perf_selection_rules.best_ppf_overall,//feature_selection_unidirectional.perf_selection_rules.best_ppf_change,
                feature_selection_performance_metrics: performance_measure.confusion_matrix.cross_validation_metrics.F1S,//.Youden,// | performance_measure.confusion_matrix.cross_validation_metrics.GM | performance_measure.confusion_matrix.cross_validation_metrics.Kappa,
                feature_selection_performance_classes: new List<int>() { 1 },
                iteration_select_max: -1,
                backwards_max_features_to_combine_per_iteration: 1,
                forwards_max_features_to_combine_per_iteration: 1,
                backwards_max_features_to_remove_per_iteration: 1,
                forwards_max_features_to_add_per_iteration: 1,
                backwards_min_features: 0,
                forwards_max_features: 0,
                backwards_max_feature_removal_attempts: 0,
                forwards_max_features_insertion_attempts: 0,
                random_baseline: 0.01d,
                margin_of_error: 0.00001d,
                max_tasks: -10);

            var (selected_features, performance) = fsu.run_unidirectional_feature_selection();

            // 3. make classify function call run_svm function
            // 4. save svm prediction/performance results to file
            // 5. save feature selection history and rankings to file
            // 6. save each iteration to enable restart if crash/pause


            program_stopwatch.Stop();
            program.WriteLine($@"{nameof(Main)}: Finished: " + program_stopwatch.Elapsed);
            for (var i = 0; i < 4; i++) Console.ReadLine();
        }

        public static void check_rows_have_equal_features(List<(List<(string comment_header, string comment_value)> comment_columns, List<(int fid, double fv)> feature_data)> dataset_instance_list)
        {
            if (dataset_instance_list.Select(a => a.feature_data.Count).Distinct().Count() != 1)
            {
                throw new Exception("number of features do not match on separate lines!");
            }
        }

        public static void find_empty_features(bool output_empty_features, List<(int fid, string alphabet, string dimension, string category, string source, string group, string member, string perspective, int alphabet_id, int dimension_id, int category_id, int source_id, int group_id, int member_id, int perspective_id)> dataset_headers, List<(List<(string comment_header, string comment_value)> comment_columns, List<(int fid, double fv)> feature_data)> dataset_instance_list)
        {
            // find always zero columns
            program.WriteLine($@"{nameof(Main)}: Finding empty features...");

            var total_columns = dataset_instance_list.Max(a => a.feature_data.Count);
            var dataset_columns_distinct_value_count = new int[total_columns];

            for (var i = 0; i < total_columns; i++)
            {
                dataset_columns_distinct_value_count[i] = dataset_instance_list.Select(a => a.feature_data[i]).Distinct().Count();
            }

            var empty_features = new List<(string header, int fid, double[] fv)>();

            for (var i = total_columns - 1; i >= 0; i--)
            {

                if (dataset_columns_distinct_value_count[i] <= 1)
                {
                    empty_features.Add((

                        header: $"{dataset_headers[i].source}.{dataset_headers[i].group}.{dataset_headers[i].member}.{dataset_headers[i].perspective}",

                        fid: dataset_headers[i].fid,
                        fv: dataset_instance_list.Select(b => b.feature_data[i].fv).ToArray()));
                }
            }

            program.WriteLine($@"{nameof(Main)}: Features which are empty: {empty_features.Count}: {(!output_empty_features ? "" : string.Join(", ", empty_features.Select(a => $"{a.fid}={string.Join("|", a.fv)} ({a.header})").ToList()))}.");

            program.WriteLine($@"{nameof(Main)}: Removing empty features...");
            for (var i = total_columns - 1; i >= 0; i--)
            {
                if (dataset_columns_distinct_value_count[i] <= 1)
                {
                    //dataset_csv_files_headers.RemoveAt(i); //uncommment if dataset_csv_files_headers is ever required
                    dataset_headers.RemoveAt(i);
                    dataset_instance_list.ForEach(a => a.feature_data.RemoveAt(i));
                }
            }
        }


        public static void check_class_id_feature(List<(int fid, string alphabet, string dimension, string category, string source, string group, string member, string perspective, int alphabet_id, int dimension_id, int category_id, int source_id, int group_id, int member_id, int perspective_id)> dataset_headers)
        {
            var is_class_id_first_features = dataset_headers.FirstOrDefault().group == "class_id";

            if (!is_class_id_first_features)
            {
                throw new Exception("class is is not the first feature");
            }


            var class_id_feature_index_count = dataset_headers.Count(b => b.group == "class_id");

            if (class_id_feature_index_count > 1)
            {
                throw new Exception("class id duplicates");
            }
            else if (class_id_feature_index_count == 0)
            {
                throw new Exception("class id not found");
            }

        }
    }
}


