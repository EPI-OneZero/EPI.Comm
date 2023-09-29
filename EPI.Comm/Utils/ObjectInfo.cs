using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace EPI.Comm.Utils
{
    internal class MarshalTypeInfo
    {
        private object obj;
        private Type type;
        public List<MarshalFieldInfo> FieldInfos { get; private set; }
        public MarshalTypeInfo(object obj)
        {
            this.obj = obj;
            type = obj?.GetType();
            InitFieldInfos();
        }
        private void InitFieldInfos()
        {
            FieldInfos = new List<MarshalFieldInfo>();
            var fieldInfos = TypeUtil.GetFields(type);
            foreach (var info in fieldInfos)
            {
                FieldInfos.Add(new MarshalFieldInfo(obj, info));
            }
        }
    }
    internal class MarshalFieldInfo
    {
        private object obj;
        private FieldInfo field;
        public Type ObjectType => obj?.GetType();
        public Type Type => field.FieldType;
        public string Name => field.Name;
        public int Offset { get; private set; }
        public int ByteSize { get; private set; }
        public bool IsArrayType => Type.IsArray;
        public MarshalFieldInfo(object obj, FieldInfo fi)
        {
            this.obj = obj;
            field = fi;
            Offset = (int)Marshal.OffsetOf(ObjectType, field.Name);
        }
    }
}
