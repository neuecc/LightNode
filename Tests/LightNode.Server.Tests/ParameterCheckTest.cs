using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace LightNode.Server.Tests
{
    [TestClass]
    public class ParameterCheckTest
    {
        [TestMethod]
        public void ParameterCheck()
        {
            MockEnv.CreateRequest("/ParameterContract/Struct").GetAsync().Result.StatusCode.Is(HttpStatusCode.BadRequest); // lack of param
            MockEnv.CreateRequest("/ParameterContract/Struct?x=1").GetString().Trim('\"').Is("1"); // ok
            MockEnv.CreateRequest("/ParameterContract/Struct?x=hoge").GetAsync().Result.StatusCode.Is(HttpStatusCode.BadRequest); // can't parse

            MockEnv.CreateRequest("/ParameterContract/Nullable").GetString().Trim('\"').Is("null");
            MockEnv.CreateRequest("/ParameterContract/Nullable?x=1").GetString().Trim('\"').Is("1");
            MockEnv.CreateRequest("/ParameterContract/Nullable?x=hoge").GetString().Trim('\"').Is("null");

            // TODO:more parameter checks
        }

        [TestMethod]
        public void MyTestMethod()
        {

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

        public int Array(int[] x)
        {
            return x.Length;
        }

        // TODO:string, enum types
    }
}
