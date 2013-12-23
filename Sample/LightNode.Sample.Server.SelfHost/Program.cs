using LightNode.Server;
using System.Linq;
using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Threading.Tasks;
using LightNode.Formatter;

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
            app.UseLightNode(
                new LightNodeOptions(AcceptVerbs.Get | AcceptVerbs.Post,
                new JavaScriptContentTypeFormatter()));
        }
    }

    public class My : LightNodeContract
    {
        public string Echo(string x)
        {
            return x;
        }

        public Task<int> Sum(int x, int? y, int z = 1000)
        {
            return Task.Run(() => x + y.Value + z);
        }
    }

    public class Room : LightNodeContract
    {
        public void Create()
        {
        }

        public async Task A(string x = "aaa", string y = null)
        {
            await Task.Yield();
        }
    }
}