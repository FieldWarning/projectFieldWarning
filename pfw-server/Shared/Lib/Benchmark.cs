using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Shared.Lib
{
    public static class GameCpuBenchmark
    {
        /// <summary>
        /// Runs a cpu benchmark, lower score is better
        /// </summary>
        /// <returns></returns>
        public static long Run()
        {
            var s = new Stopwatch();
            s.Start();
            Fibonacci(100);
            s.Stop();
            return s.ElapsedMilliseconds;
        }

        private static void Fibonacci(int len)
        {
            int a = 0, b = 1, c = 0;
            //Console.Write("{0} {1}", a, b);
            for (int i = 2; i < len; i++)
            {
                c = a + b;
                //Console.Write(" {0}", c);
                a = b;
                b = c;
            }
        }
    }
}
