using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Security.Cryptography;

namespace svm_compute
{
    public class cross_validation_remote
    {
        public static readonly object keep_alive_list_lock = new object();
        public static List<(TcpClient client, Task task, CancellationTokenSource cancellation_token_source, CancellationToken cancellation_token)> keep_alive_list = new List<(TcpClient client, Task task, CancellationTokenSource cancellation_token_source, CancellationToken cancellation_token)>();

        //public static readonly object compute_unit_hostnames_lock = new object();

        //public static List<(string hostname, List<int> ports)> compute_unit_hostnames = null;

        public static readonly object compute_loop_clients_list_lock = new object();
        public static readonly object master_loop_clients_list_lock = new object();

        public static List<TcpClient> authenticated_clients = new List<TcpClient>();
        public static List<(TcpClient client, NetworkStream stream)> compute_loop_clients = new List<(TcpClient client, NetworkStream stream)>();
        public static List<(TcpClient client, NetworkStream stream)> master_loop_clients = new List<(TcpClient client, NetworkStream stream)>();

        public static List<int> default_server_ports = new List<int>() { 843, 3121, 4155 };

        public static int max_tcp_clients = Environment.ProcessorCount * 20;

        public static bool log_tcp_traffic = false;

        public static readonly object authentication_lock = new object();
        public static readonly object cache_lock = new object();

        private static readonly object send_run_svm_request_id_lock = new object();
        private static int send_run_svm_request_id = -1;

        public static string get_client_ip(TcpClient client)
        {
            var ip = "";

            try
            {
                ip = ((IPEndPoint)client?.Client?.RemoteEndPoint)?.Address?.ToString() ?? "";
            }
            catch (ObjectDisposedException)
            {

            }
            catch (Exception e)
            {
                program.WriteLineException(e, nameof(get_client_ip),"", true, ConsoleColor.DarkGray);
            }
   
            return ip;
        }



        public static List<int> get_listen_ports()
        {
            var load_hosts = program.load_hosts();

            // find local port to listen on
            var port_search_addresses = new List<string>();

            var local_hostname = Dns.GetHostName();
            var local_hostname_full = Dns.GetHostEntry("").HostName;

            var serverName = Environment.MachineName; //host name sans domain
            var fqhn = Dns.GetHostEntry(serverName).HostName; //fully qualified hostname

            var local_ips = Dns.GetHostAddresses(local_hostname).Select(a => a.ToString()).ToList();

            port_search_addresses.Add(local_hostname);
            port_search_addresses.Add(local_hostname_full);
            port_search_addresses.Add(serverName);
            port_search_addresses.Add(fqhn);
            port_search_addresses.AddRange(local_ips);

            var listen_ports = cross_validation_remote.default_server_ports.ToList();

            if (load_hosts != null && load_hosts.Count > 0)
            {
                var p_index = load_hosts.FindIndex(a => port_search_addresses.Any(b => a.hostname.Equals(b, StringComparison.InvariantCultureIgnoreCase)));
                if (p_index > -1)
                {
                    if (load_hosts[p_index].ports != null && load_hosts[p_index].ports.Count > 0)
                    {
                        listen_ports = load_hosts[p_index].ports;
                    }
                }
            }

            listen_ports = listen_ports.Where(a => a > UInt16.MinValue && a <= UInt16.MaxValue).OrderBy(a => a).Distinct().ToList();

            if (listen_ports == null || listen_ports.Count == 0)
            {
                listen_ports = cross_validation_remote.default_server_ports.ToList();
            }

            return listen_ports;
        }

        public static readonly object get_next_incoming_connection_lock = new object();

