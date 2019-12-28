using System;
using System.Threading;

namespace svm_compute
{
    public static class thread_safe_random
    {
        [ThreadStatic] private static Random _local;

        public static Random this_threads_random => _local ?? (_local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId)));

    }
}