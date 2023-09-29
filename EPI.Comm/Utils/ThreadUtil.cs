using System;
using System.Threading;

namespace EPI.Comm.UTils
{
    internal static class ThreadUtil
    {

        public static Thread Start(ThreadStart start)
        {
            var thread = new Thread(start);
            thread.Start();
            return thread;
        }
    }
}
