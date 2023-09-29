using System;
using System.Reflection;
using System.Runtime.InteropServices;

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
        internal static bool IsBasicType(Type type)
        {
            if(type.IsPrimitive || type == typeof(string))
            {
                return true;
            }
            return false;
        }
    }
}
