using LightNode.Server;
using System.Linq;
using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Threading.Tasks;
using LightNode.Formatter;
using LightNode.Core;
using System.Text;
using LightNode.Formatter;

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
            app.UseLightNode(new LightNodeOptions(AcceptVerbs.Get | AcceptVerbs.Post, new JilContentFormatter(), new GZipJilContentFormatter())
            {
                StreamWriteOption = StreamWriteOption.BufferAndWrite
            });
        }
    }

    [Authentication(Order = 1)]
    [Session(Order = 2)]
    public class Member : LightNodeContract
    {
        public async Task<Person> Random(int seed)
        {
            var rand = new Random(seed);
            await Task.Delay(TimeSpan.FromMilliseconds(30));
            var nameSeed = "abcdefghijklmnopqrstuvwxyz";
            var f = new StringBuilder();
            var l = new StringBuilder();
            for (int i = 0; i < 10; i++)
            {
                f.Append(nameSeed[rand.Next(0, nameSeed.Length)]);
                l.Append(nameSeed[rand.Next(0, nameSeed.Length)]);
            }

            return new Person { Age = rand.Next(10, 40), FirstName = f.ToString(), LastName = l.ToString() };
        }
    }

    // dummy
    public class AuthenticationAttribute : LightNodeFilterAttribute
    {
        public override async Task Invoke(OperationContext operationContext, Func<Task> next)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(23));
            await next();
        }
    }

    // dummy
    public class SessionAttribute : LightNodeFilterAttribute
    {
        public override async Task Invoke(OperationContext operationContext, Func<Task> next)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(10));
            await next();
            await Task.Delay(TimeSpan.FromMilliseconds(5));
        }
    }

    public class Person
    {
        public int Age { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}