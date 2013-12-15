using LightNode.Server;
using System.Linq;
using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Threading.Tasks;

namespace LightNode.Sample.Server.SelfHost
{
    class Program
    {
        static void Main(string[] args)
        {
            using (WebApp.Start<Startup>("http://localhost:12345"))
            {
                Console.ReadLine();
            }
        }
    }

    public class Startup
    {
        public void Configuration(Owin.IAppBuilder app)
        {
            app.UseErrorPage();
            app.UseLightNode(new LightNodeOptions(AcceptVerbs.Get | AcceptVerbs.Post, new TextContentTypeFormatter()));
        }
    }

    public class MyClass : ILightNodeContract
    {
        public int Test1(int x, int y)
        {
            return x * y;
        }

        public void Test2(int x, string y, int z)
        {
            System.Diagnostics.Debug.WriteLine(x + "*" + y + ":" + z);
        }

        public async Task Test3()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
        public async Task<int> Test4()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            return 100;
        }
        public int Test5(int[] xs)
        {
            return xs.Sum();
        }
    }
}