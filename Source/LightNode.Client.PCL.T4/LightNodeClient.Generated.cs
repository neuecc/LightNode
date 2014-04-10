using LightNode.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace LightNode.Client
{
    public partial class LightNodeClient : _IPerf
    {
        static IContentFormatter defaultContentFormatter = new LightNode.Formatter.XmlContentFormatter();
        readonly string rootEndPoint;
        readonly HttpClient httpClient;

        public long MaxResponseContentBufferSize
        {
            get { return httpClient.MaxResponseContentBufferSize; }
            set { httpClient.MaxResponseContentBufferSize = value; }
        }

        public TimeSpan Timeout
        {
            get { return httpClient.Timeout; }
            set { httpClient.Timeout = value; }
        }

        IContentFormatter contentFormatter;
        public IContentFormatter ContentFormatter
        {
            get { return contentFormatter = (contentFormatter ?? defaultContentFormatter); }
            set { contentFormatter = value; }
        }

        public _IPerf Perf { get { return this; } }

        public LightNodeClient(string rootEndPoint)
        {
            this.httpClient = new HttpClient();
            this.rootEndPoint = rootEndPoint.TrimEnd('/');
            this.ContentFormatter = defaultContentFormatter;
        }

        public LightNodeClient(string rootEndPoint, HttpMessageHandler innerHandler)
        {
            this.httpClient = new HttpClient(innerHandler);
            this.rootEndPoint = rootEndPoint.TrimEnd('/');
            this.ContentFormatter = defaultContentFormatter;
        }

        public LightNodeClient(string rootEndPoint, HttpMessageHandler innerHandler, bool disposeHandler)
        {
            this.httpClient = new HttpClient(innerHandler, disposeHandler);
            this.rootEndPoint = rootEndPoint.TrimEnd('/');
            this.ContentFormatter = defaultContentFormatter;
        }

        protected virtual async Task PostAsync(string method, FormUrlEncodedContent content, CancellationToken cancellationToken)
        {
            var response = await httpClient.PostAsync(rootEndPoint + method, content, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }

        protected virtual async Task<T> PostAsync<T>(string method, FormUrlEncodedContent content, CancellationToken cancellationToken)
        {
            if (ContentFormatter.MediaType != null) content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(ContentFormatter.MediaType);
            var response = await httpClient.PostAsync(rootEndPoint + method, content, cancellationToken).ConfigureAwait(false);
            using (var stream = await response.EnsureSuccessStatusCode().Content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                return (T)ContentFormatter.Deserialize(typeof(T), stream);
            }
        }

        #region _IPerf

        System.Threading.Tasks.Task<LightNode.Performance.MyClass> _IPerf.EchoAsync(System.String name, System.Int32 x, System.Int32 y, LightNode.Performance.MyEnum e, System.Threading.CancellationToken cancellationToken)
        {
            var list = new List<KeyValuePair<string, string>>(4);
            if (name != null) list.Add(new KeyValuePair<string, string>("name", name.ToString()));
            list.Add(new KeyValuePair<string, string>("x", x.ToString()));
            list.Add(new KeyValuePair<string, string>("y", y.ToString()));
            list.Add(new KeyValuePair<string, string>("e", e.ToString()));

            return PostAsync<LightNode.Performance.MyClass>("/Perf/Echo", new FormUrlEncodedContent(list), cancellationToken);
        }

        System.Threading.Tasks.Task _IPerf.TestAsync(System.String a, System.Nullable<System.Int32> x, System.Threading.CancellationToken cancellationToken)
        {
            var list = new List<KeyValuePair<string, string>>(2);
            if (a != null) list.Add(new KeyValuePair<string, string>("a", a.ToString()));
            if (x != null) list.Add(new KeyValuePair<string, string>("x", x.ToString()));

            return PostAsync("/Perf/Test", new FormUrlEncodedContent(list), cancellationToken);
        }

        System.Threading.Tasks.Task _IPerf.TeAsync(System.Threading.CancellationToken cancellationToken)
        {
            var list = new List<KeyValuePair<string, string>>(0);

            return PostAsync("/Perf/Te", new FormUrlEncodedContent(list), cancellationToken);
        }

        System.Threading.Tasks.Task _IPerf.TestArrayAsync(System.String[] array, System.Int32[] array2, System.Threading.CancellationToken cancellationToken)
        {
            var list = new List<KeyValuePair<string, string>>(2);
            if (array != null) list.AddRange(array.Select(___x => new KeyValuePair<string, string>("array", ___x.ToString())));
            if (array2 != null) list.AddRange(array2.Select(___x => new KeyValuePair<string, string>("array2", ___x.ToString())));

            return PostAsync("/Perf/TestArray", new FormUrlEncodedContent(list), cancellationToken);
        }

        System.Threading.Tasks.Task _IPerf.TeVoidAsync(System.Threading.CancellationToken cancellationToken)
        {
            var list = new List<KeyValuePair<string, string>>(0);

            return PostAsync("/Perf/TeVoid", new FormUrlEncodedContent(list), cancellationToken);
        }

        System.Threading.Tasks.Task<System.String> _IPerf.Te4Async(System.String xs, System.Threading.CancellationToken cancellationToken)
        {
            var list = new List<KeyValuePair<string, string>>(1);
            if (xs != null) list.Add(new KeyValuePair<string, string>("xs", xs.ToString()));

            return PostAsync<System.String>("/Perf/Te4", new FormUrlEncodedContent(list), cancellationToken);
        }

        #endregion

    }

    public interface _IPerf
    {
        System.Threading.Tasks.Task<LightNode.Performance.MyClass> EchoAsync(System.String name, System.Int32 x, System.Int32 y, LightNode.Performance.MyEnum e, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
        System.Threading.Tasks.Task TestAsync(System.String a = null, System.Nullable<System.Int32> x = null, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
        System.Threading.Tasks.Task TeAsync(System.Threading.CancellationToken cancellationToken = default(CancellationToken));
        System.Threading.Tasks.Task TestArrayAsync(System.String[] array, System.Int32[] array2, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
        System.Threading.Tasks.Task TeVoidAsync(System.Threading.CancellationToken cancellationToken = default(CancellationToken));
        System.Threading.Tasks.Task<System.String> Te4Async(System.String xs, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    }

}