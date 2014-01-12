using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owin;
using LightNode.Formatter;

namespace LightNode.Server.Tests
{
    [TestClass]
    public class FilterTest
    {
        [TestMethod]
        public void FilterBasic()
        {
            using (var server = Microsoft.Owin.Testing.TestServer.Create(app =>
            {
                var option = new LightNodeOptions(AcceptVerbs.Get | AcceptVerbs.Post, new JavaScriptContentFormatter());
                option.Filters.Add(async (context, next) =>
                {
                    context.ContractName.Is("FilterTestContract");
                    context.OperationName.Is("Hoge");
                    context.Verb.Is(AcceptVerbs.Get);

                    var filterList = context.Environment["filter"] as List<string>;
                    filterList.Add("Global2");
                    await next();
                }, order: 1000);
                option.Filters.Add(async (context, next) =>
                {
                    context.ContractName.Is("FilterTestContract");
                    context.OperationName.Is("Hoge");
                    context.Verb.Is(AcceptVerbs.Get);

                    context.IsAttributeDefined<AllowAnonymousAttribute>().IsTrue();
                    context.IsAttributeDefined<MustMobileAttribute>().IsFalse();
                    context.GetAttributes<AllowAnonymousAttribute>().First().GetType().Is(typeof(AllowAnonymousAttribute));

                    var filterList = context.Environment["filter"] as List<string>;
                    filterList.Add("Global1");
                    await next();
                }, order: -1000);


                app.Use(async (context, next) =>
                {
                    try
                    {
                        await next();
                    }
                    finally
                    {
                        var filterList = context.Environment["filter"] as List<string>;
                        filterList.Is("Contract1", "Global1", "Method1", "Global2");
                    }
                });
                app.UseLightNode(option, typeof(MockEnv).Assembly);
            }))
            {

                server.CreateRequest("/FilterTestContract/Hoge").GetAsync().Wait();
            }
        }


        [TestMethod]
        public void FilterBasic2()
        {
            using (var server = Microsoft.Owin.Testing.TestServer.Create(app =>
            {
                var option = new LightNodeOptions(AcceptVerbs.Get | AcceptVerbs.Post, new JavaScriptContentFormatter());
                option.Filters.Add(async (context, next) =>
                {
                    context.ContractName.Is("FilterTestContract");
                    context.OperationName.Is("Huga");
                    context.Verb.Is(AcceptVerbs.Get);

                    var attr = context.GetAttributes<AllowAnonymousAttribute>();
                    attr.Count().Is(1);
                    attr.First().Message.Is("aaa");

                    context.IsAttributeDefined<MustMobileAttribute>().IsTrue();


                    var filterList = context.Environment["filter"] as List<string>;
                    filterList.Add("Global1");
                    await next();
                }, order: 1000);


                app.Use(async (context, next) =>
                {
                    try
                    {
                        await next();
                    }
                    finally
                    {
                        var filterList = context.Environment["filter"] as List<string>;
                        filterList.Is("Contract1", "Global1");
                    }
                });
                app.UseLightNode(option, typeof(MockEnv).Assembly);
            }))
            {

                server.CreateRequest("/FilterTestContract/Huga").GetAsync().Result.StatusCode.Is(System.Net.HttpStatusCode.NotFound);
            }
        }
    }

    [TestContractFilter]
    [AllowAnonymous(Message = "aaa")]
    public class FilterTestContract : LightNodeContract
    {
        [TestMethodFilter]
        public int Hoge()
        {
            return 100;
        }

        [MustMobile]
        public int Huga()
        {
            throw new Exception("Huga");
        }
    }


    public class AllowAnonymousAttribute : Attribute
    {
        public string Message { get; set; }
    }

    public class MustMobileAttribute : Attribute
    {

    }

    public class TestContractFilter : LightNodeFilterAttribute
    {
        public TestContractFilter()
        {
            Order = -int.MaxValue;
        }

        public override async Task Invoke(OperationContext operationContext, Func<Task> next)
        {
            try
            {
                operationContext.Environment["filter"] = new List<string>() { "Contract1" };
                await next();
            }
            catch
            {
                operationContext.Environment["owin.ResponseStatusCode"] = 404;
            }
            finally
            {
            }
        }
    }

    public class TestMethodFilter : LightNodeFilterAttribute
    {
        public TestMethodFilter()
        {
            Order = 0;
        }

        public override Task Invoke(OperationContext operationContext, Func<Task> next)
        {
            var filterList = operationContext.Environment["filter"] as List<string>;
            filterList.Add("Method1");
            return next();
        }
    }

}
