using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace EPI.Comm.Buffers
{
    internal interface IBuffer : IEnumerable<byte>
    {
        int Count { get; }
        byte[] GetBytes(int count);

        void AddBytes(byte[] bytes);
        void Clear();
    }
}
