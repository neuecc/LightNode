using Newtonsoft.Json;
using Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace RawOwinHandler
{
    public class Startup
    {
        readonly static Task EmptyTask = Task.FromResult<object>(null);

        public void Configuration(IAppBuilder app)
        {
            app.Run(context =>
            {
                var name = context.Request.Query.Get("name");
                var x = int.Parse(context.Request.Query.Get("x"));
                var y = int.Parse(context.Request.Query.Get("y"));
                var e = Enum.Parse(typeof(MyEnum), context.Request.Query.Get("e"));

                var mc = new MyClass { Name = name, Sum = (x + y) * (int)e };

                var json = JsonConvert.SerializeObject(mc);
                var enc = System.Text.Encoding.UTF8.GetBytes(json);
                context.Response.ContentType = "application/json";

                // sync write or async write
                if (context.Request.Query.Get("sync") == "true")
                {
                    context.Response.Body.Write(enc, 0, enc.Length);
                    return EmptyTask; // Task.FromResult<object>(null)
                }
                else
                {
                    return context.Response.Body.WriteAsync(enc, 0, enc.Length);
                }
            });
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