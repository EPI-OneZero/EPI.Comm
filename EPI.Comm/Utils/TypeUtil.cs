using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace EPI.Comm.Utils
{
    public static class TypeUtil
    {
        public static int SizeOf<T>()
        {
            return Marshal.SizeOf<T>();
        }
        public static int SizeOf(Type type)
        {
            return Marshal.SizeOf(type);
        }
        public static FieldInfo[] GetFields(Type type)
        {
            var result = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return result;
        }
        public static bool IsBasicType(object obj)
        {
            switch (Type.GetTypeCode(obj.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Char:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Double:
                case TypeCode.Single:

                    return true;
                default:
                    return false;
            }
        }
    }
}
