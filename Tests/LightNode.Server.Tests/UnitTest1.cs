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
            MockEnv.CreateRequest("/Moge/Hello").GetStringAsync().Is("\"Hello\"");
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
