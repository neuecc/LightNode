using LightNode.Core;
using System.Collections;
using System.Text;
using System.IO;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using System.Linq;

namespace LightNode.Core
{
    public interface IContentFormatter
    {
        string MediaType { get; }
        string Ext { get; }
        System.Text.Encoding Encoding { get; }
        void Serialize(System.IO.Stream stream, object obj);
        object Deserialize(System.Type type, System.IO.Stream stream);
    }
}


namespace LightNode.Formatter
{
    public class JsonNetContentFormatter : LightNode.Core.IContentFormatter
    {
        readonly string mediaType;
        readonly string ext;
        readonly Encoding encoding;
        readonly Newtonsoft.Json.JsonSerializer serializer;

        public string MediaType
        {
            get { return mediaType; }
        }

        public string Ext
        {
            get { return ext; }
        }

        public Encoding Encoding
        {
            get { return encoding; }
        }

        public JsonNetContentFormatter(string mediaType = "application/json", string ext = "json")
            : this(new Newtonsoft.Json.JsonSerializer(), mediaType, ext)
        {
        }

        public JsonNetContentFormatter(Newtonsoft.Json.JsonSerializer serializer, string mediaType = "application/json", string ext = "json")
            : this(serializer, System.Text.Encoding.UTF8, mediaType, ext)
        {
        }

        public JsonNetContentFormatter(Encoding encoding, string mediaType = "application/json", string ext = "json")
            : this(new Newtonsoft.Json.JsonSerializer(), encoding, mediaType, ext)
        {
        }

        public JsonNetContentFormatter(Newtonsoft.Json.JsonSerializer serializer, Encoding encoding, string mediaType = "application/json", string ext = "json")
        {
            this.mediaType = mediaType;
            this.ext = ext;
            this.encoding = encoding;
            this.serializer = serializer;
        }

        public void Serialize(System.IO.Stream stream, object obj)
        {
            using (var sw = new StreamWriter(stream, Encoding ?? System.Text.Encoding.UTF8))
            {
                serializer.Serialize(sw, obj);
            }
        }

        public object Deserialize(System.Type type, System.IO.Stream stream)
        {
            using (var sr = new StreamReader(stream, Encoding ?? System.Text.Encoding.UTF8))
            {
                return serializer.Deserialize(sr, type);
            }
        }
    }
}

namespace LightNode.Client
{
#if !(UNITY_METRO || UNITY_WP8) && (UNITY_4_4 || UNITY_4_3 || UNITY_4_2 || UNITY_4_1 || UNITY_4_0_1 || UNITY_4_0 || UNITY_3_5 || UNITY_3_4 || UNITY_3_3 || UNITY_3_2 || UNITY_3_1 || UNITY_3_0_0 || UNITY_3_0 || UNITY_2_6_1 || UNITY_2_6)
    // Fallback for Unity versions below 4.5
    using Hash = System.Collections.Hashtable;
    using HashEntry = System.Collections.DictionaryEntry;    
#else
    // Unity 4.5 release notes: 
    // WWW: deprecated 'WWW(string url, byte[] postData, Hashtable headers)', 
    // use 'public WWW(string url, byte[] postData, Dictionary<string, string> headers)' instead.
    using Hash = System.Collections.Generic.Dictionary<string, string>;
    using HashEntry = System.Collections.Generic.KeyValuePair<string, string>;
#endif

    public partial class LightNodeClient : _IPerf
    {
        static IContentFormatter defaultContentFormatter = new LightNode.Formatter.JsonNetContentFormatter();
        readonly string rootEndPoint;

        IContentFormatter contentFormatter;
        public IContentFormatter ContentFormatter
        {
            get { return contentFormatter = (contentFormatter ?? defaultContentFormatter); }
            set { contentFormatter = value; }
        }

        Hash defaultHeaders;
        public Hash DefaultHeaders
        {
            get { return defaultHeaders = (defaultHeaders ?? new Hash()); }
            set { defaultHeaders = value; }
        }