        public static (TcpClient, NetworkStream) get_next_connection_incoming(CancellationToken cancellation_token)
        {
            // lock so that more than one call doesn't result in ports being unavailable due to already being in use
            lock (get_next_incoming_connection_lock)
            {
                var ports = get_listen_ports();
                var cancel = new CancellationTokenSource();
                var task_cancel_token = cancel.Token;

                var server_tasks = new List<Task<(TcpClient, NetworkStream)>>();

                var client_accepted = false;
                var client_accepted_lock = new object();

                foreach (var x_port in ports)
                {
                    var listen_ip = IPAddress.Any;
                    var port = x_port;

                    var server_task = Task.Run(() =>
                    {
                        while (true)
                        {
                            if (cancellation_token.IsCancellationRequested || task_cancel_token.IsCancellationRequested)
                            {
                                return (null, null);
                            }


                            TcpClient client = null;
                            NetworkStream client_stream = null;
                            TcpListener listener = null;

                            try
                            {
                                listener = new TcpListener(listen_ip, port);

                                listener.Start();

                                while (client == null)
                                {
                                    if (cancellation_token.IsCancellationRequested || task_cancel_token.IsCancellationRequested)
                                    {
                                        client_stream?.Close();
                                        client?.Close();

                                        client_stream = null;
                                        client = null;

                                        listener?.Stop();
                                        listener = null;

                                        return (null, null);
                                    }

                                    lock (client_accepted_lock)
                                    {
                                        if (!client_accepted && listener.Pending())
                                        {
                                            client = listener.AcceptTcpClient();

                                            if (client.is_connected())
                                            {
                                                // client connected properly

                                                client_stream = client.GetStream();

                                                client.NoDelay = true;

                                                client_accepted = true;

                                                listener?.Stop();
                                                listener = null;

                                                return (client, client_stream);
                                            }
                                            else
                                            {
                                                // client didn't connect properly or disconnected
                                                client_stream?.Close();
                                                client?.Close();

                                                client_stream = null;
                                                client = null;


                                            }
                                        }
                                        else if (client_accepted)
                                        {
                                            listener?.Stop();
                                            listener = null;

                                            return (null, null);
                                        }
                                    }

                                    Task.Delay(new TimeSpan(0, 0, 2), task_cancel_token).Wait(task_cancel_token);

                                }
                            }
                            catch (SocketException se)
                            {
                                if (se.SocketErrorCode == SocketError.AddressAlreadyInUse)
                                {
                                    //System.Net.Sockets.SocketException (0x80004005): Only one usage of each socket address (protocol/network address/port) is normally permitted
                                    if (program.write_console_log) program.WriteLine(se.ToString());

                                    try
                                    {
                                        Task.Delay(new TimeSpan(0, 0, 2), task_cancel_token).Wait(task_cancel_token);
                                    }
                                    catch (Exception e)
                                    {
                                        program.WriteLineException(e, nameof(get_next_connection_incoming));
                                    }
                                }

                            }
                            catch (Exception e)
                            {
                                program.WriteLineException(e, nameof(get_next_connection_incoming));
                                continue;
                            }
                            finally
                            {
                                try
                                {
                                    lock (client_accepted_lock)
                                    {
                                        listener?.Stop();
                                        listener = null;
                                    }
                                }
                                catch (Exception e)
                                {
                                    program.WriteLineException(e, nameof(get_next_connection_incoming));
                                }
                            }

                            if (client != null && client_stream != null)
                            {
                                return (client, client_stream);
                            }
                        }
                    }, task_cancel_token);
                    server_tasks.Add(server_task);
                }

                do
                {
                    var complete_server_tasks = server_tasks.Where(a => a.IsCompleted && a.Result != (null, null)).ToList();

                    if (complete_server_tasks != null && complete_server_tasks.Count > 0)
                    {

                        var first_connection = complete_server_tasks.FirstOrDefault()?.Result ?? (null, null);

                        try
                        {
                            cancel.Cancel();
                        }
                        catch (Exception e)
                        {
                            program.WriteLineException(e, nameof(get_next_connection_incoming));
                        }

                        try
                        {
                            Task.WaitAll(server_tasks.ToArray<Task>());
                        }
                        catch (Exception e)
                        {
                            program.WriteLineException(e, nameof(get_next_connection_incoming));
                        }

                        return first_connection;
                    }
                    else
                    {
                        var incomplete_tasks = server_tasks.Except(complete_server_tasks).ToList();

                        if (incomplete_tasks != null && incomplete_tasks.Count > 0)
                        {
                            try
                            {
                                Task.WaitAny(incomplete_tasks.ToArray<Task>());
                            }
                            catch (Exception e)
                            {
                                program.WriteLineException(e, nameof(get_next_connection_incoming));
                            }
                        }
                        else
                        {
                            return (null, null);
                        }
                    }
                } while (true);

            }
        }

        public static void compute_loop(CancellationToken slave_loop_cancellation_token)
        {
            // start performance monitoring
            var spt = system_perf.get_system_perf_task();

            var local_hostname_full = Dns.GetHostEntry("").HostName;


            var tasks = new List<Task>();
            if (program.write_console_log) program.WriteLine($@"{nameof(compute_loop)}(): [{local_hostname_full}] Server: SVM Compute Unit. Hostname: {local_hostname_full}.");
            //List<TcpListener> servers = null;
            //var listen_ip = IPAddress.Any;

            var task_id = 0UL;

            var min_free_cpu = 0.25;
            var min_free_ram = 2048;

            while (true)
            {
                program.GC_Collection();

                if (slave_loop_cancellation_token.IsCancellationRequested)
                {
                    break;
                }

                var incomplete_tasks = tasks.Where(a => !a.IsCompleted).ToList();

                while (max_tcp_clients > 0 && incomplete_tasks.Count >= max_tcp_clients)
                {
                    if (program.write_console_log) program.WriteLine($@"{nameof(compute_loop)}(): [{local_hostname_full}] Server: Resource shortage. Waiting for tasks to complete before continuing. Task.WaitAny(tasks.ToArray<Task>());", true, ConsoleColor.Cyan);

                    try
                    {
                        Task.WaitAny(incomplete_tasks.ToArray<Task>());
                    }
                    catch (Exception)
                    {

                    }

                    incomplete_tasks = tasks.Where(a => !a.IsCompleted).ToList();
                }

                task_id++;

                tasks = tasks.Where(a => !a.IsCompleted).ToList();

                double free_cpu;
                double free_ram;

                do
                {
                    free_cpu = system_perf.get_average_cpu_free(10);
                    free_ram = system_perf.get_average_ram_free_mb(5);

                    if (free_cpu < min_free_cpu || free_ram < min_free_ram)
                    {
                        if (program.write_console_log) program.WriteLine($@"{nameof(compute_loop)}(): [{local_hostname_full}] Server: Resource shortage. Waiting for free resources before continuing... Free CPU = {free_cpu:0.00}, Free RAM = {free_ram}, Task ID = {task_id}.");
                    }

                    try
                    {
                        if (!slave_loop_cancellation_token.IsCancellationRequested)
                        {
                            var delay = new TimeSpan(0, 0, 5);

                            if (program.write_console_log) program.WriteLine($@"{nameof(compute_loop)}(): [{local_hostname_full}] Server: Task.Delay({delay.ToString()}, slave_loop_cancellation_token).Wait(slave_loop_cancellation_token);", true, ConsoleColor.Red);
                            Task.Delay(delay, slave_loop_cancellation_token).Wait(slave_loop_cancellation_token);
                        }
                    }
                    catch (Exception e)
                    {
                        program.WriteLineException(e,nameof(compute_loop),"", true, ConsoleColor.DarkGray);

                    }

                } while (free_cpu < min_free_cpu || free_ram < min_free_ram);

                

                var _task_id = task_id;

                var (client, stream) = get_next_connection_incoming(slave_loop_cancellation_token);

                if (client == null || stream == null)
                {
                    continue;
                }

                lock (compute_loop_clients_list_lock)
                {
                    compute_loop_clients.Add((client, stream));
                }

                var client_ip = get_client_ip(client);

                client_keep_alive(client, stream);

                var task = Task.Run(() =>
                {
                    var client_cancellation_token_source = new CancellationTokenSource();
                    var client_cancellation_token = client_cancellation_token_source.Token;

                    try
                    {
                            // read loop until closed

                            var read_loop_result = read_loop(client, stream, null, client_cancellation_token);

                        close_client(client, stream);
                    }
                    catch (Exception)
                    {
                        //program.WriteLineException(e, nameof(compute_loop), client_ip);
                    }

                    try
                    {
                        if (!client_cancellation_token_source.IsCancellationRequested)
                        {
                            client_cancellation_token_source.Cancel();
                        }
                    }
                    catch (Exception)
                    {
                        //program.WriteLineException(e, nameof(compute_loop), client_ip);
                    }

                    lock (compute_loop_clients_list_lock)
                    {
                        compute_loop_clients = compute_loop_clients.Where(a => a.client != client).ToList();
                    }

                    if (program.write_console_log) program.WriteLine($@"{client_ip}: {nameof(compute_loop)}(): [{local_hostname_full}] Server: incoming-connection task exiting. Task ID = {_task_id}.");
                }, slave_loop_cancellation_token);
                tasks.Add(task);

            }
        }

