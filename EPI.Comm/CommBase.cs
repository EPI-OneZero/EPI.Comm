using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace EPI.Comm
{
    public abstract class CommBase
    {
       
    }
    public interface ICommReceive
    {
        event CommReceiveEventHandler Received;
    }
    public interface ICommSend
    {
        void Send(byte[] bytes);
    }
    public interface IComm : ICommSend, ICommReceive
    {

    }
}
