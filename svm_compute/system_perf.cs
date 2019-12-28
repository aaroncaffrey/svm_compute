using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using svm_compute;

namespace svm_compute
{
    public static class system_perf
    {
        private static readonly object value_lock = new object();
        private static readonly float[] cpu_values = new float[60];
        private static readonly float[] ram_free_values = new float[60];
        private static int values_index = -1;
        private static (Task, CancellationTokenSource) system_perf_task = make_system_perf_task();

        public static double get_average_cpu_free(int seconds)
        {
            var x = new float[seconds + 1];

            lock (value_lock)
            {
                var j = 0;
                for (var i = values_index; i >= values_index - seconds; i--)
                {
                    var actual_index = values_index > -1 ? values_index : cpu_values.Length + values_index;
                    x[j++] = cpu_values[actual_index];
                }
            }

            var av = 1 - Math.Round(x.Average() / 100, 2);
            return av;
        }

        public static double get_average_ram_free_mb(int seconds)
        {
            var x = new float[seconds + 1];

            lock (value_lock)
            {
                var j = 0;
                for (var i = values_index; i >= values_index - seconds; i--)
                {
                    var actual_index = values_index > -1 ? values_index : cpu_values.Length + values_index;
                    x[j++] = ram_free_values[actual_index];
                }
            }
            var av = Math.Round(x.Average(), 2);
            return av;
        }

        public static (Task, CancellationTokenSource) get_system_perf_task()
        {
            lock (value_lock)
            {
                if (system_perf_task.Item1 == null || system_perf_task.Item2 == null)
                {
                    system_perf_task = make_system_perf_task();
                }
            }

            return system_perf_task;
        }

        public static (Task, CancellationTokenSource) make_system_perf_task()
        {
            var system_perf_task_cancellation_token_source = new CancellationTokenSource();
            var system_perf_task_cancellation_token = system_perf_task_cancellation_token_source.Token;

            var cpu_counter1 = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            var ram_counter1 = new PerformanceCounter("Memory", "Available MBytes");

            cpu_counter1.NextValue();
            ram_counter1.NextValue();


            lock (system_perf.value_lock)
            {
                for (var i = 0; i < 60; i++)
                {
                    cpu_values[i] = cpu_counter1.NextValue();
                    ram_free_values[i] = ram_counter1.NextValue();
                }
            }

            var system_perf_task1 = Task.Run(() =>
            {
                var cpu_counter = cpu_counter1;
                var ram_counter = ram_counter1;
             
                lock (system_perf.value_lock)
                {
                    for (var i = 0; i < 60; i++)
                    {
                        cpu_values[i] = cpu_counter.NextValue();
                        ram_free_values[i] = ram_counter.NextValue();
                    }
                }

                while (true)
                {
                    if (system_perf_task_cancellation_token.IsCancellationRequested)
                    {
                        break;
                    }

                    lock (system_perf.value_lock)
                    {
                        values_index++;
                        if (values_index >= cpu_values.Length) values_index = 0;
                        cpu_values[values_index] = cpu_counter.NextValue();
                        ram_free_values[values_index] = ram_counter.NextValue();
                    }

                    try
                    {
                        
                        if (!system_perf_task_cancellation_token.IsCancellationRequested)
                        {
                            var delay = new TimeSpan(0, 0, 1);

                            //if (program.write_console_log) program.WriteLine($@"make_system_perf_task(): Task.Delay({delay.ToString()}, system_perf_task_cancellation_token).Wait(system_perf_task_cancellation_token);", true, ConsoleColor.Red);

                            Task.Delay(delay, system_perf_task_cancellation_token).Wait(system_perf_task_cancellation_token);
                        }
                    }
                    catch (Exception e)
                    {
                        program.WriteLineException(e, nameof(make_system_perf_task),"", true, ConsoleColor.DarkGray);

                    }
                }
            }, system_perf_task_cancellation_token);

            return (system_perf_task1, system_perf_task_cancellation_token_source);
        }
    }
}
