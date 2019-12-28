using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace svm_compute
{
    public static class extensions
    {
        [DllImport("kernel32.dll", ExactSpelling = true)]

        private static extern IntPtr GetConsoleWindow();
        private static readonly IntPtr _this_console = GetConsoleWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static bool show_window_console(show_window_options show_window_options)
        {
            var ret = ShowWindow(_this_console, (int) show_window_options);
            return ret;
        }

        public enum show_window_options
        {
            SW_HIDE = 0,
            SW_MAXIMIZE = 3,
            SW_MINIMIZE = 6,
            SW_RESTORE = 9,

        };

        public static TcpState GetState(this TcpClient tcpClient)
        {
            try
            {

                var states = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections().Where(x => x.LocalEndPoint.Equals(tcpClient.Client.LocalEndPoint) && x.RemoteEndPoint.Equals(tcpClient.Client.RemoteEndPoint)).ToList();

                if (states != null || states.Count > 0)
                {
                    if (states.Count == 1)
                    {
                        return states.First().State;
                    }
                    else if (states.Count > 1)
                    {
                        var all_states = states.Select(a => a.State).Distinct().ToList();

                        if (all_states.Count == 1)
                        {
                            return all_states.First();
                        }

                    }
                }
            }
            catch (ObjectDisposedException)
            {
                return TcpState.Closed;
            }
            catch (Exception)
            {
                return TcpState.Closed;
            }
            finally
            {
            }

            return TcpState.Unknown;
        }

        //public static bool test_connection(this TcpClient client)
        //{
        //    if (client.Client.Poll(0, SelectMode.SelectRead))
        //    {
        //        if (!client.Connected)
        //        {
        //            return false;
        //        }
        //        else
        //        {
        //            byte[] b = new byte[1];
        //            try
        //            {
        //                if (client.Client.Receive(b, SocketFlags.Peek) == 0)
        //                {
        //                    // Client disconnected
        //                    return false;
        //                }
        //            }
        //            catch { return false; }
        //        }
        //    }

        //    try
        //    {
        //        if (!(client.Client.Poll(1, SelectMode.SelectRead) && client.Client.Available == 0);
        //    }
        //    catch (SocketException) { return false; }


        //    return true;
        //}


        public static bool is_connected(this Socket client)
        {
            // This is how you can determine whether a socket is still connected.
            bool blockingState = false;
            try
            {
                blockingState = client.Blocking;

                byte[] tmp = new byte[1];

                client.Blocking = false;
                client.Send(tmp, 0, 0);
                return true;
            }
            catch (SocketException e)
            {
                // 10035 == WSAEWOULDBLOCK
                if (e.NativeErrorCode.Equals(10035))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                try
                {
                    client.Blocking = blockingState;
                }
                catch (Exception)
                {

                }
                finally
                {

                }
            }
        }

        public static void unique<T>(this List<T> list)
        {
            var list2 = list.Distinct().ToList();
            list.Clear();
            list.AddRange(list2);
        }

        public static void order<T>(this List<T> list)
        {
            var list2 = list.OrderBy(a => a).ToList();
            list.Clear();
            list.AddRange(list2);
        }

        //public static string GetStringSha256Hash(this string text)
        //{
        //    if (String.IsNullOrEmpty(text))
        //        return String.Empty;

        //    using (var sha = new SHA256Managed())
        //    {
        //        byte[] textData = Encoding.UTF8.GetBytes(text);
        //        byte[] hash = sha.ComputeHash(textData);
        //        return BitConverter.ToString(hash).Replace("-", String.Empty);
        //    }
        //}

        public static List<T> RotateLeft<T>(this List<T> list, int offset)
        {
            return Rotate(list, Math.Abs(offset));
        }

        public static List<T> RotateRight<T>(this List<T> list, int offset)
        {
            return Rotate(list, -Math.Abs(offset));
        }

        public static List<T> Rotate<T>(this List<T> list, int offset)
        {
            if (Math.Abs(offset) == list.Count) offset = 0;

            if (Math.Abs(offset) > list.Count) offset = offset < 0 ? -(Math.Abs(offset) % list.Count) : (Math.Abs(offset) % list.Count);

            //program.WriteLine(offset);

            if (offset > 0) return list.Skip(offset).Concat(list.Take(offset)).ToList(); // left
            if (offset < 0) return list.Skip(list.Count - (-offset)).Take(-offset).Concat(list.Take(list.Count - (-offset))).ToList(); // right

            return list.ToList();
        }

        public static void shuffle<T>(this IList<T> list, Random random = null)
        {
            if (random == null) random = thread_safe_random.this_threads_random;

            //var k_list = new List<int>();

            for (var n = list.Count - 1; n >= 0; n--)
            {
                var k = random.Next(0, list.Count - 1);
                //k_list.Add(k);

                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            // program.WriteLine(string.Join(",",k_list));
        }

        //public static bool is_bit_set(this int b, int pos)
        //{
        //    return (b & (1 << pos)) != 0;
        //}
    }
}