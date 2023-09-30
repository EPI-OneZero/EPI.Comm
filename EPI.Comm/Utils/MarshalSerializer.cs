using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace EPI.Comm.Utils
{
    public static class MarshalSerializer
    {
        public static bool IsEnoughSizeToDeserialize(int sourceSize, int targetSize)
        {
            return sourceSize >= targetSize;
        }
        public static T Deserialize<T>(byte[] srcBytes)
        {
            var handle = GCHandle.Alloc(srcBytes, GCHandleType.Pinned);
            var result = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            handle.Free();
            return result;
        }
        public static bool IsEnoughSizeToSerialize(int sourceSize, int targetSize, int dstOffset)
        {
            return sourceSize <= targetSize - dstOffset;
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
        public static void ReverseEndian<T>(byte[] bytes)
        {
            var node = MarshalBaseModel.Create(typeof(T));

            node.ReverseEndian(bytes);
        }

    }
}
