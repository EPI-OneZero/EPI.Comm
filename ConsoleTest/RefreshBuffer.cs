using System;
using System.Collections;
using System.Collections.Generic;

namespace EPI.Comm.Buffers
{

    internal sealed class RefreshBuffer : IBuffer
    {
        private byte[] buffer;
        private int offset;

        public RefreshBuffer()
        {
        }
        #region IBuffer
        public int Count
        {
            get
            {
                if (buffer == null)
                {
                    return 0;
                }
                else
                {
                    return buffer.Length - offset;
                }
            }
        }
        public void AddBytes(byte[] bytes)
        {
            offset = 0;
            buffer = bytes;

        }
        public byte[] GetBytes(int count)
        {

            var result = new byte[count];
            Buffer.BlockCopy(buffer, offset, result, 0, count);
            offset += count;
            return result;

        }
        #endregion

        #region IEnumerable
        public IEnumerator<byte> GetEnumerator()
        {
            IEnumerable<byte> enumrator = buffer ?? Array.Empty<byte>();
            return enumrator.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            buffer = Array.Empty<byte>();
            offset = 0;
        }
        #endregion
    }
}
