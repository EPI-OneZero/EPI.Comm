using System;
using System.Collections;
using System.Collections.Generic;
using static EPI.Comm.CommConfig;
namespace EPI.Comm.Buffers
{

    internal class QueueBuffer : IBuffer
    {
        #region Field & Property
        protected byte[] buffer;
        public int Capacity => buffer.Length;
        protected int head;
        protected int tail;
        protected int queueDataCount;
        #endregion

        #region CTOR
        public QueueBuffer() : this(DefaultBufferSize)
        {

        }
        public QueueBuffer(int capacity)
        {
            buffer = new byte[capacity];
            head = 0;
            tail = 0;
            queueDataCount = 0;
        }
        #endregion

        #region IBuffer
        public int Count => queueDataCount;
        public byte[] GetBytes(int count)
        {
            var array = new byte[count];
            if (queueDataCount < count)
            {
                throw new IndexOutOfRangeException("count");
            }

            if (count > 0)
            {
                if (count <= buffer.Length - head)
                {
                    Buffer.BlockCopy(buffer, head, array, 0, count);

                }
                else
                {
                    var size2 = buffer.Length - head;
                    Buffer.BlockCopy(buffer, head, array, 0, size2);
                    Buffer.BlockCopy(buffer, 0, array, size2, count - size2);

                }
                head = (head + count) % buffer.Length;
                queueDataCount -= count;

            }

            return array;
        }

        public void AddBytes(byte[] bytes)
        {
            var count = bytes.Length;
            if (Capacity < count + queueDataCount)
            {
                int newCapacity = (count + queueDataCount) * 2;
                if (newCapacity < buffer.Length + 4)
                {
                    newCapacity = buffer.Length + 4;
                }

                SetCapacity(newCapacity);
            }
            if (count < Capacity - tail)
            {
                Buffer.BlockCopy(bytes, 0, buffer, tail, count);
            }
            else
            {
                var count2 = buffer.Length - tail;
                Buffer.BlockCopy(bytes, 0, buffer, tail, count2);
                Buffer.BlockCopy(bytes, count2, buffer, 0, count - count2);
            }
            tail = (tail + count) % buffer.Length;
            queueDataCount += count;
        }

        public void Clear()
        {
            head = 0;
            tail = 0;
            queueDataCount = 0;
        }
        private void SetCapacity(int capacity)
        {
            byte[] array = new byte[capacity];
            if (queueDataCount > 0)
            {
                if (head < tail)
                {
                    Buffer.BlockCopy(buffer, head, array, 0, queueDataCount);
                }
                else
                {
                    var size2 = buffer.Length - head;
                    Buffer.BlockCopy(buffer, head, array, 0, size2);
                    Buffer.BlockCopy(buffer, 0, array, size2, tail);
                }
            }
            buffer = array;
            head = 0;
            tail = ((queueDataCount != capacity) ? queueDataCount : 0);
        }
        #endregion

        #region IEnumerable
        public IEnumerator<byte> GetEnumerator()
        {
            if (head < tail)
            {
                for (int i = head; i < tail; i++)
                {
                    yield return buffer[i];
                }
            }
            else
            {
                for (int i = head; i < buffer.Length; i++)
                {
                    yield return buffer[i];
                }
                for (int i = 0; i < head; i++)
                {
                    yield return buffer[i];
                }
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }

}