        public static List<(string header, string text)> get_next_message(ref string messages, TcpClient client, NetworkStream stream)
        {
            var client_ip = get_client_ip(client);

            // 01heading2text30

            if (!messages.Contains((char)0)) return null;
            if (!messages.Contains((char)1)) return null;
            if (!messages.Contains((char)2)) return null;
            if (!messages.Contains((char)3)) return null;

            var last_end_message = messages.LastIndexOf((char)3);

            var messages_split = messages.Split(new char[] { (char)0 }, StringSplitOptions.RemoveEmptyEntries);

            messages = messages.Substring(last_end_message + 1);

            var msg_parsed = new List<(string header, string text)>();
            foreach (var msg in messages_split)
            {
                if (msg.Length < 3) continue;

                var index_header = msg.IndexOf((char)1);
                var index_text = msg.IndexOf((char)2);
                var index_end = msg.IndexOf((char)3);

                if (index_header == -1 || index_text == -1 || index_end == -1) continue;
                if (index_header != 0 || index_end != msg.Length - 1) continue;

                var header = msg.Substring(index_header + 1, (index_text - index_header) - 1);
                var text = msg.Substring(index_text + 1, (index_end - index_text) - 1);

                if (string.IsNullOrWhiteSpace(header) || string.IsNullOrWhiteSpace(text)) continue;

                msg_parsed.Add((header, text));
            }

            return msg_parsed;
        }



        public static bool is_authenticated(TcpClient client)
        {
            lock (authentication_lock)
            {
                return authenticated_clients.Contains(client);
            }
        }

