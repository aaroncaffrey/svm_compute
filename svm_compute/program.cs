using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Accord.Math;
using svm_compute;

namespace svm_compute
{
    public class program
    {
        //public const string root_folder = program.convert_path(@"c:\betastrands_dataset\");
        //public static bool test_mode = false;

        public static readonly bool write_console_log = false;

        public static readonly object console_lock = new object();
        public static readonly object console_log_lock = new object();

        public static int log_line_number = 0;
        public static int log_suffix = 0;
        public static bool log_console_to_file = false;

        public static libsvm_caller.svm_implementation inner_cv_svm_implementation = libsvm_caller.svm_implementation.libsvm_eval;
        public static libsvm_caller.svm_implementation outer_cv_svm_implementation = libsvm_caller.svm_implementation.libsvm;

        public static bool svm_cache_save = true;
        public static bool svm_cache_load = true;

        public static string program_start_time;

        public static TimeSpan point_max_time = new TimeSpan(0, 0, 0, 45);
        public static TimeSpan process_max_time = new TimeSpan(0, 0, 3, 0);

        public static TimeSpan tcp_connection_timeout = new TimeSpan(0, 0, 0, 15);
        public static TimeSpan tcp_stream_read_timeout = new TimeSpan(0, 2, 0);
        public static TimeSpan tcp_stream_write_timeout = new TimeSpan(0, 2, 0);

        public static TimeSpan unauthed_data_received_timeout = new TimeSpan(0, 1, 0);
        public static TimeSpan unauthed_message_received_timeout = new TimeSpan(0, 1, 0);

        public static TimeSpan authed_data_received_timeout = new TimeSpan(0, 3, 0);
        public static TimeSpan authed_message_received_timeout = new TimeSpan(2, 0, 0);


        public static object write_lock = new object();


        public static object append_bytes_lock = new object();

        public static char ram_drive = find_ram_disk()?.First() ?? '\0';


        public static bool inner_cv_probability_estimates = false;
        public static bool outer_cv_probability_estimates = false;

        public static bool inner_cv_shrinking_heuristics = true;
        public static bool outer_cv_shrinking_heuristics = true;

        public static void AppendAllBytes(string path, byte[] bytes, bool temp_file = false)
        {
            if (string.IsNullOrWhiteSpace(path) || bytes == null || bytes.Length == 0)
            {
                return;
            }

            var success = false;

            path = convert_path(path, temp_file);

            try
            {
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            do
            {
                try
                {
                    lock (append_bytes_lock)
                    {
                        using (var stream = new FileStream(path, FileMode.Append))
                        {
                            stream.Write(bytes, 0, bytes.Length);

                            stream.Flush();
                        }
                    }

                    success = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Task.Delay(new TimeSpan(0, 0, 10)).Wait();
                }
            } while (!success);
        }

        public static void AppendAllLines(string path, IEnumerable<string> contents, bool temp_file = false)
        {
            var success = false;

            path = convert_path(path, temp_file);

            try
            {
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            do
            {

                try
                {
                    lock (write_lock)
                    {
                        File.AppendAllLines(path, contents);
                        success = true;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Task.Delay(new TimeSpan(0, 0, 10)).Wait();
                }


            } while (!success);
        }

        public static void WriteAllLines(string path, IEnumerable<string> contents, bool temp_file = false)
        {
            var success = false;

            path = convert_path(path, temp_file);

            try
            {
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            do
            {

                try
                {
                    lock (write_lock)
                    {
                        File.WriteAllLines(path, contents);
                        success = true;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Task.Delay(new TimeSpan(0, 0, 10)).Wait();
                }


            } while (!success);
        }

        public static void WriteAllText(string path, string contents, bool temp_file = false)
        {
            var success = false;

            path = convert_path(path, temp_file);

            try
            {
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            do
            {

                try
                {
                    lock (write_lock)
                    {
                        File.WriteAllText(path, contents);
                        success = true;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Task.Delay(new TimeSpan(0, 0, 10)).Wait();
                }


            } while (!success);
        }


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
                    program.WriteLineException(e, nameof(GC_Collection),"", true, ConsoleColor.DarkGray);
                    throw;
                }
            }
        }

        //public static bool console_log_enabled = true;

        public static void WriteLineException(Exception e, string method = "", string msg = "", bool timestamp = true, ConsoleColor foreground_cc = ConsoleColor.White, ConsoleColor background_cc = ConsoleColor.Black)
        {
            var max_levels = 10;
            var i = 0;

            do
            {
                program.WriteLine($@"{e.GetType()}: #{i} ""{method}"" ""{msg}"" ""{e.Source}"" ""{e.TargetSite}"" ""{e.Message}"" --> ""{e.StackTrace}""", timestamp, foreground_cc, background_cc);

                e = e.InnerException;

                i++;

            } while (i < max_levels && e != null);
        }

        public static void WriteLine(string text, bool timestamp = true, ConsoleColor foreground_cc = ConsoleColor.White, ConsoleColor background_cc = ConsoleColor.Black)
        {
            //if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Enter)
            //{
            //    lock (console_lock)
            //    {
            //        console_log_enabled = !console_log_enabled;
            //    }
            //}

            //if (!console_log_enabled) return;


            var dt = timestamp ? $"[{DateTime.Now}] " : "";

            //var text2 = text;

            //if (Environment.OSVersion.Platform != PlatformID.Unix && Environment.OSVersion.Platform != PlatformID.MacOSX)
            //{
            //    if (dt.Length + text2.Length > Console.WindowWidth)
            //    {
            //        text2 = text2.Substring(0, (Console.WindowWidth - dt.Length) - 4) + "...";
            //    }
            //}

            //lock (console_lock)
            //{
            //    var reset = false;

            //    if (foreground_cc != ConsoleColor.White)
            //    {
            //        Console.ForegroundColor = foreground_cc;
            //        reset = true;
            //    }

            //    if (background_cc != ConsoleColor.Black)
            //    {
            //        Console.BackgroundColor = background_cc;
            //        reset = true;
            //    }

            //    Console.WriteLine($"{dt}{text2}");

            //    if (reset)
            //    {
            //        Console.ResetColor();
            //    }
            //}


            //Console.ForegroundColor = foreground_cc;
            //Console.BackgroundColor = background_cc;

            //Console.WriteLine($"{dt}{text2}");
            Console.WriteLine($"{dt}{text}");
            //Console.ResetColor();




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


                    program.AppendAllLines(program.convert_path($@"c:\svm_compute\console_log\{program_start_time}\{program_start_time}_{master_or_compute}_console_log_{log_suffix}.log"), new string[] { text });

                }
            }


        }

        //public static void maximise_window()
        //{
        //    if (Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Win32S || Environment.OSVersion.Platform == PlatformID.Win32Windows)
        //    {
        //        extensions.show_window_console(extensions.show_window_options.SW_MAXIMIZE);
        //    }
        //}


        public static string master_or_compute;

        public static string convert_path(string path, bool temp_file = false)
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                if (path.Length >= 2 && char.IsLetter(path[0]) && path[1] == ':')
                {
                    path = '~' + path.Substring(2);
                }
            }

            if (temp_file && char.IsLetter(ram_drive))
            {
                // on windows, copy to ramdisk if available

                if (Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Win32S || Environment.OSVersion.Platform == PlatformID.Win32Windows || Environment.OSVersion.Platform == PlatformID.WinCE)
                {
                    if (path.Length >= 2 && char.IsLetter(path[0]) && path[1] == ':')
                    {
                        path = ram_drive + path.Substring(1);
                    }
                }
            }

            //if (!path.StartsWith("//"))
            //{
                if (Path.DirectorySeparatorChar != '\\' && path.Contains('\\')) path = path.Replace('\\', Path.DirectorySeparatorChar);

                if (Path.DirectorySeparatorChar != '/' && path.Contains('/')) path = path.Replace('/', Path.DirectorySeparatorChar);
            //}

            //if (path.EndsWith(".exe")) path = path.Substring(0, path.Length - 4) + "";
            //if (path.EndsWith(".bat")) path = path.Substring(0, path.Length - 4) + ".sh";

            return path;
        }

        public static string find_ram_disk()
        {
            var ram_drives = DriveInfo.GetDrives().Where(a => a.VolumeLabel.ToLowerInvariant().Contains("RamDisk".ToLowerInvariant()) && (a.DriveType == DriveType.Fixed || a.DriveType == DriveType.Ram)).OrderByDescending(a => a.TotalSize).ToList();


            if (ram_drives != null && ram_drives.Count > 0)
            {
                return ram_drives.First().Name;
            }

            return null;
        }

        public static void Main(string[] args)
        {
            var a1 = new string[] { "Interface", "Neighbourhood", "Protein" };
            var c1 = new List<string>() { "2D", "3D", "2D_3D" };
            var f1 = new List<string>() { "Filter-based", "All features" };
            var p1 = new List<string>() {"Individual predictors", "Group predictors"};

            var g1 = new string[] { "Backwards", "Forwards" };
            var h1 = new string[] { "Hyper-parameter search", "Default hyper-parameters" };

            foreach (var a in a1)
            foreach (var c in c1)
            foreach (var f in f1)
            foreach (var p in p1)
            foreach (var g in g1)
            foreach (var h in h1)
                Console.WriteLine($"{a},{c},{f},{p},{g},{h}");
                    Console.ReadLine();
            return;
    #if DEBUG
            if (program.write_console_log) program.WriteLine($@"{nameof(Main)}(): Running in debug mode...", true, ConsoleColor.Red); //press enter to continue.
            //Console.ReadLine();
#endif
            Process this_process = Process.GetCurrentProcess();

            var priority_boost_enabled = false;
            var priority_class = ProcessPriorityClass.Normal;

            try { this_process.PriorityBoostEnabled = priority_boost_enabled; } catch (Exception) { }
            try { this_process.PriorityClass = priority_class; } catch (Exception) { }


            //program_start_time = DateTime.Now.ToString().Replace(":", "-").Replace("/", "-").Replace(" ", "_");
            program_start_time = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");

            //Directory.CreateDirectory(convert_path($@"c:\svm_compute\input_dataset\"));
            //Directory.CreateDirectory(convert_path($@"c:\svm_compute\hosts\"));
            //Directory.CreateDirectory(convert_path($@"c:\svm_compute\cache\request\"));
            //Directory.CreateDirectory(convert_path($@"c:\svm_compute\cache\response\"));
            //Directory.CreateDirectory(convert_path($@"c:\svm_compute\console_log\{program_start_time}\"));
            //Directory.CreateDirectory(convert_path($@"c:\svm_compute\tcp_log\{program_start_time}\"));
            //Directory.CreateDirectory(convert_path($@"c:\svm_compute\charts\{program_start_time}\"));
            //Directory.CreateDirectory(convert_path($@"c:\svm_compute\feature_selection\{program_start_time}\"));

            //maximise_window();
            libsvm_caller.wait_libsvm();

            var this_exe = Environment.GetCommandLineArgs()[0];


            var is_master_unit = args.Any(a => a == "/master");
            var is_compute_unit = args.Any(a => a == "/compute");

            if (is_compute_unit) is_master_unit = false;

            if (!is_master_unit && !is_compute_unit) is_master_unit = true;

            if (is_master_unit) master_or_compute = "master";
            if (is_compute_unit) master_or_compute = "compute";
            if (is_master_unit && is_compute_unit) master_or_compute = "master_and_compute";

            var slave_task_cancellation_source = new CancellationTokenSource();
            var master_task_cancellation_source = new CancellationTokenSource();

            if (is_compute_unit && is_master_unit) throw new Exception();
            if (!is_compute_unit && !is_master_unit) throw new Exception();

            if (is_compute_unit)
            {
                cross_validation_remote.compute_loop(slave_task_cancellation_source.Token);
            }

            if (is_master_unit)
            {
                Process.Start(this_exe, "/compute");

                load_hosts();

                master_compute(args, master_task_cancellation_source.Token);
            }

            if (program.write_console_log) program.WriteLine($@"{nameof(Main)}(): Program exiting...");
        }



