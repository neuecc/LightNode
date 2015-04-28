using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace LightNode.Server.Tests
{
    [TestClass]
    public class ReturnStatusCodeTest
    {
        [TestMethod]
        public void StatusCode()
        {
            MockEnv.CreateRequest("/StatusCodeContract/Redirect").GetAsync().Result.StatusCode.Is(System.Net.HttpStatusCode.Redirect);
            MockEnv.CreateRequest("/StatusCodeContract/Unauthrized").GetAsync().Result.StatusCode.Is(System.Net.HttpStatusCode.Unauthorized);

            var r1 = MockEnv.CreateRequest("/StatusCodeContract/ResonPhrase").GetAsync().Result;
            r1.StatusCode.Is(System.Net.HttpStatusCode.ServiceUnavailable);
            r1.ReasonPhrase.Is("Unavailable!!!");

            var r2 = MockEnv.CreateRequest("/StatusCodeContract/Content").GetAsync().Result;
            r2.StatusCode.Is(System.Net.HttpStatusCode.UseProxy);
            r2.ReasonPhrase.Is("Pro!!!");
            var ddd = r2.Content.ReadAsStringAsync().Result;
            ddd.Is("\"UseProxy....\"");
        }
    }


    public class StatusCodeContract : LightNodeContract
    {
        public void Redirect()
        {
            throw new ReturnStatusCodeException(System.Net.HttpStatusCode.Redirect);
        }

        public int Unauthrized()
        {
            throw new ReturnStatusCodeException(System.Net.HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public Task<int> ResonPhrase()
        {
            throw new ReturnStatusCodeException(HttpStatusCode.ServiceUnavailable, "Unavailable!!!");
        }

        [TestMethod]
        public Task Content()
        {
            throw new ReturnStatusCodeException(HttpStatusCode.UseProxy, "Pro!!!", "UseProxy....");
        }
    }
}