        partial void OnAfterInitialized();

        partial void OnBeforeRequest(string contractName, string operationName, List<KeyValuePair<string, string[]>> contentList, ref Hash headerForUse);

        partial void ResultFilter<T>(ref IObservable<T> source);

        public _IPerf Perf { get { return this; } }

        public LightNodeClient(string rootEndPoint)
        {
            this.rootEndPoint = rootEndPoint.TrimEnd('/');
            this.ContentFormatter = defaultContentFormatter;
            OnAfterInitialized();
        }

        Hash CopyHeaders()
        {
            if(defaultHeaders == null) return defaultHeaders;
            var hash = new Hash();
            foreach(HashEntry item in defaultHeaders) hash.Add(item.Key, item.Value);
            return hash;
        }

        protected virtual IObservable<Unit> _PostAsync(string contract, string operation, WWWForm content, List<KeyValuePair<string, string[]>> contentList, IProgress<float> reportProgress)
        {
            var deferredOperation = Observable.Defer(() =>
            {
                var headers = CopyHeaders();
                if (contentList.Count == 0) content.AddField("_", "_"); // Unity's WWW - POST request with a zero-sized post buffer is not supported!

                OnBeforeRequest(contract, operation, contentList, ref headers);
                var postObservable = (headers == null)
                    ? ObservableWWW.PostWWW(rootEndPoint + "/" + contract + "/" + operation, content, reportProgress)
                    : ObservableWWW.PostWWW(rootEndPoint + "/" + contract + "/" + operation, content, headers, reportProgress);
                 var weboperation = postObservable
                    .Select(_ =>
                    {
                        return Unit.Default;
                    });
                ResultFilter(ref weboperation);

                return weboperation;
            });
            return deferredOperation;
        }

        protected virtual IObservable<T> _PostAsync<T>(string contract, string operation, WWWForm content, List<KeyValuePair<string, string[]>> contentList, IProgress<float> reportProgress)
        {
            var deferredOperation = Observable.Defer(() =>
            {
                var headers = CopyHeaders();
                if (contentList.Count == 0) content.AddField("_", "_"); // add dummy

                OnBeforeRequest(contract, operation, contentList, ref headers);
                var postObservable = (headers == null) 
                    ? ObservableWWW.PostWWW(rootEndPoint + "/" + contract + "/" + operation, content, reportProgress)
                    : ObservableWWW.PostWWW(rootEndPoint + "/" + contract + "/" + operation, content, headers, reportProgress);
                var weboperation = postObservable
                    .Select(x =>
                    {
                        using (var ms = new MemoryStream(x.bytes))
                        {
                            var value = (T)ContentFormatter.Deserialize(typeof(T), ms);
                            return value;
                        }
                    });
                ResultFilter(ref weboperation);

                return weboperation;
            });
            return deferredOperation;
        }

        #region _IPerf

        IObservable<LightNode.Performance.MyClass> _IPerf.Echo(System.String name, System.Int32 x, System.Int32 y, LightNode.Performance.MyEnum e, IProgress<float> reportProgress)
        {
            var list = new List<KeyValuePair<string, string[]>>();
            var form = new WWWForm();
            if (name != null)
            {
                form.AddField("name", name);
                list.Add(new KeyValuePair<string, string[]>("name", new[] { name }));
            }
            form.AddField("x", x.ToString());
            list.Add(new KeyValuePair<string, string[]>("x", new[] { x.ToString() }));
            form.AddField("y", y.ToString());
            list.Add(new KeyValuePair<string, string[]>("y", new[] { y.ToString() }));
            form.AddField("e", ((System.Int32)e).ToString());
            list.Add(new KeyValuePair<string, string[]>("e", new[] { ((System.Int32)e).ToString() }));

            return _PostAsync<LightNode.Performance.MyClass>("Perf", "Echo", form, list, reportProgress);
        }

