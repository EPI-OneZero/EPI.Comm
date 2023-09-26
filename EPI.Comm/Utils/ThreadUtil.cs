using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EPI.Comm.UTils
{
    internal static class ThreadUtil
    {
     
        public static Thread Start(Action action)
        {
            var thread = new Thread(new ThreadStart(action));
            thread.Start();
            thread.IsBackground= true;
            return thread;
        }
    }
}
