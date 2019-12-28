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

        public static readonly object compute_unit_hostnames_lock = new object();

        public static List<(string hostname, int port)> compute_unit_hostnames = null;

        public static readonly object incoming_connections_list_lock = new object();
        public static readonly object outgoing_connections_list_lock = new object();

        public static List<TcpClient> authenticated_clients = new List<TcpClient>();
        public static List<(TcpClient client, NetworkStream stream)> incoming_connections_list = new List<(TcpClient client, NetworkStream stream)>();
        public static List<(TcpClient client, NetworkStream stream)> outgoing_connections_list = new List<(TcpClient client, NetworkStream stream)>();

        public static int default_server_port = 843;

        public static int max_tcp_clients = 10;
        public static bool log_console_to_tcp = false;
        public static readonly object authentication_lock = new object();
        public static readonly object cache_lock = new object();

        private static readonly object send_run_svm_request_id_lock = new object();
        private static int send_run_svm_request_id = -1;

        public static string get_client_ip(TcpClient client)
        {
            var ip = "";

            try
            {
                ip = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
            }
            catch (ObjectDisposedException)
            {

            }
            catch (Exception e)
            {
                program.WriteLine($"{nameof(get_client_ip)}: " + e.ToString(), true, ConsoleColor.DarkGray);
            }
            finally
            {

            }

            return ip;
        }

        public static bool compute_incoming = true;

        public static void compute_loop(CancellationToken slave_loop_cancellation_token)
        {
            var spt = system_perf.get_system_perf_task();

            var local_hostname = Dns.GetHostName();
            program.WriteLine($@"[{local_hostname}] Server: {nameof(compute_loop)}()");
            var tasks = new List<Task>();
            program.WriteLine($@"[{local_hostname}] Server: SVM Compute Unit. (Press Esc to exit). Hostname: {local_hostname}.");
            TcpListener server = null;
            var listen_ip = IPAddress.Any;
            var listen_port = cross_validation_remote.default_server_port;
            var task_id = 0UL;

            var min_free_cpu = 0.25;
            var min_free_ram = 2048;

            var is_outgoing_client = !compute_incoming;
            var is_incoming_server = compute_incoming;

            while (!Console.KeyAvailable || Console.ReadKey(true).Key != ConsoleKey.Escape)
            {
                program.GC_Collection();

                if (slave_loop_cancellation_token.IsCancellationRequested)
                {
                    break;
                }

                var incomplete_tasks = tasks.Where(a => !a.IsCompleted).ToList();

                while (max_tcp_clients > -1 && incomplete_tasks.Count >= max_tcp_clients)
                {
                    program.WriteLine($@"{nameof(compute_loop)}(): Server: Resource shortage. Waiting for tasks to complete before continuing. Task.WaitAny(tasks.ToArray<Task>());", true, ConsoleColor.Cyan);

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
                        program.WriteLine($@"Server: Resource shortage. Waiting for free resources before continuing... Free CPU = {free_cpu}, Free RAM = {free_ram}, Task ID = {task_id}.");
                    }

                    try
                    {
                        if (!slave_loop_cancellation_token.IsCancellationRequested)
                        {
                            var delay = new TimeSpan(0, 0, 5);

                            program.WriteLine($@"{nameof(compute_loop)}(): Task.Delay({delay.ToString()}, slave_loop_cancellation_token).Wait(slave_loop_cancellation_token);", true, ConsoleColor.Red);
                            Task.Delay(delay, slave_loop_cancellation_token).Wait(slave_loop_cancellation_token);
                        }
                    }
                    catch (Exception e)
                    {
                        program.WriteLine($"{nameof(compute_loop)}: " + e.ToString(), true, ConsoleColor.DarkGray);

                    }

                } while (free_cpu < min_free_cpu || free_ram < min_free_ram);

                if (is_incoming_server)
                {
                    try
                    {
                        server = new TcpListener(listen_ip, cross_validation_remote.default_server_port);
                        server.Start();
                        program.WriteLine($@"{local_hostname}] Server: Waiting for a connection on {listen_ip}:{listen_port}... Free CPU = {free_cpu}, Free RAM = {free_ram}, Task ID = {task_id}.");
                    }
                    catch (Exception e)
                    {
                        program.WriteLine($@"[{local_hostname}] [{nameof(Exception)}] Server: Could not start TCP server. {e}. Task ID = {task_id}.");
                        try
                        {
                            if (!slave_loop_cancellation_token.IsCancellationRequested)
                            {
                                var delay = new TimeSpan(0, 0, 5);

                                program.WriteLine($@"compute_loop(): Task.Delay({delay.ToString()}, slave_loop_cancellation_token).Wait(slave_loop_cancellation_token);", true, ConsoleColor.Red);
                                Task.Delay(delay, slave_loop_cancellation_token).Wait(slave_loop_cancellation_token);
                            }
                        }
                        catch (Exception exception)
                        {
                            program.WriteLine($"{nameof(compute_loop)}(): " + exception.ToString(), true, ConsoleColor.DarkGray);

                        }
                        continue;
                    }


                    TcpClient client = null;
                    client = server.AcceptTcpClient();



                    server.Stop();

                  

                    try
                    {
                        if (!client.Connected || client.GetState() != TcpState.Established || !client.Client.is_connected())
                        {
                            // connection error
                            client.Close();// close_client(client, stream);
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        program.WriteLine(e.ToString());
                    }

                    client.NoDelay = true;
                    var stream = client.GetStream();

                    lock (incoming_connections_list_lock)
                    {
                        incoming_connections_list.Add((client, stream));
                    }

                    var client_ip = get_client_ip(client);

                    

                    var client_keep_alive = cross_validation_remote.client_keep_alive(client, stream);

                    //var stopwatch_client = new Stopwatch();
                    //stopwatch_client.Start();

                    var client_cancellation_token_source = new CancellationTokenSource();
                    var client_cancellation_token = client_cancellation_token_source.Token;

                    var _task_id = task_id;

                    var task = Task.Run(() =>
                    {
                        var client1 = client;
                        var stream1 = stream;
                        try
                        {
                            // read loop until closed

                            var read_loop_result = read_loop(client, stream, null, client_cancellation_token);

                            close_client(client, stream);
                        }
                        catch (Exception e)
                        {
                            do
                            {
                                //program.WriteLine($@"[{local_hostname}] [{l_task_id}] [{client_ip}] 321 {nameof(Exception)}: \"{e.Source}\" \"{e.TargetSite}\" \"{e.Message}\" --> \"{e.StackTrace}\"");
                                e = e.InnerException;
                            } while (e != null);
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
                            //program.WriteLine(e.ToString());
                        }

                        lock (incoming_connections_list_lock)
                        {
                            incoming_connections_list = incoming_connections_list.Where(a => a.client != client).ToList();// Remove((client, stream));
                        }

                        program.WriteLine($@"{client_ip}: [{local_hostname}] Server: incoming-connection task exiting. Task ID = {_task_id}.");
                    }, slave_loop_cancellation_token);
                    tasks.Add(task);
                }

                if (is_outgoing_client)
                {
                    var server_address = "PPRSB1025-C06";
                    var server_port = default_server_port;


                    //var stopwatch_client = new Stopwatch();
                    //stopwatch_client.Start();

                    var client_cancellation_token_source = new CancellationTokenSource();
                    var client_cancellation_token = client_cancellation_token_source.Token;

                    var _task_id = task_id;

                    try
                    {
                        var client = new TcpClient() { NoDelay = true };

                        var connect_timeout = new TimeSpan(0, 0, 15);

                        client.ConnectAsync(server_address, server_port).Wait(connect_timeout);


                        if (!client.Connected || client.GetState() != TcpState.Established || !client.Client.is_connected())
                        {
                            client.Close();
                            continue;
                        }

                        client.NoDelay = true;

                        var client_ip = get_client_ip(client);

                        var stream = client.GetStream();

                        lock (incoming_connections_list_lock)
                        {
                            incoming_connections_list.Add((client, stream));
                        }

                        var client_keep_alive = cross_validation_remote.client_keep_alive(client, stream);


                        var task = Task.Run(() =>
                        {
                            var client1 = client;
                            var stream1 = stream;
                            try
                            {
                                // read loop until closed

                                var read_loop_result = read_loop(client, stream, null, client_cancellation_token);

                                close_client(client, stream);
                            }
                            catch (Exception e)
                            {
                                do
                                {
                                    //program.WriteLine($@"[{local_hostname}] [{l_task_id}] [{client_ip}] 321 {nameof(Exception)}: \"{e.Source}\" \"{e.TargetSite}\" \"{e.Message}\" --> \"{e.StackTrace}\"");
                                    e = e.InnerException;
                                } while (e != null);
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
                                //program.WriteLine(e.ToString());
                            }

                            lock (incoming_connections_list_lock)
                            {
                                incoming_connections_list = incoming_connections_list.Where(a => a.client != client).ToList();// Remove((client, stream));
                            }

                            program.WriteLine($@"{client_ip}: [{local_hostname}] Server: incoming-connection task exiting. Task ID = {_task_id}.");
                        }, slave_loop_cancellation_token);
                        tasks.Add(task);

                    }
                    catch (Exception)
                    {

                    }
                    finally
                    {

                    }
                }

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

            var hash1 = Hash(request.Trim());

            try
            {
                var folder = @"c:\svm_compute\cache\";
                Directory.CreateDirectory(folder);
                hash1 = Hash(request.Trim());
                var filename = hash1;
                //var f_request = Path.Combine(folder, "request", $"request_{filename}.txt");
                var f_response = Path.Combine(folder, "response", $"response_{filename}.txt");

                lock (cache_lock)
                {
                    // note: caching the request takes up a lot of disk space (probably due to the large dataset size and feature descriptions plus meta data)

                    //File.WriteAllText(f_request, request);
                    File.WriteAllText(f_response, response);
                }

                program.WriteLine($@"cache_request_response(): saved response to cache, hash={hash1}");
            }
            catch (Exception e)
            {
                program.WriteLine($@"cache_request_response(): could not cache response, hash={hash1}: {e.ToString()}");

            }
            finally
            {

            }
        }

        public static string get_cached_response(string request)
        {
            var cache_found = false;
            var cache_loaded = false;
            var hash1 = "";

            try
            {
                var folder = @"c:\svm_compute\cache\";
                Directory.CreateDirectory(folder);
                hash1 = Hash(request.Trim());
                var filename = hash1;

                //var f_request = Path.Combine(folder, "request", $"request_{filename}.txt");
                var f_response = Path.Combine(folder, "response", $"response_{filename}.txt");

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
                program.WriteLine($@"get_cached_response(): hash={hash1}, cache_found={cache_found}, cache_loaded={cache_loaded}");
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

                var is_outgoing = false;
                lock (outgoing_connections_list_lock) { is_outgoing = outgoing_connections_list.Any(a => a.client == client); }

                var is_incoming = false;
                lock (incoming_connections_list_lock) { is_incoming = incoming_connections_list.Any(a => a.client == client); }

                var is_authenticated = cross_validation_remote.is_authenticated(client);

                program.WriteLine($@"{client_ip}: {client.Client.Handle} RECEIVED from {(is_outgoing ? "outgoing" : "")}{(is_incoming ? "incoming" : "")} {(is_authenticated ? "authed" : "unauthed")} client: {msg.header} --> {(string.IsNullOrWhiteSpace(msg.text) ? "" : msg.text.Substring(0, msg.text.Length >= 1024 ? 1024 : msg.text.Length))}...");

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
                                    program.WriteLine($@"{client_ip}: process_messages(): Task.Delay({delay.ToString()}, cancellation_token).Wait(cancellation_token);", true, ConsoleColor.Red);
                                    Task.Delay(delay, cancellation_token).Wait(cancellation_token);
                                }
                            }
                            catch (Exception e)
                            {
                                program.WriteLine($"{nameof(process_messages)}(): " + e.ToString(), true, ConsoleColor.DarkGray);

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

                            var request_svm_params = cross_validation.run_svm_params.deserialise(request_json);

                            if (request_svm_params == null)
                            {
                                program.WriteLine($@"{nameof(process_messages)}(): cross_validation.run_svm_params.deserialise(request_json) == NULL", true, ConsoleColor.Red);
                                close_client(client, stream);
                                break;
                            }

                            request_svm_params.run_remote = false;

                            send_message("SVM_REQUEST_RECEIVED", "1", client, stream);

                            var request_svm_return = cross_validation.run_svm(request_svm_params, cancellation_token);

                            if (request_svm_return != null && request_svm_return.run_svm_return_data != null && request_svm_return.run_svm_return_data.Count > 0)
                            {
                                var request_svm_return_json_serialised = cross_validation.run_svm_return.serialise(request_svm_return);

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
                                program.WriteLine($@"{nameof(process_messages)}(): cross_validation.run_svm_return.deserialise(svm_response) == NULL", true, ConsoleColor.Red);
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
                            program.WriteLine($@"{client_ip}: unknown command: " + msg.header + " " + msg.text);
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

        public static object append_bytes_lock = new object();

        public static void AppendAllBytes(string path, byte[] bytes)
        {
            if (string.IsNullOrWhiteSpace(path) || bytes == null || bytes.Length == 0)
            {
                return;
            }

            lock (append_bytes_lock)
            {
                using (var stream = new FileStream(path, FileMode.Append))
                {
                    stream.Write(bytes, 0, bytes.Length);

                    stream.Flush();
                }
            }
        }

        public static void send_message(string header, string text, TcpClient client, NetworkStream stream)
        {
            var client_ip = get_client_ip(client);


            var is_outgoing = false;
            lock (outgoing_connections_list_lock) { is_outgoing = outgoing_connections_list.Any(a => a.client == client); }

            var is_incoming = false;
            lock (incoming_connections_list_lock) { is_incoming = incoming_connections_list.Any(a => a.client == client); }


            var is_authenticated = cross_validation_remote.is_authenticated(client);


            program.WriteLine($@"{client_ip}: {client.Client.Handle} SEND to {(is_outgoing ? "outgoing" : "")}{(is_incoming ? "incoming" : "")} {(is_authenticated ? "authed" : "unauthed")} client: {header} --> {(string.IsNullOrWhiteSpace(text) ? "" : text.Substring(0, text.Length >= 1024 ? 1024 : text.Length))}...");

            //program.WriteLine($@"[{client_ip}] 501 Sending message");
            if (!client.Connected || !stream.CanWrite || client.GetState() != TcpState.Established || !client.Client.is_connected())
            {
                close_client(client, stream);
                return;
            }

            var prev_no_delay = client.NoDelay;
            var prev_read_timeout = stream.ReadTimeout;
            var prev_write_timeout = stream.WriteTimeout;

            var sb = new StringBuilder();
            sb.Append((char)0);
            sb.Append((char)1);
            sb.Append(header);
            sb.Append((char)2);
            sb.Append(text);
            sb.Append((char)3);
            sb.Append((char)0);
            var msg = sb.ToString();



            client.NoDelay = true;
            stream.ReadTimeout = (int)new TimeSpan(0, 2, 0).TotalMilliseconds;
            stream.WriteTimeout = (int)new TimeSpan(0, 2, 0).TotalMilliseconds;

            try
            {
                var msg_bytes = Encoding.ASCII.GetBytes(msg);

                program.WriteLine($@"{client_ip}: send_message(): stream.WriteAsync(msg_bytes, 0, msg_bytes.Length).Wait();");
                stream.WriteAsync(msg_bytes, 0, msg_bytes.Length).Wait();
                if (log_console_to_tcp)
                {
                    AppendAllBytes($@"c:\svm_compute\tcp_log\{program.program_start_time}\{program.program_start_time}_{program.master_or_compute}_tcp_write_ip_{client_ip}.log", msg_bytes);
                }

                program.WriteLine($@"{client_ip}: send_message(): stream.FlushAsync().Wait();");
                stream.FlushAsync().Wait();
            }
            catch (Exception e)
            {
                program.WriteLine($@"{client_ip}: " + e.ToString());
                close_client(client, stream);
            }
            finally
            {
                client.NoDelay = prev_no_delay;
                stream.ReadTimeout = prev_read_timeout;
                stream.WriteTimeout = prev_write_timeout;
            }
        }

        public static void close_client(TcpClient client, NetworkStream stream)
        {
            var client_ip = get_client_ip(client);

            program.WriteLine($@"{client_ip}: close_client()");

            if (stream != null)
            {
                try
                {
                    stream?.Close();
                }
                catch (Exception)
                {
                }
                finally
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
                finally
                {
                }
            }

            lock (authentication_lock)
            {
                if (authenticated_clients != null && authenticated_clients.Count > 0)
                {
                    var c_before = authenticated_clients.Count;
                    authenticated_clients = authenticated_clients.Where(a => a != client).ToList();
                    var c_after = authenticated_clients.Count;

                    //var removed = c_after < c_before;// authenticated_clients.Remove(client);
                    //if (!removed) { program.WriteLine($@"{client_ip}: close_client(): could not remove authentication token (not found)."); }
                }
            }

            lock (incoming_connections_list_lock)
            {
                try
                {
                    var c_before = incoming_connections_list.Count;
                    incoming_connections_list = incoming_connections_list.Where(a => a.client != client).ToList();
                    var c_after = incoming_connections_list.Count;

                    //var removed = c_after < c_before;// incoming_connections_list.Remove((client,stream));// = incoming_master_clients.Where(a => a != client).ToList();
                    //if (!removed) { program.WriteLine($@"{client_ip}: close_client(): could not remove incoming connection (not found)."); }
                }
                catch (Exception e)
                {
                    program.WriteLine(e.ToString());
                }
                finally
                {

                }
            }

            lock (outgoing_connections_list_lock)
            {
                try
                {
                    var c_before = outgoing_connections_list.Count;
                    outgoing_connections_list = outgoing_connections_list.Where(a => a.client != client).ToList();
                    var c_after = outgoing_connections_list.Count;

                    //var removed = c_after < c_before;// outgoing_connections_list.Remove((client,stream));// = outgoing_connections_list.Where(a => a.client != client).ToList();
                    //if (!removed) { program.WriteLine($@"{client_ip}: close_client(): could not remove outgoing connection (not found)."); }
                }
                catch (Exception e)
                {
                    program.WriteLine(e.ToString());
                }
                finally
                {

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
                        if (client.Connected && stream.CanWrite && client.GetState() == TcpState.Established && client.Client.is_connected())
                        {
                            var buffer = new byte[1];
                            buffer[0] = 22;

                            program.WriteLine($@"{client_ip}: keep_alive_task(): stream.WriteAsync(buffer, 0, buffer.Length, keep_alive_cancellation_token).Wait(keep_alive_cancellation_token);");
                            stream.WriteAsync(buffer, 0, buffer.Length, keep_alive_cancellation_token).Wait(keep_alive_cancellation_token);
                            if (log_console_to_tcp)
                            {
                                AppendAllBytes($@"c:\svm_compute\tcp_log\{program.program_start_time}\{program.program_start_time}_{program.master_or_compute}_tcp_write_ip_{client_ip}.log", buffer);
                            }

                            program.WriteLine($@"{client_ip}: keep_alive_task(): stream.FlushAsync(keep_alive_cancellation_token).Wait(keep_alive_cancellation_token);");
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
                            program.WriteLine($@"{client_ip}: keep_alive_task(): {lvl} {e.Source} {e.TargetSite} {e.StackTrace}");
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

                            program.WriteLine($@"{client_ip}: keep_alive_task(): Task.Delay({keep_alive_delay.ToString()}, keep_alive_cancellation_token).Wait(keep_alive_cancellation_token);", true, ConsoleColor.Red);
                            Task.Delay(keep_alive_delay, keep_alive_cancellation_token).Wait(keep_alive_cancellation_token);
                        }
                    }
                    catch (Exception e)
                    {
                        program.WriteLine($"{nameof(client_keep_alive)}(): " + e.ToString(), true, ConsoleColor.DarkGray);
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

            if (client.Connected && stream.CanWrite && stream.CanRead && client.GetState() == TcpState.Established && client.Client.is_connected())
            {
                send_auth_request(client, stream);
            }
            else
            {
                close_client(client, stream);
                return null;
            }

            while (client.Connected && stream.CanRead && client.GetState() == TcpState.Established && client.Client.is_connected())
            {
                if (cancellation_token.IsCancellationRequested)
                {
                    return null;
                }

                var bytes_read_total = 0;

                while (stream.DataAvailable)
                {
                    var buffer = new byte[1024 * 10];
                    var bytes_read = stream.Read(buffer, 0, buffer.Length);
                    if (log_console_to_tcp)
                    {
                        AppendAllBytes($@"c:\svm_compute\tcp_log\{program.program_start_time}\{program.program_start_time}_{program.master_or_compute}_tcp_read_ip_{client_ip}.log", buffer.Take(bytes_read).ToArray());
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

                    var read_timeout = new TimeSpan(0, 2, 0);
                    var message_timeout = new TimeSpan(2, 0, 0);

                    if (time_since_last_read >= read_timeout)
                    {
                        // 
                        program.WriteLine($@"read_loop(): {client_ip}: read timeout = {read_timeout.ToString()}, time_since_last_read = {time_since_last_read.ToString()}, time_now = {time_now}", true, ConsoleColor.Magenta);
                        close_client(client, stream);
                        break;
                    }


                    if (time_since_last_message >= message_timeout)
                    {
                        // 
                        program.WriteLine($@"read_loop(): {client_ip}: message timeout = {message_timeout.ToString()}, time_since_last_message = {time_since_last_message.ToString()}, time_now = {time_now}", true, ConsoleColor.Magenta);
                        close_client(client, stream);
                        break;
                    }


                    try
                    {
                        if (!cancellation_token.IsCancellationRequested)
                        {
                            var read_wait = new TimeSpan(0, 0, 0, 5);

                            program.WriteLine($@"read_loop(): {client_ip}: Task.Delay({read_wait.ToString()}, cancellation_token).Wait(cancellation_token);", true, ConsoleColor.DarkGray);
                            Task.Delay(read_wait, cancellation_token).Wait(cancellation_token);
                        }
                    }
                    catch (Exception e)
                    {
                        program.WriteLine($"{nameof(read_loop)}(): " + e.ToString(), true, ConsoleColor.DarkGray);

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

        public static cross_validation.run_svm_return send_run_svm_request(cross_validation.run_svm_params run_svm_remote_params, CancellationToken cancellation_token)
        {
            // this method finds a compute unit and sends it the svm parameters and data


            //bool return_predictions, bool return_performance, bool return_meta_data, bool output_threshold_adjustment_performance,
            //var perform_outer_cv_on_master = true;
            //if (perform_outer_cv_on_master)
            //{
            //    run_svm_remote_params.outer_cv_folds = 1;
            //}
            //var retries = ulong.MaxValue;
            //for (var outer_cv_index = 0; outer_cv_index < outer_cv_folds; outer_cv_index++)
            //{
            //retries--;
            //program.WriteLine($@"[{l_run_svm_remote_id}] [{run_svm_remote_params.experiment_id1}] [{run_svm_remote_params.experiment_id2}] [{client_ip}] 430 Connected.  Host stats: ""{slave_conn_info.hostname}:{slave_conn_info.port}"" " + $@"[total connection attempts: {slave_conn_info.total_connection_attempts}] " + $@"[total successful connections: {slave_conn_info.total_successful_attempts}] " + $@"[total compute requests sent: {slave_conn_info.compute_requests_sent}] " + $@"[total compute responses received: {slave_conn_info.compute_responses_received}] " + $@"[total corrupt messages received: {slave_conn_info.corrupt_message_received_count}] " + $@"[total message timeouts: {slave_conn_info.request_timeout_count}] " + $@"[total tcp errors: {slave_conn_info.tcp_error_count}] " + $@"[total connect errors: {slave_conn_info.connect_error_count}] [total state error: {slave_conn_info.state_error_count}] [total read error: {slave_conn_info.read_error_count}]");
            //program.WriteLine($@"[{l_run_svm_remote_id}] [{run_svm_remote_params.experiment_id1}] [{run_svm_remote_params.experiment_id2}] [{client_ip}] 491 Sending request to slave (length = {run_svm_remote_params_serialised.Length}).");


            if (!run_svm_remote_params.run_remote)
            {
                var result = cross_validation.run_svm(run_svm_remote_params, cancellation_token);
                return result;
            }

            // note: outgoing_client / incoming_server are the opposite of their values in compute_loop()
            var is_outgoing_client = compute_incoming;
            var is_incoming_server = !compute_incoming;


            int send_run_svm_request_id = 0;

            lock (send_run_svm_request_id_lock)
            {
                send_run_svm_request_id = cross_validation_remote.send_run_svm_request_id++;
            }

            var show_exceptions = true;

            // copy to avoid accidental overwrites
            run_svm_remote_params = new cross_validation.run_svm_params(run_svm_remote_params);

            // serialise
            var svm_request_serialised_json = "";//cross_validation.run_svm_params.serialise_json(run_svm_remote_params);

            do 
            {
                svm_request_serialised_json = cross_validation.run_svm_params.serialise_json(run_svm_remote_params);

                if (string.IsNullOrEmpty(svm_request_serialised_json))
                {
                    program.WriteLine($"{nameof(send_run_svm_request)}(): {nameof(svm_request_serialised_json)} is empty.", true, ConsoleColor.Red);

                    var delay = new TimeSpan(0, 0, 30);
                    Task.Delay(delay).Wait();
                }

            } while (string.IsNullOrEmpty(svm_request_serialised_json));

            var cache1 = get_cached_response(svm_request_serialised_json);

            if (!string.IsNullOrWhiteSpace(cache1))
            {
                var r = cross_validation.run_svm_return.deserialise(cache1);

                if (r != null && r.run_svm_return_data != null && r.run_svm_return_data.Count > 0)
                {
                    return r;
                }
            }


            // setup default slave hostname list 
            //lock (compute_unit_health_info_list_lock)
            //{
            //    if (slave_health_info_list == null || slave_health_info_list.Count == 0)
            //    {
            //        slave_health_info_list = slave_hostnames.Select(hostname => new cross_validation_remote.compute_unit_health_info() { hostname = hostname.hostname, port = hostname.port }).ToList();
            //    }
            //}

            program.WriteLine($@"{nameof(send_run_svm_request)}(): [{send_run_svm_request_id}] Client: Finding svm compute unit to calculate.");

            var host_list_reordered = new List<(string hostname, int port)>();


            while (true)
            {

                if (is_outgoing_client)
                {
                    program.load_hosts();

                    lock (compute_unit_hostnames_lock)
                    {
                        host_list_reordered = cross_validation_remote.compute_unit_hostnames.ToList();
                    }

                    if (host_list_reordered == null || host_list_reordered.Count == 0)
                    {
                        try
                        {
                            program.WriteLine($@"{nameof(send_run_svm_request)}(): [{send_run_svm_request_id}] Client: Hosts list is empty.", true, ConsoleColor.Red);

                            Task.Delay(new TimeSpan(0, 0, 0, 30), cancellation_token).Wait(cancellation_token);
                        }
                        catch (Exception)
                        {

                        }

                        continue;
                    }

                    host_list_reordered.shuffle();
                    host_list_reordered.shuffle();
                }


                //lock (compute_unit_health_info_list_lock)
                //{ 
                //    //reordered_slaves = slave_health_info_list.OrderBy(a => a.total_connection_attempts).ThenBy(a => a.total_successful_attempts).ThenBy(a => a.compute_requests_sent).ThenByDescending(a => a.compute_responses_received).ToList();

                //    reordered_compute_units = slave_health_info_list.ToList();
                //    reordered_compute_units.shuffle();
                //}


                if (is_outgoing_client)
                {
                    // connect to remote slave server
                    for (var compute_unit_index = 0; compute_unit_index < host_list_reordered.Count; compute_unit_index++)
                    {
                        var compute_unit_address = host_list_reordered[compute_unit_index];

                        var cache = get_cached_response(svm_request_serialised_json);

                        if (!string.IsNullOrWhiteSpace(cache))
                        {
                            var r = cross_validation.run_svm_return.deserialise(cache);

                            if (r != null && r.run_svm_return_data != null && r.run_svm_return_data.Count > 0)
                            {
                                return r;
                            }
                        }

                        program.GC_Collection();

                        //var compute_unit_health_info = reordered_compute_units[compute_unit_index];
                        string client_ip = "";
                        var stopwatch_client = new Stopwatch();
                        stopwatch_client.Start();
                        program.WriteLine($@"{nameof(send_run_svm_request)}(): [{send_run_svm_request_id}] [{compute_unit_address.hostname}] Client: compute_unit_index = {(compute_unit_index + 1)} / {host_list_reordered.Count}");

                        var num_connections_to_host = 0;
                        lock (outgoing_connections_list_lock)
                        {
                            var remote_host_entry = Dns.GetHostEntry(compute_unit_address.hostname);
                            var remote_host_address_list = remote_host_entry.AddressList.Select(a => a.ToString()).Union(new List<string>() { compute_unit_address.hostname }).Where(a => !string.IsNullOrWhiteSpace(a)).Distinct().ToList();

                            var currently_connected_address_list = outgoing_connections_list.Select(a => get_client_ip(a.client)).Where(a => !string.IsNullOrWhiteSpace(a)).ToList();

                            num_connections_to_host = currently_connected_address_list.Count(a => remote_host_address_list.Contains(a));


                        }

                        if (max_tcp_clients > 0 && num_connections_to_host >= max_tcp_clients)
                        {
                            program.WriteLine($@"{nameof(send_run_svm_request)}(): [{send_run_svm_request_id}] [{compute_unit_address.hostname}] Client: num_connections_to_host >= max_tcp_clients; {(compute_unit_index + 1)} / {host_list_reordered.Count}");

                            Task.Delay(new TimeSpan(0, 0, 0, 1), cancellation_token).Wait(cancellation_token);
                            continue;
                        }

                        try
                        {
                            TcpClient outgoing_client = new TcpClient { NoDelay = true };

                            var connection_timeout = new TimeSpan(0, 0, 0, 15);

                            program.WriteLine($@"{nameof(send_run_svm_request)}(): outgoing_client.ConnectAsync({compute_unit_address.hostname}, {compute_unit_address.port}).Wait({connection_timeout.ToString()});");

                            try
                            {
                                outgoing_client.ConnectAsync(compute_unit_address.hostname, compute_unit_address.port).Wait(connection_timeout);
                            }
                            catch (Exception e)
                            {
                                program.WriteLine($@"{nameof(send_run_svm_request)}(): Connect error: {compute_unit_address.hostname}:{compute_unit_address.port}: {e.ToString()}");
                                outgoing_client.Close();
                                continue;
                            }
                            finally
                            {

                            }

                            if (!outgoing_client.Connected || outgoing_client.GetState() != TcpState.Established || !outgoing_client.Client.is_connected())
                            {
                                try
                                {
                                    outgoing_client.Close();
                                }
                                catch (Exception)
                                {


                                }
                                finally
                                {

                                }

                                program.WriteLine($@"{nameof(send_run_svm_request)}(): connection timeout to {compute_unit_address.hostname}:{compute_unit_address.port}");

                                //close_client(outgoing_client, null, null);
                                continue;
                            }

                            var outgoing_client_ip = get_client_ip(outgoing_client);

                            NetworkStream stream = outgoing_client.GetStream();
                            lock (outgoing_connections_list_lock)
                            {
                                outgoing_connections_list.Add((outgoing_client, stream));
                            }

                            var client_keep_alive = cross_validation_remote.client_keep_alive(outgoing_client, stream);

                            // read loop until closed
                            var messages_to_send = new List<(string header, string text)>();

                            messages_to_send.Add(("SVM_REQUEST", svm_request_serialised_json));

                            var result = read_loop(outgoing_client, stream, messages_to_send, cancellation_token);

                            if (result != null && result.run_svm_return_data != null && result.run_svm_return_data.Count > 0)
                            {
                                var svm_response_serialised_json = cross_validation.run_svm_return.serialise(result);

                                cache_request_response(svm_request_serialised_json, svm_response_serialised_json);

                                return result;
                            }
                        }
                        catch (Exception e)
                        {
                            do
                            {
                                if (show_exceptions) program.WriteLine($@"{nameof(send_run_svm_request)}(): [{send_run_svm_request_id}] [{client_ip}] Client: {nameof(Exception)}: ""{e.Source}"" ""{e.TargetSite}"" ""{e.Message}"" --> ""{e.StackTrace}""");
                                e = e.InnerException;
                            } while (e != null);
                        }
                    }
                }

                //if (is_incoming_server)
                //{
                //    TcpListener server = null;
                //    var listen_ip = IPAddress.Any;
                //    var listen_port = cross_validation_remote.default_server_port;
                //    var local_hostname = Dns.GetHostName();

                //    try
                //    {
                //        server = new TcpListener(listen_ip, listen_port);
                //        server.Start();
                //        program.WriteLine($@"{nameof(send_run_svm_request)}(): [{local_hostname}] Server: Waiting for a connection on {listen_ip}:{listen_port}...");
                //    }
                //    catch (Exception e)
                //    {
                //        program.WriteLine($@"{nameof(send_run_svm_request)}(): [{local_hostname}] [{nameof(Exception)}] Server: Could not start TCP server. {e}. Task ID = {task_id}.");
                //        try
                //        {
                //            if (!slave_loop_cancellation_token.IsCancellationRequested)
                //            {
                //                var delay = new TimeSpan(0, 0, 5);

                //                program.WriteLine($@"compute_loop(): Task.Delay({delay.ToString()}, slave_loop_cancellation_token).Wait(slave_loop_cancellation_token);", true, ConsoleColor.Red);
                //                Task.Delay(delay, slave_loop_cancellation_token).Wait(slave_loop_cancellation_token);
                //            }
                //        }
                //        catch (Exception exception)
                //        {
                //            program.WriteLine($"{nameof(compute_loop)}(): " + exception.ToString(), true, ConsoleColor.DarkGray);

                //        }
                //        continue;
                //    }


                //    TcpClient client = null;
                //    client = server.AcceptTcpClient();
                //    server.r


                //    server.Stop();

                //    var client_ip = get_client_ip(client);

                //    client.NoDelay = true;
                //    var stream = client.GetStream();

                //    try
                //    {
                //        if (!client.Connected || !stream.CanWrite || !stream.CanRead || client.GetState() != TcpState.Established || !client.Client.is_connected())
                //        {
                //            // connection error
                //            close_client(client, stream);
                //            continue;
                //        }
                //    }
                //    catch (Exception e)
                //    {
                //        program.WriteLine(e.ToString());
                //    }

                //    lock (incoming_connections_list_lock)
                //    {
                //        incoming_connections_list.Add((client, stream));
                //    }

                //    var client_keep_alive = cross_validation_remote.client_keep_alive(client, stream);

                //    //var stopwatch_client = new Stopwatch();
                //    //stopwatch_client.Start();

                //    var client_cancellation_token_source = new CancellationTokenSource();
                //    var client_cancellation_token = client_cancellation_token_source.Token;

                //    var _task_id = task_id;

                //    var task = Task.Run(() =>
                //    {
                //        var client1 = client;
                //        var stream1 = stream;
                //        try
                //        {
                //            // read loop until closed

                //            var read_loop_result = read_loop(client, stream, null, client_cancellation_token);

                //            close_client(client, stream);
                //        }
                //        catch (Exception e)
                //        {
                //            do
                //            {
                //                //program.WriteLine($@"[{local_hostname}] [{l_task_id}] [{client_ip}] 321 {nameof(Exception)}: \"{e.Source}\" \"{e.TargetSite}\" \"{e.Message}\" --> \"{e.StackTrace}\"");
                //                e = e.InnerException;
                //            } while (e != null);
                //        }

                //        try
                //        {
                //            if (!client_cancellation_token_source.IsCancellationRequested)
                //            {
                //                client_cancellation_token_source.Cancel();
                //            }
                //        }
                //        catch (Exception)
                //        {
                //            //program.WriteLine(e.ToString());
                //        }

                //        lock (incoming_connections_list_lock)
                //        {
                //            incoming_connections_list = incoming_connections_list.Where(a => a.client != client).ToList();// Remove((client, stream));
                //        }

                //        program.WriteLine($@"{client_ip}: [{local_hostname}] Server: incoming-connection task exiting. Task ID = {_task_id}.");
                //    }, slave_loop_cancellation_token);
                //    tasks.Add(task);
                //}

                program.WriteLine($@"{nameof(send_run_svm_request)}(): [{send_run_svm_request_id}] Client: No compute unit available.");

                try
                {
                    if (!cancellation_token.IsCancellationRequested)
                    {
                        var delay = new TimeSpan(0, 0, 0, 30);

                        program.WriteLine($@"{nameof(send_run_svm_request)}(): Task.Delay({delay.ToString()}, cancellation_token).Wait(cancellation_token);", true, ConsoleColor.Red);
                        Task.Delay(delay, cancellation_token).Wait(cancellation_token);
                    }
                }
                catch (Exception e)
                {
                    program.WriteLine($"{nameof(send_run_svm_request)}: " + e.ToString(), true, ConsoleColor.DarkGray);

                }
            }
        }

        //public static object compute_unit_list_lock = new object();
        //public static List<(TcpClient client, Stream stream)> compute_unit_list = new List<(TcpClient client, Stream stream)>();

        //public static void tcp_server()
        //{
        //    TcpListener server = null;
        //    var listen_ip = IPAddress.Any;
        //    var listen_port = cross_validation_remote.default_server_port;
        //    var local_hostname = Dns.GetHostName();


        //    server = new TcpListener(listen_ip, listen_port);
        //    server.Start();

        //    var tasks = new List<Task>();

        //    while (true)
        //    {
        //        try
        //        {
        //            var client = server.AcceptTcpClient();

        //            if (!client.Connected || client.GetState() != TcpState.Established || !client.Client.is_connected())
        //            {
        //                // connection error
        //                client.Close();

        //                continue;
        //            }

        //            lock (compute_unit_list_lock)
        //            {
        //                var stream = client.GetStream();
        //                compute_unit_list.Add((client, stream));
        //            }
                    
        //        }
        //        catch (Exception)
        //        {

        //        }


        //        Task.Delay(new TimeSpan(0, 0, 10)).Wait();

        //    }

        //    server.Stop();

        //}

    }
}
