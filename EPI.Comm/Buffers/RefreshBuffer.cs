using System;
using System.Collections;
using System.Collections.Generic;

namespace EPI.Comm.Buffers
{
    /// <summary>
    /// 질문 : 
    /// </summary>
    internal class RefreshBuffer : IBuffer
    {
        protected byte[] buffer;

        public int Count
        {
            get
            {
                if(buffer == null)
                {
                    return 0;
                }
                else
                {
                    return buffer.Length - offset;
                }
            }
        }
        protected int offset = 0;

        public RefreshBuffer()
        {
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
            throw new NotImplementedException();
        }
        #endregion
    }

}
