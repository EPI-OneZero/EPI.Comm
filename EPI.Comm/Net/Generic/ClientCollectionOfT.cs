using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;

namespace EPI.Comm.Net.Generic
{
    public class ClientCollection<THeader> : ReadOnlyCollection<TcpNetClient<THeader>> where THeader : new()
    {
        protected internal ClientCollection(IList<TcpNetClient<THeader>> list) : base(list)
        {
        }
        public TcpNetClient<THeader> this[IPEndPoint remoteEndpoint] => this.FirstOrDefault(c => c?.RemoteEndPoint == remoteEndpoint);
    }
    public class ClientCollection<THeader, TFooter> : ReadOnlyCollection<TcpNetClient<THeader, TFooter>> where THeader : new() where TFooter : new()
    {
        protected internal ClientCollection(IList<TcpNetClient<THeader, TFooter>> list) : base(list)
        {
        }
        public TcpNetClient<THeader, TFooter> this[IPEndPoint remoteEndpoint] => this.FirstOrDefault(c => c?.RemoteEndPoint == remoteEndpoint);
    }
}
