﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Runtime.Remoting.Messaging;
using System.Reflection;
using System.Collections;

namespace ConsoleTest
{
    internal class QueueBuffer
    {
        public byte[] buffer;
        protected int head;
        protected int tail;
        protected int size;
        protected int version;
        public int Capacity => buffer.Length;
        public int Count => size;

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
        public byte[] Dequeue(int count)
        {
            var array = new byte[count];
            if (size < count)
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
        public void Enqueue(byte[] bytes)
        {
            var count = bytes.Length;
            if (Capacity < count + size)
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
        private void UpdateHead(int num)
        {
            head = (head + num) % buffer.Length;
        }
        private void UpdateTail(int num)
        {
            tail = (tail + num) % buffer.Length;
        }
        private void UpdateSize(int num)
        {
            size += num;
        }

        private void SetCapacity(int capacity)
        {
            byte[] array = new byte[capacity];
            if (size > 0)
            {
                if (head < tail)
                {
                    Buffer.BlockCopy(buffer, head, array, 0, size);
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
            tail = ((size != capacity) ? size : 0);
            version++;
        }
    }

    /// <summary>
    /// 프로그램
    /// </summary>
    public class Program
    {
        /// <summary>
        /// 메인함수
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
         
        }
       
     
    }
}
