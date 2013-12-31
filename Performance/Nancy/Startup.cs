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
            Get["/{name}/{x}/{y}/{e}"] = (x) =>
            {
                return Response.AsJson(new MyClass { Name = x.name, Sum = ((int)x.x + (int)x.y) * (int)x.e });
            };
        }
    }

    public class MyClass
    {
        public string Name { get; set; }
        public int Sum { get; set; }
    }

    public enum MyEnum
    {
        A = 2,
        B = 3,
        C = 4
    }
}