using System.Threading;

namespace EPI.Comm.Utils
{
    internal static class DelegateUtil
    {

        public static Thread Start(ThreadStart start)
        {
            var thread = new Thread(start);
            thread.Start();
            return thread;
        }
    }
}
