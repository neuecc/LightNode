using Microsoft.Owin.Hosting;
using Owin;
using System;

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
}