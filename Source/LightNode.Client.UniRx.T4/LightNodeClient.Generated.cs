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
    using Hash = System.Collections.Generic.Dictionary<string, string>;
    using HashEntry = System.Collections.Generic.KeyValuePair<string, string>;

    public abstract partial class LightNodeClient : _IPerf
    {
        static IContentFormatter defaultContentFormatter = new LightNode.Formatter.JsonNetContentFormatter();
        static IContentFormatter plainJsonContentFormatter = new LightNode.Formatter.JsonNetContentFormatter();
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
                if (contentList.Count == 0) content.AddField("_", "_"); // add dummy

                OnBeforeRequest(contract, operation, contentList, ref headers);
                var postObservable = (headers == null)
                    ? ObservableWWW.PostWWW(rootEndPoint + "/" + contract + "/" + operation, content, reportProgress)
                    : ObservableWWW.PostWWW(rootEndPoint + "/" + contract + "/" + operation, content, headers, reportProgress);
                 var weboperation = postObservable
                    .Select(_ =>
                    {
                        return Unit.Default;
                    });

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
                        string header;
                        if (x.responseHeaders.TryGetValue("Content-Encoding", out header) && header == "gzip")
                        {
                            using (var ms = new MemoryStream(x.bytes))
                            {
                                var value = (T)ContentFormatter.Deserialize(typeof(T), ms);
                                return value;
                            }
                        }
                        else
                        {
                            using (var ms = new MemoryStream(x.bytes))
                            {
                                var value = (T)plainJsonContentFormatter.Deserialize(typeof(T), ms);
                                return value;
                            }
                        }
                    });

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

        IObservable<Unit> _IPerf.Test(System.String a, System.Nullable<System.Int32> x, IProgress<float> reportProgress)
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

            return _PostAsync("Perf", "Test", form, list, reportProgress);
        }

        IObservable<Unit> _IPerf.Te(IProgress<float> reportProgress)
        {
            var list = new List<KeyValuePair<string, string[]>>();
            var form = new WWWForm();

            return _PostAsync("Perf", "Te", form, list, reportProgress);
        }

        IObservable<Unit> _IPerf.TestArray(System.String[] array, System.Int32[] array2, IProgress<float> reportProgress)
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

            return _PostAsync("Perf", "TestArray", form, list, reportProgress);
        }

        #endregion

    }

    public interface _IPerf
    {
        IObservable<LightNode.Performance.MyClass> Echo(System.String name, System.Int32 x, System.Int32 y, LightNode.Performance.MyEnum e, IProgress<float> reportProgress = null);
        IObservable<Unit> Test(System.String a = null, System.Nullable<System.Int32> x = null, IProgress<float> reportProgress = null);
        IObservable<Unit> Te(IProgress<float> reportProgress = null);
        IObservable<Unit> TestArray(System.String[] array, System.Int32[] array2, IProgress<float> reportProgress = null);
    }

}

