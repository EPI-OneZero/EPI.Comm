using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using EPI.Comm.Net.Events;
using EPI.Comm.Net.Generic.Events;

namespace EPI.Comm
{
    /// <summary>
    /// 질문 : 이거 만들고보니 필요없는 것 같은데 버려야하나?
    /// 그리고 아래 인터페이스 정의는 적절한가?
    /// </summary>
    public abstract class CommBase
    {
        public const int DefaultBufferSize = 8192;
    }

    public interface IComm
    {
        void Send(byte[] bytes);
        event PacketEventHandler Received;
    }

    public interface IComm<Theader, Tfooter> : IComm
    {
        void Send(Theader header, byte[] body, Tfooter footer);
        new event PacketEventHandler<Theader, Tfooter> Received;


    }
    public interface IComm<Theader> : IComm
    {
        void Send(Theader header, byte[] body);
        new event PacketEventHandler<Theader> Received;

    }
}
