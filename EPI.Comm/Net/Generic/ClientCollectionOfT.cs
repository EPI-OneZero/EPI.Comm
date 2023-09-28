using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;

namespace EPI.Comm.Net.Generic
{
    public sealed class ClientCollection<Theader> : ReadOnlyCollection<TcpNetClient<Theader>> where Theader : new()
    {
        internal ClientCollection(IList<TcpNetClient<Theader>> list) : base(list)
        {
        }
        public TcpNetClient<Theader> this[IPEndPoint remoteEndpoint] => this.FirstOrDefault(c => c?.RemoteEndPoint == remoteEndpoint);
    }
    public sealed class ClientCollection<Theader, Tfooter> : ReadOnlyCollection<TcpNetClient<Theader, Tfooter>> where Theader : new() where Tfooter : new()
    {
        internal ClientCollection(IList<TcpNetClient<Theader, Tfooter>> list) : base(list)
        {
        }
        public TcpNetClient<Theader, Tfooter> this[IPEndPoint remoteEndpoint] => this.FirstOrDefault(c => c?.RemoteEndPoint == remoteEndpoint);
    }
}
