using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Owin.Testing;
using Owin;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Collections.Concurrent;

namespace LightNode.Server.Tests
{
    [TestClass]
    public class BasicOperation
    {
        [TestMethod]
        public void Hello()
        {
            MockEnv.CreateRequest("/Hello/Say").GetString().Is("\"Hello LightNode\"");
            MockEnv.CreateRequest("/Hello/Say").PostAndGetString(new StringKeyValuePairCollection()).Is("\"Hello LightNode\"");
        }

        [TestMethod]
        public void GetBasic()
        {
            MockEnv.CreateRequest("/TestContract/Add?x=10&y=20").GetString().Is("30");
            MockEnv.CreateRequest("/TestContract/AddWithDefault?y=20&z=5&x=10").GetString().Is("35");
            MockEnv.CreateRequest("/TestContract/AddWithDefault?x=10&y=20").GetString().Is("330");

            MockEnv.CreateRequest("/TestContract/TaskAdd?x=10&y=20").GetString().Is("30");
            MockEnv.CreateRequest("/TestContract/TaskAddWithDefault?y=20&z=5&x=10").GetString().Is("35");
            MockEnv.CreateRequest("/TestContract/TaskAddWithDefault?x=10&y=20").GetString().Is("330");

            var guid = Guid.NewGuid().ToString();
            TestContract.VoidBeforeAfter[guid] = "Before";
            MockEnv.CreateRequest("/TestContract/VoidCheck?after=After&guid=" + guid).GetAsync().Result.Dispose();
            TestContract.VoidBeforeAfter[guid].Is("After");

            TestContract.VoidBeforeAfter[guid] = "Before";
            MockEnv.CreateRequest("/TestContract/TaskVoidCheck?after=After&guid=" + guid).GetAsync().Result.Dispose();
            TestContract.VoidBeforeAfter[guid].Is("After");
        }

        [TestMethod]
        public void PostBasic()
        {
            MockEnv.CreateRequest("/TestContract/Add")
                .PostAndGetString(new StringKeyValuePairCollection { { "x", "10" }, { "y", "20" } })
                .Is("30");
            MockEnv.CreateRequest("/TestContract/AddWithDefault")
                .PostAndGetString(new StringKeyValuePairCollection { { "y", "20" }, { "z", "5" }, { "x", "10" } })
                .Is("35");
            MockEnv.CreateRequest("/TestContract/AddWithDefault")
                .PostAndGetString(new StringKeyValuePairCollection { { "y", "20" }, { "x", "10" } })
                .Is("330");

            MockEnv.CreateRequest("/TestContract/TaskAdd")
                .PostAndGetString(new StringKeyValuePairCollection { { "x", "10" }, { "y", "20" } })
                .Is("30");
            MockEnv.CreateRequest("/TestContract/TaskAddWithDefault")
                .PostAndGetString(new StringKeyValuePairCollection { { "y", "20" }, { "z", "5" }, { "x", "10" } })
                .Is("35");
            MockEnv.CreateRequest("/TestContract/TaskAddWithDefault")
                .PostAndGetString(new StringKeyValuePairCollection { { "y", "20" }, { "x", "10" } })
                .Is("330");

            var guid = Guid.NewGuid().ToString();
            TestContract.VoidBeforeAfter[guid] = "Before";
            MockEnv.CreateRequest("/TestContract/VoidCheck")
                .And(x => x.Content = new FormUrlEncodedContent(new StringKeyValuePairCollection { { "guid", guid }, { "after", "After" } }))
                .PostAsync().Result.Dispose();
            TestContract.VoidBeforeAfter[guid].Is("After");

            TestContract.VoidBeforeAfter[guid] = "Before";
            MockEnv.CreateRequest("/TestContract/TaskVoidCheck")
                .And(x => x.Content = new FormUrlEncodedContent(new StringKeyValuePairCollection { { "guid", guid }, { "after", "After" } }))
                .PostAsync().Result.Dispose();
            TestContract.VoidBeforeAfter[guid].Is("After");
        }

        [TestMethod]
        public void CaseSensitive()
        {
            MockEnv.CreateRequest("/heLLo/pInG").GetString().Is("\"Pong\"");
        }

        [TestMethod]
        public void NotFound()
        {
            MockEnv.CreateRequest("").GetAsync().Result.StatusCode.Is(System.Net.HttpStatusCode.NotFound);
            MockEnv.CreateRequest("/hello").GetAsync().Result.StatusCode.Is(System.Net.HttpStatusCode.NotFound);
            MockEnv.CreateRequest("/hello/").GetAsync().Result.StatusCode.Is(System.Net.HttpStatusCode.NotFound);
            MockEnv.CreateRequest("/hello/pin").GetAsync().Result.StatusCode.Is(System.Net.HttpStatusCode.NotFound);
            MockEnv.CreateRequest("/hello/pingoo").GetAsync().Result.StatusCode.Is(System.Net.HttpStatusCode.NotFound);
            MockEnv.CreateRequest("/hello/ping/oo").GetAsync().Result.StatusCode.Is(System.Net.HttpStatusCode.NotFound);
        }
    }

    public class Hello : ILightNodeContract
    {
        public string Say()
        {
            return "Hello LightNode";
        }

        public string Ping()
        {
            return "Pong";
        }
    }

    public class TestContract : ILightNodeContract
    {
        public int Add(int x, int y)
        {
            return x + y;
        }

        public int AddWithDefault(int x, int y, int z = 300)
        {
            return x + y + z;
        }

        public Task<int> TaskAdd(int x, int y)
        {
            return Task.Run(() => x + y);
        }
        public Task<int> TaskAddWithDefault(int x, int y, int z = 300)
        {
            return Task.Run(() => x + y + z);
        }

        public static ConcurrentDictionary<string, string> VoidBeforeAfter = new ConcurrentDictionary<string, string>();
        public void VoidCheck(string guid, string after)
        {
            VoidBeforeAfter[guid] = after;
        }

        public async Task TaskVoidCheck(string guid, string after)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(10)).ConfigureAwait(false);
            VoidBeforeAfter[guid] = after;
        }
    }
}
