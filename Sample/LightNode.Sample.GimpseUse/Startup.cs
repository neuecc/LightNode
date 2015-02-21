using CloudStructures;
using Glimpse.CloudStructures.Redis;
using Glimpse.LightNode;
using LightNode.Formatter;
using LightNode.Formatter.Jil;
using LightNode.Server;
using Microsoft.Owin;
using Owin;
using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

[assembly: Microsoft.Owin.OwinStartup(typeof(LightNode.Sample.GlimpseUse.Startup))]

namespace LightNode.Sample.GlimpseUse
{
    public static class Redis
    {
        public static RedisSettings Settings = new RedisSettings("127.0.0.1,allowAdmin=true", tracerFactory: () => new GlimpseRedisCommandTracer());
    }

    public class Startup
    {
        public void Configuration(Owin.IAppBuilder app)
        {
            app.EnableGlimpse();
            app.MapWhen(x => !x.Request.Path.Value.StartsWith("/glimpse.axd", StringComparison.OrdinalIgnoreCase), x =>
            {
                x.UseLightNode(new LightNodeOptions(AcceptVerbs.Get | AcceptVerbs.Post, new JilContentFormatter(), new GZipJilContentFormatter())
                {
                    OperationCoordinatorFactory = new GlimpseProfilingOperationCoordinatorFactory(),
                    StreamWriteOption = StreamWriteOption.BufferAndWrite
                });
            });
            
app.Map("/v1", x =>
{
    x.UseLightNode(new LightNodeOptions(), typeof(v1Contract).Assembly);
});

app.Map("/v2", x =>
{
    x.UseLightNode(new LightNodeOptions(), typeof(v2Contract).Assembly);
});
        }
    }

    public class Sample : LightNodeContract
    {
        // use specified content formatter
        [OperationOption(AcceptVerbs.Get, typeof(HtmlContentFormatterFactory))]
        public string Html()
        {
            return "<html><body>aaa</body></html>";
        }
    }


    [Authentication(Order = 1)]
    [Session(Order = 2)]
    public class Member : LightNodeContract
    {
        public async Task<Person> Random(int seed)
        {
            //await Redis.Settings.String<string>("Person?Seed=" + seed).Get();
            var rand = new Random(seed);
            await Task.Delay(TimeSpan.FromMilliseconds(2));
            var nameSeed = "abcdefghijklmnopqrstuvwxyz";
            var f = new StringBuilder();
            var l = new StringBuilder();
            for (int i = 0; i < 10; i++)
            {
                f.Append(nameSeed[rand.Next(0, nameSeed.Length)]);
                l.Append(nameSeed[rand.Next(0, nameSeed.Length)]);
            }
            // var _ = nameSeed[1000]; // exception

            return new Person { Age = rand.Next(10, 40), FirstName = f.ToString(), LastName = l.ToString() };
        }
    }

    // dummy
    public class AuthenticationAttribute : LightNodeFilterAttribute
    {
        public override async Task Invoke(OperationContext operationContext, Func<Task> next)
        {
            //await Redis.Settings.String<string>("Auth:1").Get();
            await Task.Delay(TimeSpan.FromMilliseconds(2));
            await next();
        }
    }

    // dummy
    public class SessionAttribute : LightNodeFilterAttribute
    {
        public override async Task Invoke(OperationContext operationContext, Func<Task> next)
        {
            //await Redis.Settings.Dictionary<string, string>("Session:1").GetAll();
            await Task.Delay(TimeSpan.FromMilliseconds(1));
            await next();
            //await Redis.Settings.Dictionary<string, string>("Session:1").Set("a", "b");
            await Task.Delay(TimeSpan.FromMilliseconds(1));
        }
    }

    public class Person
    {
        public int Age { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}