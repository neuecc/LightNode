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
        public void Configuration(IAppBuilder app)
        {
            app.Run(context =>
            {
                var name = context.Request.Query.Get("name");
                var x = int.Parse(context.Request.Query.Get("x"));
                var y = int.Parse(context.Request.Query.Get("y"));

                var mc = new MyClass { Name = name, Sum = x + y };

                var json = JsonConvert.SerializeObject(mc);
                var enc = System.Text.Encoding.UTF8.GetBytes(json);
                context.Response.ContentType = "application/json";
                return context.Response.Body.WriteAsync(enc, 0, enc.Length);
            });
        }
    }

    public class MyClass
    {
        public string Name { get; set; }
        public int Sum { get; set; }
    }
}