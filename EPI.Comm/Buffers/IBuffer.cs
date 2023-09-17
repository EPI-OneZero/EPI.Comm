using System;
using System.Collections.Generic;
using System.Text;

namespace EPI.Comm.Buffers
{
    internal interface IBuffer
    {
        int Count { get; }
        byte[] GetBytes(int count);

        void AddBytes(byte[] bytes);
    }
    internal class QueueBuffer : IBuffer
    {
        protected byte[] buffer;
        public int Capacity => buffer.Length;
        protected int head;
        protected int tail;
        protected int queueDataCount;
        protected int version;
        public int Count => queueDataCount;

        public QueueBuffer() : this(4)
        {

        }
        public QueueBuffer(int capacity)
        {
            buffer = new byte[capacity];
            head = 0;
            tail = 0;
            version = 0;
        }
        public byte[] GetBytes(int count)
        {
            var array = new byte[count];
            if (queueDataCount < count)
            {
                throw new ArgumentOutOfRangeException("count");
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
                UpdateHead(count);
                UpdateSize(-count);

                version++;
            }

            return array;
        }
        public void AddBytes(byte[] bytes)
        {
            var count = bytes.Length;
            if (Capacity < count + queueDataCount)
            {
                int newCapacity = (int)(count * 2);
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
            version++;
            UpdateTail(count);
            UpdateSize(count);
        }
        private void UpdateHead(int count)
        {
            head = (head + count) % buffer.Length;
        }
        private void UpdateTail(int count)
        {
            tail = (tail + count) % buffer.Length;
        }
        private void UpdateSize(int num)
        {
            queueDataCount += num;
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
            version++;
        }
    }
    internal class RefreshBuffer : IBuffer
    {
        protected byte[] buffer;

        public int Count => buffer?.Length ?? 0;
       
        public RefreshBuffer()
        {
        }
       
        public void AddBytes(byte[] bytes)
        {
            buffer = bytes;
        }
        public byte[] GetBytes(int count)
        {
            if(buffer.Length < count)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            else
            {
                Array.Resize(ref buffer, count);
                return buffer;
            }
        }
    }

}
