using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;

namespace EPI.Comm.Net
{
    public class ClientCollection : ReadOnlyCollection<TcpNetClient>
    {
        internal ClientCollection(IList<TcpNetClient> list) : base(list)
        {
        }
        public TcpNetClient this[IPEndPoint remoteEndpoint] => this.FirstOrDefault(c => c?.RemoteEndPoint == remoteEndpoint);
    }
}
