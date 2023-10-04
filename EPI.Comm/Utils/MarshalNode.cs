﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace EPI.Comm.Utils
{
    internal abstract class MarshalNodeBase
    {
        public abstract void GenerateInfo(List<EndianInfo> infos);
        public int Offset { get; internal set; }

        internal static EndianInfo[] Create(Type type)
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
        public MarshalNode(Type t, int offset, bool isUnicodeClass)
        {
            Offset = offset;
            Type = !t.IsEnum ? t : Enum.GetUnderlyingType(t);
            if (Type == typeof(char) && isUnicodeClass)
            {
                Size = 2;
            }
            else
            {
                Size = Marshal.SizeOf(Type);
            }
            if (!Type.IsPrimitive)
                InitSubNodes(Type.IsUnicodeClass);
        }
        class InfoPair
        {
            public InfoPair(FieldInfo info, int off)
            {
                this.info = info;
                this.off = off;
            }
            public FieldInfo info;
            public int off;

        }
        private void InitSubNodes(bool isUnicodeClass)
        {
            var fields = from info in GetFieldInfos(Type)
                         let off = (int)Marshal.OffsetOf(Type, info.Name)
                         select new InfoPair(info, off);
            foreach (var field in fields)
            {
                var fieldType = field.info.FieldType;
                var fieldOffset = field.off + Offset;
                if (fieldType.IsArray || fieldType == typeof(string))
                {
                    TypeNodes.Add(new MarshalArrayNode(field.info, fieldOffset, isUnicodeClass));
                }
                else
                {

                    TypeNodes.Add(new MarshalNode(fieldType, fieldOffset, isUnicodeClass));
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
        public MarshalArrayNode(FieldInfo fieldInfo, int offset, bool isUnicodeClass)
        {
            int sizeConst = fieldInfo.GetCustomAttribute<MarshalAsAttribute>().SizeConst;

            var ienumerable = fieldInfo.FieldType.GetInterface(typeof(IEnumerable<>).Name);
            var itemType = ienumerable.GenericTypeArguments[0];
            ItemType = new MarshalNode(itemType, offset, isUnicodeClass);
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
