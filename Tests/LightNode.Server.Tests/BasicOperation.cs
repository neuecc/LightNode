using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Owin.Testing;
using Owin;
using System.Net.Http;
using System.Threading.Tasks;

namespace LightNode.Server.Tests
{
    [TestClass]
    public class BasicOperation
    {
        [TestMethod]
        public void Hello()
        {
            MockEnv.CreateRequest("/Hello/Say").GetStringAsync().Is("\"Hello LightNode\"");
        }

        [TestMethod]
        public void GetBasic()
        {
            MockEnv.CreateRequest("/TestContract/Add?x=10&y=20").GetStringAsync().Is("30");
            MockEnv.CreateRequest("/TestContract/AddWithDefault?y=20&z=5&x=10").GetStringAsync().Is("35");
            MockEnv.CreateRequest("/TestContract/AddWithDefault?x=10&y=20").GetStringAsync().Is("330");

            MockEnv.CreateRequest("/TestContract/TaskAdd?x=10&y=20").GetStringAsync().Is("30");
            MockEnv.CreateRequest("/TestContract/TaskAddWithDefault?y=20&z=5&x=10").GetStringAsync().Is("35");
            MockEnv.CreateRequest("/TestContract/TaskAddWithDefault?x=10&y=20").GetStringAsync().Is("330");

            TestContract.VoidBeforeAfter.Is("Before");
            MockEnv.CreateRequest("/TestContract/VoidCheck").GetAsync().Result.Dispose();
            TestContract.VoidBeforeAfter.Is("After");

            TestContract.VoidBeforeAfter = "Before";
            MockEnv.CreateRequest("/TestContract/TaskVoidCheck").GetAsync().Result.Dispose();
            TestContract.VoidBeforeAfter.Is("After");
        }
    }

    public class Hello : ILightNodeContract
    {
        public string Say()
        {
            return "Hello LightNode";
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

        public static string VoidBeforeAfter = "Before";
        public void VoidCheck()
        {
            VoidBeforeAfter = "After";
        }

        public async Task TaskVoidCheck()
        {
            await Task.Delay(TimeSpan.FromMilliseconds(10)).ConfigureAwait(false);
            VoidBeforeAfter = "After";
        }
    }
}
