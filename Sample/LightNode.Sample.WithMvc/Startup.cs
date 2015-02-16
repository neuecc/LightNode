using LightNode.Server;
using Microsoft.Owin;
using Owin;
using System;
using System.Text;
using System.Threading.Tasks;

[assembly: OwinStartupAttribute(typeof(LightNode.Sample.WithMvc.Startup))]
namespace LightNode.Sample.WithMvc
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Map("/api",  x =>
            {
                x.UseLightNode();
            });
        }
    }
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

    public class Person
    {
        public int Age { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
