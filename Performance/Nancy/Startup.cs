using Nancy;
using Owin;

namespace Nancyfx
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseNancy(new Nancy.Owin.NancyOptions
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
                return Response.AsJson(new MyClass { Name = x.name, Sum = (int) x.x + (int) x.y });
            };
        }
    }

    public class MyClass
    {
        public string Name { get; set; }
        public int Sum { get; set; }
    }
}