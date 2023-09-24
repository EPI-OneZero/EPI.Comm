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
  
}
