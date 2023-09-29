using System;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;

namespace EPI.Comm.Utils
{
    public static class ObjectUtil
    {
        public static int SizeOf<T>()
        {
            return Marshal.SizeOf<T>();
        }
        public static void ReverseEndian<T>(T t)
        {
            var fields = GetFields(t.GetType());
        }
        private static void Test()
        {
            int x=1;
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
