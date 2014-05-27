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
#if !(UNITY_METRO || UNITY_WP8)
    using Hash = System.Collections.Hashtable;
    using HashEntry = System.Collections.DictionaryEntry;
#else
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

        partial void AddAdditionalFlow<T>(ref IObservable<T> operation);

        public _IPerf Perf { get { return this; } }

        public LightNodeClient(string rootEndPoint)
        {
            this.rootEndPoint = rootEndPoint.TrimEnd('/');
            this.ContentFormatter = defaultContentFormatter;
            OnAfterInitialized();
        }

        protected virtual IObservable<Unit> _PostAsync(string method, WWWForm content, IProgress<float> reportProgress)
        {
            var deferredOperation = Observable.Defer(() =>
            {
                var postObservable = (defaultHeaders == null)
                    ? ObservableWWW.PostAndGetBytes(rootEndPoint + method, content, reportProgress)
                    : ObservableWWW.PostAndGetBytes(rootEndPoint + method, content, defaultHeaders, reportProgress);
                var operation = postObservable.Select(_ => Unit.Default);
                return operation;
            });
            AddAdditionalFlow(ref deferredOperation);
            return deferredOperation;
        }

        protected virtual IObservable<T> _PostAsync<T>(string method, WWWForm content, IProgress<float> reportProgress)
        {
            var deferredOperation = Observable.Defer(() =>
            {
                var postObservable = (defaultHeaders == null)
                    ? ObservableWWW.PostAndGetBytes(rootEndPoint + method, content, reportProgress)
                    : ObservableWWW.PostAndGetBytes(rootEndPoint + method, content, defaultHeaders, reportProgress);
                var operation = postObservable
                    .Select(x =>
                    {
                        using (var ms = new MemoryStream(x))
                        {
                            var value = (T)ContentFormatter.Deserialize(typeof(T), ms);
                            return value;
                        }
                    });
                return operation;
            });
            AddAdditionalFlow(ref deferredOperation);
            return deferredOperation;
        }

        #region _IPerf

        IObservable<LightNode.Performance.MyClass> _IPerf.Echo(System.String name, System.Int32 x, System.Int32 y, LightNode.Performance.MyEnum e, IProgress<float> reportProgress)
        {
            var form = new WWWForm();
            if (name != null) form.AddField("name", name);
            form.AddField("x", x.ToString());
            form.AddField("y", y.ToString());
            form.AddField("e", ((System.Int32)e).ToString());

            return _PostAsync<LightNode.Performance.MyClass>("/Perf/Echo", form, reportProgress);
        }

        IObservable<Unit> _IPerf.Test(System.String a, System.Nullable<System.Int32> x, System.Nullable<LightNode.Performance.MyEnum2> z, IProgress<float> reportProgress)
        {
            var form = new WWWForm();
            if (a != null) form.AddField("a", a);
            if (x != null) form.AddField("x", x.ToString());
            if (z != null) form.AddField("z", ((System.UInt64)z).ToString());

            return _PostAsync("/Perf/Test", form, reportProgress);
        }

        IObservable<Unit> _IPerf.Te(IProgress<float> reportProgress)
        {
            var form = new WWWForm();

            return _PostAsync("/Perf/Te", form, reportProgress);
        }

        IObservable<Unit> _IPerf.TestArray(System.String[] array, System.Int32[] array2, LightNode.Performance.MyEnum[] array3, IProgress<float> reportProgress)
        {
            var form = new WWWForm();
            if (array != null) foreach (var ___x in array) form.AddField("array", ___x);
            if (array2 != null) foreach (var ___x in array2) form.AddField("array2", ___x.ToString());
            if (array3 != null) foreach (var ___x in array3) form.AddField("array3", ((System.Int32)___x).ToString());

            return _PostAsync("/Perf/TestArray", form, reportProgress);
        }

        IObservable<Unit> _IPerf.TeVoid(IProgress<float> reportProgress)
        {
            var form = new WWWForm();

            return _PostAsync("/Perf/TeVoid", form, reportProgress);
        }

        IObservable<System.String> _IPerf.Te4(System.String xs, IProgress<float> reportProgress)
        {
            var form = new WWWForm();
            if (xs != null) form.AddField("xs", xs);

            return _PostAsync<System.String>("/Perf/Te4", form, reportProgress);
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

