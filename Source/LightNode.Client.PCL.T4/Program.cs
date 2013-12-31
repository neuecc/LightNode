using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightNode.Client
{
    class Program
    {
        static async Task Run()
        {
            var client = new LightNodeClient("http://localhost:54097");

            var tasks = Enumerable.Range(1, 1000).Select(_ =>
            {
                return client.Perf.EchoAsync("hoge", 10, 2, Performance.MyEnum.B);
            });

            var v = await Task.WhenAll(tasks);
        }

        static void Main(string[] args)
        {
            var sw = Stopwatch.StartNew();
            Run().Wait();

            Console.WriteLine(sw.Elapsed);
        }
    }
}
