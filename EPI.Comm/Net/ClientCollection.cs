using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EPI.Comm.Net
{
    public sealed class ClientCollection : ReadOnlyCollection<TcpNetClient>
    {
        internal ClientCollection(IList<TcpNetClient> list) : base(list)
        {
        }
    }
}
