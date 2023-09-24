using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EPI.Comm
{
    /// <summary>
    /// 질문 : 이 사용자 정의 예외는 적절한 설계가 아닌 것같..
    /// 이렇게 만들어서 쓰는거 맞나?
    /// </summary>
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
