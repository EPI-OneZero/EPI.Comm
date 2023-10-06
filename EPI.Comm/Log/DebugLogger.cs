using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace EPI.Comm.Log
{
    internal abstract class Logger
    {
        public static Logger Default { get; private set; } = new DebugLogger();

        public bool IsEnabled { get; set; }
        public void WriteLine(string message)
        {
            if (IsEnabled)
            {
                WriteLineMessage(message);
            }
        }
        public void WriteLineCaller([CallerMemberName] string caller = "")
        {
            if (IsEnabled)
            {
                WriteLineMessage(caller);
            }
        }
        protected abstract void WriteLineMessage(string message);
        protected abstract void WriteMessage(string message);


    }
    internal sealed class DebugLogger : Logger
    {
        internal DebugLogger()
        {

        }
        protected override void WriteLineMessage(string message)
        {
            Debug.WriteLine(message);
        }

        protected override void WriteMessage(string message)
        {
            Debug.Write(message);
        }
    }

}