        IObservable<Unit> _IPerf.Test(System.String a, System.Nullable<System.Int32> x, System.Nullable<LightNode.Performance.MyEnum2> z, IProgress<float> reportProgress)
        {
            var list = new List<KeyValuePair<string, string[]>>();
            var form = new WWWForm();
            if (a != null)
            {
                form.AddField("a", a);
                list.Add(new KeyValuePair<string, string[]>("a", new[] { a }));
            }
            if (x != null)
            {
                form.AddField("x", x.ToString());
                list.Add(new KeyValuePair<string, string[]>("x", new[] { x.ToString() }));
            }
            if (z != null)
            {
                form.AddField("z", ((System.UInt64)z).ToString());
                list.Add(new KeyValuePair<string, string[]>("z", new[] { ((System.UInt64)z).ToString() }));
            }

            return _PostAsync("Perf", "Test", form, list, reportProgress);
        }

        IObservable<Unit> _IPerf.Te(IProgress<float> reportProgress)
        {
            var list = new List<KeyValuePair<string, string[]>>();
            var form = new WWWForm();

            return _PostAsync("Perf", "Te", form, list, reportProgress);
        }

        IObservable<Unit> _IPerf.TestArray(System.String[] array, System.Int32[] array2, LightNode.Performance.MyEnum[] array3, IProgress<float> reportProgress)
        {
            var list = new List<KeyValuePair<string, string[]>>();
            var form = new WWWForm();
            if (array != null)
            {
                var l2 = new List<string>();
                foreach (var ___x in array)
                {
                    form.AddField("array", ___x);
                    l2.Add(___x);
                }
                list.Add(new KeyValuePair<string, string[]>("array", l2.ToArray()));
            }
            if (array2 != null)
            {
                var l2 = new List<string>();
                foreach (var ___x in array2)
                {
                    form.AddField("array2", ___x.ToString());
                    l2.Add(___x.ToString());
                }
                list.Add(new KeyValuePair<string, string[]>("array2", l2.ToArray()));
            }
            if (array3 != null)
            {
                var l2 = new List<string>();
                foreach (var ___x in array3)
                {
                    form.AddField("array3", ((System.Int32)___x).ToString());
                    l2.Add(((System.Int32)___x).ToString());
                }
                list.Add(new KeyValuePair<string, string[]>("array3", l2.ToArray()));
            }

            return _PostAsync("Perf", "TestArray", form, list, reportProgress);
        }

        IObservable<Unit> _IPerf.TeVoid(IProgress<float> reportProgress)
        {
            var list = new List<KeyValuePair<string, string[]>>();
            var form = new WWWForm();

            return _PostAsync("Perf", "TeVoid", form, list, reportProgress);
        }

        IObservable<System.String> _IPerf.Te4(System.String xs, IProgress<float> reportProgress)
        {
            var list = new List<KeyValuePair<string, string[]>>();
            var form = new WWWForm();
            if (xs != null)
            {
                form.AddField("xs", xs);
                list.Add(new KeyValuePair<string, string[]>("xs", new[] { xs }));
            }

            return _PostAsync<System.String>("Perf", "Te4", form, list, reportProgress);
        }

        #endregion

    }

    public interface _IPerf
    {
        IObservable<LightNode.Performance.MyClass> Echo(System.String name, System.Int32 x, System.Int32 y, LightNode.Performance.MyEnum e, IProgress<float> reportProgress = null);
        IObservable<Unit> Test(System.String a = null, System.Nullable<System.Int32> x = null, System.Nullable<LightNode.Performance.MyEnum2> z = null, IProgress<float> reportProgress = null);
        IObservable<Unit> Te(IProgress<float> reportProgress = null);
        IObservable<Unit> TestArray(System.String[] array, System.Int32[] array2, LightNode.Performance.MyEnum[] array3, IProgress<float> reportProgress = null);
        IObservable<Unit> TeVoid(IProgress<float> reportProgress = null);
        IObservable<System.String> Te4(System.String xs, IProgress<float> reportProgress = null);
    }

}

