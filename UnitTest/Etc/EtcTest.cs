using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTest.Etc
{
    [TestClass]
    public class EtcTest
    {
        [TestMethod]
        public void Test()
        {
            var list = new List<int>
            {
            };
            var reads = list.AsReadOnly();
            var th1 = new Thread(() =>
            {
                for (int i = 0; i < 1000000; i++)
                {
                    list.Remove(i);
                    list.Remove(i);
                    list.Remove(i);
                    list.Remove(i);
                    list.Remove(i);
                    list.Add(i);
                    list.Add(i);
                    list.Add(i);
                    list.Add(i);
                    list.Add(i);
                    Thread.Sleep(1);
                    Console.WriteLine("1 : " +list.Count);
                }
            });
            var th2 = new Thread(() =>
            {
                for (int i = 0; i < 1000000; i++)
                {
                    var x= list.ToArray();
                    var y= list.ToList();
                    Console.WriteLine("2 : "+ x.Length);
                    Thread.Sleep(1);
                }
            });
            th2.Start();
            th1.Start();
        }
    }
}
