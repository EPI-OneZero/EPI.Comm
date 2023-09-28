using System;
using System.Threading;

namespace EPI.Comm.UTils
{
    internal static class ThreadUtil
    {

        public static Thread Start(Action action)
        {
            var thread = new Thread(new ThreadStart(action));
            thread.Start();
            thread.IsBackground = true;
            return thread;
        }
    }
}
