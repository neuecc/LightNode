using System;
using Owin;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Collections.Generic;
using LightNode.Formatter;

namespace LightNode.Server.Tests
{
    [TestClass]
    public class ParameterCheckTest
    {
        [TestMethod]
        public void Struct()
        {
            MockEnv.CreateRequest("/ParameterContract/Struct").GetAsync().Result.StatusCode.Is(HttpStatusCode.BadRequest); // lack of param
            MockEnv.CreateRequest("/ParameterContract/Struct?x=1").GetString().Trim('\"').Is("1"); // ok
            MockEnv.CreateRequest("/ParameterContract/Struct?x=hoge").GetAsync().Result.StatusCode.Is(HttpStatusCode.BadRequest); // can't parse
            MockEnv.CreateRequest("/ParameterContract/Struct?x=10&x=20&x=30").GetString().Trim('\"').Is("10"); // over parameter
        }

        [TestMethod]
        public void Nullable()
        {
            MockEnv.CreateRequest("/ParameterContract/Nullable").GetString().Trim('\"').Is("null");
            MockEnv.CreateRequest("/ParameterContract/Nullable?x=1").GetString().Trim('\"').Is("1");
            MockEnv.CreateRequest("/ParameterContract/Nullable?x=hoge").GetString().Trim('\"').Is("null");
        }

        [TestMethod]
        public void DefaultValue()
        {
            MockEnv.CreateRequest("/ParameterContract/DefaultValue").GetString().Trim('\"').Is("100");
            MockEnv.CreateRequest("/ParameterContract/DefaultValue?x=1").GetString().Trim('\"').Is("1");
            MockEnv.CreateRequest("/ParameterContract/DefaultValue?x=hoge").GetString().Trim('\"').Is("100");
        }

        [TestMethod]
        public void String()
        {
            MockEnv.CreateRequest("/ParameterContract/String").GetAsync().Result.StatusCode.Is(HttpStatusCode.BadRequest);
            MockEnv.CreateRequest("/ParameterContract/String?x=hoge").GetString().Trim('\"').Is("hoge");

            using(var server = Microsoft.Owin.Testing.TestServer.Create(app =>
            {
                app.UseLightNode(
                    new LightNodeOptions(AcceptVerbs.Get | AcceptVerbs.Post,
                        new JavaScriptContentFormatter(),
                        new TextContentFormatter()){ ParameterStringImplicitNullAsDefault = true}
                    , typeof(MockEnv).Assembly);
            }))
            {
                server.CreateRequest("/ParameterContract/String").GetString().Trim('\"').Is("null");
            }
        }

        [TestMethod]
        public void StringNullDefaultValue()
        {
            MockEnv.CreateRequest("/ParameterContract/StringNullDefaultValue").GetString().Trim('\"').Is("null");
            MockEnv.CreateRequest("/ParameterContract/StringNullDefaultValue?x=hoge").GetString().Trim('\"').Is("hoge");
            MockEnv.CreateRequest("/ParameterContract/StringNullDefaultValue?x=hoge&x=huga").GetString().Trim('\"').Is("hoge");
        }

        [TestMethod]
        public void StringDefaultValue()
        {
            MockEnv.CreateRequest("/ParameterContract/StringDefaultValue").GetString().Trim('\"').Is("hogehoge");
            MockEnv.CreateRequest("/ParameterContract/StringDefaultValue?x=hoge").GetString().Trim('\"').Is("hoge");
            MockEnv.CreateRequest("/ParameterContract/StringDefaultValue?x=hoge&x=huga").GetString().Trim('\"').Is("hoge");
        }

        [TestMethod]
        public void Array()
        {
            MockEnv.CreateRequest("/ParameterContract/Array").GetString().Trim('\"').Is("0");
            MockEnv.CreateRequest("/ParameterContract/Array?x=100").GetString().Trim('\"').Is("100");
            MockEnv.CreateRequest("/ParameterContract/Array?x=1&x=2").GetString().Trim('\"').Is("3");
            MockEnv.CreateRequest("/ParameterContract/Array?x=1&x=hoge").GetString().Trim('\"').Is("0");

            MockEnv.CreateRequest("/ParameterContract/Array2?x=&y=10").GetString().Trim('\"').Is("10");
        }

