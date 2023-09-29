using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace EPI.Comm.Utils
{
    internal sealed class MarshalTypeInfo
    {
        private readonly Type Type;
        public int Size { get; private set; }
        public List<MarshalFieldInfo> FieldInfos { get; private set; }
        public MarshalTypeInfo(Type objType)
        {
            Type = objType;
            Size = Marshal.SizeOf(Type);
            if (!TypeUtil.IsBasicType(objType))
            {
                InitFieldInfos();
            }
        }
        private void InitFieldInfos()
        {
            FieldInfos = new List<MarshalFieldInfo>();
            var fieldInfos = GetFields(Type)
                .Select(info => new MarshalFieldInfo(info))
                .OrderBy(f => f.Offset);

            FieldInfos.AddRange(fieldInfos);
        }
        public static FieldInfo[] GetFields(Type type)
        {
            var result = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return result;
        }

        public void ReverseEndian(byte[] bytes, int offset)
        {

        }
    }
    internal sealed class MarshalFieldInfo
    {
        public FieldInfo FieldInfo { get; private set; }
        public Type OwnerType { get; private set; }
        public Type FieldType { get; private set; }
        public MarshalTypeInfo TypeInfo { get; set; }
        public string Name { get; private set; }
        public int Offset { get; private set; }
        public int Size { get; private set; }
        public MarshalFieldInfo(FieldInfo info)
        {
            OwnerType = info.ReflectedType;
            FieldInfo = info;
            FieldType = info.FieldType;
            Name = info.Name;
            Offset = (int)Marshal.OffsetOf(OwnerType, FieldInfo.Name);
            SetSize();
            if (!TypeUtil.IsBasicType(FieldType) && !FieldType.IsArray)
            {
                TypeInfo = new MarshalTypeInfo(FieldType);
            }
            else if(FieldType.IsArray)
            {

            }
        }
        private void SetSize()
        {
            try
            {
                Size = Marshal.SizeOf(FieldType);
            }
            catch (ArgumentException)
            {
                var marshalAs = FieldInfo.GetCustomAttribute(typeof(MarshalAsAttribute)) as MarshalAsAttribute;

                if (marshalAs != null)
                {
                    Size = marshalAs.SizeConst = Marshal.SizeOf(FieldType);
                }
            }
        }
        public void ReverseEndian(byte[] bytes, int offset)
        {
            offset += Offset;

        }
    }
}
