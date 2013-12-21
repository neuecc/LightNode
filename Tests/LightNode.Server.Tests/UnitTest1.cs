using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Owin.Testing;
using Owin;
using System.Net.Http;
using System.Threading.Tasks;

namespace LightNode.Server.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var client = MockEnv.CreateHttpClient();
            client.GetStringAsync("http://localhost/Moge/Hello").Result.Is("\"Hello\"");
        }
    }

    public class Moge : ILightNodeContract
    {
        public string Hello()
        {
            return "Hello";
        }
    }
}
