using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Owin;
using System.Net.Http;

namespace LightNode.Server.Tests
{
    // Owin Mock Environment

    [TestClass]
    public class MockEnv
    {
        public static TestServer TestServer { get; private set; }

        public static HttpClient CreateHttpClient()
        {
            return new HttpClient(TestServer.Handler);
        }
        public static RequestBuilder CreateRequest(string path)
        {
            return TestServer.CreateRequest(path);
        }

        [AssemblyInitialize]
        public static void Initialize(TestContext cx)
        {
            TestServer = TestServer.Create(app =>
            {
                app.UseLightNode(
                    new LightNodeOptions(AcceptVerbs.Get | AcceptVerbs.Post, new JavaScriptContentTypeFormatter())
                    , typeof(MockEnv).Assembly);
            });
        }

        [AssemblyCleanup]
        public static void Cleanup()
        {
            if (TestServer != null) TestServer.Dispose();
        }
    }

    public static class RequestBuilderExtensions
    {
        public static string GetStringAsync(this RequestBuilder builder)
        {
            return builder.GetAsync().Result.Content.ReadAsStringAsync().Result;
        }
        public static byte[] GetByteArrayAsync(this RequestBuilder builder)
        {
            return builder.GetAsync().Result.Content.ReadAsByteArrayAsync().Result;
        }
    }
}