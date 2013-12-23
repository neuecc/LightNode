using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nancy
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseNancy(new Owin.NancyOptions
            {
                Bootstrapper = new DefaultNancyBootstrapper()
            });
        }
    }

    public class HomeModule : NancyModule
    {
        public HomeModule()
        {
            Get["/{name}/{x}/{y}"] = (x) =>
            {
                return new MyClass { Name = x.name, Sum = x.x + x.y };
            };
        }
    }

    public class MyClass
    {
        public string Name { get; set; }
        public int Sum { get; set; }
    }
}