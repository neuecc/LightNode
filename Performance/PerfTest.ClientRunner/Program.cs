using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PerfTest.ClientRunner
{
    class Program
    {
        const string EndPoint = "http://localhost:35358/Handler1.ashx";
        const int Concurrency = 200;
        const int RequestCount = 10000;

        static async Task Run()
        {
            var clinet = new HttpClient();

            var semaphore = new SemaphoreSlim(Concurrency);
            var tasks = new List<Task>();
            for (int i = 0; i < RequestCount; i++)
            {
                await semaphore.WaitAsync();
                var task = clinet.GetStringAsync(EndPoint)
                    .ContinueWith((_, state) => ((SemaphoreSlim)state).Release(), semaphore);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        static void Main(string[] args)
        {
            ServicePointManager.UseNagleAlgorithm = false;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.DefaultConnectionLimit = 1000;

            ThreadPool.SetMinThreads(500, 500);

            var sw = Stopwatch.StartNew();
            Run().Wait();
            sw.Stop();
            var time = sw.ElapsedMilliseconds;

            Console.WriteLine("Concurrency:" + Concurrency);
            Console.WriteLine("RequestCount:" + RequestCount);
            Console.WriteLine("Elapsed:" + sw.Elapsed);
            var requestPerSecond = RequestCount * 1000 / time;
            Console.WriteLine("RequestPerSecond:" + requestPerSecond + " [#/sec]");

            var timePerRequest = time / RequestCount;
            Console.WriteLine("TimePerRequest:" + timePerRequest + " [ms]");
        }
    }
}
