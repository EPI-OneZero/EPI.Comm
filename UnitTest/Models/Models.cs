using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace UnitTest.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header : IEquatable<Header>
    {
        public int Src { get; set; }
        public int Dst { get; set; }
        public int Code { get; set; }
        public int BodySize { get; set; }
        public Header()
        {
        }
        public static Header GetRandom()
        {
            var random = new Random();
            var result = new Header();
            result.Src = random.Next();
            result.Dst = random.Next();
            result.Code = random.Next();
            result.BodySize = random.Next(0, 10);
            return result;
        }
        public static int GetBodySize(Header header)
        {
            return header.BodySize;
        }
        public bool Equals(Header other)
        {
            if (Src.Equals(other?.Src)
                && Dst.Equals(other?.Dst)
                && Code.Equals(other?.Code)
                && BodySize.Equals(other?.BodySize))
            {
                return false;
            }
            return true;
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as Header);
        }

        public override int GetHashCode()
        {
            int hashCode = 297015005;
            hashCode = hashCode * -1521134295 + Src.GetHashCode();
            hashCode = hashCode * -1521134295 + Dst.GetHashCode();
            hashCode = hashCode * -1521134295 + Code.GetHashCode();
            hashCode = hashCode * -1521134295 + BodySize.GetHashCode();
            return hashCode;
        }
    }
    public class PacketWithHeader : IEquatable<PacketWithHeader>
    {
        public Header Header { get; set; }
        public byte[] Body { get; set; }

        public bool Equals(PacketWithHeader other)
        {
            if (Header.Equals(other) && Enumerable.SequenceEqual(Body, other?.Body ?? new byte[0]))
            {
                return true;
            }
            return false;
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as PacketWithHeader);
        }

        public override int GetHashCode()
        {
            int hashCode = -306108907;
            hashCode = hashCode * -1521134295 + EqualityComparer<Header>.Default.GetHashCode(Header);
            hashCode = hashCode * -1521134295 + EqualityComparer<byte[]>.Default.GetHashCode(Body);
            return hashCode;
        }

        public virtual void SetRandom()
        {
            var header = Header.GetRandom();
            Header = header;
            var body = new byte[header.BodySize];
            var random = new Random();
            random.NextBytes(body);
            Body = body;
        }

    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Footer //: IEquatable<Footer>
    {
        public ushort Etx { get; set; }
        public static Footer GetRandom()
        {
            var result = new Footer();
            var random = new Random();
            result.Etx = (ushort)random.Next(ushort.MinValue, ushort.MaxValue + 1);
            return result;
        }
        public override bool Equals(object obj)
        {

            return Equals(obj as Footer);
        }
        public bool Equals(Footer other)
        {
            return Etx.Equals(other?.Etx);
        }
        public override int GetHashCode()
        {
            return 316244556 + Etx.GetHashCode();
        }
    }
    public class PacketWithHeaderFooter : PacketWithHeader, IEquatable<PacketWithHeaderFooter>
    {
        public Footer Footer { get; set; }

        public override void SetRandom()
        {
            base.SetRandom();
            Footer = Footer.GetRandom();
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as PacketWithHeaderFooter);
        }

        public bool Equals(PacketWithHeaderFooter other)
        {
            if (base.Equals(other))
            {
                return Footer.Equals(other.Footer);
            }
            else return false;
        }

        public override int GetHashCode()
        {
            int hashCode = 2089734760;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Footer>.Default.GetHashCode(Footer);
            return hashCode;
        }
    }
}
