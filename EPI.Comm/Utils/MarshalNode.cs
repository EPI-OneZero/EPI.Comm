using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace EPI.Comm.Utils
{
    internal abstract class MarshalNodeBase : IDisposable
    {
     
        public void ReverseEndian(byte[] bytes)
        {

        }
        public abstract void GenerateInfo(List<EndianInfo> infos);

        public int Offset { get; internal set; }
        public int Size { get; internal set; }
        internal static EndianInfo[] Create(Type type)
        {
            var item = new MarshalNode(type, 0);
            var result = new List<EndianInfo>();
            item.GenerateInfo(result);
            item.Dispose();
            return result.ToArray();
        }
        public static FieldInfo[] GetFields(Type type)
        {
            var result = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return result;
        }
        internal static Type ConvertEnumToNumType(Type t)
        {
            return !t.IsEnum ? t : Enum.GetUnderlyingType(t);
        }
        public void Dispose()
        {
        }
    }
    internal sealed class MarshalNode : MarshalNodeBase
    {
        public Type Type { get; set; }

        public List<MarshalNodeBase> TypeNodes { get; set; } = new List<MarshalNodeBase>();
        public MarshalNode(Type t, int offset)
        {
            Offset = offset;
            Type = ConvertEnumToNumType(t);

            Size = Marshal.SizeOf(Type);
            if (!Type.IsPrimitive)
                InitSubNodes();
        }
        private void InitSubNodes()
        {
            var fields = from field in GetFields(Type)
                         let off = (int)Marshal.OffsetOf(Type, field.Name)
                         select (field, off);
            foreach (var fi in fields)
            {
                var fieldType = fi.field.FieldType;
                if (IsArrayType(fieldType))
                {
                    TypeNodes.Add(new MarshalArrayNode(fi.field, fi.off + Offset));
                }
                else
                {

                    TypeNodes.Add(new MarshalNode(fieldType, fi.off + Offset));
                }
            }
        }
        public override void GenerateInfo(List<EndianInfo> infos)
        {
            if (TypeNodes.Count == 0 && Size > 1)
            {
                infos.Add(new EndianInfo() { Offset = Offset, Size = Size });
            }
            foreach (var item in TypeNodes)
            {
                item.GenerateInfo(infos);
            }
        }
        private static bool IsArrayType(Type t)
        {
            if (t.IsArray || t == typeof(string))
            {
                return true;
            }
            return false;
        }
    }
    internal sealed class MarshalArrayNode : MarshalNodeBase
    {
        public MarshalNode ItemType { get; set; }
        public int Count { get; set; }
        public MarshalArrayNode(FieldInfo fieldInfo, int offset)
        {
            var ienumerable = fieldInfo.FieldType.GetInterface(typeof(IEnumerable<>).Name);
            var sizeConst = fieldInfo.GetCustomAttribute<MarshalAsAttribute>().SizeConst;
            var itemType = ienumerable.GenericTypeArguments[0];
            ItemType = new MarshalNode(itemType, offset);
            Count = sizeConst;
            Offset = offset;
        }
        public override void GenerateInfo(List<EndianInfo> infos)
        {
            var offset = Offset;
            for (int i = 0; i < Count; i++)
            {
                ItemType.GenerateInfo(infos);
                ItemType.Offset += ItemType.Size;
            }
            ItemType.Offset = offset;
        }
    }
    internal struct EndianInfo
    {
        public int Size;
        public int Offset;
    }
}
