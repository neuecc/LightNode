using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Owin;
using System.Collections.Generic;
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
                    new LightNodeOptions(AcceptVerbs.Get | AcceptVerbs.Post,
                        new JavaScriptContentTypeFormatter(),
                        new TextContentTypeFormatter())
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
        public static string GetString(this RequestBuilder builder)
        {
            return builder.GetAsync().Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result;
        }

        public static byte[] GetByteArray(this RequestBuilder builder)
        {
            return builder.GetAsync().Result.EnsureSuccessStatusCode().Content.ReadAsByteArrayAsync().Result;
        }

        public static string PostAndGetString(this RequestBuilder builder, StringKeyValuePairCollection nameValueCollection)
        {
            return builder.And(x => x.Content = new FormUrlEncodedContent(nameValueCollection))
                .PostAsync().Result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result;
        }

        public static byte[] PostAndGetByteArray(this RequestBuilder builder, StringKeyValuePairCollection nameValueCollection)
        {
            return builder.And(x => x.Content = new FormUrlEncodedContent(nameValueCollection))
                .PostAsync().Result.EnsureSuccessStatusCode().Content.ReadAsByteArrayAsync().Result;
        }
    }

    public class StringKeyValuePairCollection : IEnumerable<KeyValuePair<string, string>>
    {
        List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();

        public void Add(string key, string value)
        {
            list.Add(new KeyValuePair<string, string>(key, value));
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}