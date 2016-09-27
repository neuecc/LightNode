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
#if DEBUG
    public partial class LightNodeClient : _IPerf, _IDebugOnlyTest, _IDebugOnlyMethodTest
#else
    public partial class LightNodeClient : _IPerf, _IDebugOnlyMethodTest
#endif
    {
        static IContentFormatter defaultContentFormatter = new LightNode.Formatter.JsonNetContentFormatter();
        readonly string rootEndPoint;
        readonly HttpClient httpClient;

        partial void OnAfterInitialized();

        public System.Net.Http.Headers.HttpRequestHeaders DefaultRequestHeaders
        {
            get { return httpClient.DefaultRequestHeaders; }
        }

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
        public _IDebugOnlyMethodTest DebugOnlyMethodTest { get { return this; } }

#if DEBUG
        public _IDebugOnlyTest DebugOnlyTest { get { return this; } }
#endif

        public LightNodeClient(string rootEndPoint)
        {
            this.httpClient = new HttpClient();
            this.rootEndPoint = rootEndPoint.TrimEnd('/');
            this.ContentFormatter = defaultContentFormatter;
            OnAfterInitialized();
        }

        public LightNodeClient(string rootEndPoint, HttpMessageHandler innerHandler)
        {
            this.httpClient = new HttpClient(innerHandler);
            this.rootEndPoint = rootEndPoint.TrimEnd('/');
            this.ContentFormatter = defaultContentFormatter;
            OnAfterInitialized();
        }

        public LightNodeClient(string rootEndPoint, HttpMessageHandler innerHandler, bool disposeHandler)
        {
            this.httpClient = new HttpClient(innerHandler, disposeHandler);
            this.rootEndPoint = rootEndPoint.TrimEnd('/');
            this.ContentFormatter = defaultContentFormatter;
            OnAfterInitialized();
        }

        protected virtual async Task PostAsync(string method, HttpContent content, CancellationToken cancellationToken)
        {
            var response = await httpClient.PostAsync(rootEndPoint + method, content, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }

        protected virtual async Task<T> PostAsync<T>(string method, HttpContent content, CancellationToken cancellationToken)
        {
            var response = await httpClient.PostAsync(rootEndPoint + method, content, cancellationToken).ConfigureAwait(false);
            using (var stream = await response.EnsureSuccessStatusCode().Content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                return (T)ContentFormatter.Deserialize(typeof(T), stream);
            }
        }

        #region _IPerf



        System.Threading.Tasks.Task<LightNode.Performance.MyClass> _IPerf.EchoAsync(System.String name, System.Int32 x, System.Int32 y, LightNode.Performance.MyEnum e, System.Threading.CancellationToken cancellationToken)
        {
            HttpContent __content = null;
            var list = new List<KeyValuePair<string, string>>(4);
            if (name != null) list.Add(new KeyValuePair<string, string>("name", name));
            list.Add(new KeyValuePair<string, string>("x", x.ToString()));
            list.Add(new KeyValuePair<string, string>("y", y.ToString()));
            list.Add(new KeyValuePair<string, string>("e", ((System.Int32)e).ToString()));
            __content = new FormUrlEncodedContent(list);
            return PostAsync<LightNode.Performance.MyClass>("/Perf/Echo", __content, cancellationToken);
        }


        System.Threading.Tasks.Task _IPerf.TestAsync(System.String a, System.Nullable<System.Int32> x, System.Nullable<LightNode.Performance.MyEnum2> z, System.Threading.CancellationToken cancellationToken)
        {
            HttpContent __content = null;
            var list = new List<KeyValuePair<string, string>>(3);
            if (a != null) list.Add(new KeyValuePair<string, string>("a", a));
            if (x != null) list.Add(new KeyValuePair<string, string>("x", x.ToString()));
            if (z != null) list.Add(new KeyValuePair<string, string>("z", ((System.UInt64)z).ToString()));
            __content = new FormUrlEncodedContent(list);
            return PostAsync("/Perf/Test", __content, cancellationToken);
        }


        System.Threading.Tasks.Task _IPerf.TeAsync(System.Threading.CancellationToken cancellationToken)
        {
            HttpContent __content = null;
            var list = new List<KeyValuePair<string, string>>(0);
            __content = new FormUrlEncodedContent(list);
            return PostAsync("/Perf/Te", __content, cancellationToken);
        }


        System.Threading.Tasks.Task _IPerf.TestArrayAsync(System.String[] array, System.Int32[] array2, LightNode.Performance.MyEnum[] array3, System.Threading.CancellationToken cancellationToken)
        {
            HttpContent __content = null;
            var list = new List<KeyValuePair<string, string>>(3);
            if (array != null) list.AddRange(array.Select(___x => new KeyValuePair<string, string>("array", ___x)));
            if (array2 != null) list.AddRange(array2.Select(___x => new KeyValuePair<string, string>("array2", ___x.ToString())));
            if (array3 != null) list.AddRange(array3.Select(___x => new KeyValuePair<string, string>("array3", ((System.Int32)___x).ToString())));
            __content = new FormUrlEncodedContent(list);
            return PostAsync("/Perf/TestArray", __content, cancellationToken);
        }


        System.Threading.Tasks.Task _IPerf.TeVoidAsync(System.Threading.CancellationToken cancellationToken)
        {
            HttpContent __content = null;
            var list = new List<KeyValuePair<string, string>>(0);
            __content = new FormUrlEncodedContent(list);
            return PostAsync("/Perf/TeVoid", __content, cancellationToken);
        }


        System.Threading.Tasks.Task _IPerf.ByteArrayCheck1Async(System.Int32 x, System.String y, System.Byte[] byteArray, System.Threading.CancellationToken cancellationToken)
        {
            HttpContent __content = null;
            var __multi = new MultipartFormDataContent();
            __multi.Add(new StringContent(x.ToString()), "x");
            if (y != null) __multi.Add(new StringContent(y), "y");
            __multi.Add(new ByteArrayContent(byteArray), "byteArray");
            __content = __multi;
            return PostAsync("/Perf/ByteArrayCheck1", __content, cancellationToken);
        }


        System.Threading.Tasks.Task _IPerf.ByteArrayCheck2Async(System.String[] array, System.Int32[] array2, LightNode.Performance.MyEnum[] array3, System.Byte[] byteArray, System.Threading.CancellationToken cancellationToken)
        {
            HttpContent __content = null;
            var __multi = new MultipartFormDataContent();
            if (array != null) foreach(var __x in array) { __multi.Add(new StringContent(__x), "array"); }
            if (array2 != null) foreach(var __x in array2) { __multi.Add(new StringContent(__x.ToString()), "array2"); }
            if (array3 != null) foreach(var __x in array3) { __multi.Add(new StringContent(((System.Int32)__x).ToString()), "array3"); }
            __multi.Add(new ByteArrayContent(byteArray), "byteArray");
            __content = __multi;
            return PostAsync("/Perf/ByteArrayCheck2", __content, cancellationToken);
        }


        System.Threading.Tasks.Task _IPerf.ByteArrayCheck3Async(System.Int32 x, System.String y, System.Byte[] byteArray, System.String a, System.Nullable<System.Int32> xxx, System.Nullable<LightNode.Performance.MyEnum2> z, System.Threading.CancellationToken cancellationToken)
        {
            HttpContent __content = null;
            var __multi = new MultipartFormDataContent();
            __multi.Add(new StringContent(x.ToString()), "x");
            if (y != null) __multi.Add(new StringContent(y), "y");
            __multi.Add(new ByteArrayContent(byteArray), "byteArray");
            if (a != null) __multi.Add(new StringContent(a), "a");
            if (xxx != null) __multi.Add(new StringContent(xxx.ToString()), "xxx");
            if (z != null) __multi.Add(new StringContent(((System.UInt64)z).ToString()), "z");
            __content = __multi;
            return PostAsync("/Perf/ByteArrayCheck3", __content, cancellationToken);
        }


        System.Threading.Tasks.Task<System.String> _IPerf.Te4Async(System.String xs, System.Threading.CancellationToken cancellationToken)
        {
            HttpContent __content = null;
            var list = new List<KeyValuePair<string, string>>(1);
            if (xs != null) list.Add(new KeyValuePair<string, string>("xs", xs));
            __content = new FormUrlEncodedContent(list);
            return PostAsync<System.String>("/Perf/Te4", __content, cancellationToken);
        }


        System.Threading.Tasks.Task<System.String> _IPerf.PostStringAsync(System.String hoge, System.Threading.CancellationToken cancellationToken)
        {
            HttpContent __content = null;
            var list = new List<KeyValuePair<string, string>>(1);
            if (hoge != null) list.Add(new KeyValuePair<string, string>("hoge", hoge));
            __content = new FormUrlEncodedContent(list);
            return PostAsync<System.String>("/Perf/PostString", __content, cancellationToken);
        }


        #endregion

        #region _IDebugOnlyTest

#if DEBUG


        System.Threading.Tasks.Task _IDebugOnlyTest.HogeAsync(System.Threading.CancellationToken cancellationToken)
        {
            HttpContent __content = null;
            var list = new List<KeyValuePair<string, string>>(0);
            __content = new FormUrlEncodedContent(list);
            return PostAsync("/DebugOnlyTest/Hoge", __content, cancellationToken);
        }


#endif
        #endregion

        #region _IDebugOnlyMethodTest


#if DEBUG

        System.Threading.Tasks.Task _IDebugOnlyMethodTest.HogeAsync(System.Threading.CancellationToken cancellationToken)
        {
            HttpContent __content = null;
            var list = new List<KeyValuePair<string, string>>(0);
            __content = new FormUrlEncodedContent(list);
            return PostAsync("/DebugOnlyMethodTest/Hoge", __content, cancellationToken);
        }
#endif


        #endregion

    }

    public interface _IPerf
    {
        System.Threading.Tasks.Task<LightNode.Performance.MyClass> EchoAsync(System.String name, System.Int32 x, System.Int32 y, LightNode.Performance.MyEnum e, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
        System.Threading.Tasks.Task TestAsync(System.String a = null, System.Nullable<System.Int32> x = null, System.Nullable<LightNode.Performance.MyEnum2> z = null, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
        System.Threading.Tasks.Task TeAsync(System.Threading.CancellationToken cancellationToken = default(CancellationToken));
        System.Threading.Tasks.Task TestArrayAsync(System.String[] array, System.Int32[] array2, LightNode.Performance.MyEnum[] array3, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
        System.Threading.Tasks.Task TeVoidAsync(System.Threading.CancellationToken cancellationToken = default(CancellationToken));
        System.Threading.Tasks.Task ByteArrayCheck1Async(System.Int32 x, System.String y, System.Byte[] byteArray, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
        System.Threading.Tasks.Task ByteArrayCheck2Async(System.String[] array, System.Int32[] array2, LightNode.Performance.MyEnum[] array3, System.Byte[] byteArray, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
        System.Threading.Tasks.Task ByteArrayCheck3Async(System.Int32 x, System.String y, System.Byte[] byteArray, System.String a = null, System.Nullable<System.Int32> xxx = null, System.Nullable<LightNode.Performance.MyEnum2> z = null, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
        System.Threading.Tasks.Task<System.String> Te4Async(System.String xs, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
        System.Threading.Tasks.Task<System.String> PostStringAsync(System.String hoge, System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    }
#if DEBUG
    public interface _IDebugOnlyTest
    {
        System.Threading.Tasks.Task HogeAsync(System.Threading.CancellationToken cancellationToken = default(CancellationToken));
    }
#endif
    public interface _IDebugOnlyMethodTest
    {
#if DEBUG
        System.Threading.Tasks.Task HogeAsync(System.Threading.CancellationToken cancellationToken = default(CancellationToken));
#endif
    }
}

