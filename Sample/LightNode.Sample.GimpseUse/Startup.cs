using Glimpse.Core.Framework;
using Glimpse.LightNode;
using LightNode.Formatter;
using LightNode.Server;
using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

[assembly: OwinStartup(typeof(LightNode.Sample.GlimpseUse.Startup))]

namespace LightNode.Sample.GlimpseUse
{
    public class Startup
    {
        public void Configuration(Owin.IAppBuilder app)
        {
            app.EnableGlimpse();
            app.MapWhen(x => !x.Request.Path.Value.StartsWith("/glimpse.axd", StringComparison.OrdinalIgnoreCase), x =>
            {
                x.UseLightNode(new LightNodeOptions(AcceptVerbs.Get | AcceptVerbs.Post,
                    new JavaScriptContentFormatter()) { OperationCoordinator = new GlimpseProfilingOperationCoordinator() });
            });



        }
    }

    [MyAtrr]
    public class MyClass : LightNodeContract
    {
        public Person Echo(string x)
        {
            //if (x == "hoge") throw new ReturnStatusCodeException(System.Net.HttpStatusCode.SeeOther);
            //if (x == "huga") throw new InvalidOperationException("nanika");

            return new Person { Age = 21, FirstName = x, LastName = x };
        }
    }

    public class MyAtrr : LightNodeFilterAttribute
    {
        public override Task Invoke(OperationContext operationContext, Func<Task> next)
        {
            return next();
        }
    }

    public class Person
    {
        public int Age { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
