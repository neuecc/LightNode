using System;
using System.Linq;
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

        [TestMethod]
        public void Array()
        {
            MockEnv.CreateRequest("/ArrayContract/Sum?xs=1&xs=2&xs=3").GetString().Is("6");
            MockEnv.CreateRequest("/ArrayContract/Sum?xs=1000").GetString().Is("1000"); // single arg

            MockEnv.CreateRequest("/ArrayContract/Sum2?x=2&xs=1&xs=2&xs=3&y=30&ys=40&ys=50").GetString().Is("128");
            MockEnv.CreateRequest("/ArrayContract/Sum2?x=2&xs=1&y=30&ys=50").GetString().Is("83");
        }

        [TestMethod]
        public void ParameterMismatch()
        {
            MockEnv.CreateRequest("/TestContract/Add").GetAsync().Result.StatusCode.Is(System.Net.HttpStatusCode.BadRequest);
            MockEnv.CreateRequest("/TestContract/Add?x=10").GetAsync().Result.StatusCode.Is(System.Net.HttpStatusCode.BadRequest);
            MockEnv.CreateRequest("/TestContract/Add?x=10&x=20").GetAsync().Result.StatusCode.Is(System.Net.HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public void InvalidTypeParameter()
        {
            var v = MockEnv.CreateRequest("/TestContract/Add?x=hoge&y=huga").GetAsync().Result.StatusCode;
        }
    }

    public class Hello : LightNodeContract
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

    public class TestContract : LightNodeContract
    {
        public int Add(int x, int y)
        {
            Environment.IsNotNull();
            return x + y;
        }

        public int AddWithDefault(int x, int y, int z = 300)
        {
            Environment.IsNotNull();
            return x + y + z;
        }

        public Task<int> TaskAdd(int x, int y)
        {
            Environment.IsNotNull();
            return Task.Run(() => x + y);
        }
        public Task<int> TaskAddWithDefault(int x, int y, int z = 300)
        {
            Environment.IsNotNull();
            return Task.Run(() => x + y + z);
        }

        public static ConcurrentDictionary<string, string> VoidBeforeAfter = new ConcurrentDictionary<string, string>();
        public void VoidCheck(string guid, string after)
        {
            Environment.IsNotNull();
            VoidBeforeAfter[guid] = after;
        }

        public async Task TaskVoidCheck(string guid, string after)
        {
            Environment.IsNotNull();
            await Task.Delay(TimeSpan.FromMilliseconds(10)).ConfigureAwait(false);
            VoidBeforeAfter[guid] = after;
        }
    }

    public class ArrayContract : LightNodeContract
    {
        public int Sum(int[] xs)
        {
            return xs.Sum();
        }

        public int Sum2(int x, int[] xs, int y, int[] ys)
        {
            return x + xs.Sum() + y + ys.Sum();
        }
    }
}