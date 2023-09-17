using System.Runtime.InteropServices;

namespace EPI.Comm.Net.Generic
{
    public class Packet<Theader, Tfooter> : Packet<Theader>
    {
        public Tfooter Footer { get; internal set; }
       
    }
    public class Packet<Theader>
    {
        public Theader Header { get; internal set; }
        public byte[] Body { get; internal set; }
       
        
        public virtual int FullSize => Marshal.SizeOf(Header) + Body?.Length ?? 0;
    }
  
}
