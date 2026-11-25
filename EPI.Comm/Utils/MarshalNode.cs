using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace EPI.Comm.Utils
{
    public abstract class MarshalNodeBase
    {
        public abstract void GenerateInfo(List<EndianInfo> infos);
        public int Offset { get; internal set; }

        public static EndianInfo[] Create(Type type)
        {
            var item = new MarshalNode(type, 0, type.IsUnicodeClass);
            var result = new List<EndianInfo>();
            item.GenerateInfo(result);
            return result.ToArray();
        }
    }
    internal sealed class MarshalNode : MarshalNodeBase
    {
        public Type Type { get; set; }
        public int Size { get; internal set; }
        public List<MarshalNodeBase> TypeNodes { get; set; } = new List<MarshalNodeBase>();
        public MarshalNode(Type t, int offset, bool isUnicodeClass, UnmanagedType? unmanagedType = null)
        {
            Offset = offset;
            Type = !t.IsEnum ? t : Enum.GetUnderlyingType(t);

            Size = GetSize(Type, isUnicodeClass, unmanagedType);
            if (!Type.IsPrimitive)
                InitSubNodes(Type.IsUnicodeClass);
        }
        private static int GetSize(Type t, bool isUnicodeClass, UnmanagedType? unmanagedType)
        {
            int result;
            if (t == typeof(char))
            {
                if (unmanagedType.HasValue && unmanagedType.Value != 0x00)
                {
                    result = GetCharSize(unmanagedType.Value);
                }
                else if (isUnicodeClass)
                {
                    result = 2;
                }
                else
                {
                    result = 1;
                }
            }
            else if (t == typeof(bool) && unmanagedType.HasValue && IsSize1(unmanagedType.Value))
            {
                result = 1;
            }
            else
            {
                result = Marshal.SizeOf(t);
            }
            return result;
        }
        private static int GetCharSize(UnmanagedType unmanagedType)
        {
            switch (unmanagedType)
            {
                case UnmanagedType.I2:
                case UnmanagedType.U2:
                    return 2;
                case UnmanagedType.I1:
                case UnmanagedType.U1:
                default:
                    return 1;
            }
        }
        private static bool IsSize1(UnmanagedType unmanagedType)
        {
            switch (unmanagedType)
            {
                case UnmanagedType.I1:
                case UnmanagedType.U1:
                    return true;
                default:
                    return false;
            }
        }
   
        private void InitSubNodes(bool isUnicodeClass)
        {
            foreach (var field in GetFieldInfos(Type))
            {
                var fieldType = field.FieldType;
                var fieldOffset = Offset + Marshal.OffsetOf(Type, field.Name).ToInt32();
                var marshal = field.GetCustomAttribute<MarshalAsAttribute>();

                if (fieldType.IsArray || fieldType == typeof(string))
                {
                    TypeNodes.Add(new MarshalArrayNode(fieldType, fieldOffset, isUnicodeClass, marshal));
                }
                else
                {
                    TypeNodes.Add(new MarshalNode(fieldType, fieldOffset, isUnicodeClass, marshal?.Value));
                }
            }
        }

        private static FieldInfo[] GetFieldInfos(Type type)
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
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

    }
    internal sealed class MarshalArrayNode : MarshalNodeBase
    {
        public MarshalNode ItemType { get; set; }
        public int Count { get; set; }
        public MarshalArrayNode(Type fieldType, int offset, bool isUnicodeClass, MarshalAsAttribute marshal)
        {
            int sizeConst = marshal.SizeConst;

            var ienumerable = fieldType.GetInterface(typeof(IEnumerable<>).Name);
            var itemType = ienumerable.GenericTypeArguments[0];
            ItemType = new MarshalNode(itemType, offset, isUnicodeClass, marshal?.ArraySubType);
            Count = sizeConst;
            Offset = offset;
        }
        public override void GenerateInfo(List<EndianInfo> infos)
        {
            var itemInfos = new List<EndianInfo>();
            ItemType.GenerateInfo(itemInfos);

            for (int i = 0; i < Count; i++)
            {
                foreach (var info in itemInfos)
                {
                    infos.Add(new EndianInfo
                    {
                        Offset = info.Offset + i * ItemType.Size,
                        Size = info.Size
                    });
                }
            }
        }
    }
    public struct EndianInfo
    {
        public int Size;
        public int Offset;
    }
}
