using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
        internal static CommException CreateCommException([CallerMemberName] string caller = "")
        {
            return new CommException(caller);
        }
        internal static CommException CreateCommException(Exception e, [CallerMemberName] string caller = "")
        {
            return new CommException(caller, e);
        }
    }
}
