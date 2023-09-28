using System;
using System.Collections;
using System.Collections.Generic;

namespace EPI.Comm.Buffers
{
    /// <summary>
    /// 질문 : AddBytes 함수는 말그대로 버퍼 저장 배열 자체를 갈아치우는 함수인데 이건 LSP위반인가?
    /// 참고 : GetBytes는 앞에서부터 디큐처럼 동작함
    /// </summary>
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
            throw new NotImplementedException();
        }
        #endregion
    }

}
