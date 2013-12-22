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
            var client = new LightNodeClient("http://localhost:12345") { ContentFormatter = new LightNode.Formatters.JavaScriptContentTypeFormatter() };

        

            var v = client.My.Echo("hogehoge").Result;
            Console.WriteLine(v);
        }
    }
}
