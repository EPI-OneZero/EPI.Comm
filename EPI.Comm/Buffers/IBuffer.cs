using System.Collections.Generic;

namespace EPI.Comm.Buffers
{

    public interface IBuffer : IEnumerable<byte>
    {
        int Count { get; }
        byte[] GetBytes(int count);

        void AddBytes(byte[] bytes);
        void Clear();
    }
}
