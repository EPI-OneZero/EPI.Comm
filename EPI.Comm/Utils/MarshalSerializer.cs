using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace EPI.Comm.Utils
{
    public static class MarshalSerializer
    {
        private static readonly ConcurrentDictionary<Type, EndianInfo[]> EndianInfos = new ConcurrentDictionary<Type, EndianInfo[]>();
        public static T Deserialize<T>(byte[] srcBytes)
        {
            T result;
            var handle = GCHandle.Alloc(srcBytes, GCHandleType.Pinned);
            try
            {
                result = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }

            return result;
        }
        public static void Serialize<T>(T src, byte[] dst, int dstOffset)
        {
            var handle = GCHandle.Alloc(dst, GCHandleType.Pinned);
            try
            {
                var ptr = handle.AddrOfPinnedObject() + dstOffset;
                Marshal.StructureToPtr(src, ptr, false);
                Marshal.DestroyStructure(ptr, typeof(T));

            }
            finally 
            {
                handle.Free();
            }

        }
        public static void ReverseEndian<T>(byte[] bytes, int offset)
        {
            var infos = EndianInfos.GetOrAdd(typeof(T), MarshalNodeBase.Create);

            for (int i = 0; i < infos.Length; i++)
            {
                Array.Reverse(bytes, infos[i].Offset + offset, infos[i].Size);
            }
        }
    }
}
