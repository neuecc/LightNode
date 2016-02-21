using LightNode.Formatter;
using LightNode.Server;
using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightNode.Sample.Server.ForAngularClient
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
            app.Map("/api", builder =>
            {
                builder.UseLightNode(new LightNodeOptions(AcceptVerbs.Get | AcceptVerbs.Post, new JsonNetContentFormatter())
                {
                    StreamWriteOption = StreamWriteOption.BufferAndWrite,
                    ParameterEnumAllowsFieldNameParse = true,
                    ErrorHandlingPolicy = ErrorHandlingPolicy.ReturnInternalServerErrorIncludeErrorDetails,
                    OperationMissingHandlingPolicy = OperationMissingHandlingPolicy.ReturnErrorStatusCodeIncludeErrorDetails,
                });
            });
        }
    }

    public class Member : LightNodeContract
    {
        /// <summary>
        /// aaa
        /// </summary>
        /// <param name="seed">see:d</param>
        public async Task<Person> Random(int seed)
        {
            await Task.Delay(seed * 1000);

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

            return new Person
            {
                Age = rand.Next(10, 40),
                BirthDay = DateTime.Now,
                Gender = (Gender)rand.Next(0, 1),
                FirstName = f.ToString(),
                LastName = l.ToString()
            };
        }

        public async Task<City> City(bool isBoolTest)
        {
            return new City { Name = "Random", People = await Task.WhenAll(Enumerable.Range(1, 5).Select(x => Random(isBoolTest ? x : 0))) };
        }

        public string Echo(Test test)
        {
            return test.ToString();
        }

    }

    [DefineTypeScriptGenerate] // for property of complex type
    public enum Gender
    {
        Male,
        Female
    }

    public class Person
    {
        public int Age { get; set; }
        public DateTime BirthDay { get; set; }
        public Gender Gender { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class City
    {
        public string Name { get; set; }
        public IReadOnlyCollection<Person> People { get; set; }
    }

    public enum Test
    {
        Foo,
        Bar
    }

    [DefineTypeScriptGenerate]
    public class NoReferenceClass
    {
        public int Foo { get; set; }
    }
}
