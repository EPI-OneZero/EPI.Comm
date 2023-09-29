using System;
using System.Runtime.CompilerServices;

namespace EPI.Comm
{

    internal class CommException : Exception
    {
        internal CommException(string message, Exception e) : base(message, e.InnerException)
        {

        }
        internal CommException(string message) : base(message)
        {

        }

        internal static CommException CreateCommException(string message)
        {
            return new CommException(message);
        }
        internal static CommException CreateCommException(Exception e, [CallerMemberName] string caller = "")
        {
            return new CommException(caller, e);
        }
    }
}
