using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace EPI.Comm.Utils
{
    internal class MarshalTypeInfo
    {
        private Type type;
        public int Size { get; private set; }
        public List<MarshalFieldInfo> FieldInfos { get; private set; }
        public MarshalTypeInfo(Type objType)
        {
            type = objType;
            Size = TypeUtil.SizeOf(type);
            if (!TypeUtil.IsBasicType(objType))
            {
                InitFieldInfos();
            }
        }
        private void InitFieldInfos()
        {
            FieldInfos = new List<MarshalFieldInfo>();
            var fieldInfos = TypeUtil.GetFields(type);
            foreach (var info in fieldInfos)
            {
                FieldInfos.Add(new MarshalFieldInfo(type, info));
            }
        }
    }
    internal class MarshalFieldInfo
    {
        private FieldInfo fieldInfo;
        public Type ObjectType { get; private set; }
        public Type FieldType { get; private set; }
        public MarshalTypeInfo TypeInfo { get; set; }
        public string Name { get; private set; }
        public int Offset { get; private set; }
        public int ByteSize { get; private set; }
        public bool IsArrayType => FieldType.IsArray;
        public MarshalFieldInfo(Type objType, FieldInfo info)
        {
            ObjectType = objType;
            fieldInfo = info;
            FieldType = info.FieldType;
            Name = info.Name;
            Offset = (int)Marshal.OffsetOf(ObjectType, fieldInfo.Name);
        }
    }
}
