using System;
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
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="srcBytes"></param>
        /// <param name="targetSize"></param>
        /// <param name="srcOffset"></param>
        /// <param name="caller"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        internal static T Deserialize<T>(byte[] srcBytes, int targetSize, int srcOffset, [CallerMemberName] string caller = "")
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
                throw new IndexOutOfRangeException($"{caller} : {nameof(srcBytes)}");
            }
        }

        internal static void Serialize<T>(T src, byte[] dst, int dstOffset, int srcSize)
        {
            if (IsEnoughSize(srcSize, dst.Length - dstOffset, 0))
            {
                var handle = GCHandle.Alloc(src, GCHandleType.Pinned);

                Marshal.Copy(handle.AddrOfPinnedObject(), dst, dstOffset, srcSize);
            }
            else
            {
                throw new IndexOutOfRangeException(nameof(srcSize));
            }
          
        }

    }
}
