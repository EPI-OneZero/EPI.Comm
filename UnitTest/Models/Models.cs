using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

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
            var random = new Random((int)DateTime.Now.Ticks);
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
        public override string ToString()
        {
            return $"{Src:X8} \t{Dst:X8}\t {Code:X8}\t {BodySize:X8}\r\n";
        }
    }
    public class PacketWithHeader : IEquatable<PacketWithHeader>
    {
        public byte[] FullPacket { get; set; }
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
            Thread.Sleep(10);
            var header = Header.GetRandom();
            Header = header;
            var body = new byte[header.BodySize];
            var random = new Random((int)DateTime.Now.Ticks);
            random.NextBytes(body);
            Body = body;
        }
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append($"헤더 : {Header}");
            builder.Append("바디");
            AddBytesTostring(builder, Body);
            AddTostring(builder);
            builder.Append($"전체 수신 바이트 : {FullPacket.Length}");
            AddBytesTostring(builder, FullPacket);
            return builder.ToString();
        }
        private void AddBytesTostring(StringBuilder builder, byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                if (i % 8 == 0)
                {
                    builder.AppendLine();
                }
                builder.Append(bytes[i].ToString("X2"));
                builder.Append("\t");
            }
            builder.AppendLine();
        }
        protected virtual void AddTostring(StringBuilder builder)
        {

        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public class Msg
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
        public string Message;
        public Msg(string msg)
        {
            Message = msg;
        }
        public override string ToString()
        {
            return Message;
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public class Footer //: IEquatable<Footer>
    {
        public enum MyEnum : ushort
        {
            AABB = 0xAABB,
        }
        public Msg Message = new Msg("한글");
        public MyEnum My { get; set; } = MyEnum.AABB;
        public ushort Etx { get; set; }
        public static Footer Get()
        {
            var result = new Footer();
            result.Etx = 0xeecc;
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
        public override string ToString()
        {
            return $" {Message}\t {My}\t {((short)Etx):x4}";
        }
    }
    public class PacketWithHeaderFooter : PacketWithHeader, IEquatable<PacketWithHeaderFooter>
    {
        public Footer Footer { get; set; }

        public override void SetRandom()
        {
            base.SetRandom();
            Footer = Footer.Get();
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
        protected override void AddTostring(StringBuilder builder)
        {
            base.AddTostring(builder);
            builder.AppendLine($"푸터 : {Footer}");
        }
    }
}
