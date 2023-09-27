using System;
using System.Data.SqlTypes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EPI.Comm.Net.Generic.Packets
{
    internal static class PacketSerializer
    {
        internal static bool IsEnoughSize(int sourceSize, int targetSize, int srcOffset)
        {
            return sourceSize - srcOffset >= targetSize;
        }
        internal static T DeserializeByMarshal<T>(byte[] srcBytes, int targetSize, int srcOffset)
        {
            if (IsEnoughSize(srcBytes?.Length ?? 0, targetSize, srcOffset))
            {
                var handle = GCHandle.Alloc(srcBytes, GCHandleType.Pinned);
                var result = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject() + srcOffset);
                handle.Free();
                return result;
            }
            else
            {
                throw new IndexOutOfRangeException($"{nameof(srcBytes)}");
            }
        }
        internal static void SerializeByMarshal<T>(T src, byte[] dst, int dstOffset, int srcSize)
        {
            if (IsEnoughSize(srcSize, dst.Length - dstOffset, 0))
            {
                var handle = GCHandle.Alloc(src, GCHandleType.Pinned);

                Marshal.Copy(handle.AddrOfPinnedObject(), dst, dstOffset, srcSize);
            }
            else
            {
                throw new IndexOutOfRangeException($"{nameof(srcSize)}");
            }
          
        }

    }
}
