using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightNode.Server.Tests
{
    [TestClass]
    public class IgnoreOperationTest
    {
        [TestMethod]
        public void Ignore()
        {
            MockEnv.CreateRequest("/IgnoreContract/Hoge").GetAsync().Result.StatusCode.Is(System.Net.HttpStatusCode.NotFound);
            MockEnv.CreateRequest("/IgnoreMethod/Hoge").GetAsync().Result.StatusCode.Is(System.Net.HttpStatusCode.NotFound);
            MockEnv.CreateRequest("/IgnoreMethod/Huga").GetString().Is("1");
        }
    }

    [IgnoreOperation]
    public class IgnoreContract : LightNodeContract
    {
        public void Hoge()
        {
        }
    }

    public class IgnoreMethod : LightNodeContract
    {
        [IgnoreOperation]
        public void Hoge()
        {
        }

        public int Huga()
        {
            return 1;
        }
    }
}
