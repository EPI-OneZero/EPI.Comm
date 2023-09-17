namespace EPI.Comm.Net.Generic
{
    public class Packet<Theader, Tfooter> : Packet<Theader>
    {
        public Tfooter Footer { get; internal set; }
        public Packet()
        {

        }
    }
    public class Packet<Theader>
    {
        public Theader Header { get; internal set; }
        public byte[] Body { get; internal set; }
    }
    public struct EmptyFooter
    {
    }
}
