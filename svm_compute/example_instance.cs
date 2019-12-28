using System;
using System.Collections.Generic;
using System.Linq;
using svm_compute;

namespace svm_compute
{
    public class example_instance
    {
        //public int row;

        

        public readonly int instance_id;

        public readonly List<(int fid, double fv)> feature_list; // = new List<(int fid, double fv)>();
        public readonly List<(string comment_header, string comment_value)> comment_columns;

        public int class_id()
        {
            if (feature_list == null || feature_list.Count == 0) return 0;

            return (int) feature_list.First(a => a.fid == 0).fv;
        }

        
        public example_instance(int instance_id, List<(int fid, double fv)> feature_list, List<(string comment_header, string comment_value)> comment_columns)
        {
            this.instance_id = instance_id;
            this.feature_list = feature_list;
            this.comment_columns = comment_columns;
        }


        public static List<(int fid, double fv_min, double fv_max)> get_scaling_params(List<example_instance> training_rows)
        {

            var print_params = true;

            if (print_params)
            {
                var param_list = new List<(string key, string value)>() {(nameof(training_rows), training_rows.ToString()),};

                if (program.write_console_log) program.WriteLine($@"{nameof(get_scaling_params)}({string.Join(", ", param_list.Select(a => $"{a.key}=\"{a.value}\"").ToList())});");
            }

            var fids = training_rows.First().feature_list.Select(a => a.fid).ToList();

            var feature_vals = fids.Select(fid => (fid, fvs: training_rows.Select(a => double_compat.fix_double(a.feature_list.First(b => b.fid == fid).fv)).ToList())).ToList();

            var scaling_params = feature_vals.Select(a => (a.fid, fv_min: double_compat.fix_double(a.fvs.Min()), fv_max: double_compat.fix_double(a.fvs.Max()))).ToList();

            return scaling_params;
        }

        //public static double compare_examples(example_instance row1, example_instance row2)
        //{
        //    if (row1 == row2) return (double) 0.0;
        //
        //    // fids may not be in correct order
        //    var equality_percentage = (double) row1.feature_list.Select((a, i) => i != 0 && row2.feature_list[i].fv == a.fv).Count(a => a == true) / ((double) row1.feature_list.Count - 1);
        //    //if (equality_percentage >= 0.99m)
        //    //{
        //    //     if (program.write_console_log) program.WriteLine(string.Join(",", row1.feature_list.Select(a=>(a.fid+":"+a.fv).PadLeft(5)).ToList()));
        //    //     if (program.write_console_log) program.WriteLine(string.Join(",", row2.feature_list.Select(a=>(a.fid+":"+a.fv).PadLeft(5)).ToList()));
        //    //     if (program.write_console_log) program.WriteLine();
        //    //}
        //    return equality_percentage;
        //}

        public static (string[] data, string[] comments) feature_list_encode(
            int nest_cv_repeat_index,
            List<example_instance> example_instance_list, 
            //cross_validation.math_operation math_op, 
            cross_validation.scaling_method scaling_method, 
            List<(int fid, double min, double max)> scaling_params,
            bool sparse_format = true)
        {

            var print_params = true;

            if (print_params)
            {
                var param_list = new List<(string key, string value)>()
                {
                    (nameof(nest_cv_repeat_index), nest_cv_repeat_index.ToString()),
                    (nameof(example_instance_list), example_instance_list.ToString()),
                    (nameof(scaling_method), scaling_method.ToString()),
                    (nameof(scaling_params), scaling_params.ToString()),
                    (nameof(sparse_format), sparse_format.ToString()),
                };

                if (program.write_console_log) program.WriteLine($@"{nameof(feature_list_encode)}({string.Join(", ", param_list.Select(a => $"{a.key}=\"{a.value}\"").ToList())});");
            }

            if (scaling_params == null || scaling_params.Count == 0)
            {
                scaling_method = cross_validation.scaling_method.no_scaling;
            }
            
            //var scaled = example_instance_list.Select(example_instance => $"{example_instance.class_id()} {string.Join(" ", example_instance.feature_list.OrderBy(a => a.fid).Skip(1).Select(feat => (fid: feat.fid, fv: scale_value(feat.fid, feat.fv, scaling_method, scaling_params))).Where(a => !sparse_format || a.fv != 0.0).Select(a => $"{a.fid}:{a.fv}").ToList())} #outer_cv_index,{nest_cv_repeat_index};instance_id,{example_instance.instance_id};{string.Join(";", example_instance.comment_columns.Where(g => !g.comment_header.StartsWith("#", StringComparison.InvariantCultureIgnoreCase)).Select(c => $"{c.comment_header},{c.comment_value}").ToList())}").ToArray();
            
            var scaled_data = example_instance_list.Select(example_instance => $"{example_instance.class_id()} {string.Join(" ", example_instance.feature_list.OrderBy(a => a.fid).Skip(1).Select(feat => (fid: feat.fid, fv: scale_value(feat.fid, feat.fv, scaling_method, scaling_params))).Where(a => !sparse_format || a.fv != 0.0).Select(a => $"{a.fid}:{a.fv}").ToList())}").ToArray();
            var comments = example_instance_list.Select(example_instance => $"actual_class_id,{example_instance.class_id()},outer_cv_index,{nest_cv_repeat_index};instance_id,{example_instance.instance_id};{string.Join(";", example_instance.comment_columns.Where(g => !g.comment_header.StartsWith("#", StringComparison.InvariantCultureIgnoreCase)).Select(c => $"{c.comment_header},{c.comment_value}").ToList())}").ToArray();

            return (scaled_data, comments);
        }


        public static double scale_value(int feature_id, double value, cross_validation.scaling_method scaling_method, List<(int fid, double min, double max)> scaling_params)
        {
            if (scaling_method == cross_validation.scaling_method.no_scaling || feature_id <= 0 || scaling_params == null || scaling_params.Count == 0)
            {
                value = double_compat.fix_double(value);

                return value;
            }

            if (scaling_method == cross_validation.scaling_method.square_root)
            {
                value = Math.Sqrt(value);

                value = double_compat.fix_double(value);

                return value;
            }

            var scale_min = 0d;
            var scale_max = 0d;

            if (scaling_method == cross_validation.scaling_method.scale_zero_to_plus_one)
            {
                scale_min = 0d;
                scale_max = 1d;
            }
            else if (scaling_method == cross_validation.scaling_method.scale_minus_one_to_plus_one)
            {
                scale_min = -1d;
                scale_max = +1d;
            }

            var param = scaling_params.Count > feature_id && scaling_params[feature_id].fid == feature_id ? scaling_params[feature_id] : scaling_params.First(c => c.fid == feature_id);
            var col_min = param.min;
            var col_max = param.max;

            value = scale_value(value, col_min, col_max, scale_min, scale_max);

            value = double_compat.fix_double(value);

            return value;

        }

        public static double scale_value(double value, double min, double max)
        {
            var x = double_compat.fix_double(value - min);
            var y = double_compat.fix_double(max - min);

            if (x == 0) return 0;
            if (y == 0) return value;

            var scaled = x / y;

            scaled = double_compat.fix_double(scaled);

            return scaled;
        }
        public static double scale_value(double value, double col_min, double col_max, double scale_min = -1, double scale_max = +1)
        {
            if (scale_min == scale_max || col_min == col_max)
            {
               
            }

            var x = (scale_max - scale_min) * (value - col_min);
            var y = (col_max - col_min);
            var z = scale_min;

            x = double_compat.fix_double(x);
            y = double_compat.fix_double(y);
            z = double_compat.fix_double(z);

            var scaled = (x / y) + z;

            scaled = double_compat.fix_double(scaled);

            return scaled;
        }

        public static double math_op(cross_validation.math_operation data_operation, double value)
        {

            //var param_list = new List<(string key, string value)>()
            //{
            //    (nameof(data_operation),data_operation.ToString()),
            //    (nameof(value),value.ToString()),
            //};

            //if (program.write_console_log) program.WriteLine($@"{nameof(math_op)}({string.Join(", ", param_list.Select(a => $"{a.key}=\"{a.value}\"").ToList())});");


            switch (data_operation)
            {
                case cross_validation.math_operation.none:
                    return value; // changed from break; 

                    
                    
                case cross_validation.math_operation.abs_method:
                    value = Math.Abs(value);

                    break;
                case cross_validation.math_operation.acos_method:
                    value = Math.Acos(value);

                    break;
                case cross_validation.math_operation.asin_method:
                    value = Math.Asin(value);

                    break;
                case cross_validation.math_operation.atan_method:
                    value = Math.Atan(value);

                    break;
                case cross_validation.math_operation.atan2_method:
                    value = Math.Atan2(value, value);

                    break;
                case cross_validation.math_operation.big_mul_method:
                    value = Math.BigMul((int)value, (int)value);

                    break;
                case cross_validation.math_operation.ceiling_method:
                    value = Math.Ceiling(value);

                    break;
                case cross_validation.math_operation.cos_method:
                    value = Math.Cos(value);

                    break;
                case cross_validation.math_operation.cosh_method:
                    value = Math.Cosh(value);

                    break;
                case cross_validation.math_operation.exp_method:
                    value = Math.Exp(value);

                    break;
                case cross_validation.math_operation.floor_method:
                    value = Math.Floor(value);

                    break;
                case cross_validation.math_operation.log_method:
                    value = Math.Log(value);

                    break;
                case cross_validation.math_operation.log10_method:
                    value = Math.Log10(value);

                    break;
                case cross_validation.math_operation.pow_method:
                    value = Math.Pow(value, value);

                    break;
                case cross_validation.math_operation.round_method:
                    value = Math.Round(value);

                    break;
                case cross_validation.math_operation.sign_method:
                    value = Math.Sign(value);

                    break;
                case cross_validation.math_operation.sin_method:
                    value = Math.Sin(value);

                    break;
                case cross_validation.math_operation.sinh_method:
                    value = Math.Sinh(value);

                    break;
                case cross_validation.math_operation.sqrt_method:
                    value = Math.Sqrt(value);

                    break;
                case cross_validation.math_operation.tan_method:
                    value = Math.Tan(value);

                    break;
                case cross_validation.math_operation.tanh_method:
                    value = Math.Tanh(value);

                    break;
                case cross_validation.math_operation.truncate_method:
                    value = Math.Truncate(value);

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(data_operation), data_operation, null);
            }

            value = double_compat.fix_double(value);

            return value;
        }
    }
}
//private static List<(string filename, string[] file_data)> file_cache = new List<(string, string[])>();
        //public int class_id;
        //public List<(int fid, double feature_value)> feature_list;
        //public List<(int fid, bool feature_enabled)> feature_status_list;

        //public example_instance(int class_id, List<(int fid, double feature_value)> feature_list)
        //{
        //    this.class_id = class_id;
        //    this.feature_list = feature_list;
        //    //this.feature_status_list = null;
        //}

        //private example_instance(string line)
        //{
        //    feature_list_decode(line);
        //}

        //public example_instance(example_instance row)
        //{
        //    class_id = row.class_id;
        //    feature_list = row.feature_list?.Select(a => (a.fid, a.feature_value)).ToList();
        //    feature_status_list = row.feature_status_list;
        //}




        //public static void set_feature_status(List<(int fid, bool feature_enabled)> feature_status, int fid, bool status)
        //{
        //    var index = feature_status.FindIndex(a => a.fid == fid);
        //    if (index == -1) throw new Exception("feature does not exist");

        //    feature_status[index] = (fid, status);
        //}




        //public static int get_total_features(List<example_instance> rows)
        //{
        //    return rows.SelectMany(a => a.feature_list.Select(b => b.fid).ToList()).Distinct().OrderBy(a => a).Count();
        //}

        //public static int get_first_fid(List<example_instance> rows)
        //{
        //    return rows.SelectMany(a => a.feature_list.Select(b => b.fid).ToList()).Distinct().OrderBy(a => a).First();
        //}

        //public static int get_last_fid(List<example_instance> rows)
        //{
        //    return rows.SelectMany(a => a.feature_list.Select(b => b.fid).ToList()).Distinct().OrderBy(a => a).Last();
        //}

        //public static int[] get_fids(List<example_instance> rows)
        //{
        //    return rows.SelectMany(a => a.feature_list.Select(b => b.fid).ToList()).Distinct().OrderBy(a => a).ToArray();
        //}

        //public static List<example_instance> LoadFiles(string[] files, bool reload_cache = false)
        //{
        //    foreach (var s in files.Where(a => reload_cache || file_cache.All(b => b.filename != a)))
        //    {
        //        file_cache = file_cache.Where(a => a.filename != s).ToList();
        //        file_cache.Add((s, File.Exists(s) ? File.ReadAllLines(s) : new string[] { }));
        //    }

        //    var data = files.SelectMany(a => file_cache.Last(b => b.filename == a).file_data).ToArray();
        //    var rows = LoadLines(data);
        //    return rows;
        //}

        //public static List<example_instance> LoadLines(string[] lines)
        //{
        //    var rows = lines.Select(a => new example_instance(a)).ToList();
        //    var fids = rows.SelectMany(a => a.feature_list.Select(b => b.fid).ToList()).Distinct().ToList();
        //    var feature_status_list = fids.Select(a => (a, true)).ToList();

        //    foreach (var row in rows) row.feature_status_list = feature_status_list;

        //    return rows;
        //}

        //public void feature_list_decode(string line)
        //{
        //    this.class_id = -1;

        //    var line_split = line.Split();//.ToList();

        //    if (line_split.Length > 0) this.class_id = int.Parse(line_split[0]);

        //    if (line_split.Length <= 1) return; // || line_split.Skip(1).Any(a => !a.Contains(':'))) return;

        //    this.feature_list = line_split.Select((a, i) =>
        //    {
        //        var b = a.Split(':');
        //        return (int.Parse(b[0]), double.Parse(b[1], NumberStyles.Float, CultureInfo.InvariantCulture));
        //    }).ToList();
        //}


//    }
//}
