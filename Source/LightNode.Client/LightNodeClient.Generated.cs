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
    public partial class LightNodeClient : _IMyOperations, _IRoomOperations
    {
        static IContentFormatter defaultContentFormatter = new LightNode.Formatters.DataContractJsonContentTypeFormatter();
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

        public _IMyOperations My { get { return this; } }
        public _IRoomOperations Room { get { return this; } }

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
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(ContentFormatter.MediaType);
            var response = await httpClient.PostAsync(rootEndPoint + method, content, cancellationToken).ConfigureAwait(false);
            using (var stream = await response.EnsureSuccessStatusCode().Content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                return (T)ContentFormatter.Deserialize(typeof(T), stream);
            }
        }

       #region _IMyOperations

        System.Threading.Tasks.Task<System.String> _IMyOperations.Echo(System.String x, System.Threading.CancellationToken cancellationToken)
		{
		    return PostAsync<System.String>("/My/Echo", new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("x", x.ToString()),
            }), cancellationToken);
		}

        System.Threading.Tasks.Task<System.Int32> _IMyOperations.Sum(System.Int32 x, System.Nullable<System.Int32> y, System.Int32 z, System.Threading.CancellationToken cancellationToken)
		{
		    return PostAsync<System.Int32>("/My/Sum", new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("x", x.ToString()),
                new KeyValuePair<string, string>("y", y.ToString()),
                new KeyValuePair<string, string>("z", z.ToString()),
            }), cancellationToken);
		}

	   #endregion

       #region _IRoomOperations

        System.Threading.Tasks.Task _IRoomOperations.Create(System.Threading.CancellationToken cancellationToken)
		{
		    return PostAsync("/Room/Create", new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
            }), cancellationToken);
		}

	   #endregion

    }

    public interface _IMyOperations
    {
        System.Threading.Tasks.Task<System.String> Echo(System.String x, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
        System.Threading.Tasks.Task<System.Int32> Sum(System.Int32 x, System.Nullable<System.Int32> y, System.Int32 z = 1000, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    }

    public interface _IRoomOperations
    {
        System.Threading.Tasks.Task Create(System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    }

}