        [TestMethod]
        public void Enum()
        {
            MockEnv.CreateRequest("/ParameterContract/Enum").GetAsync().Result.StatusCode.Is(HttpStatusCode.BadRequest);
            MockEnv.CreateRequest("/ParameterContract/Enum?fruit=2").GetString().Trim('\"').Is("Orange");
            MockEnv.CreateRequest("/ParameterContract/Enum?fruit=oRange").GetString().Trim('\"').Is("Orange");
            MockEnv.CreateRequest("/ParameterContract/Enum?fruit=hogemoge").GetAsync().Result.StatusCode.Is(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public void EnumNullable()
        {
            MockEnv.CreateRequest("/ParameterContract/EnumNullable").GetString().Trim('\"').Is("null");
            MockEnv.CreateRequest("/ParameterContract/EnumNullable?fruit=2").GetString().Trim('\"').Is("Orange");
            MockEnv.CreateRequest("/ParameterContract/EnumNullable?fruit=oRange").GetString().Trim('\"').Is("Orange");
            MockEnv.CreateRequest("/ParameterContract/EnumNullable?fruit=hogemoge").GetString().Trim('\"').Is("null");
        }

        [TestMethod]
        public void EnumDefaultValue()
        {
            MockEnv.CreateRequest("/ParameterContract/EnumDefaultValue").GetString().Trim('\"').Is("Apple");
            MockEnv.CreateRequest("/ParameterContract/EnumDefaultValue?fruit=2").GetString().Trim('\"').Is("Orange");
            MockEnv.CreateRequest("/ParameterContract/EnumDefaultValue?fruit=oRange").GetString().Trim('\"').Is("Orange");
            MockEnv.CreateRequest("/ParameterContract/EnumDefaultValue?fruit=hogemoge").GetString().Trim('\"').Is("Apple");
        }

        [TestMethod]
        public void EnumArray()
        {
            MockEnv.CreateRequest("/ParameterContract/EnumArray").GetString().Trim('\"').Is("Empty");
            MockEnv.CreateRequest("/ParameterContract/EnumArray?fruit=2").GetString().Trim('\"').Is("Orange");
            MockEnv.CreateRequest("/ParameterContract/EnumArray?fruit=oRange&fruit=3").GetString().Trim('\"').Is("Orange,Apple");
            MockEnv.CreateRequest("/ParameterContract/EnumArray?fruit=oRange&fruit=hogemoge").GetString().Trim('\"').Is("Empty");
        }

        [TestMethod]
        public void CacheSize()
        {
            var dict = typeof(AllowRequestType).GetField("convertTypeDictionary", System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .GetValue(null);

            var count = ((Dictionary<Type, LightNode.Server.AllowRequestType.TryParse>)dict).Count;
            count.Is(33);

            var dict2 = typeof(AllowRequestType).GetField("convertArrayTypeDictionary", System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .GetValue(null);
            var count2 = ((Dictionary<Type, Func<IEnumerable<string>, object>>)dict2).Count;
            count2.Is(16);
        }
    }

    public class ParameterContract : LightNodeContract
    {
        public int Struct(int x)
        {
            return x;
        }

        public string Nullable(int? x)
        {
            return (x == null) ? "null" : x.Value.ToString();
        }

        public int DefaultValue(int x = 100)
        {
            return x;
        }

        public string String(string x)
        {
            return (x == null) ? "null" : x;
        }

        public string StringNullDefaultValue(string x = null)
        {
            return (x == null) ? "null" : x;
        }

        public string StringDefaultValue(string x = "hogehoge")
        {
            return x;
        }

        public int Array(int[] x)
        {
            x.IsNotNull();

            return x.Sum();
        }

        public int Array2(int[] x, int y)
        {
            x.IsNotNull();

            return x.Sum() + y;
        }

        public string Enum(Fruit fruit)
        {
            return fruit.ToString();
        }

        public string EnumNullable(Fruit? fruit)
        {
            return (fruit == null) ? null : fruit.ToString();
        }

        public string EnumDefaultValue(Fruit fruit = Fruit.Apple)
        {
            return fruit.ToString();
        }

        public string EnumArray(Fruit[] fruit)
        {
            if (fruit.Length == 0) return "Empty";
            return string.Join(",", fruit);
        }
    }

    public enum Fruit
    {
        Grape = 1,
        Orange = 2,
        Apple = 3
    }
}