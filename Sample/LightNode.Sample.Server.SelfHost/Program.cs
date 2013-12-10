using LightNode.Server;
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
            app.UseLightNode();
        }
    }

    public class MyClass : ILightNodeContract
    {
        public async Task Test(int x)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}