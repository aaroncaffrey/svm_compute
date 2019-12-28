using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using svm_compute;

namespace svm_compute
{

    public static class random_id_generator
    {
        private static readonly object _unique_int_lock = new object();
        private static readonly Random _unique_string_random = new Random();

        private static int _unique_int = 0;
        private static string _unique_string = next_string();

        public static string random_string(int length)
        {
            //var print_params = true;

            //if (print_params)
            //{
                //var param_list = new List<(string key, string value)>() {(nameof(length), length.ToString()),};

                //if (program.write_console_log) program.WriteLine($@"{nameof(random_string)}({string.Join(", ", param_list.Select(a => $"{a.key}=\"{a.value}\"").ToList())});");
            //}

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[_unique_string_random.Next(0, s.Length - 1)]).ToArray());
        }

        public static int next_int()
        {
            
            //var param_list = new List<(string key, string value)>() { };

            //if (program.write_console_log) program.WriteLine($@"{nameof(next_int)}({string.Join(", ", param_list.Select(a => $"{a.key}=\"{a.value}\"").ToList())});");
            
            lock (_unique_int_lock)
            {
                _unique_int++;

                return _unique_int;
            }
        }

        public static string next_string()
        {
            
            //var param_list = new List<(string key, string value)>() { };

            //if (program.write_console_log) program.WriteLine($@"{nameof(next_string)}({string.Join(", ", param_list.Select(a => $"{a.key}=\"{a.value}\"").ToList())});");
            
            lock (_unique_int_lock)
            {
                _unique_string = random_string(5);

                return _unique_string;
            }
        }


        public static string get_unique_id()
        {
            
            //var param_list = new List<(string key, string value)>() { };

            //if (program.write_console_log) program.WriteLine($@"{nameof(get_unique_id)}({string.Join(", ", param_list.Select(a => $"{a.key}=\"{a.value}\"").ToList())});");
            
            return $"{_unique_string}{next_int().ToString().PadLeft(10, '0')}";
        }
    }

}