        public static string Hash(string stringToHash)
        {
            using (var sha1 = new SHA1Managed())
            {
                return BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(stringToHash))).Replace("-", "");
            }
        }



        public static void cache_request_response(string request, string response)
        {
            if (!program.svm_cache_save) return;

            var hash1 = "";

            try
            {
                hash1 = Hash(request.Trim());
                var folder = @"c:\svm_compute\cache\";
                //Directory.CreateDirectory(folder);
                hash1 = Hash(request.Trim());
                var filename = hash1;
                //var f_request = Path.Combine(folder, "request", $"request_{filename}.txt");
                var f_response = program.convert_path(Path.Combine(folder, "response", $"response_{filename}.txt"));

                lock (cache_lock)
                {
                    // note: caching the request takes up a lot of disk space (probably due to the large dataset size and feature descriptions plus meta data)

                    program.WriteAllText(f_response, response);
                }

                if (program.write_console_log) program.WriteLine($@"cache_request_response(): saved response to cache, hash={hash1}");
            }
            catch (Exception e)
            {
                program.WriteLineException(e, nameof(cache_request_response), $@"could not cache response, hash={hash1}");

            }
        }

        public static string get_cached_response(string request)
        {
            if (!program.svm_cache_load) return null;

            var cache_found = false;
            var cache_loaded = false;
            var hash1 = "";

            try
            {
                var folder = @"c:\svm_compute\cache\";
                //Directory.CreateDirectory(folder);
                hash1 = Hash(request.Trim());
                var filename = hash1;

                //var f_request = Path.Combine(folder, "request", $"request_{filename}.txt");
                var f_response = program.convert_path(Path.Combine(folder, "response", $"response_{filename}.txt"));

                if (!string.IsNullOrWhiteSpace(f_response))
                {
                    if (File.Exists(f_response) && new FileInfo(f_response).Length > 0)
                    {
                        cache_found = true;

                        lock (cache_lock)
                        {
                            var data = File.ReadAllText(f_response);

                            var test = cross_validation.run_svm_return.deserialise(data);

                            if (test != null && test.run_svm_return_data != null && test.run_svm_return_data.Count > 0)
                            {
                                cache_loaded = true;

                                return data;
                            }
                        }
                    }
                }

            }
            catch (Exception)
            {

            }
            finally
            {
                if (program.write_console_log) program.WriteLine($@"get_cached_response(): hash={hash1}, cache_found={cache_found}, cache_loaded={cache_loaded}");
            }

            return null;

        }


        public static (bool exit, cross_validation.run_svm_return run_svm_return) process_messages(List<(string header, string text)> msg_parsed, TcpClient client, NetworkStream stream, CancellationToken cancellation_token)
        {
            var client_ip = get_client_ip(client);

            foreach (var msg in msg_parsed)
            {
                if (cancellation_token.IsCancellationRequested)
                {
                    close_client(client, stream);
                    return (true, null);
                }

                var is_master_client = false;
                lock (master_loop_clients_list_lock) { is_master_client = master_loop_clients.Any(a => a.client == client); }

                var is_compute_client = false;
                lock (compute_loop_clients_list_lock) { is_compute_client = compute_loop_clients.Any(a => a.client == client); }

                var is_authenticated = cross_validation_remote.is_authenticated(client);

                if (program.write_console_log) program.WriteLine($@"{client_ip}: {client.Client.Handle} RECEIVED from {(is_master_client ? "master" : "")}{(is_compute_client ? "compute" : "")} {(is_authenticated ? "authed" : "unauthed")} client: {msg.header} --> {(string.IsNullOrWhiteSpace(msg.text) ? "" : msg.text.Substring(0, msg.text.Length >= 1024 ? 1024 : msg.text.Length))}...");

                if (!is_authenticated)
                {
                    switch (msg.header)
                    {
                        case "AUTH_REQUEST":

                            send_message("AUTH_RESPONSE", "0", client, stream);

                            continue;

                        case "AUTH_RESPONSE":

                            lock (authentication_lock)
                            {
                                if (!authenticated_clients.Contains(client))
                                {
                                    authenticated_clients.Add(client);
                                }
                            }

                            continue;

                        default:

                            close_client(client, stream);
                            return (true, null);

                    }
                }
                else
                {
                    switch (msg.header)
                    {
                        case "CLOSE":
                            send_message("CLOSE_RECEIVED", "0", client, stream);
                            try
                            {
                                if (!cancellation_token.IsCancellationRequested)
                                {
                                    var delay = new TimeSpan(0, 0, 5);
                                    if (program.write_console_log) program.WriteLine($@"{client_ip}: process_messages(): Task.Delay({delay.ToString()}, cancellation_token).Wait(cancellation_token);", true, ConsoleColor.Red);
                                    Task.Delay(delay, cancellation_token).Wait(cancellation_token);
                                }
                            }
                            catch (Exception e)
                            {
                                program.WriteLineException(e, nameof(process_messages),"", true, ConsoleColor.DarkGray);

                            }

                            close_client(client, stream);
                            return (true, null);

                        case "CLOSE_RECEIVED":
                            close_client(client, stream);
                            return (true, null);

                        case "SVM_REQUEST":
                            send_message("SVM_REQUEST_RECEIVED", "0", client, stream);

                            var request_json = msg.text;

                            var cache = get_cached_response(request_json);

                            if (!string.IsNullOrWhiteSpace(cache))
                            {
                                send_message("SVM_RESPONSE", cache, client, stream);

                                break;
                            }

                            var request_svm_params = run_svm_params.deserialise(request_json);

                            if (request_svm_params == null)
                            {
                                if (program.write_console_log) program.WriteLine($@"{nameof(process_messages)}(): cross_validation.run_svm_params.deserialise(request_json) == NULL", true, ConsoleColor.Red);
                                close_client(client, stream);
                                break;
                            }

                            request_svm_params.run_remote = false;

                            send_message("SVM_REQUEST_RECEIVED", "1", client, stream);

                            var inner_cv_svm_implementation = program.inner_cv_svm_implementation;
                            var outer_cv_svm_implementation = program.outer_cv_svm_implementation;

                            var request_svm_return = cross_validation.run_svm(request_svm_params, cancellation_token, inner_cv_svm_implementation, outer_cv_svm_implementation);

                            if (request_svm_return != null && request_svm_return.run_svm_return_data != null && request_svm_return.run_svm_return_data.Count > 0)
                            {
                                var request_svm_return_json_serialised = cross_validation.run_svm_return.serialise(request_svm_return);

                                // cache on the local svm compute unit
                                cache_request_response(request_json, request_svm_return_json_serialised);

                                send_message("SVM_RESPONSE", request_svm_return_json_serialised, client, stream);
                            }
                            else
                            {
                                send_message("SVM_RESPONSE_FAILED", "0", client, stream);
                            }

                            break;

                        case "SVM_RESPONSE_FAILED":
                            close_client(client, stream);
                            return (true, null);

                        case "SVM_REQUEST_RECEIVED":
                            break;

                        case "SVM_RESPONSE":
                            send_message("SVM_RESPONSE_RECEIVED", "0", client, stream);

                            //var svm_req = "";
                            var svm_response = msg.text;

                            var response_run_svm_return = cross_validation.run_svm_return.deserialise(svm_response);

                            if (response_run_svm_return == null)
                            {
                                if (program.write_console_log) program.WriteLine($@"{nameof(process_messages)}(): cross_validation.run_svm_return.deserialise(svm_response) == NULL", true, ConsoleColor.Red);
                                close_client(client, stream);
                                return (true, null);
                            }
                            else
                            {
                                send_message("SVM_RESPONSE_RECEIVED", "1", client, stream);
                                close_client(client, stream);

                                return (true, response_run_svm_return);
                            }


                        case "SVM_RESPONSE_RECEIVED":
                            if (msg.text == "1")
                            {
                                close_client(client, stream);
                                return (true, null);
                            }
                            break;

                        default:
                            if (program.write_console_log) program.WriteLine($@"{client_ip}: unknown command: " + msg.header + " " + msg.text);
                            close_client(client, stream);
                            return (true, null);
                    }
                }
            }

            return (false, null);
        }

        public void stream_write()
        {

        }

        public static void send_auth_request(TcpClient client, NetworkStream stream)
        {
            send_message("AUTH_REQUEST", "0", client, stream);
        }


        public static void send_message(string header, string text, TcpClient client, NetworkStream stream)
        {
            var client_ip = get_client_ip(client);


            var is_master_client = false;
            lock (master_loop_clients_list_lock) { is_master_client = master_loop_clients.Any(a => a.client == client); }

            var is_compute_client = false;
            lock (compute_loop_clients_list_lock) { is_compute_client = compute_loop_clients.Any(a => a.client == client); }


            var is_authenticated = cross_validation_remote.is_authenticated(client);


            if (program.write_console_log) program.WriteLine($@"{client_ip}: {client.Client.Handle} SEND to {(is_master_client ? "master" : "")}{(is_compute_client ? "compute" : "")} {(is_authenticated ? "authed" : "unauthed")} client: {header} --> {(string.IsNullOrWhiteSpace(text) ? "" : text.Substring(0, text.Length >= 1024 ? 1024 : text.Length))}...");

            //if (program.write_console_log) program.WriteLine($@"[{client_ip}] 501 Sending message");
            if (!client.is_connected())
            {
                close_client(client, stream);
                return;
            }



            var sb = new StringBuilder();
            sb.Append((char)0);
            sb.Append((char)1);
            sb.Append(header);
            sb.Append((char)2);
            sb.Append(text);
            sb.Append((char)3);
            sb.Append((char)0);
            var msg = sb.ToString();

            try
            {
                var prev_no_delay = client.NoDelay;
                var prev_read_timeout = stream.ReadTimeout;
                var prev_write_timeout = stream.WriteTimeout;

                try
                {


                    client.NoDelay = true;
                    stream.ReadTimeout = (int)program.tcp_stream_read_timeout.TotalMilliseconds;
                    stream.WriteTimeout = (int)program.tcp_stream_write_timeout.TotalMilliseconds;


                    var msg_bytes = Encoding.ASCII.GetBytes(msg);

                    if (program.write_console_log) program.WriteLine($@"{client_ip}: send_message(): stream.WriteAsync(msg_bytes, 0, msg_bytes.Length).Wait();");
                    stream.WriteAsync(msg_bytes, 0, msg_bytes.Length).Wait();
                    if (log_tcp_traffic)
                    {
                        program.AppendAllBytes(program.convert_path($@"c:\svm_compute\tcp_log\{program.program_start_time}\{program.program_start_time}_{program.master_or_compute}_tcp_write_ip_{client_ip}.log"), msg_bytes);
                    }

                    if (program.write_console_log) program.WriteLine($@"{client_ip}: send_message(): stream.FlushAsync().Wait();");
                    stream.FlushAsync().Wait();
                }
                catch (Exception e)
                {
                    program.WriteLineException(e, nameof(send_message), client_ip);
                    close_client(client, stream);
                }
                finally
                {
                    try
                    {
                        client.NoDelay = prev_no_delay;
                        stream.ReadTimeout = prev_read_timeout;
                        stream.WriteTimeout = prev_write_timeout;
                    }
                    catch (Exception e)
                    {
                        program.WriteLineException(e, nameof(send_message), client_ip);
                    }
                }

            }
            catch (Exception e)
            {
                program.WriteLineException(e, nameof(send_message), client_ip);
            }
        }

        public static void close_client(TcpClient client, NetworkStream stream)
        {
            var client_ip = get_client_ip(client);

            if (program.write_console_log) program.WriteLine($@"{client_ip}: close_client()");

            if (stream != null)
            {
                try
                {
                    stream?.Close();
                }
                catch (Exception)
                {

                }
            }

            if (client != null)
            {
                try
                {
                    client?.Close();
                }
                catch (Exception)
                {

                }
            }

            lock (authentication_lock)
            {
                if (authenticated_clients != null && authenticated_clients.Count > 0)
                {
                    //var c_before = authenticated_clients.Count;
                    authenticated_clients = authenticated_clients.Where(a => a != client).ToList();
                    //var c_after = authenticated_clients.Count;

                    //var removed = c_after < c_before;// authenticated_clients.Remove(client);
                    //if (!removed) { if (program.write_console_log) program.WriteLine($@"{client_ip}: close_client(): could not remove authentication token (not found)."); }
                }
            }

            lock (compute_loop_clients_list_lock)
            {
                try
                {
                    //var c_before = compute_loop_clients.Count;
                    compute_loop_clients = compute_loop_clients.Where(a => a.client != client).ToList();
                    //var c_after = compute_loop_clients.Count;

                    //var removed = c_after < c_before;// incoming_connections_list.Remove((client,stream));// = incoming_master_clients.Where(a => a != client).ToList();
                    //if (!removed) { if (program.write_console_log) program.WriteLine($@"{client_ip}: close_client(): could not remove incoming connection (not found)."); }
                }
                catch (Exception e)
                {
                    program.WriteLineException(e, nameof(close_client), client_ip);
                }
            }

            lock (master_loop_clients_list_lock)
            {
                try
                {
                    //var c_before = master_loop_clients.Count;
                    master_loop_clients = master_loop_clients.Where(a => a.client != client).ToList();
                    //var c_after = master_loop_clients.Count;

                    //var removed = c_after < c_before;// outgoing_connections_list.Remove((client,stream));// = outgoing_connections_list.Where(a => a.client != client).ToList();
                    //if (!removed) { if (program.write_console_log) program.WriteLine($@"{client_ip}: close_client(): could not remove outgoing connection (not found)."); }
                }
                catch (Exception e)
                {
                    program.WriteLineException(e, nameof(close_client), client_ip);
                }
            }

            lock (keep_alive_list_lock)
            {
                var keep_alive = keep_alive_list.Where(a => ReferenceEquals(a.client, client)).ToList();
                keep_alive_list = keep_alive_list.Except(keep_alive).ToList();

                if (keep_alive != null && keep_alive.Count > 0)
                {
                    foreach (var k in keep_alive)
                    {
                        if ((k.cancellation_token != null && !k.cancellation_token.IsCancellationRequested) || (k.cancellation_token_source != null && !k.cancellation_token_source.IsCancellationRequested))
                        {
                            k.cancellation_token_source?.Cancel();
                        }
                    }
                }
            }
        }


        public static (Task keep_alive_task, CancellationTokenSource keep_alive_cancellation_source, CancellationToken keep_alive_cancellation_token) client_keep_alive(TcpClient client, NetworkStream stream)
        {
            var client_ip = get_client_ip(client);

            var keep_alive_cancellation_source = new CancellationTokenSource();
            var keep_alive_cancellation_token = keep_alive_cancellation_source.Token;

            var keep_alive_task = Task.Run(() =>
            {
                //var keep_alive_cancellation_source1 = keep_alive_cancellation_source;
                //var keep_alive_cancellation_token1 = keep_alive_cancellation_token;

                //var client_ip = get_client_ip(client);

                while (true)
                {
                    if (keep_alive_cancellation_token.IsCancellationRequested)
                    {
                        break;
                    }

                    try
                    {
                        if (client.is_connected())
                        {
                            var buffer = new byte[1];
                            buffer[0] = 22;

                            if (program.write_console_log) program.WriteLine($@"{client_ip}: keep_alive_task(): stream.WriteAsync(buffer, 0, buffer.Length, keep_alive_cancellation_token).Wait(keep_alive_cancellation_token);");
                            stream.WriteAsync(buffer, 0, buffer.Length, keep_alive_cancellation_token).Wait(keep_alive_cancellation_token);
                            if (log_tcp_traffic)
                            {
                                program.AppendAllBytes(program.convert_path($@"c:\svm_compute\tcp_log\{program.program_start_time}\{program.program_start_time}_{program.master_or_compute}_tcp_write_ip_{client_ip}.log"), buffer);
                            }

                            if (program.write_console_log) program.WriteLine($@"{client_ip}: keep_alive_task(): stream.FlushAsync(keep_alive_cancellation_token).Wait(keep_alive_cancellation_token);");
                            stream.FlushAsync(keep_alive_cancellation_token).Wait(keep_alive_cancellation_token);
                        }
                        else
                        {
                            close_client(client, stream);
                            break;
                        }

                    }
                    catch (Exception e)
                    {
                        var lvl = 0;

                        do
                        {
                            if (program.write_console_log) program.WriteLine($@"{client_ip}: keep_alive_task(): {lvl} {e.Source} {e.TargetSite} {e.StackTrace}");
                            lvl++;
                            e = e.InnerException;
                        } while (e != null);

                        close_client(client, stream);
                        break;
                    }

                    try
                    {
                        if (!keep_alive_cancellation_token.IsCancellationRequested)
                        {
                            var keep_alive_delay = new TimeSpan(0, 0, 30);

                            if (program.write_console_log) program.WriteLine($@"{client_ip}: keep_alive_task(): Task.Delay({keep_alive_delay.ToString()}, keep_alive_cancellation_token).Wait(keep_alive_cancellation_token);", true, ConsoleColor.Red);
                            Task.Delay(keep_alive_delay, keep_alive_cancellation_token).Wait(keep_alive_cancellation_token);
                        }
                    }
                    catch (Exception e)
                    {
                        program.WriteLineException(e, nameof(client_keep_alive),"", true, ConsoleColor.DarkGray);
                        close_client(client, stream);

                        break;
                    }
                }
            }, keep_alive_cancellation_token);

            lock (keep_alive_list_lock)
            {
                keep_alive_list.Add((client, keep_alive_task, keep_alive_cancellation_source, keep_alive_cancellation_token));
            }

            return (keep_alive_task, keep_alive_cancellation_source, keep_alive_cancellation_token);
        }

        //public static void connection_event(string conn_event, TcpClient client, NetworkStream stream, compute_unit_health_info slave_health)
        //{
        //    if (slave_health == null) return;
        //
        //    lock (compute_unit_health_info_list_lock)
        //    {
        //        switch (conn_event)
        //        {
        //            case "connection_attempt":
        //                slave_health.total_connection_attempts++;
        //                break;
        //        }
        //    }
        //}

        public static cross_validation.run_svm_return read_loop(TcpClient client, NetworkStream stream, List<(string header, string text)> messages_to_send, CancellationToken cancellation_token)
        {
            var client_ip = get_client_ip(client);

            var message_str = "";
            var message_buffer = new StringBuilder();
            var time_last_read = DateTime.Now;
            var bytes_read_overall = 0;
            var total_messages_overall = 0;
            var time_last_message = DateTime.Now;

            if (client.is_connected())
            {
                send_auth_request(client, stream);
            }
            else
            {
                close_client(client, stream);
                return null;
            }

            while (client.is_connected())
            {
                if (cancellation_token.IsCancellationRequested)
                {
                    return null;
                }

                var bytes_read_total = 0;

                while (stream.DataAvailable)
                {
                    if (cancellation_token.IsCancellationRequested)
                    {
                        return null;
                    }

                    var buffer = new byte[1024 * 10];
                    var bytes_read = stream.Read(buffer, 0, buffer.Length);
                    if (log_tcp_traffic)
                    {
                        program.AppendAllBytes(program.convert_path($@"c:\svm_compute\tcp_log\{program.program_start_time}\{program.program_start_time}_{program.master_or_compute}_tcp_read_ip_{client_ip}.log"), buffer.Take(bytes_read).ToArray());
                    }

                    var buffer_str = Encoding.ASCII.GetString(buffer, 0, bytes_read);

                    if (bytes_read > 0)
                    {
                        bytes_read_total += bytes_read;
                        bytes_read_overall += bytes_read;

                        time_last_read = DateTime.Now;

                        message_buffer.Append(buffer_str);
                    }
                }

                if (bytes_read_overall >= 1024 && total_messages_overall == 0)
                {
                    close_client(client, stream);
                    break;
                }

                if (bytes_read_total > 0)
                {
                    var new_msg = message_buffer.ToString();
                    new_msg = new_msg.Replace("" + (char)22, "");
                    message_str += new_msg;
                    message_buffer.Clear();

                    var msg_parsed = get_next_message(ref message_str, client, stream);


                    if (msg_parsed != null && msg_parsed.Count > 0)
                    {
                        time_last_message = DateTime.Now;

                        total_messages_overall += msg_parsed.Count;

                        var process_messages_result = process_messages(msg_parsed, client, stream, cancellation_token);

                        if (process_messages_result.run_svm_return != null)
                        {
                            return process_messages_result.run_svm_return;
                        }

                        if (process_messages_result.exit)
                        {
                            return null;
                        }

                        if (is_authenticated(client))
                        {
                            while (messages_to_send != null && messages_to_send.Count > 0)
                            {
                                var x = messages_to_send.First();
                                messages_to_send = messages_to_send.Skip(1).ToList();
                                send_message(x.header, x.text, client, stream);
                            }
                        }
                    }
                }
                else
                {
                    var time_now = DateTime.Now;
                    var time_since_last_read = time_now.Subtract(time_last_read);
                    var time_since_last_message = time_now.Subtract(time_last_message);




                    var data_received_timeout = is_authenticated(client) ? program.authed_data_received_timeout : program.unauthed_data_received_timeout;
                    var message_received_timeout = is_authenticated(client) ? program.authed_message_received_timeout : program.unauthed_message_received_timeout;


                    if (time_since_last_read >= data_received_timeout)
                    {
                        // 
                        if (program.write_console_log) program.WriteLine($@"read_loop(): {client_ip}: read timeout = {data_received_timeout.ToString()}, time_since_last_read = {time_since_last_read.ToString()}, time_now = {time_now}", true, ConsoleColor.Magenta);
                        close_client(client, stream);
                        break;
                    }


                    if (time_since_last_message >= message_received_timeout)
                    {
                        // 
                        if (program.write_console_log) program.WriteLine($@"read_loop(): {client_ip}: message timeout = {message_received_timeout.ToString()}, time_since_last_message = {time_since_last_message.ToString()}, time_now = {time_now}", true, ConsoleColor.Magenta);
                        close_client(client, stream);
                        break;
                    }


                    try
                    {
                        if (!cancellation_token.IsCancellationRequested)
                        {
                            var read_wait = new TimeSpan(0, 0, 0, 5);

                            if (program.write_console_log) program.WriteLine($@"read_loop(): {client_ip}: Task.Delay({read_wait.ToString()}, cancellation_token).Wait(cancellation_token);", true, ConsoleColor.DarkGray);
                            Task.Delay(read_wait, cancellation_token).Wait(cancellation_token);
                        }
                    }
                    catch (Exception e)
                    {
                        program.WriteLineException(e, nameof(read_loop),"", true, ConsoleColor.DarkGray);

                    }
                }
            }

            return null;
            //close_client(client, stream, slave_health);
        }


        // this function connects to a slave to compute the result, then returns it


        //public static int send_svm_request_delay = 0;
        //public static object send_svm_request_delay_lock = new object();

        //public static DateTime time_last_request = DateTime.Now;

        private static object get_next_outgoing_connection_lock = new object();
        private static List<(string hostname, List<int> ports)> host_list = new List<(string hostname, List<int> ports)>();
        private static int host_list_index = -1;

        public static (TcpClient, NetworkStream) get_next_outgoing_connection(CancellationToken cancellation_token)
        {
            var load_hosts = program.load_hosts();

            if (load_hosts == null || load_hosts.Count == 0)
            {
                try
                {
                    if (program.write_console_log) program.WriteLine($@"{nameof(send_run_svm_request)}(): [{send_run_svm_request_id}] Client: Hosts list is empty.", true, ConsoleColor.Red);

                    Task.Delay(new TimeSpan(0, 0, 0, 30), cancellation_token).Wait(cancellation_token);
                }
                catch (Exception)
                {

                }

                return (null, null);
            }

            var host_list_index_copy = 0;
            lock (get_next_outgoing_connection_lock)
            {
                var old_host_list_flat = host_list.SelectMany(a => a.ports.Select(b => $"{a.hostname}:{b}")).ToList();
                var new_host_list_flat = load_hosts.SelectMany(a => a.ports.Select(b => $"{a.hostname}:{b}")).ToList();

                if (!old_host_list_flat.SequenceEqual(new_host_list_flat))
                {
                    host_list = load_hosts;
                    cross_validation_remote.host_list_index = -1;
                }

                cross_validation_remote.host_list_index++;

                if (cross_validation_remote.host_list_index > load_hosts.Count - 1)
                {
                    cross_validation_remote.host_list_index = 0;
                }

                host_list_index_copy = cross_validation_remote.host_list_index;
            }

            
            for (var _host_index = 0; _host_index < load_hosts.Count; _host_index++)
            {
                var host_index = _host_index + host_list_index_copy;

                if (host_index > load_hosts.Count - 1) host_index = host_index - (load_hosts.Count);
                
                
                var host_and_ports = load_hosts[host_index];

                for (var port_index = 0; port_index < host_and_ports.ports.Count; port_index++)
                {
                    var compute_unit_address = (hostname: host_and_ports.hostname, port: host_and_ports.ports[port_index]);

                    var num_connections_to_host = 0;

                    lock (master_loop_clients_list_lock)
                    {
                        var remote_host_entry = Dns.GetHostEntry(compute_unit_address.hostname);
                        var remote_host_address_list = remote_host_entry.AddressList.Select(a => a.ToString()).Union(new List<string>() { compute_unit_address.hostname }).Where(a => !string.IsNullOrWhiteSpace(a)).Distinct().ToList();

                        var currently_connected_address_list = master_loop_clients.Select(a => get_client_ip(a.client)).Where(a => !string.IsNullOrWhiteSpace(a)).ToList();

                        num_connections_to_host = currently_connected_address_list.Count(a => remote_host_address_list.Contains(a));
                    }

                    if (max_tcp_clients > 0 && num_connections_to_host >= max_tcp_clients)
                    {
                        if (program.write_console_log) program.WriteLine($@"{nameof(send_run_svm_request)}(): [{send_run_svm_request_id}] [{compute_unit_address.hostname}:{compute_unit_address.port}] Client: num_connections_to_host >= max_tcp_clients; {(host_index + 1)} / {load_hosts.Count}");

                        Task.Delay(new TimeSpan(0, 0, 0, 1), cancellation_token).Wait(cancellation_token);

                        break;
                    }

                    try
                    {
                        TcpClient outgoing_client = new TcpClient { NoDelay = true };
                        NetworkStream outgoing_client_stream = null;

                        var connection_timeout = program.tcp_connection_timeout;

                        if (program.write_console_log) program.WriteLine($@"{nameof(send_run_svm_request)}(): outgoing_client.ConnectAsync({compute_unit_address.hostname}, {compute_unit_address.port}).Wait({connection_timeout.ToString()});");

                        try
                        {
                            outgoing_client.ConnectAsync(compute_unit_address.hostname, compute_unit_address.port).Wait(connection_timeout);

                            outgoing_client_stream = outgoing_client.GetStream();

                            outgoing_client.NoDelay = true;
                        }
                        catch (Exception e)
                        {
                            program.WriteLineException(e, nameof(get_next_outgoing_connection), $@"Connect error: {compute_unit_address.hostname}:{compute_unit_address.port}");

                            outgoing_client?.Close();

                            continue;
                        }

                        if (!outgoing_client.is_connected())
                        {
                            try
                            {
                                outgoing_client?.Close();
                            }
                            catch (Exception)
                            {

                            }

                            if (program.write_console_log) program.WriteLine($@"{nameof(send_run_svm_request)}(): Connection timeout to {compute_unit_address.hostname}:{compute_unit_address.port}");

                            continue;
                        }

                        if (outgoing_client != null && outgoing_client_stream != null)
                        {
                            lock (get_next_outgoing_connection_lock)
                            {
                                cross_validation_remote.host_list_index = host_index;
                            }

                            return (outgoing_client, outgoing_client_stream);
                        }
                    }
                    catch (Exception e)
                    {
                        program.WriteLineException(e, nameof(get_next_outgoing_connection));
                    }


                }
            }

            return (null, null);
        }

        public static cross_validation.run_svm_return send_run_svm_request(run_svm_params run_svm_remote_params, CancellationToken cancellation_token)
        {
            // this method finds a compute unit and sends it the svm parameters and data

            if (!run_svm_remote_params.run_remote)
            {
                var inner_cv_svm_implementation = program.inner_cv_svm_implementation;
                var outer_cv_svm_implementation = program.outer_cv_svm_implementation;
                var result = cross_validation.run_svm(run_svm_remote_params, cancellation_token, inner_cv_svm_implementation, outer_cv_svm_implementation);
                return result;
            }

            int send_run_svm_request_id = 0;

            lock (send_run_svm_request_id_lock)
            {
                send_run_svm_request_id = cross_validation_remote.send_run_svm_request_id++;
            }

            //var show_exceptions = true;

            // copy to avoid accidental overwrites
            run_svm_remote_params = new run_svm_params(run_svm_remote_params);

            // serialise
            var svm_request_serialised_json = "";

            do
            {
                svm_request_serialised_json = run_svm_params.serialise_json(run_svm_remote_params);

                if (string.IsNullOrEmpty(svm_request_serialised_json))
                {
                    if (program.write_console_log) program.WriteLine($"{nameof(send_run_svm_request)}(): {nameof(svm_request_serialised_json)} is empty.", true, ConsoleColor.Red);

                    var delay = new TimeSpan(0, 0, 30);

                    try
                    {
                        Task.Delay(delay, cancellation_token).Wait(cancellation_token);
                    }
                    catch (Exception e)
                    {
                        program.WriteLineException(e, nameof(send_run_svm_request));
                        throw;
                    }
                }

            } while (string.IsNullOrEmpty(svm_request_serialised_json));


            if (program.write_console_log) program.WriteLine($@"{nameof(send_run_svm_request)}(): [{send_run_svm_request_id}] Client: Finding svm compute unit to calculate.");

            while (true)
            {
                var cache = get_cached_response(svm_request_serialised_json);

                if (!string.IsNullOrWhiteSpace(cache))
                {
                    var r = cross_validation.run_svm_return.deserialise(cache);

                    if (r != null && r.run_svm_return_data != null && r.run_svm_return_data.Count > 0)
                    {
                        return r;
                    }
                }

                var (master_client, master_client_stream) = get_next_outgoing_connection(cancellation_token);

                if (master_client == null)
                {
                    if (program.write_console_log) program.WriteLine($@"{nameof(send_run_svm_request)}(): [{send_run_svm_request_id}] Client: No compute unit available.");

                    try
                    {
                        if (!cancellation_token.IsCancellationRequested)
                        {
                            var delay = new TimeSpan(0, 0, 0, 30);

                            if (program.write_console_log) program.WriteLine($@"{nameof(send_run_svm_request)}(): Task.Delay({delay.ToString()}, cancellation_token).Wait(cancellation_token);", true, ConsoleColor.Red);
                            Task.Delay(delay, cancellation_token).Wait(cancellation_token);
                        }
                    }
                    catch (Exception e)
                    {
                        program.WriteLineException(e, nameof(send_run_svm_request), "", true, ConsoleColor.DarkGray);

                    }

                    continue;
                }

                var client_ip = get_client_ip(master_client);

                lock (master_loop_clients_list_lock)
                {
                    master_loop_clients.Add((master_client, master_client_stream));
                }

                var client_keep_alive = cross_validation_remote.client_keep_alive(master_client, master_client_stream);

                // read loop until closed
                var messages_to_send = new List<(string header, string text)>();

                messages_to_send.Add(("SVM_REQUEST", svm_request_serialised_json));

                var result = read_loop(master_client, master_client_stream, messages_to_send, cancellation_token);

                if (result != null && result.run_svm_return_data != null && result.run_svm_return_data.Count > 0)
                {
                    var svm_response_serialised_json = cross_validation.run_svm_return.serialise(result);

                    // cache on the master unit
                    cache_request_response(svm_request_serialised_json, svm_response_serialised_json);

                    return result;
                }
            }
        }
    }
}








