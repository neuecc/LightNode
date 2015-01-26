using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LightNode.Server.Tests
{
    [TestClass]
    public class ValueProviderTest
    {
        [TestMethod]
        public void Get()
        {
            var mockEnv = new Dictionary<string, object>();
            mockEnv["owin.RequestQueryString"] = "a=huga&b=nano&c=tako&a=takotyop";
            mockEnv["owin.RequestBody"] = new MemoryStream();

            var provider = new ValueProvider(mockEnv, AcceptVerbs.Get);
            provider.GetValue("a").IsInstanceOf<List<string>>().Is("huga", "takotyop");
            provider.GetValue("b").IsInstanceOf<string>().Is("nano");
            provider.GetValue("c").IsInstanceOf<string>().Is("tako");
        }

        [TestMethod]
        public void Post()
        {
            var more = "b=zzz&hugahuga=413413&b=tamanegi";
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(more));
            ms.Position = 0;

            var mockEnv = new Dictionary<string, object>();
            mockEnv["owin.RequestQueryString"] = "a=huga&b=nano&c=tako&a=takotyop";
            mockEnv["owin.RequestBody"] = ms;
            mockEnv["owin.RequestHeaders"] = new Dictionary<string, string[]>() { { "Content-Type", new[] { "application/x-www-form-urlencoded" } } };

            var provider = new ValueProvider(mockEnv, AcceptVerbs.Post);
            provider.GetValue("a").IsInstanceOf<List<string>>().Is("huga", "takotyop");
            provider.GetValue("b").IsInstanceOf<List<string>>().Is("nano", "zzz", "tamanegi");
            provider.GetValue("c").IsInstanceOf<string>().Is("tako");
            provider.GetValue("hugahuga").IsInstanceOf<string>().Is("413413");
        }
    }
}
