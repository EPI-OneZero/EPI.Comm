using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EPI.Comm.Utils
{
    internal static class ObjectUtil
    {

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int SizeOf<T> ()
        {
            return Marshal.SizeOf<T>();
        }
    }
}
