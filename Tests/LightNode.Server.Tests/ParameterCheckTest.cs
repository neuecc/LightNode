using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

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
            MockEnv.CreateRequest("/ParameterContract/String").GetString().Trim('\"').Is("null");
            MockEnv.CreateRequest("/ParameterContract/String?x=hoge").GetString().Trim('\"').Is("hoge");
        }

        [TestMethod]
        public void StringDefaultValue()
        {
            MockEnv.CreateRequest("/ParameterContract/StringDefaultValue").GetString().Trim('\"').Is("hogehoge");
            MockEnv.CreateRequest("/ParameterContract/StringDefaultValue?x=hoge").GetString().Trim('\"').Is("hoge");
            MockEnv.CreateRequest("/ParameterContract/StringDefaultValue?x=hoge&x=huga").GetString().Trim('\"').Is("hoge");
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

        public string StringDefaultValue(string x = "hogehoge")
        {
            return x;
        }

        public int Array(int[] x)
        {
            return x.Length;
        }

        // TODO:string, enum types
    }

    public enum Fruit
    {
        Grape = 0,
        Orange = 1,
        Apple = 2
    }
}
