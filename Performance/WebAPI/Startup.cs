using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace WebAPI
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            app.UseWebApi(config);
        }
    }

    public class PerfController : ApiController
    {
        [HttpGet]
        [Route("Get/{name}/{x}/{y}")]
        public MyClass Get(string name, int x, int y)
        {
            return new MyClass { Name = name, Sum = x + y };
        }
    }

    public class MyClass
    {
        public string Name { get; set; }
        public int Sum { get; set; }
    }
}