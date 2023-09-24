using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EPI.Comm.Utils
{
    internal static class ObjectUtil
    {

        /// <summary>
        /// https://learn.microsoft.com/ko-kr/dotnet/api/system.runtime.interopservices.ObjectUtil.SizeOf?view=netframework-4.7.2
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int SizeOf<T> ()
        {
            return Marshal.SizeOf<T>();
        }
    }
}
