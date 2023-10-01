using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace EPI.Comm.Utils
{
    public static class MarshalSerializer
    {
        private static readonly Dictionary<Type, EndianInfo[]> EndianInfos = new Dictionary<Type, EndianInfo[]>();
        public static bool IsEnoughSizeToDeserialize(int sourceSize, int dstSize)
        {
            return sourceSize >= dstSize;
        }
        public static T Deserialize<T>(byte[] srcBytes)
        {
            var handle = GCHandle.Alloc(srcBytes, GCHandleType.Pinned);
            var result = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            handle.Free();
            return result;
        }
        public static bool IsEnoughSizeToSerialize(int sourceSize, int dstSize, int dstOffset)
        {
            return sourceSize <= dstSize - dstOffset;
        }
        public static void Serialize<T>(T src, byte[] dst, int dstOffset, int srcSize)
        {
            if (IsEnoughSizeToSerialize(srcSize, dst.Length, dstOffset))
            {
                var handle = GCHandle.Alloc(dst, GCHandleType.Pinned);
                Marshal.StructureToPtr(src, handle.AddrOfPinnedObject() + dstOffset, false);
                Marshal.DestroyStructure(handle.AddrOfPinnedObject() + dstOffset, typeof(T));
                handle.Free();
            }
            else
            {
                throw new IndexOutOfRangeException($"{nameof(srcSize)}");
            }
        }
        public static void ReverseEndian<T>(byte[] bytes, int offset)
        {
            var type = typeof(T);
            if (!EndianInfos.ContainsKey(type))
            {
                EndianInfos.Add(type, MarshalNodeBase.Create(typeof(T)));
            }

            var infos = EndianInfos[type];

            for (int i = 0; i < infos.Length; i++)
            {
                Array.Reverse(bytes, infos[i].Offset + offset, infos[i].Size);
            }
        }

    }
}
