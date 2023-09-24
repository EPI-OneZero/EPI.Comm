using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EPI.Comm.Net.Generic.Packets
{
    /// <summary>
    /// https://learn.microsoft.com/ko-kr/dotnet/api/system.runtime.interopservices.gchandle.alloc?view=netframework-4.7.2
    /// https://learn.microsoft.com/ko-kr/dotnet/api/system.runtime.interopservices.marshal.ptrtostructure?view=netframework-4.7.2
    /// https://learn.microsoft.com/ko-kr/dotnet/api/system.runtime.interopservices.marshal.copy?view=netframework-4.7.2
    /// </summary>
    internal static class PacketSerializer
    {
        internal static bool IsEnoughSize(int sourceSize, int targetSize, int srcOffset)
        {
            return sourceSize - srcOffset >= targetSize;
        }
        internal static T DeserializeByMarshal<T>(byte[] srcBytes, int targetSize, int srcOffset, [CallerMemberName] string caller = "")
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

        internal static void SerializeByMarshal<T>(T src, byte[] dst, int dstOffset, int srcSize)
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
