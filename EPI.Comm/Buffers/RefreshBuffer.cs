using System;
using System.Collections;
using System.Collections.Generic;

namespace EPI.Comm.Buffers
{

    internal class RefreshBuffer : IBuffer
    {
        protected byte[] buffer;
        protected int offset = 0;

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

            if (buffer == null || buffer.Length < count + offset)
            {
                throw new IndexOutOfRangeException("count");
            }
            else
            {
                var result = new byte[count];
                Buffer.BlockCopy(buffer, offset, result, 0, count);
                offset += count;
                return result;
            }

        }
        #endregion

        #region IEnumerable
        public IEnumerator<byte> GetEnumerator()
        {
            IEnumerable<byte> ienum = buffer ?? new byte[0];
            return ienum?.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            buffer = new byte[0];
            offset = 0;
        }
        #endregion
    }
}
