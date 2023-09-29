using System;
using System.Runtime.InteropServices;

namespace EPI.Comm.Utils
{
    internal static class MarshalSerializer
    {
        internal static bool IsEnoughSizeToDeserialize(int sourceSize, int targetSize)
        {
            return sourceSize >= targetSize;
        }
        internal static T Deserialize<T>(byte[] srcBytes)
        {
            var handle = GCHandle.Alloc(srcBytes, GCHandleType.Pinned);
            var result = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            handle.Free();
            return result;
        }
        internal static bool IsEnoughSizeToSerialize(int sourceSize, int targetSize, int dstOffset)
        {
            return sourceSize <= targetSize - dstOffset;
        }
        internal static void Serialize<T>(T src, byte[] dst, int dstOffset, int srcSize)
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

    }
}
