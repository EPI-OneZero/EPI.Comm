using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EPI.Comm.Net.Generic
{
    public sealed class ClientCollection<Theader> : ReadOnlyCollection<TcpNetClient<Theader>> where Theader : new()
    {
        internal ClientCollection(IList<TcpNetClient<Theader>> list) : base(list)
        {
        }
    }
    public sealed class ClientCollection<Theader, Tfooter> : ReadOnlyCollection<TcpNetClient<Theader, Tfooter>> where Theader : new() where Tfooter : new()
    {
        internal ClientCollection(IList<TcpNetClient<Theader, Tfooter>> list) : base(list)
        {
        }
    }
}