        public static List<(string hostname, List<int> ports)> load_hosts()
        {
            var hosts_file = program.convert_path($@"c:\svm_compute\hosts\hosts.csv");

            try
            {
                if (File.Exists(hosts_file) && new FileInfo(hosts_file).Length > 0)
                {


                    var hosts_lines = File.ReadAllLines(hosts_file).Skip(1).ToList();
                    var hosts_data = hosts_lines.Where(a => !string.IsNullOrWhiteSpace(a)).Select(a =>
                    {
                        var x = a.Split(',').Select(b => b.Trim()).ToList();

                        if (x.Count < 3) return (hostname: null, ports: null, enabled: false);

                        var hostname = x[0];
                        var ports = x[1].Split(new char[] { ';', '-', '_', ',', '/', ' ', '\t' }).Select(p => int.Parse(p)).ToList();
                        var enabled = !(string.Equals(x[2], "FALSE", StringComparison.InvariantCultureIgnoreCase) || string.Equals(x[2], "F", StringComparison.InvariantCultureIgnoreCase) || (x[2].All(c => char.IsDigit(c)) && int.Parse(x[2]) == 0));

                        if (ports == null || ports.Count == 0)
                        {
                            ports = cross_validation_remote.default_server_ports.ToList();
                        }

                        return (hostname: hostname, ports: ports, enabled: enabled);

                    }).Where(a => !string.IsNullOrWhiteSpace(a.hostname) && a.ports != null && a.ports.All(p => p > UInt16.MinValue && p <= Int16.MaxValue) && a.enabled).ToList();



                    var result = hosts_data.Select(a => (a.hostname, a.ports)).ToList();

                    return result;

                }
                else
                {
                    if (program.write_console_log) program.WriteLine("Hosts file is missing.", true, ConsoleColor.Red);
                    return null;
                }
            }
            catch (Exception e)
            {
                program.WriteLineException(e, nameof(load_hosts),"", true, ConsoleColor.Red);
                return null;
            }
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

            //if (program.write_console_log) program.WriteLine($@"{nameof(Main)}: {nameof(search_matches)}: {string.Join(", ", search_matches)}");   

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


        public static
            (
            List<(int fid, string alphabet, string dimension, string category, string source, string group, string member, string perspective, int alphabet_id, int dimension_id, int category_id, int source_id, int group_id, int member_id, int perspective_id)> dataset_headers,
            List<(int filename_index, int line_index, List<(string comment_header, string comment_value)> comment_columns)> dataset_comment_row_values,
            List<(List<(string comment_header, string comment_value)> comment_columns, List<(int fid, double fv)> feature_data)> dataset_instance_list
            )
            read_binary_dataset(
            bool output_feature_tree,
            int negative_class_id,
            int positive_class_id,
            List<(int class_id, string class_name)> class_names
            )
        {

            var lock_table = new object();

            var table_alphabet = new List<string>();
            var table_dimension = new List<string>();
            var table_category = new List<string>();
            var table_source = new List<string>();
            var table_group = new List<string>();
            var table_member = new List<string>();
            var table_perspective = new List<string>();

            var dataset_csv_files = new List<string>()
            {
                program.convert_path(Path.Combine($@"c:\svm_compute\input_dataset\", $@"f__[{class_names.First(a => a.class_id == positive_class_id).class_name}].csv")),
                program.convert_path(Path.Combine($@"c:\svm_compute\input_dataset\", $@"f__[{class_names.First(a => a.class_id == negative_class_id).class_name}].csv")),
            };

            var dataset_header_csv_files = new List<string>()
            {
                program.convert_path(Path.Combine($@"c:\svm_compute\input_dataset\", $@"h__[{class_names.First(a => a.class_id == positive_class_id).class_name}].csv")),
                program.convert_path(Path.Combine($@"c:\svm_compute\input_dataset\", $@"h__[{class_names.First(a => a.class_id == negative_class_id).class_name}].csv")),
            };

            var dataset_comment_csv_files = new List<string>
            {
                program.convert_path(Path.Combine($@"c:\svm_compute\input_dataset\", $@"c__[{class_names.First(a => a.class_id == positive_class_id).class_name}].csv")),
                program.convert_path(Path.Combine($@"c:\svm_compute\input_dataset\", $@"c__[{class_names.First(a => a.class_id == negative_class_id).class_name}].csv")),
            };

            if (program.write_console_log) program.WriteLine($@"{nameof(read_binary_dataset)}.{nameof(output_feature_tree)} = {output_feature_tree}");
            if (program.write_console_log) program.WriteLine($@"{nameof(read_binary_dataset)}.{nameof(negative_class_id)} = {negative_class_id}");
            if (program.write_console_log) program.WriteLine($@"{nameof(read_binary_dataset)}.{nameof(positive_class_id)} = {positive_class_id}");
            if (program.write_console_log) program.WriteLine($@"{nameof(read_binary_dataset)}.{nameof(class_names)} = {string.Join(", ", class_names)}");

            if (program.write_console_log) program.WriteLine($@"{nameof(read_binary_dataset)}: {nameof(dataset_csv_files)}: {string.Join(", ", dataset_csv_files)}");
            if (program.write_console_log) program.WriteLine($@"{nameof(read_binary_dataset)}: {nameof(dataset_header_csv_files)}: {string.Join(", ", dataset_header_csv_files)}");
            if (program.write_console_log) program.WriteLine($@"{nameof(read_binary_dataset)}: {nameof(dataset_comment_csv_files)}: {string.Join(", ", dataset_comment_csv_files)}");

            if (program.write_console_log) program.WriteLine($@"{nameof(read_binary_dataset)}: Reading non-novel dataset headers...");

            // READ HEADER CSV FILE - ALL CLASSES HAVE THE SAME HEADERS/FEATURES

            List<(int fid, string alphabet, string dimension, string category, string source, string group, string member, string perspective, int alphabet_id, int dimension_id, int category_id, int source_id, int group_id, int member_id, int perspective_id)> header_data = File.ReadAllLines(dataset_header_csv_files.First()).Skip(1).Select((a, i) =>
            {
                var b = a.Split(',');
                var fid = int.Parse(b[0]);
                //if (fid!=i) throw new Exception();

                var alphabet = b[1];
                var dimension = b[2];
                var category = b[3];
                var source = b[4];
                var group = b[5];
                var member = b[6];
                var perspective = b[7];

                const string def = "default";

                if (string.IsNullOrWhiteSpace(alphabet)) alphabet = def;
                if (string.IsNullOrWhiteSpace(dimension)) dimension = def;
                if (string.IsNullOrWhiteSpace(category)) category = def;
                if (string.IsNullOrWhiteSpace(source)) source = def;
                if (string.IsNullOrWhiteSpace(group)) group = def;
                if (string.IsNullOrWhiteSpace(member)) member = def;
                if (string.IsNullOrWhiteSpace(perspective)) perspective = def;

                lock (lock_table)
                {
                    var duplicate = true;
                    if (!table_alphabet.Contains(alphabet)) { table_alphabet.Add(alphabet); duplicate = false; }
                    if (!table_dimension.Contains(dimension)) { table_dimension.Add(dimension); duplicate = false; }
                    if (!table_category.Contains(category)) { table_category.Add(category); duplicate = false; }
                    if (!table_source.Contains(source)) { table_source.Add(source); duplicate = false; }
                    if (!table_group.Contains(group)) { table_group.Add(group); duplicate = false; }
                    if (!table_member.Contains(member)) { table_member.Add(member); duplicate = false; }
                    if (!table_perspective.Contains(perspective)) { table_perspective.Add(perspective); duplicate = false; }

                    if (duplicate)
                    {
                        //Console.WriteLine("Duplicate: " + a);
                        //Console.ReadLine();
                    }
                }


                var alphabet_id = table_alphabet.LastIndexOf(alphabet);
                var dimension_id = table_dimension.LastIndexOf(dimension);
                var category_id = table_category.LastIndexOf(category);
                var source_id = table_source.LastIndexOf(source);
                var group_id = table_group.LastIndexOf(group);
                var member_id = table_member.LastIndexOf(member);
                var perspective_id = table_perspective.LastIndexOf(perspective);

                alphabet = table_alphabet[alphabet_id];
                dimension = table_dimension[dimension_id];
                category = table_category[category_id];
                source = table_source[source_id];
                group = table_group[group_id];
                member = table_member[member_id];
                perspective = table_perspective[perspective_id];

                return (fid: fid, alphabet: alphabet, dimension: dimension, category: category, source: source, group: group, member: member, perspective: perspective,
                                    alphabet_id: alphabet_id, dimension_id: dimension_id, category_id: category_id, source_id: source_id, group_id: group_id, member_id: member_id, perspective_id: perspective_id);
            }).ToList();

            // READ (DATA) COMMENTS CSV FILE - THESE ARE CLASS AND INSTANCE SPECIFIC
            var data_comments_header = File.ReadLines(dataset_comment_csv_files.First()).First().Split(',');

            var total_features = header_data.Count;
            if (program.write_console_log) program.WriteLine($@"{nameof(read_binary_dataset)}: {nameof(total_features)}: {total_features}");
            if (program.write_console_log) program.WriteLine($@"{nameof(read_binary_dataset)}: {nameof(table_alphabet)}: {table_alphabet.Count}: {string.Join(", ", table_alphabet)}");
            if (program.write_console_log) program.WriteLine($@"{nameof(read_binary_dataset)}: {nameof(table_dimension)}: {table_dimension.Count}: {string.Join(", ", table_dimension)}");
            if (program.write_console_log) program.WriteLine($@"{nameof(read_binary_dataset)}: {nameof(table_category)}: {table_category.Count}: {string.Join(", ", table_category)}");
            if (program.write_console_log) program.WriteLine($@"{nameof(read_binary_dataset)}: {nameof(table_source)}: {table_source.Count}: {string.Join(", ", table_source)}");
            if (program.write_console_log) program.WriteLine($@"{nameof(read_binary_dataset)}: {nameof(table_group)}: {table_group.Count}: {string.Join(", ", table_group)}");
            if (program.write_console_log) program.WriteLine($@"{nameof(read_binary_dataset)}: {nameof(table_member)}: {table_member.Count}: {string.Join(", ", table_member)}");
            if (program.write_console_log) program.WriteLine($@"{nameof(read_binary_dataset)}: {nameof(table_perspective)}: {table_perspective.Count}: {string.Join(", ", table_perspective)}");
            if (program.write_console_log) program.WriteLine($@"");

            List<(int filename_index, int line_index, List<(string comment_header, string comment_value)> comment_columns)> dataset_comment_row_values = null;
            List<(List<(string comment_header, string comment_value)> comment_columns, List<(int fid, double fv)> feature_data)> dataset_instance_list = null;

            // data comments: load comment lines as key-value pairs.  note: these are variables associated with each example instance rather than specific features.
            if (program.write_console_log) program.WriteLine($@"{nameof(read_binary_dataset)}: Reading data comments...");
            dataset_comment_row_values = dataset_comment_csv_files.AsParallel().AsOrdered().SelectMany((filename, filename_index) => File.ReadAllLines(filename).Skip(1/*header line*/)./*Take(20).*/Select((line, line_index) =>

                (filename_index: filename_index, line_index: line_index, comment_columns: line.Split(',').Select((col, col_index) =>
                (comment_header: data_comments_header[col_index], comment_value: col)).ToList())).ToList()).ToList();

            // data comments: filter out any '#' commented out key-value pairs 
            if (program.write_console_log) program.WriteLine($@"{nameof(read_binary_dataset)}: Removing data comments which are commented out...");
            dataset_comment_row_values = dataset_comment_row_values.AsParallel().AsOrdered().Select(a => { a.comment_columns = a.comment_columns.Where(b => b.comment_header.FirstOrDefault() != '#').ToList(); return a; }).ToList();

            // data set: load data
            if (program.write_console_log) program.WriteLine($@"{nameof(read_binary_dataset)}: Reading data...");
            dataset_instance_list = dataset_csv_files.AsParallel().AsOrdered().SelectMany((filename, filename_index) => File.ReadAllLines(filename).Skip(1/*skip header*/)./*Take(20).*/Select((line, line_index) =>
                (
                    comment_columns: dataset_comment_row_values.First(b => b.filename_index == filename_index && b.line_index == line_index).comment_columns,
                    feature_data: line.Split(',').Select((column_value, fid) => (fid, fv: double_compat.fix_double(column_value))).ToList()
                )
            ).ToList()).ToList();

            if (dataset_comment_row_values.Count != dataset_instance_list.Count)
            {
                throw new Exception();
            }

            if (program.write_console_log) program.WriteLine($@"{nameof(read_binary_dataset)}: Checking all dataset columns are the same length...");
            var dataset_num_diferent_column_length = dataset_instance_list.Select(a => a.feature_data.Count).Distinct().Count();
            if (dataset_num_diferent_column_length != 1) throw new Exception();

            if (program.write_console_log) program.WriteLine($@"{nameof(read_binary_dataset)}: Checking dataset headers and dataset columns are the same length...");
            var header_length = header_data.Count;
            var dataset_column_length = dataset_instance_list.First().feature_data.Count;
            if (dataset_column_length != header_length) throw new Exception();

            if (program.write_console_log) program.WriteLine($@"{nameof(read_binary_dataset)}: Checking all dataset comment columns are the same length...");
            var comments_num_different_column_length = dataset_instance_list.Select(a => a.comment_columns.Count).Distinct().Count();
            if (comments_num_different_column_length != 1) throw new Exception();




            return (header_data, dataset_comment_row_values, dataset_instance_list);
        }


        public static List<string> load_filter_based_fs2()
        {
            var folder = @"c:\users\aaron\desktop\filter_based_fs\";
            var files = Directory.GetFiles(folder, "*.csv").ToList();

            //e.g. "SS_HEC.mpsa_dipeptides.1.subsequence_1d.mpsa_unsplit_psipred-uniref90_split_dipeptides_0_SS_HEC_dist_normal_0.E_E.default"

            var headers = files.Select(a => File.ReadLines(a).First()).Select(a => a.Split(',').ToList()).ToList();

            var r = new List<string>();

            var max = headers.Max(a => a.Count);

            for (var i = 0; i < max; i++)
            {
                for (var j = 0; j < headers.Count; j++)
                {
                    if (headers[j].Count - 1 >= i)
                    {
                        r.Add(headers[j][i]);
                    }
                }
            }

            r = r.Distinct().ToList();


            return r;
        }

        public static List<(string alphabet, string category, int dimension, string source, string @group, string member, string perspective)> load_filter_based_fs()
        {
            var folder = @"c:\users\aaron\desktop\filter_based_fs\";
            var files = Directory.GetFiles(folder, "*.csv").ToList();

            //e.g. "SS_HEC.mpsa_dipeptides.1.subsequence_1d.mpsa_unsplit_psipred-uniref90_split_dipeptides_0_SS_HEC_dist_normal_0.E_E.default"

            var headers = files.Select(a => File.ReadLines(a).First()).Select(a =>
            {
                var x = a.Split(',').Select(b =>
                 {
                     var y = b.Split().First().Split('.');

                     return (alphabet: y[0], category: y[1], dimension: int.Parse(y[2]), source: y[3], group: y[4], member: y[5], perspective: y[6]);
                 }).ToList();


                return x;
            }).ToList();

            //var values = files.Select(a => File.ReadLines(a).Skip(1).First()).Select(a =>
            //{
            //    var x = a.Split(',').Select(b => b.Split().First()).ToList();
            //
            //    return x;
            //}).ToList();

            var r = new List<(string alphabet, string category, int dimension, string source, string @group, string member, string perspective)>();

            var max = headers.Max(a => a.Count);

            for (var i = 0; i < max; i++)
            {
                for (var j = 0; j < headers.Count; j++)
                {
                    if (headers[j].Count - 1 >= i)
                    {
                        r.Add(headers[j][i]);
                    }
                }
            }

            r = r.Distinct().ToList();

            //var r = headers.SelectMany(a => a).Distinct().ToList();

            return r;
        }

        public static (List<(int fid, string alphabet, string dimension, string category, string source, string @group, string member, string perspective, int alphabet_id, int dimension_id, int category_id, int source_id, int group_id, int member_id, int perspective_id)> dataset_headers,
            List<(List<(string comment_header, string comment_value)> comment_columns, List<(int fid, double fv)> feature_data)> dataset_instance_list) find_duplicate_groups(
            List<IGrouping<(int alphabet_id, int category_id, int dimension_id, int source_id, int group_id), (int fid, string alphabet, string dimension, string category, string source, string group, string member, string perspective, int alphabet_id, int dimension_id, int category_id, int source_id, int group_id, int member_id, int perspective_id)>> dataset_headers_grouped,
            List<(int fid, string alphabet, string dimension, string category, string source, string group, string member, string perspective, int alphabet_id, int dimension_id, int category_id, int source_id, int group_id, int member_id, int perspective_id)> dataset_headers,
            List<(List<(string comment_header, string comment_value)> comment_columns, List<(int fid, double fv)> feature_data)> dataset_instance_list)
        {

            if (program.write_console_log) program.WriteLine($@"{nameof(master_compute)}(): Checking for duplicate groups...");

            


            //var dataset_headers_grouped_by_size = dataset_headers_grouped.Select(a => a.ToList()).GroupBy(a => a.Count).ToList();
            //foreach (var g in dataset_headers_grouped_by_size)
            //{
            //    var groups_same_size = g.ToList();
            //    var groups_same_size_values = groups_same_size.Select(a => a.SelectMany(b => dataset_instance_list.Select(c => c.feature_data[b.fid].fv).ToList()).ToList()).ToList();

            var cluster_lock = new object();
            var tasks = new List<Task<List<List<int>>>>();

            var values = new List<List<double>>();
            for (var _i = 0; _i < dataset_headers_grouped.Count; _i++)
            {
                var group_a = dataset_headers_grouped[_i];

                var group_a_values = group_a.SelectMany(b => dataset_instance_list.Select(c => c.feature_data.Count > b.fid && c.feature_data[b.fid].fid == b.fid ? c.feature_data[b.fid].fv : c.feature_data.First(d => d.fid == b.fid).fv).ToList()).ToList();

                values.Add(group_a_values);
            }

            for (var _i = 0; _i < dataset_headers_grouped.Count; _i++)
            {
                var i = _i;

                var group_a = dataset_headers_grouped[i];

                var group_a_values = values[i];

                tasks.Add(Task.Run(() =>
                {
                    var needs_merging = new List<List<int>>();

                    for (var _j = 0; _j < dataset_headers_grouped.Count; _j++)
                    {
                        var j = _j;

                        if (j <= i) continue;

                        var group_b = dataset_headers_grouped[j];

                        if (group_a.Count() != group_b.Count()) continue;

                        var group_b_values = values[j];

                        if (group_a_values.SequenceEqual(group_b_values))
                        {
                            //lock (cluster_lock)
                            //{
                                var clusters = needs_merging.Where(a => a.Contains(i) || a.Contains(j)).ToList();

                                List<int> cluster = null;

                                if (clusters.Count == 0)
                                {
                                    cluster = new List<int>();
                                    needs_merging.Add(cluster);
                                }
                                else if (clusters.Count == 1)
                                {
                                    cluster = clusters.First();
                                }
                                else if (clusters.Count >= 2)
                                {
                                    cluster = clusters.SelectMany(a => a).Distinct().OrderBy(a=>a).ToList();
                                    needs_merging = clusters.Except(clusters).ToList();
                                    needs_merging.Add(cluster);
                                }

                                if (!cluster.Contains(i)) cluster.Add(i);
                                if (!cluster.Contains(j)) cluster.Add(j);

                                var x = cluster.ToList();
                                cluster.Clear();
                                cluster.AddRange(x.OrderBy(a => a).ToList());
                                //if (program.write_console_log) program.WriteLine($"Duplicate groups: {group_a.First().alphabet}.{group_a.First().category}.{group_a.First().dimension}.{group_a.First().source}.{group_a.First().group} and {group_b.First().alphabet}.{group_b.First().category}.{group_b.First().dimension}.{group_b.First().source}.{group_b.First().group}");
                            //}
                        }
                    }

                    return needs_merging;
                }));
            }
            Task.WaitAll(tasks.ToArray<Task>());

            var needs_merging1 = new List<List<int>>();
            var needs_merging2 = tasks.SelectMany(a => a.Result).ToList();

            for (var i = 0; i < needs_merging2.Count; i++)
            {
                var cluster = needs_merging2[i];

                var m = needs_merging1.Where(a => cluster.Any(b => a.Contains(b))).ToList();
                var m2 = m.SelectMany(a => a).ToList();
                m2.AddRange(cluster);
                m2 = m2.Distinct().OrderBy(a => a).ToList();

                needs_merging1 = needs_merging1.Except(m).ToList();
                needs_merging1.Add(m2);
            }

            needs_merging1 = needs_merging1.OrderBy(a => a.Count).ThenBy(a => a.Sum()).ToList();

            var new_columns = new List<List<(int fid, double fv)>>();
            var new_headers = new List<(int fid, string alphabet, string dimension, string category, string source, string @group, string member, string perspective, int alphabet_id, int dimension_id, int category_id, int source_id, int group_id, int member_id, int perspective_id)>();

            var fids_to_skip = new List<int>();

            var head_replace = new List<(int fid, string alphabet, string dimension, string category, string source, string @group, int alphabet_id, int dimension_id, int category_id, int source_id, int group_id)>();

            var last_alphabet_id = dataset_headers.Max(a => a.alphabet_id);
            var last_dimension_id = dataset_headers.Max(a => a.dimension_id);
            var last_category_id = dataset_headers.Max(a => a.category_id);
            var last_source_id = dataset_headers.Max(a => a.source_id);
            var last_group_id = dataset_headers.Max(a => a.group_id);
            var last_member_id = dataset_headers.Max(a => a.member_id);
            var last_perspective_id = dataset_headers.Max(a => a.perspective_id);

            for (var i = 0; i < needs_merging1.Count; i++)
            {
                var c = needs_merging1[i];

                var take_fids = dataset_headers_grouped[c.First()].Select(a => a.fid).ToList();

                var ignore_fids = c.SelectMany(b => dataset_headers_grouped[b].Select(a => a.fid).ToList()).Except(take_fids).ToList();
                fids_to_skip.AddRange(ignore_fids);

                //alphabet, category, dimension, source, group

                var alphabet = string.Join("|", c.SelectMany(a => dataset_headers_grouped[a].Select(b => b.alphabet).ToList()).Distinct().OrderBy(d => d).ToList());
                var category = string.Join("|", c.SelectMany(a => dataset_headers_grouped[a].Select(b => b.category).ToList()).Distinct().OrderBy(d => d).ToList());
                var dimension = string.Join("|", c.SelectMany(a => dataset_headers_grouped[a].Select(b => b.dimension).ToList()).Distinct().OrderBy(d => d).ToList());
                var source = string.Join("|", c.SelectMany(a => dataset_headers_grouped[a].Select(b => b.source).ToList()).Distinct().OrderBy(d => d).ToList());
                var group = string.Join("|", c.SelectMany(a => dataset_headers_grouped[a].Select(b => b.group).ToList()).Distinct().OrderBy(d => d).ToList());

                var alphabet_id =  alphabet == dataset_headers_grouped[c.First()].First().alphabet ? dataset_headers_grouped[c.First()].First().alphabet_id : ++last_alphabet_id;
                var category_id = category == dataset_headers_grouped[c.First()].First().category ? dataset_headers_grouped[c.First()].First().category_id : ++last_category_id;
                var dimension_id = dimension == dataset_headers_grouped[c.First()].First().dimension ? dataset_headers_grouped[c.First()].First().dimension_id : ++last_dimension_id;
                var source_id = source == dataset_headers_grouped[c.First()].First().source ? dataset_headers_grouped[c.First()].First().source_id : ++last_source_id;
                var group_id = group == dataset_headers_grouped[c.First()].First().group ? dataset_headers_grouped[c.First()].First().group_id : ++last_group_id;

                

                take_fids.ForEach(f => head_replace.Add((f, alphabet, dimension, category, source, group, alphabet_id, dimension_id, category_id, source_id, group_id)));
            }

            var new_fid = -1;

            for (var i = 0; i < dataset_headers.Count; i++)
            {

                var h = dataset_headers[i];

                
                if (fids_to_skip.Contains(h.fid)) continue;


                var r = head_replace.FindIndex(a => a.fid == h.fid);
                if (r > -1)
                {
                    var x = head_replace[r];
                    h = (h.fid, x.alphabet, x.dimension, x.category, x.source, x.group, h.member, h.perspective, x.alphabet_id, x.dimension_id, x.category_id, x.source_id, x.group_id, h.member_id, h.perspective_id);
                    
                }

                new_fid++;

                new_headers.Add((new_fid, h.alphabet, h.dimension, h.category, h.source, h.group, h.member, h.perspective, h.alphabet_id, h.dimension_id, h.category_id, h.source_id, h.group_id, h.member_id, h.perspective_id));


                // get column (feature)
                var y = dataset_instance_list.Select(a => a.feature_data.Count > h.fid && a.feature_data[h.fid].fid == h.fid ? (new_fid, a.feature_data[h.fid].fv) : (new_fid, a.feature_data.First(b => b.fid == h.fid).fv)).ToList();

                // add column (feature) to list
                new_columns.Add(y);
            }


            if (program.write_console_log) program.WriteLine($@"Finished merging group clusters: {needs_merging1.Count}");

            var new_f = dataset_instance_list.Select((row, row_index) => (row.comment_columns, new_columns.Select((col, col_index) => col[row_index]).ToList())).ToList();

            return (new_headers, new_f);

        }

        public static (List<(int fid, string alphabet, string dimension, string category, string source, string @group, string member, string perspective, int alphabet_id, int dimension_id, int category_id, int source_id, int group_id, int member_id, int perspective_id)> dataset_headers,
            List<(List<(string comment_header, string comment_value)> comment_columns, List<(int fid, double fv)> feature_data)> dataset_instance_list)
            find_duplicate_columns(
            List<(int fid, string alphabet, string dimension, string category, string source, string @group, string member, string perspective, int alphabet_id, int dimension_id, int category_id, int source_id, int group_id, int member_id, int perspective_id)> dataset_headers,
            List<(List<(string comment_header, string comment_value)> comment_columns, List<(int fid, double fv)> feature_data)> dataset_instance_list)
        {
            if (program.write_console_log) program.WriteLine("Finding duplicate columns.");

            var total = dataset_instance_list.First().feature_data.Count;

            var cols = new List<List<double>>();

            for (var col = 0; col < total; col++)
            {
                var col_values = dataset_instance_list.Select(a => a.feature_data[col].fv).ToList();
                cols.Add(col_values);
            }


            var tasks = new List<Task<List<List<int>>>>();
            
            var clusters_lock = new object();

            for (var _col = 0; _col < cols.Count; _col++)
            {
                var col = _col;

                tasks.Add(Task.Run(() =>
                {

                    var needs_merging = new List<List<int>>();

                    for (var _col2 = 0; _col2 < cols.Count; _col2++)
                    {
                        var col2 = _col2;

                        if (col2 <= col) continue;

                        var col_values1 = cols[col];
                        var col_values2 = cols[col2];

                        if (col_values1.SequenceEqual(col_values2))
                        {
                            //lock (clusters_lock)
                            //{
                                var clusters = needs_merging.Where(a => a.Contains(col) || a.Contains(col2)).ToList();

                                List<int> cluster = null;

                                if (clusters.Count == 0)
                                {
                                    cluster = new List<int>();
                                    needs_merging.Add(cluster);
                                }
                                else if (clusters.Count == 1)
                                {
                                    cluster = clusters.First();
                                }
                                else if (clusters.Count >= 2)
                                {
                                    cluster = clusters.SelectMany(a => a).Distinct().OrderBy(a=>a).ToList();
                                    needs_merging = clusters.Except(clusters).ToList();
                                    needs_merging.Add(cluster);
                                }

                                if (!cluster.Contains(col)) cluster.Add(col);
                                if (!cluster.Contains(col2)) cluster.Add(col2);

                                var x = cluster.ToList();
                                cluster.Clear();
                                cluster.AddRange(x.OrderBy(a => a).ToList());

                                //var name1 = dataset_headers[col];
                                //var name2 = dataset_headers[col2];

                                //Console.WriteLine($@"Duplicate column: ""{name1}"" #{col} and ""{name2}"" #{col2}");
                            //}
                        }
                    }

                    return needs_merging;
                }));
            }

            Task.WaitAll(tasks.ToArray<Task>());
            var needs_merging1 = new List<List<int>>();
            var needs_merging2 = tasks.SelectMany(a => a.Result).ToList();

            for (var i = 0; i < needs_merging2.Count; i++)
            {
                var cluster = needs_merging2[i];

                var m = needs_merging1.Where(a => cluster.Any(b => a.Contains(b))).ToList();
                var m2 = m.SelectMany(a => a).ToList();
                m2.AddRange(cluster);
                m2 = m2.Distinct().OrderBy(a => a).ToList();

                needs_merging1 = needs_merging1.Except(m).ToList();
                needs_merging1.Add(m2);
            }

            needs_merging1 = needs_merging1.OrderBy(a => a.Count).ThenBy(a => a.Sum()).ToList();

            var duplicates = needs_merging1.Sum(a => a.Count);
            if (program.write_console_log) program.WriteLine($@"Number of duplicate columns clusters: {needs_merging1.Count}");
            if (program.write_console_log) program.WriteLine($@"Number of duplicates: {duplicates} / {total} ({((double)duplicates / (double)total) * 100:0.00}%)");
            if (program.write_console_log) program.WriteLine("");


            if (program.write_console_log) program.WriteLine($@"Merging clusters: {needs_merging1.Count}");

            var last_alphabet_id = dataset_headers.Max(a => a.alphabet_id);
            var last_dimension_id = dataset_headers.Max(a => a.dimension_id);
            var last_category_id = dataset_headers.Max(a => a.category_id);
            var last_source_id = dataset_headers.Max(a => a.source_id);
            var last_group_id = dataset_headers.Max(a => a.group_id);
            var last_member_id = dataset_headers.Max(a => a.member_id);
            var last_perspective_id = dataset_headers.Max(a => a.perspective_id);

            var new_columns = new List<List<(int fid, double fv)>>();
            var new_headers = new List<(int fid, string alphabet, string dimension, string category, string source, string @group, string member, string perspective, int alphabet_id, int dimension_id, int category_id, int source_id, int group_id, int member_id, int perspective_id)>();

            var fid = -1;

            var merged = new List<int>();
            for (var col = 0; col < cols.Count; col++)
            {
                var cluster = needs_merging1.FirstOrDefault(a => a.Contains(col));

                if (cluster == null || cluster.Count == 0)
                {
                    fid++;

                    var h = dataset_headers[col];
                    h.fid = fid;

                    new_columns.Add(cols[col].Select(fv => (fid, fv)).ToList());
                    new_headers.Add(h);
                }
                else
                {
                    if (!merged.Any(a => cluster.Contains(a)))
                    {
                        fid++;

                        merged.AddRange(cluster);

                        var col2 = cluster.First();

                        new_columns.Add(cols[col2].Select(fv => (fid, fv)).ToList());

                        var headers = dataset_headers.Where((a, i) => cluster.Contains(i)).ToList();

                        var alphabet = string.Join("|", headers.Select(a => a.alphabet).Distinct().OrderBy(a => a).ToList());
                        var dimension = string.Join("|", headers.Select(a => a.dimension).Distinct().OrderBy(a => a).ToList());
                        var category = string.Join("|", headers.Select(a => a.category).Distinct().OrderBy(a => a).ToList());
                        var source = string.Join("|", headers.Select(a => a.source).Distinct().OrderBy(a => a).ToList());
                        var group = string.Join("|", headers.Select(a => a.@group).Distinct().OrderBy(a => a).ToList());
                        var member = string.Join("|", headers.Select(a => a.member).Distinct().OrderBy(a => a).ToList());
                        var perspective = string.Join("|", headers.Select(a => a.perspective).Distinct().OrderBy(a => a).ToList());

                        var alphabet_id = alphabet == headers.First().alphabet ? headers.First().alphabet_id : ++last_alphabet_id;
                        var dimension_id = dimension == headers.First().dimension ? headers.First().dimension_id : ++last_dimension_id;
                        var category_id = category == headers.First().category ? headers.First().category_id : ++last_category_id;
                        var source_id = source == headers.First().source ? headers.First().source_id : ++last_source_id;
                        var group_id = group == headers.First().group ? headers.First().group_id : ++last_group_id;
                        var member_id = member == headers.First().member ? headers.First().member_id : ++last_member_id;
                        var perspective_id = perspective == headers.First().perspective ? headers.First().perspective_id : ++last_perspective_id;

                        var headers_merged = (fid, alphabet, dimension, category, source, group, member, perspective, alphabet_id, dimension_id, category_id, source_id, group_id, member_id, perspective_id);

                        new_headers.Add(headers_merged); //dataset_headers[col2]);
                    }
                }
            }

            if (program.write_console_log) program.WriteLine($@"Finished merging clusters: {needs_merging1.Count}");

            var new_f = dataset_instance_list.Select((row, row_index) => (row.comment_columns, new_columns.Select(col => col[row_index]).ToList())).ToList();
            return (new_headers, new_f);
        }

        public static List<(string alphabet, string dimension, string category, string source, string @group, string member, string perspective)> load_filter_csv(string file)
        {
            var data = File.ReadAllLines(file).Where(a => !string.IsNullOrWhiteSpace(a)).Select(a => a.Split(',')).Skip(1).SelectMany(d =>
            {

                var results = new List<(string alphabet, string dimension, string category, string source, string @group, string member, string perspective)>();


                var alphabet_split = d[0].Split(new char[] { '/', '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var alphabet in alphabet_split)
                {
                    var dimension_split = d[1].Split(new char[] {'/', '|'}, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var dimension in dimension_split)
                    {
                        var category_split = d[2].Split(new char[] {'/', '|'}, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var category in category_split)
                        {
                            var source_split = d[3].Split(new char[] {'/', '|'}, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var source in source_split)
                            {
                                var group_split = d[4].Split(new char[] {'/', '|'}, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var group in group_split)
                                {
                                    var memeber_split = d[5].Split(new char[] {'/', '|'}, StringSplitOptions.RemoveEmptyEntries);
                                    foreach (var member in memeber_split)
                                    {
                                        var perspective_split = d[6].Split(new char[] {'/', '|'}, StringSplitOptions.RemoveEmptyEntries);

                                        foreach (var perspective in perspective_split)
                                        {
                                            var alphabet1 = string.IsNullOrWhiteSpace(alphabet) || alphabet == "*" ? null : alphabet;
                                            var dimension1 = string.IsNullOrWhiteSpace(dimension) || dimension == "*" ? null : dimension;
                                            var category1 = string.IsNullOrWhiteSpace(category) || category == "*" ? null : category;
                                            var source1 = string.IsNullOrWhiteSpace(source) || source == "*" ? null : source;
                                            var group1 = string.IsNullOrWhiteSpace(group) || group == "*" ? null : group;
                                            var member1 = string.IsNullOrWhiteSpace(member) || member == "*" ? null : member;
                                            var perspective1 = string.IsNullOrWhiteSpace(perspective) || perspective == "*" ? null : perspective;

                                            results.Add((alphabet: alphabet1, dimension: dimension1, category: category1, source: source1, group: group1, member: member1, perspective: perspective1));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }




                return results;

            }).ToList();

            return data;
        }

        public static bool WildcardMatch(string text, string wildcard)
        {
            //if ((text != null && wildcard == null) || (text == null && wildcard != null)) return false;

            if (string.Equals(text, wildcard, StringComparison.InvariantCultureIgnoreCase)) return true;

            

            if (wildcard.Any(b => b == '*' || b == '?'))
            {
                var reg = "^" + Regex.Escape(wildcard).Replace("\\?", ".").Replace("\\*", ".*") + "$";

                return Regex.IsMatch(text, reg);
            }

            return false;
        }

        public static void master_compute(string[] args, CancellationToken token)
        {
            var filter_based_names = load_filter_based_fs();

            var load_filter_csv = program.load_filter_csv(@"c:\users\aaron\desktop\features1.csv"); // features2.csv

            //var filter_based_names2 = load_filter_based_fs2();

            //var azure_names = File.ReadLines(@"C:\svm_compute\azure_csv.csv").First().Split(',');

            //filter_based_names2 = filter_based_names2.Except(azure_names).ToList();

            var program_stopwatch = new Stopwatch();
            program_stopwatch.Start();

            var output_feature_tree = false;
            var output_empty_features = false;



            //var job_names_filter = args.IndexOf("/names") > -1 ? args[args.IndexOf("/names") + 1].Split(',').ToList() : null;

            //job_names_filter.ForEach(a=>if (program.write_console_log) program.WriteLine(a));

            //Console.ReadLine();
            if (program.write_console_log) program.WriteLine($@"{nameof(Main)}: {nameof(output_feature_tree)}: {string.Join(", ", output_feature_tree)}");

            // setup class labels and class names
            var negative_class_id = -1;
            var positive_class_id = +1;
            var class_names = new List<(int class_id, string class_name)>() { (negative_class_id, "standard_coil"), (positive_class_id, "dimorphic_coil") };


            // read dataset
            var (dataset_headers, dataset_comment_row_values, dataset_instance_list) = read_binary_dataset(false, negative_class_id, positive_class_id, class_names);


            if (load_filter_csv != null && load_filter_csv.Count > 0)
            {
                var ignore_alphabet = false;
                var ignore_dimension = false;
                var ignore_category = false;
                var ignore_source = false;
                var ignore_group = false;
                var ignore_member = true;
                var ignore_perspective = true;

                var wildcard_headers = dataset_headers.Where(a =>
                    a.fid == 0 ||
                    load_filter_csv.Any(b => 
                        (ignore_alphabet || b.alphabet == null || WildcardMatch(a.alphabet, b.alphabet)) &&
                        (ignore_dimension || b.dimension == null || WildcardMatch(a.dimension, b.dimension)) &&
                        (ignore_category || b.category == null || WildcardMatch(a.category, b.category)) &&
                        (ignore_source || b.source == null || WildcardMatch(a.source, b.source)) &&
                        (ignore_group || b.group == null || WildcardMatch(a.group, b.group)) &&
                        (ignore_member || b.member == null || WildcardMatch(a.member, b.member)) &&
                        (ignore_perspective || b.perspective == null || WildcardMatch(a.perspective, b.perspective)) 
                    )).ToList();

                dataset_headers = wildcard_headers;
            }

            //var filter_overall_and_normal = false;
            //var filter_uniprotkb = false;

            //if (filter_overall_and_normal)
            //{
            //    if (program.write_console_log) program.WriteLine("Filtering for Normal and Overall alphabets...");
            //    dataset_headers = dataset_headers.Where(a =>
            //        a.fid == 0 || string.IsNullOrWhiteSpace(a.alphabet) ||
            //        string.Equals(a.alphabet, "Normal", StringComparison.InvariantCultureIgnoreCase) ||
            //        string.Equals(a.alphabet, "Overall", StringComparison.InvariantCultureIgnoreCase)).ToList();
            //}

            //if (filter_uniprotkb)
            //{
            //    if (program.write_console_log) program.WriteLine("Filtering for UniProtKB alphabets...");
            //    dataset_headers = dataset_headers.Where(a => a.fid == 0 || string.Equals(a.alphabet, "UniProtKB", StringComparison.InvariantCultureIgnoreCase)).ToList();
            //}



            if (program.write_console_log) program.WriteLine("Reading header FIDs after filters applied...");
            var dataset_headers_fids = dataset_headers.AsParallel().AsOrdered().Select(a => a.fid).ToList();

            // fids are equal to the indexes, if the list hasn't been modified.

            if (program.write_console_log) program.WriteLine("Filtering data set to FIDs...");
            //dataset_instance_list = dataset_instance_list.AsParallel().AsOrdered().Select(a => (a.comment_columns, a.feature_data.Where(b => dataset_headers_fids.Contains(b.fid)).ToList())).ToList();

            dataset_instance_list = dataset_instance_list.AsParallel().AsOrdered().Select(a => (a.comment_columns, dataset_headers_fids.Select(fid =>
            {
                if (fid != a.feature_data[fid].fid) throw new Exception();
                return a.feature_data[fid];
            }).ToList())).ToList();



            // output feature tree
            if (output_feature_tree)
            {
                if (program.write_console_log) program.WriteLine("");
                var sw1 = new Stopwatch();
                sw1.Start();
                if (program.write_console_log) program.WriteLine($@"{nameof(master_compute)}(): Building feature tree... Calling {nameof(feature_tree.group_dataset_headers_all_levels)}(...)...");
                feature_tree.group_dataset_headers_all_levels(dataset_headers);
                sw1.Stop();
                if (program.write_console_log) program.WriteLine($@"{nameof(master_compute)}(): Finished feature tree... {sw1.Elapsed.ToString()}...");
            }

            // check dataset format

            var check_class_id_position = true;
            var check_row_lengths = true;
            var check_empty_features = true;
            var find_duplicate_groups = true;

            var check_duplicate_columns = true;
            var filter_to_azure_lists = false;
            var check_dataset_headers_distinct = true;
            var convert_to_azure_csv = false;

            if (check_class_id_position)
            {
                if (program.write_console_log) program.WriteLine($@"{nameof(master_compute)}(): Calling {nameof(check_class_id_feature)}(...)...");
                check_class_id_feature(dataset_headers);
            }

            if (check_row_lengths)
            {
                if (program.write_console_log) program.WriteLine($@"{nameof(master_compute)}(): Calling {nameof(check_rows_have_equal_features)}(...)...");
                check_rows_have_equal_features(dataset_instance_list);
            }

            if (check_empty_features)
            {
                if (program.write_console_log) program.WriteLine($@"{nameof(master_compute)}(): Calling {nameof(find_empty_features)}(...)...");
                find_empty_features(output_empty_features, dataset_headers, dataset_instance_list);
                //merge_duplicate_features(dataset_headers, dataset_instance_list);
            }

            if (check_row_lengths)
            {
                if (program.write_console_log) program.WriteLine($@"{nameof(master_compute)}(): Calling {nameof(check_rows_have_equal_features)}(...)...");
                check_rows_have_equal_features(dataset_instance_list);
            }

            
            




            if (find_duplicate_groups)
            {
                // group headers 
                if (program.write_console_log) program.WriteLine($@"{nameof(master_compute)}(): Grouping dataset headers...");
                // does groupby work in parallel?  or does parallel split up the data?
                //var t1 = new Stopwatch();
                //t1.Start();
                
                var dataset_headers_grouped1 = dataset_headers.GroupBy(a => (a.alphabet_id, a.category_id, a.dimension_id, a.source_id, a.group_id)).ToList();
                //t1.Stop();

                //var t2=new Stopwatch();
                //t2.Start();
                //var dataset_headers_grouped2 = dataset_headers.AsParallel().AsOrdered().GroupBy(a => (a.alphabet_id, a.category_id, a.dimension_id, a.source_id, a.group_id)).ToList();
                //t2.Stop();

                //var g1 = dataset_headers_grouped1.Select(a => a.ToList()).ToList();
                //var g2 = dataset_headers_grouped2.Select(a => a.ToList()).ToList();

                //for (var i = 0; i < g1.Count; i++)
                //{
                //    if (!g1[i].SequenceEqual(g2[i])) throw new Exception();
                //}

                if (program.write_console_log) program.WriteLine($@"{nameof(master_compute)}(): Calling {nameof(find_duplicate_groups)}(...)...");

                var n = program.find_duplicate_groups(dataset_headers_grouped1, dataset_headers, dataset_instance_list);

                dataset_headers = n.dataset_headers;
                dataset_instance_list = n.dataset_instance_list;
            }

            if (check_duplicate_columns)
            {
                if (program.write_console_log) program.WriteLine($@"{nameof(master_compute)}(): Calling {nameof(find_duplicate_columns)}(...)...");

                var n = program.find_duplicate_columns(dataset_headers, dataset_instance_list);

                dataset_headers = n.dataset_headers;
                dataset_instance_list = n.dataset_instance_list;
            }

            if (check_dataset_headers_distinct)
            {

                var dataset_headers_distinct = dataset_headers.Select(a => (a.alphabet, a.dimension, a.category, a.source, a.group, a.member, a.perspective)).Distinct().ToList();

                if (dataset_headers_distinct.Count != dataset_headers.Count)
                {
                    var d = dataset_headers.Count - dataset_headers_distinct.Count ;
                    if (program.write_console_log) program.WriteLine($@"Dataset headers have duplicates: headers: {dataset_headers.Count}, distinct headers: {dataset_headers_distinct.Count}, duplicates: {d}");
                }
            }

           

            

            if (convert_to_azure_csv)
            {
                if (program.write_console_log) program.WriteLine("Converting to azure csv format...");
                var azure_csv = new List<string>();

                azure_csv.Add(string.Join(",", dataset_headers.Select(col => $@"{col.alphabet}.{col.category}.{col.dimension}.{col.source}.{col.group}.{col.member}.{col.perspective}").ToList()));

                foreach (var row in dataset_instance_list)
                {
                    azure_csv.Add(string.Join(",", row.feature_data.Select(a => a.fv).ToList()));
                }

                var azure_csv_filename = $@"c:\svm_compute\azure_csv.csv";
                if (program.write_console_log) program.WriteLine("Saving: " + azure_csv_filename);

                program.WriteAllLines(azure_csv_filename, azure_csv);
            }

            if (filter_to_azure_lists)
            {
                dataset_headers = dataset_headers.Where((a,i) => filter_based_names.Any(b =>
                    
                    string.Equals(a.alphabet, b.alphabet, StringComparison.InvariantCultureIgnoreCase) && 
                    string.Equals(a.dimension, b.dimension.ToString(), StringComparison.InvariantCultureIgnoreCase) &&
                    string.Equals(a.category, b.category, StringComparison.InvariantCultureIgnoreCase) && 
                    string.Equals(a.source, b.source, StringComparison.InvariantCultureIgnoreCase) && 
                    string.Equals(a.@group, b.@group, StringComparison.InvariantCultureIgnoreCase) && 
                    string.Equals(a.member, b.member, StringComparison.InvariantCultureIgnoreCase) && 
                    string.Equals(a.perspective, b.perspective, StringComparison.InvariantCultureIgnoreCase)
                    
                    )).ToList();

                var fids = dataset_headers.Select(a => a.fid).ToList();

                var fid_indexes = dataset_instance_list.FirstOrDefault().feature_data.Select((a, i) => fids.Contains(a.fid)).ToList();

                dataset_instance_list = dataset_instance_list.Select(a => (a.comment_columns, a.feature_data.Where((b, i) => fid_indexes[i]).ToList())).ToList();

                for (var i = 0; i < dataset_headers.Count; i++)
                {
                    if (dataset_headers[i].fid != dataset_instance_list.FirstOrDefault().feature_data[i].fid) throw new Exception();
                }

                dataset_headers = dataset_headers.Select((a, i) => (i, a.alphabet, a.dimension, a.category, a.source, a.group, a.member, a.perspective, a.alphabet_id, a.dimension_id, a.category_id, a.source_id, a.group_id, a.member_id, a.perspective_id)).ToList();

                dataset_instance_list = dataset_instance_list.Select(a => (a.comment_columns, a.feature_data.Select((b,j) => (j, b.fv)).ToList())).ToList();

                for (var i = 0; i < dataset_headers.Count; i++)
                {
                    if (dataset_headers[i].fid != dataset_instance_list.FirstOrDefault().feature_data[i].fid) throw new Exception();
                }

                if (filter_based_names.Count != dataset_headers.Count)
                {
                    var missing = filter_based_names.Where(b => !dataset_headers.Any(a => string.Equals(a.alphabet, b.alphabet, StringComparison.InvariantCultureIgnoreCase) &&
                                                                                          string.Equals(a.dimension, b.dimension.ToString(), StringComparison.InvariantCultureIgnoreCase) &&
                                                                                          string.Equals(a.category, b.category, StringComparison.InvariantCultureIgnoreCase) &&
                                                                                          string.Equals(a.source, b.source, StringComparison.InvariantCultureIgnoreCase) &&
                                                                                          string.Equals(a.@group, b.@group, StringComparison.InvariantCultureIgnoreCase) &&
                                                                                          string.Equals(a.member, b.member, StringComparison.InvariantCultureIgnoreCase) &&
                                                                                          string.Equals(a.perspective, b.perspective, StringComparison.InvariantCultureIgnoreCase)
                    )).ToList();

                    missing.ForEach(a=> Console.WriteLine("Missing: " + a));

                    throw new Exception();
                }
            }



            if (program.write_console_log) program.WriteLine($@"{nameof(master_compute)}(): Converting dataset format...");

            // group headers 
            if (program.write_console_log) program.WriteLine($@"{nameof(master_compute)}(): Grouping dataset headers...");
            
            var dataset_headers_grouped = dataset_headers.GroupBy(a => (a.alphabet_id, a.category_id, a.dimension_id, a.source_id, a.group_id)).ToList();

            var features_input = dataset_headers_grouped.Skip(1).AsParallel().AsOrdered().Select((a, i) => new feature_selection_unidirectional.feature_set()
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

            var remove_large_groups = true;
            var order_groups_by_size = true;

            if (remove_large_groups)
            {
                var max_features_per_group = 100;
                if (program.write_console_log) program.WriteLine($@"{nameof(master_compute)}(): Removing feature groups with more than {max_features_per_group} features...");
                var remove = features_input.Where(a => a.set_members.Count > max_features_per_group).ToList();
                features_input = features_input.Except(remove).ToList();
            }

            if (order_groups_by_size)
            {
                if (program.write_console_log) program.WriteLine($@"{nameof(master_compute)}(): Ordering feature groups by size...");
                features_input = features_input.OrderBy(a => a.set_members.Count).ToList();
            }

            // check all header feature ids exist
            foreach (var i in features_input)
            {
                foreach (var j in i.set_members)
                {
                    var fid = j.fid;

                    var exists = (dataset_instance_list.FirstOrDefault().feature_data.Count > fid && dataset_instance_list.FirstOrDefault().feature_data[fid].fid == fid) || (dataset_instance_list.FirstOrDefault().feature_data.FindIndex(a => a.fid == fid) > -1);

                    if (!exists)
                    {
                        throw new Exception();
                    }
                }
            }

            foreach (var j in dataset_headers)
            {
                var fid = j.fid;

                var exists = (dataset_instance_list.FirstOrDefault().feature_data.Count > fid && dataset_instance_list.FirstOrDefault().feature_data[fid].fid == fid) || (dataset_instance_list.FirstOrDefault().feature_data.FindIndex(a => a.fid == fid) > -1);

                if (!exists)
                {
                    throw new Exception();
                }
            }

            // check all features have a header
            foreach (var j in dataset_instance_list.FirstOrDefault().feature_data)
            {
                var fid = j.fid;

                

                var exists1 = (dataset_headers.Count > fid && dataset_headers[fid].fid == fid) || (dataset_headers.FindIndex(a=> a.fid == fid) > -1);


                var exists2 = fid == 0 || features_input.Any(b => b.set_members.Any(c => c.fid == fid));

                if (!exists1 || !exists2) throw new Exception();
            }

            // 2. run feature selection
            var rsp = new run_svm_params();
            rsp.set_defaults();

            // cv has an outer-outer layer called randomisation_cv_folds...

            rsp.run_remote = false;// true;//false; //true;
            rsp.outer_cv_folds = 5;
            rsp.outer_cv_folds_to_skip = rsp.outer_cv_folds - 1; // previous value: 0  // not sure if this is correct
            rsp.randomisation_cv_folds = rsp.outer_cv_folds; // previous value: 1
            rsp.inner_cv_folds = 5;
            rsp.class_names = class_names;
            rsp.kernels = new List<libsvm_caller.libsvm_kernel_type>() { libsvm_caller.libsvm_kernel_type.rbf };
            //rsp.kernels = new List<libsvm_caller.libsvm_kernel_type>() { libsvm_caller.libsvm_kernel_type.rbf };
            rsp.cross_validation_metrics_class_list = new List<int>() { +1 };
            rsp.cross_validation_metrics = performance_measure.confusion_matrix.cross_validation_metrics.F1S;
            //rsp.kernel_parameter_search_methods = new List<libsvm_caller.kernel_parameter_search_method>() {libsvm_caller.kernel_parameter_search_method.none};
            rsp.kernel_parameter_search_methods = new List<libsvm_caller.kernel_parameter_search_method>() {libsvm_caller.kernel_parameter_search_method.grid_internal};
            rsp.cost = 1;
            rsp.gamma = 0.1;

            //rsp.print();


            // Alternative unused open ports: 843, 3121 or 4155

            var all_vs_all_selection = true;
            var categories_selection = false;
            var alphabet_selection = false;
            var category_alphabet_selection = false;

            var join_mpsa = false;
            var join_pseaac = false;
            var join_aaindex_lit = false;


            var feature_selection_jobs = new List<(string job_name, List<feature_selection_unidirectional.feature_set> features_input)>();

            if (all_vs_all_selection)
            {
                feature_selection_jobs.Add(("all", features_input));
            }

            if (categories_selection)
            {
                var x = features_input.GroupBy(a => a.category).Select(a => (a.Key, a.ToList())).OrderBy(a => a.Item2.Count).ToList();
                //if (program.write_console_log) program.WriteLine();
                //if (program.write_console_log) program.WriteLine(x.Count + ": " + string.Join(", ", x.Select(a => a.Item2.Count).ToList()));
                feature_selection_jobs.AddRange(x);
            }

            if (alphabet_selection)
            {
                var x = features_input.GroupBy(a => a.alphabet).Select(a => (a.Key, a.ToList())).OrderBy(a => a.Item2.Count).ToList();
                //if (program.write_console_log) program.WriteLine();
                //if (program.write_console_log) program.WriteLine(x.Count + ": " + string.Join(", ", x.Select(a => a.Item2.Count).ToList()));
                feature_selection_jobs.AddRange(x);
            }

            if (category_alphabet_selection)
            {
                var x = features_input.GroupBy(a => (a.alphabet, a.category)).Select(a => (a.Key.alphabet + "_" + a.Key.category, a.ToList())).OrderBy(a => a.Item2.Count).ToList();
                //if (program.write_console_log) program.WriteLine();
                //if (program.write_console_log) program.WriteLine(x.Count + ": " + string.Join(", ", x.Select(a => a.Item2.Count).ToList()));

                feature_selection_jobs.AddRange(x);
            }

            
            if (join_mpsa)
            {
                var x = features_input.Where(a => a.category.ToLowerInvariant().Contains("mpsa")).ToList();
                feature_selection_jobs.Add(("mpsa", x));
            }

            
            if (join_pseaac)
            {
                var pse_aac = new string[] { "average_dipeptide_distance", "average_seq_positions", "dipeptides", "dipeptides_binary", "motifs", "motifs_binary", "oaac", "oaac_binary" };

                var x = features_input.Where(a => pse_aac.Any(b => a.category.ToLowerInvariant().Contains(b))).ToList();

                var x1 = x.GroupBy(a => a.alphabet).Select(a => ("pse_aac_" + a.Key, a.ToList())).ToList();

                feature_selection_jobs.AddRange(x1);

            }

            
            if (join_aaindex_lit)
            {
                var aa_lit = new string[] { "aaindex_accessibility", "aaindex_affinity", "aaindex_aggregation", "aaindex_charge", "aaindex_coil", "aaindex_disorder", "aaindex_dna_binding", "aaindex_interaction", "aaindex_intersection", "aaindex_ppi", "aaindex_strand", "aaindex_subnuclear", "aaindex_zernike" };

                var x = features_input.Where(a => aa_lit.Any(b => a.category.ToLowerInvariant().Contains(b))).ToList();

                var x1 = x.GroupBy(a => a.alphabet).Select(a => ("aaindex_lit_" + a.Key, a.ToList())).ToList();

                feature_selection_jobs.AddRange(x1);
            }


            // 37: 4, 153, 3402, 5, 77, 4, 17, 156, 156, 26, 26, 26, 392, 38, 38, 16, 16, 16, 6, 182, 182, 26, 6, 57, 57, 10, 4, 4, 4, 4, 2, 2, 1, 1, 1, 1, 1
            //
            // 11: 1238, 1284, 1284, 17, 780, 119, 119, 106, 67, 57, 48
            //
            // 124: 4, 6, 1134, 1134, 1134, 5, 10, 4, 17, 156, 156, 26, 26, 26, 54, 10, 10, 2, 2, 2, 10, 10, 2, 2, 2, 24, 24, 10, 10, 2, 182, 182, 26, 4, 4, 2, 2, 2, 4, 4, 2, 2, 2, 21, 21, 10, 10, 2, 4, 4, 2, 2, 2, 18, 9, 14, 14, 2, 14, 14, 2, 2, 52, 52, 2, 2, 2, 2, 2, 15, 2, 9, 2, 2, 2, 2, 2, 12, 2, 9, 2, 2, 2, 2, 2, 12, 2, 2, 52, 52, 2, 52, 10, 10, 2, 10, 10, 2, 2, 2, 2, 2, 1, 1, 26, 1, 1, 1, 1, 6, 6, 1, 26, 1, 1, 1, 1, 26, 1, 3, 3, 1, 1, 1

            //37: 1, 1, 1, 1, 1, 2, 2, 4, 4, 4, 4, 4, 4, 5, 6, 6, 10, 16, 16, 16, 17, 26, 26, 26, 26, 38, 38, 57, 57, 77, 153, 156, 156, 182, 182, 392, 3402

            //11: 17, 48, 57, 67, 106, 119, 119, 780, 1238, 1284, 1284

            //124: 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 5, 6, 6, 6, 9, 9, 9, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 12, 12, 14, 14, 14, 14, 15, 17, 18, 21, 21, 24, 24, 26, 26, 26, 26, 26, 26, 26, 52, 52, 52, 52, 52, 54, 118:156, 119:156, 120:182, 121:182, 122:1134, 123:1134, 124:1134


            //  1   2   3   4   5   6   7   8   9   10  11  12  13  14  15  16  17  18  19  20  21  22  23  24  25  26  27  28  29  30  31  32  33  34  35  36  37  38  39  40  41  42  43  44  45  46  47  48  49  50  51  52  53  54  55  56  57  58  59  60  61  62  63  64  65  66  67  68  69  70  71  72  73  74  75  76  77  78  79  80  81  82  83  84  85  86  87  88  89  90  91  92  93  94  95  96  97  98  99  100 101 102 103 104 105 106 107 108 109 110 111 112 113 114 115 116 117 118 119 120 121 122 123 124 125 126 127 128 129 130 131 132 133 134 135 136 137 138 139 140 141 142 143 144 145 146 147 148 149 150 151 152 153 154 155 156 157 158 159 160 161 162 163 164 165 166-aaindex  167-aaindex     168-aaindex     169-overall     170-hydro   171-physichem     172-aaindex
            //  1   1   1   1   1   1   1   1   1   1   1   1   1   1   1   1   1   1   1   1   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   2   3   3   4   4   4   4   4   4   4   4   4   4   4   4   4   4   5   5   6   6   6   6   6   9   9   9   10  10  10  10  10  10  10  10  10  10  10  10  10  10  12  12  14  14  14  14  15  16  16  16  17  17  17  18  21  21  24  24  26  26  26  26  26  26  26  26  26  26  26  38  38  48  52  52  52  52  52  54  57  57  57  67  77  106 119 119 153 156 156 156 156 182 182 182 182 392 780      1134            1134       1134         1238            1284            1284            3402


            //var skip_names = new string[] {"aaindex_accessibility", "aaindex_affinity", "aaindex_aggregation", "aaindex_charge", "aaindex_coil", "aaindex_disorder", "aaindex_dna_binding", "aaindex_interaction", "aaindex_intersection", "aaindex_ppi", "aaindex_strand", "aaindex_subnuclear", "aaindex_zernike", "aa_average_dipeptide_distance", "aa_average_seq_positions", "aa_dipeptides", "aa_dipeptides_binary", "aa_motifs", "aa_motifs_binary", "aa_oaac", "aa_oaac_binary", "blast_pssm", "default", "default_r_peptides", "dna_binding", "Hydrophobicity_aaindex_affinity", "Hydrophobicity_aaindex_aggregation", "Hydrophobicity_aaindex_coil", "Hydrophobicity_aaindex_intersection", "Hydrophobicity_aaindex_ppi", "Hydrophobicity_aaindex_zernike", "Hydrophobicity_aa_average_dipeptide_distance", "Hydrophobicity_aa_average_seq_positions", "Hydrophobicity_aa_dipeptides", "Hydrophobicity_aa_dipeptides_binary", "Hydrophobicity_aa_motifs", "Hydrophobicity_aa_motifs_binary", "Hydrophobicity_aa_oaac", "Hydrophobicity_aa_oaac_binary", "Hydrophobicity_blast_pssm", "Hydrophobicity_iup", "Hydrophobicity_mpsa", "iup", "length_sequence", "list.txt", "mpsa_average_dipeptide_distance", "mpsa_average_seq_positions", "mpsa_oaac", "mpsa_oaac_binary", "Normal", "Normal_aa_average_seq_positions", "Normal_aa_motifs", "Normal_aa_motifs_binary", "Normal_aa_oaac", "Normal_aa_oaac_binary", "Normal_blast_pssm", "Normal_mpsa", "Overall_aaindex_accessibility", "Overall_aaindex_affinity", "Overall_aaindex_aggregation", "Overall_aaindex_charge", "Overall_aaindex_coil", "Overall_aaindex_disorder", "Overall_aaindex_dna_binding", "Overall_aaindex_interaction", "Overall_aaindex_intersection", "Overall_aaindex_ppi", "Overall_aaindex_strand", "Overall_aaindex_subnuclear", "Overall_aaindex_zernike", "Overall_blast_pssm", "Overall_dna_binding", "Overall_iup", "Overall_length_sequence", "Overall_mpsa", "Overall_sable", "PdbSum", "PdbSum_aa_average_dipeptide_distance", "PdbSum_aa_average_seq_positions", "PdbSum_aa_dipeptides", "PdbSum_aa_dipeptides_binary", "PdbSum_aa_motifs", "PdbSum_aa_motifs_binary", "PdbSum_aa_oaac", "PdbSum_aa_oaac_binary", "PdbSum_blast_pssm", "PdbSum_iup", "PdbSum_mpsa", "Physicochemical_aaindex_affinity", "Physicochemical_aaindex_aggregation", "Physicochemical_aaindex_coil", "Physicochemical_aaindex_intersection", "Physicochemical_aaindex_ppi", "Physicochemical_aaindex_zernike", "Physicochemical_aa_average_dipeptide_distance", "Physicochemical_aa_average_seq_positions", "Physicochemical_aa_dipeptides", "Physicochemical_aa_dipeptides_binary", "Physicochemical_aa_motifs", "Physicochemical_aa_motifs_binary", "Physicochemical_aa_oaac", "Physicochemical_aa_oaac_binary", "Physicochemical_blast_pssm", "Physicochemical_iup", "Physicochemical_mpsa", "r_peptides", "sable", "SS_HEC_mpsa_average_dipeptide_distance", "SS_HEC_mpsa_average_seq_positions", "SS_HEC_mpsa_oaac", "SS_HEC_mpsa_oaac_binary", "TableVenn", "TableVenn_aa_average_seq_positions", "TableVenn_aa_motifs", "TableVenn_aa_motifs_binary", "TableVenn_aa_oaac", "TableVenn_aa_oaac_binary", "TableVenn_blast_pssm", "TableVenn_iup", "TableVenn_mpsa", "UniProtKb", "UniProtKb_aa_average_dipeptide_distance", "UniProtKb_aa_average_seq_positions", "UniProtKb_aa_dipeptides", "UniProtKb_aa_dipeptides_binary", "UniProtKb_aa_motifs", "UniProtKb_aa_motifs_binary", "UniProtKb_aa_oaac", "UniProtKb_aa_oaac_binary", "UniProtKb_blast_pssm", "UniProtKb_iup", "UniProtKb_mpsa", "Venn1", "Venn1_aa_average_dipeptide_distance", "Venn1_aa_average_seq_positions", "Venn1_aa_dipeptides", "Venn1_aa_dipeptides_binary", "Venn1_aa_motifs", "Venn1_aa_motifs_binary", "Venn1_aa_oaac", "Venn1_aa_oaac_binary", "Venn1_blast_pssm", "Venn1_iup", "Venn1_mpsa", "Venn2", "Venn2_aa_average_dipeptide_distance", "Venn2_aa_average_seq_positions", "Venn2_aa_dipeptides", "Venn2_aa_dipeptides_binary", "Venn2_aa_motifs", "Venn2_aa_motifs_binary", "Venn2_aa_oaac", "Venn2_aa_oaac_binary", "Venn2_blast_pssm", "Venn2_iup", "Venn2_mpsa", "aaindex_lit_Hydrophobicity", "aaindex_lit_Overall", "aaindex_lit_Physicochemical", "pse_aac_Normal", "pse_aac_PdbSum", "pse_aac_TableVenn", "pse_aac_UniProtKb", "pse_aac_Venn1", "pse_aac_Venn2"};
            var results = new List<(List<feature_selection_unidirectional.feature_set> selected_features, feature_selection_unidirectional.score_metrics performance)>();

            feature_selection_jobs = feature_selection_jobs.OrderBy(a => a.features_input.Count).ToList();

            for (var index = 0; index < feature_selection_jobs.Count; index++)
            {
                var a = feature_selection_jobs[index];
                //var x = a.features_input.Select(b => $"{index}: {b.alphabet} {b.dimension} {b.category} {b.source}").Distinct().ToList();
                //x.ForEach(z => if (program.write_console_log) program.WriteLine(z));

                if (program.write_console_log) program.WriteLine((index + 1) + " " + a.job_name);
            }

            //Console.ReadLine();

            //if (job_names_filter != null && job_names_filter.Count > 0)
            //{
            //    feature_selection_jobs = feature_selection_jobs.Where(a => job_names_filter.Contains(a.job_name)).ToList();
            //}

            //feature_selection_jobs = feature_selection_jobs.Skip(feature_selection_jobs.FindIndex(a => a.job_name == "Overall_aaindex_all") + 1).ToList();

            program.svm_cache_load = false;
            program.svm_cache_save = false;


            program.AppendAllLines($@"c:\svm_compute\svm_impl_timer.csv", new List<string>() { "svm_implementation,kernel,outer_folds,inner_folds,index,time,job,items,metric,score,score_ppf" });

            var test_impl = new List<libsvm_caller.svm_implementation>() { libsvm_caller.svm_implementation.libsvm_eval };
            var test_kernels = new List<libsvm_caller.libsvm_kernel_type>() {/*libsvm_caller.libsvm_kernel_type.linear, libsvm_caller.libsvm_kernel_type.polynomial, libsvm_caller.libsvm_kernel_type.sigmoid,*/ libsvm_caller.libsvm_kernel_type.rbf };

            for (var i = 0; i <= feature_selection_jobs.Count; i++)
            {

                //foreach (var kernel in test_kernels)
                //{
                //    foreach (libsvm_caller.svm_implementation svm_implementation in test_impl)
                //    {
                //        for (var outer_folds = 2; outer_folds <= 10; outer_folds++)
                //        {
                //            for (var inner_folds = 2; inner_folds <= 10; inner_folds++)
                //            {
                //rsp.cross_validation_metrics_class_list=new List<int>() { +1 };
                //rsp.cross_validation_metrics = performance_measure.confusion_matrix.cross_validation_metrics.F1S;
                //rsp.run_remote = false;
                //rsp.outer_cv_folds = outer_folds;
                //rsp.outer_cv_folds_to_skip = rsp.outer_cv_folds - 1;
                //rsp.randomisation_cv_folds = rsp.outer_cv_folds;
                //rsp.inner_cv_folds = inner_folds;
                //rsp.class_names = class_names;
                //rsp.kernels = new List<libsvm_caller.libsvm_kernel_type>() {kernel};



                GC.Collect();
                GC.Collect();
                Task.Delay(new TimeSpan(0, 0, 2)).Wait();

                var timer = new Stopwatch();
                timer.Start();

                //program.inner_cv_svm_implementation = svm_implementation;
                //program.outer_cv_svm_implementation = svm_implementation;

                //if (program.write_console_log) program.WriteLine(svm_implementation.ToString());



                var n = "";
                var fi = new List<feature_selection_unidirectional.feature_set>();

                var summary = new List<string>();

                if (i == feature_selection_jobs.Count)
                {
                    //continue; // temp line

                    if (results == null && results.Count == 0) break;

                    if (feature_selection_jobs.Count == 1) break;


                    n = "final";
                    fi = results.SelectMany(a => a.selected_features).GroupBy(a => a.set_id).Select(a => a.First()).Distinct().ToList();

                    summary = fi.Select(a => $"{a.alphabet},{a.dimension},{a.category},{a.source},{a.@group},{a.set_id}").ToList();
                    summary.Insert(0, $"alphabet,dimension,category,source,group,set_id");
                }
                else
                {
                    var feature_selection_job = feature_selection_jobs[i];

                    n = feature_selection_job.job_name;
                    fi = feature_selection_job.features_input;
                }

                //n = $"{n}_{i}_{svm_implementation}_{kernel}";
                //if (skip_names.Contains(n)) continue;  // temp line


                var results_output_folder = program.convert_path($@"c:\svm_compute\feature_selection\{program.program_start_time}\{n}\");
                //Directory.CreateDirectory(results_output_folder);

                // max_tasks = number of compute units * number of concurrent connections * (1 active and 1 waiting task)
                // this could change during execution...

                // pay attention to: run_remote; kernel_type; performance_metric; performance_classes; 

                if (summary != null && summary.Count > 0)
                {
                    program.WriteAllLines(Path.Combine(results_output_folder, "summary.csv"), summary);
                }

                //Overall_aaindex_all

                if (program.write_console_log) program.WriteLine($"i = {(i + 1)}/{(feature_selection_jobs.Count + 1)}, name = \"{n}\", features_input count = {fi.Count}");
                if (program.write_console_log) program.WriteLine($"Setting feature_selection_unidirectional() parameters.");
                var fsu = new feature_selection_unidirectional(
                    results_output_folder: results_output_folder, 
                    run_svm_params: rsp, 
                    dataset_instance_list: dataset_instance_list, 
                    features_input: fi, 
                    base_features: null, 
                    //feature_selection_combinator: feature_selection_unidirectional.feature_selection_combinators.individual_features,
                    feature_selection_combinator: feature_selection_unidirectional.feature_selection_combinators.feature_sets,
                    //feature_selection_type: feature_selection_unidirectional.feature_selection_types.forwards_then_backwards,
                    //feature_selection_type: feature_selection_unidirectional.feature_selection_types.forwards_and_backwards, 
                    feature_selection_type: feature_selection_unidirectional.feature_selection_types.backwards_and_forwards, 
                    perf_selection_rule: feature_selection_unidirectional.perf_selection_rules.best_score, // OR: best_score ?!
                    feature_selection_performance_metrics: performance_measure.confusion_matrix.cross_validation_metrics.F1S, 
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
                    random_baseline: 0.01d
                    );



                if (program.write_console_log) program.WriteLine($"Calling run_unidirectional_feature_selection()");

                var result = fsu.run_unidirectional_feature_selection();

                var (selected_features, performance) = result;



                results.Add(result);

                timer.Stop();

                //var svm_impl_timer = string.Join(",", new string[] {svm_implementation.ToString(), kernel.ToString(), outer_folds.ToString(), inner_folds.ToString(), i.ToString(), timer.Elapsed.ToString(), n, fi.Count.ToString(), performance.score_metric.ToString()  , performance.score_after.ToString(), performance.score_ppf_after.ToString()});
                //program.AppendAllLines($@"c:\svm_compute\svm_impl_timer.csv", new List<string>() {svm_impl_timer});
                //if (program.write_console_log) program.WriteLine(svm_impl_timer);
            }

            var hyperparamater_selections = new string[] { "default_parameters", "grid_search" };
            var datasets = new string[] { "interface" }; // "neighbourhood", "protein"
            var dimensions = new string[] { "2D "}; // "2D_3D", "3D"
            var prunings = new string[] { "filter-based", "all_features" };
            var predictor_types = new string[] {"individual", "group"};
            var search_directions = new string[] { "forwards", "backwards"};
            
            // reconsider, think about, whether ranking once and then using forward would work??  i.e. rank one time all features, then iteratively add them by their rank order?  is that better/acceptable solution?  does it save processing time?

            foreach (var hyperparamter_selection in hyperparamater_selections)
            {
                foreach (var dataset in datasets)
                {
                    foreach (var dimension in dimensions)
                    {
                        foreach (var pruning in prunings)
                        {
                            foreach (var predictor_type in predictor_types)
                            {
                                foreach (var search_direction in search_directions)
                                {



                                }
                            }
                        }
                    }
                }
            }


            program_stopwatch.Stop();

            //program.console_log_enabled = true;
            //if (program.write_console_log) program.WriteLine($@"{nameof(Main)}: Finished: " + program_stopwatch.Elapsed);
            program.WriteLine($@"{nameof(Main)}: Finished: " + program_stopwatch.Elapsed);

            while (Console.ReadKey(true).Key != ConsoleKey.Escape) ;
        }



        public static void check_rows_have_equal_features(List<(List<(string comment_header, string comment_value)> comment_columns, List<(int fid, double fv)> feature_data)> dataset_instance_list)
        {
            if (dataset_instance_list.Select(a => a.feature_data.Count).Distinct().Count() != 1)
            {
                throw new Exception("number of features do not match on separate lines!");
            }
        }



        public static void merge_duplicate_features(
                List<(int fid, string alphabet, string dimension, string category, string source, string group, string member, string perspective,
                int alphabet_id, int dimension_id, int category_id, int source_id, int group_id, int member_id, int perspective_id)> dataset_headers,
                List<(List<(string comment_header, string comment_value)> comment_columns,
                List<(int fid, double fv)> feature_data)> dataset_instance_list)
        {
            var total_columns = dataset_instance_list.Max(a => a.feature_data.Count);

            var merge_list = new List<List<int>>();

            for (var i = 0; i < total_columns; i++)
            {
                for (var j = 0; j < total_columns; j++)
                {
                    if (i <= j) continue;

                    var seq_i = dataset_instance_list[i].feature_data.Select(a => a.fv).ToList();
                    var seq_j = dataset_instance_list[j].feature_data.Select(a => a.fv).ToList();

                    if (seq_i.SequenceEqual(seq_j))
                    {
                        var merge = merge_list.FindIndex(a => a.Any(b => b == i || b == j));
                        if (merge == -1)
                        {
                            merge_list.Add(new List<int>() { i, j });
                        }
                        else
                        {
                            merge_list[merge] = merge_list[merge].Union(new List<int>() { i, j }).ToList();
                        }
                    }
                }
            }

            merge_list.ForEach(a =>
            {
                if (program.write_console_log) program.WriteLine("Merging duplicate indexes: " + string.Join(", ", a));
            });
            // need to also merge headers

        }

        public static void find_empty_features
            (
                bool output_empty_features,
                List<(int fid, string alphabet, string dimension, string category, string source, string group, string member, string perspective, int alphabet_id, int dimension_id, int category_id, int source_id, int group_id, int member_id, int perspective_id)> dataset_headers,
                List<(List<(string comment_header, string comment_value)> comment_columns, List<(int fid, double fv)> feature_data)> dataset_instance_list
            )
        {
            // find always zero columns
            

            var total_headers = dataset_headers.Count;
            var total_columns = dataset_instance_list.Max(a => a.feature_data.Count);

            if (program.write_console_log) program.WriteLine($@"{nameof(Main)}: Finding empty features... headers: {total_headers}, columns: {total_columns}");

            var feature_empty = new bool[total_columns];

            var dataset_columns_distinct_value_count = new int[total_columns];
            var empty_features = new List<int>();

            for (var i = 1; i < total_columns; i++) // start at 1 to skip class_id
            {
                dataset_columns_distinct_value_count[i] = dataset_instance_list.Select(a => a.feature_data[i].fv).Distinct().Count();

                if (dataset_columns_distinct_value_count[i] <= 1)
                {
                    empty_features.Add(i);
                    feature_empty[i] = true;
                }

            }

            if (program.write_console_log) program.WriteLine($@"{nameof(Main)}: Checking FIDs match: {empty_features.Count}.");

            empty_features = empty_features.OrderByDescending(a => a).ToList();

            for (var i = 0; i < empty_features.Count; i++)
            {
                var empty_index = empty_features[i];

                //if (output_empty_features)
                //{
                //    if (program.write_console_log) program.WriteLine("Empty: " + empty_index);
                //}

                var fid1 = dataset_instance_list.First().feature_data[empty_index].fid;
                var fid2 = dataset_headers[empty_index].fid;

                if (fid1 != fid2)
                {
                    throw new Exception();
                }

                // removal very slow, so changed method.
                //dataset_instance_list.ForEach(a => a.feature_data.RemoveAt(empty_index));
                //dataset_headers.RemoveAt(empty_index);
            }

            if (program.write_console_log) program.WriteLine($@"{nameof(Main)}: Removing features which are empty: {empty_features.Count}.");
            for (var index = 0; index < dataset_instance_list.Count; index++)
            {
                dataset_instance_list[index] = (dataset_instance_list[index].comment_columns, dataset_instance_list[index].feature_data.Where((b, i) => !feature_empty[i]).ToList());
            }

            var xx = dataset_headers.Where((b, i) => !feature_empty[i]).ToList();
            
            dataset_headers.Clear();
            dataset_headers.AddRange(xx);

            // check headers and features are the same length

            if (program.write_console_log) program.WriteLine($@"{nameof(Main)}: Checking new column count matches header count.");

            dataset_instance_list.ForEach(a => { if (a.feature_data.Count != dataset_headers.Count) throw new Exception(); });

            // re-number headers 

            if (program.write_console_log) program.WriteLine($@"{nameof(Main)}: Renumbering header FIDs.");
            for (var new_fid = 0; new_fid < dataset_headers.Count; new_fid++)
            {
                var x = dataset_headers[new_fid];
                dataset_headers[new_fid] = (fid: new_fid, x.alphabet, x.dimension, x.category, x.source, x.group, x.member, x.perspective, x.alphabet_id, x.dimension_id, x.category_id, x.source_id, x.group_id, x.member_id, x.perspective_id);
            }

            // re-number features

            if (program.write_console_log) program.WriteLine($@"{nameof(Main)}: Renumbering features FIDs.");
            for (var row = 0; row < dataset_instance_list.Count; row++)
            {
                for (var new_fid = 0; new_fid < dataset_instance_list[row].feature_data.Count; new_fid++)
                {
                    var x = dataset_instance_list[row].feature_data[new_fid];
                    dataset_instance_list[row].feature_data[new_fid] = (fid: new_fid, x.fv);
                }
            }

            total_headers = dataset_headers.Count;
            total_columns = dataset_instance_list.Max(a => a.feature_data.Count);

            if (program.write_console_log) program.WriteLine($@"{nameof(Main)}: Finished finding empty features... headers: {total_headers}, columns: {total_columns}");
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


