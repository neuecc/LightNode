using CloudStructures;
using Glimpse.CloudStructures.Redis;
using Glimpse.LightNode;
using LightNode.Formatter;
using LightNode.Server;
using Microsoft.Owin;
using Owin;
using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

// [assembly: Microsoft.Owin.OwinStartup(typeof(LightNode.Sample.GlimpseUse.Startup))]

namespace LightNode.Sample.GlimpseUse
{
    public class Html : LightNode.Server.OperationOptionAttribute
    {
        public Html(AcceptVerbs acceptVerbs = AcceptVerbs.Get | AcceptVerbs.Post)
            : base(acceptVerbs, typeof(HtmlContentFormatterFactory))
        {

        }
    }

    public static class Redis
    {
        public static RedisSettings Settings = new RedisSettings("127.0.0.1,allowAdmin=true", tracerFactory: () => new GlimpseRedisCommandTracer());
    }

    public class Startup
    {
        public void Configuration(Owin.IAppBuilder app)
        {
            app.EnableGlimpse();
            app.Map("/api", builder =>
            {
                builder.UseLightNode(new LightNodeOptions(AcceptVerbs.Get | AcceptVerbs.Post, new JilContentFormatter(), new GZipJilContentFormatter())
                {
                    OperationCoordinatorFactory = new GlimpseProfilingOperationCoordinatorFactory(),
                    StreamWriteOption = StreamWriteOption.BufferAndWrite,
                    ParameterEnumAllowsFieldNameParse = true,
                    ErrorHandlingPolicy = ErrorHandlingPolicy.ReturnInternalServerErrorIncludeErrorDetails,
                    OperationMissingHandlingPolicy = OperationMissingHandlingPolicy.ReturnErrorStatusCodeIncludeErrorDetails,
                    Logger = LightNode.Diagnostics.LightNodeEventSource.Log
                });
            });
            app.Map("/swagger", builder =>
            {
                var xmlName = "LightNode.Sample.GlimpseUse.xml";
                var xmlPath = HttpContext.Current.Server.MapPath("~/bin/" + xmlName);
                //var xmlPath = System.AppDomain.CurrentDomain.BaseDirectory + "\\" + xmlName;

                builder.UseLightNodeSwagger(new Swagger.SwaggerOptions("LightNodeSample", "/api")
                {
                    XmlDocumentPath = xmlPath,
                    IsEmitEnumAsString = true
                });
            });

            app.MapWhen(x => !x.Request.Path.Value.StartsWith("/Glimpse.axd", StringComparison.InvariantCultureIgnoreCase), builder =>
            {
                builder.Run(ctx =>
                {
                    ctx.Response.StatusCode = 404;
                    return Task.FromResult(0);
                });
            });

        }
    }

    /// <summary>
    /// My Sample...
    /// </summary>
    public class Sample : LightNodeContract
    {

        // use specified content formatter
        /// <summary>
        /// HTMLGET
        /// </summary>
        /// <returns></returns>
        [OperationOption(AcceptVerbs.Get | AcceptVerbs.Post, typeof(HtmlContentFormatterFactory))]
        public string Html()
        {
            return "<html><body>aaa</body></html>";
        }
    }

    public enum MyFruits
    {
        Orange, Apple, Grape
    }

    public enum JapaneseFruit
    {
        オレンジ, リンゴ, ぶどう
    }

    /// <summary>
    /// My Member...
    /// </summary>
    [Authentication(Order = 1)]
    [Session(Order = 2)]
    public class Member : LightNodeContract
    {
        /// <summary>
        /// Generate Random Person
        /// </summary>
        /// <remarks>This area is generated from Xml doc-comment remarks.</remarks>
        /// <param name="seed">Random seed.</param>
        /// <param name="fruit">Suports enum.</param>
        [Post]
        public async Task<Person> Random(int seed, Fruit fruit = Fruit.Banana)
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

        /// <summary>
        /// ほげもげ
        /// </summary>
        /// <remarks>ふふが！</remarks>
        /// <param name="x">えっくす</param>
        /// <param name="y">わい</param>
        /// <param name="jf">日本語マン</param>
        /// <param name="fruit">英語マン</param>
        /// <returns>Ret</returns>
        [Post]
        public object Test(int x, string y, string[] abc, JapaneseFruit jf, Fruit fruit = Fruit.Banana)
        {
            return new { x, y, abc = string.Join(", ", abc), jf = jf.ToString(), fruit = fruit.ToString() };
        }

        /// <summary>
        /// ほげもげ
        /// </summary>
        /// <remarks>ふふが！</remarks>
        /// <param name="x">えっくす</param>
        /// <param name="y">わい</param>
        /// <param name="jf">日本語マン</param>
        /// <param name="fruit">英語マン</param>
        /// <returns>Ret</returns>
        [Get]
        public object TestGet(int x, string y, string[] abc, JapaneseFruit jf, Fruit fruit = Fruit.Banana)
        {
            return new { x, y, abc = string.Join(", ", abc), jf = jf.ToString(), fruit = fruit.ToString() };
        }

        public void BadRequest()
        {
            throw new ReturnStatusCodeException(System.Net.HttpStatusCode.BadRequest, content: "Bad Requestにゃん");
        }


        [Html]
        public string Html()
        {
            return "<html><body><h1>aaa</h1></body></html>";
        }

        [Get]
        public void Get()
        {
        }

        [Post]
        public void Post()
        {
        }

        [Put]
        public void Put()
        {
        }

        [Delete]
        public void Delete()
        {
        }

        [Patch]
        public void Patch()
        {
        }
    }

    public enum Fruit
    {
        Apple, Banana
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

        /// <summary>
        /// M
        /// </summary>
        void Moge()
        {
        }

        /// <summary>
        /// M
        /// </summary>
        /// <param name="x">x</param>
        void Moge(int x)
        {
        }
    }
}