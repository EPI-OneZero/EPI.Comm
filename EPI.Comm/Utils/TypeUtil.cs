using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace EPI.Comm.Utils
{
    public static class TypeUtil
    {
       
        internal static bool IsBasicType(Type type)
        {
            if (type.IsPrimitive || type == typeof(string))
            {
                return true;
            }
            return false;
        }
    }
}
