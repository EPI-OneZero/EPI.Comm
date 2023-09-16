namespace EPI.Comm.Tcp.Generic
{
    public class Packet<Theader, Tfooter>
    {
        public Theader Header { get; internal set; }
        public byte[] Body { get; internal set; }
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
}
