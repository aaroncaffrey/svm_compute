using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using svm_compute;

namespace svm_compute
{
    public class auto_update
    {
        public static void auto_update_loop(string this_exe, string args, CancellationToken cancellation_token, string update_url = "")
        {
            if (string.IsNullOrWhiteSpace(update_url)) return;

            //update_url = "http://PPRSB1025-C06.kuds.kingston.ac.uk/svm_update.exe"
            var folder = Path.GetDirectoryName(this_exe);

            var this_exe_length = new FileInfo(this_exe).Length;
            var this_exe_checksum = hash.GetSha256Hash(this_exe);
            var update_filename = Path.Combine(folder, "update.exe");

            while (true)
            {
                if (cancellation_token.IsCancellationRequested)
                {
                    break;
                }

                if (program.write_console_log) program.WriteLine($@"Checking for update.");
                try
                {
                    using (var client = new WebClient())
                    {


                        client.DownloadFile(update_url, update_filename);
                        var update_length = File.Exists(update_filename) ? new FileInfo(update_filename).Length : 0;

                        if (update_length > 0)
                        {
                            var update_checksum = hash.GetSha256Hash(update_filename);

                            var update_rename_filename = Path.Combine(folder, $"{update_checksum}.exe");

                            if ((this_exe_length != update_length || update_checksum != this_exe_checksum) && !File.Exists(update_rename_filename))
                            {


                                File.Move(update_filename, update_rename_filename);

                                if (File.Exists(update_rename_filename))
                                {
                                    //todo: execute updated file with parameters

                                    var psi = new ProcessStartInfo() { FileName = update_rename_filename, Arguments = args };

                                    try
                                    {
                                        Process current = Process.GetCurrentProcess();
                                        var processes_before_execute = Process.GetProcessesByName(current.ProcessName).Where(a => a.Id != current.Id).ToList();

                                        var p = Process.Start(psi);

                                        if (p != null)
                                        {

                                            foreach (var process in processes_before_execute)
                                            {
                                                process.Kill();
                                            }


                                            Process.GetCurrentProcess().Kill();
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        program.WriteLineException(e, nameof(auto_update_loop));
                                    }

                                    //todo: kill current process
                                }

                            }
                        }

                    }
                }
                catch (Exception)// e)
                {
                    if (program.write_console_log) program.WriteLine($@"No update found.");
                }


                try
                {
                    if (!cancellation_token.IsCancellationRequested)
                    {
                        var delay = new TimeSpan(0, 0, 15, 0);

                        if (program.write_console_log) program.WriteLine($@"Task.Delay({delay}, cancellation_token).Wait(cancellation_token);", true, ConsoleColor.Red);

                        Task.Delay(delay, cancellation_token).Wait(cancellation_token);
                    }
                }
                catch (Exception e)
                {
                    program.WriteLineException(e, nameof(auto_update_loop));
                    
                }
            }
        }

    }
}
