using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using System.Runtime.InteropServices;

namespace EPI.Comm.Utils
{
    internal interface IReverseEndian
    {
        void ReverseEndian(byte[] bytes);
    }
    internal abstract class MarshalBaseModel
    {
        private static readonly Dictionary<Type, MarshalBaseModel> MarshalNodes = new Dictionary<Type, MarshalBaseModel>();
        public abstract void ReverseEndian(byte[] bytes);
        public int Offset { get; internal set; }
        public int Size { get; internal set; }
        internal static MarshalBaseModel Create(Type type)
        {
            if(!MarshalNodes.ContainsKey(type))
            {
                var item = CreateSingleType(type, 0);
                MarshalNodes.Add(type, item);
            }
            return MarshalNodes[type];
        }
        private static MarshalSingleModel CreateSingleType(Type type, int offset)
        {
            var result = new MarshalSingleModel(type);
            result.Offset = offset;
            result.Size= Marshal.SizeOf(type);
            if (!type.IsPrimitive)
            {
                var fields = from field in GetFields(type)
                             let off = (int)Marshal.OffsetOf(type, field.Name)
                             select (field, off);
                foreach (var fi in fields)
                {
                    var fieldType = fi.field.FieldType;
                    if (IsArrayType(fieldType))
                    {
                        result.TypeNodes.Add(CreateArrayType(fi.field, fi.off + offset));
                    }
                    else
                    {
           
                        result.TypeNodes.Add(CreateSingleType(fieldType, fi.off + offset));
                    }
                }
           
            }
            return result;
        }
        private static MarshalArrayModel CreateArrayType(FieldInfo fieldInfo, int offset)
        {
            var ienumerable = fieldInfo.FieldType.GetInterface(typeof(IEnumerable<>).Name);
            var sizeConst = fieldInfo.GetCustomAttribute<MarshalAsAttribute>().SizeConst;
            var itemType = ienumerable.GenericTypeArguments[0];
            var itemMarshalType = CreateSingleType(itemType, offset);

            var result = new MarshalArrayModel(itemMarshalType, sizeConst);

            result.Offset = offset;
            return result;
        }
        private static bool IsArrayType(Type t)
        {
            if (t.IsArray||t == typeof(string))
            {
                return true;
            }
            return false;
        }
        public static FieldInfo[] GetFields(Type type)
        {
            var result = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return result;
        }
    }
    internal sealed class MarshalSingleModel : MarshalBaseModel
    {
        public Type Type { get; set; }

        public List<MarshalBaseModel> TypeNodes { get; set; } = new List<MarshalBaseModel>();
        public MarshalSingleModel(Type t)
        {
            Type = t;
        }
        public override void ReverseEndian(byte[] bytes)
        {
            if (TypeNodes.Count == 0 && Size > 1)
            {
                Array.Reverse(bytes, Offset, Size);
            }
            foreach (var item in TypeNodes)
            {
                item.ReverseEndian(bytes);
            }
        }
    }
    internal sealed class MarshalArrayModel : MarshalBaseModel
    {
        public MarshalSingleModel ItemType { get; set; }
        public int Count { get; set; }
        public MarshalArrayModel(MarshalSingleModel itemMarshalType, int count)
        {
            ItemType = itemMarshalType;
            Count = count;
        }

        public override void ReverseEndian(byte[] bytes)
        {
            var offset = Offset;
            for (int i = 0; i < Count; i++)
            {
                ItemType.ReverseEndian(bytes);
                ItemType.Offset += ItemType.Size;
            }
            ItemType.Offset = offset;
        }
    }
}
