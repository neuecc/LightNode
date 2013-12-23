using LightNode.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightNode.Sample.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            // not yet...

            var client = new LightNodeClient("http://localhost:12345") { ContentFormatter = new LightNode.Formatter.JavaScriptContentTypeFormatter() };

        

            var v = client.My.EchoAsync("hogehoge").Result;
            Console.WriteLine(v);
        }
    }
}
