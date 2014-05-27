using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.IO;
using LightNode.Core;
using System.Collections.Generic;
using System.Linq;

namespace LightNode.Core
{
    public interface IContentFormatter
    {
        string MediaType { get; }
        string Ext { get; }
        System.Text.Encoding Encoding { get; }
        void Serialize(System.IO.Stream stream, object obj);
        object Deserialize(Type type, System.IO.Stream stream);
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

        public object Deserialize(Type type, System.IO.Stream stream)
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

        public _IPerf Perf { get { return this; } }

        public LightNodeClient(string rootEndPoint)
        {
            this.rootEndPoint = rootEndPoint.TrimEnd('/');
            this.ContentFormatter = defaultContentFormatter;
            OnAfterInitialized();
        }

        Hash MergeHeaders(Hash contentHeaders)
        {
            if (defaultHeaders == null) return contentHeaders;

            foreach (HashEntry item in defaultHeaders)
            {
                contentHeaders.Add(item.Key, item.Value);
            }
            return contentHeaders;
        }

        protected virtual IEnumerator _PostAsync(string method, WWWForm content, Action<Exception> onError, Action<float> reportProgress)
        {
            using (var www = new WWW(rootEndPoint + method, content.data, MergeHeaders(content.headers)))
            {
                while (!www.isDone)
	            {
                    if (reportProgress != null)
                    {
                        try
                        {
                            reportProgress(www.progress);
                        }
                        catch (Exception ex)
                        {
                            if (onError != null) onError(ex);
                            yield break;
                        }
                    }
                    yield return null;
	            }

                if (www.error != null)
                {
                    if (onError != null) onError(new Exception(www.error ?? ""));
                }
            }
        }

        protected virtual IEnumerator _PostAsync<T>(string method, WWWForm content, Action<T> onCompleted, Action<Exception> onError, Action<float> reportProgress)
        {
            using (var www = new WWW(rootEndPoint + method, content.data, MergeHeaders(content.headers)))
            {
                while (!www.isDone)
	            {
                    if (reportProgress != null)
                    {
                        try
                        {
                            reportProgress(www.progress);
                        }
                        catch (Exception ex)
                        {
                            if (onError != null) onError(ex);
                            yield break;
                        }
                    }
                    yield return null;
	            }
                
                if (www.error != null)
                {
                    var ex = new Exception(www.error ?? "");
                    if (onError != null) onError(ex);
                }
                else
                {
                    try
                    {
                        using (var ms = new MemoryStream(www.bytes))
                        {
                            var value = (T)ContentFormatter.Deserialize(typeof(T), ms);
                            onCompleted(value);
                        }
                    }
                    catch(Exception ex)
                    {
                        if (onError != null) onError(ex);
                    }
                }
            }
        }

        #region _IPerf

        IEnumerator _IPerf.EchoAsync(System.String name, System.Int32 x, System.Int32 y, LightNode.Performance.MyEnum e, Action<LightNode.Performance.MyClass> onCompleted, Action<Exception> onError, Action<float> reportProgress)
        {
            var form = new WWWForm();
            if (name != null) form.AddField("name", name);
            form.AddField("x", x.ToString());
            form.AddField("y", y.ToString());
            form.AddField("e", ((System.Int32)e).ToString());

            return _PostAsync<LightNode.Performance.MyClass>("/Perf/Echo", form, onCompleted, onError, reportProgress);
        }

        IEnumerator _IPerf.TestAsync(System.String a, System.Nullable<System.Int32> x, System.Nullable<LightNode.Performance.MyEnum2> z, Action<Exception> onError, Action<float> reportProgress)
        {
            var form = new WWWForm();
            if (a != null) form.AddField("a", a);
            if (x != null) form.AddField("x", x.ToString());
            if (z != null) form.AddField("z", ((System.UInt64)z).ToString());

            return _PostAsync("/Perf/Test", form, onError, reportProgress);
        }

        IEnumerator _IPerf.TeAsync(Action<Exception> onError, Action<float> reportProgress)
        {
            var form = new WWWForm();

            return _PostAsync("/Perf/Te", form, onError, reportProgress);
        }

        IEnumerator _IPerf.TestArrayAsync(System.String[] array, System.Int32[] array2, LightNode.Performance.MyEnum[] array3, Action<Exception> onError, Action<float> reportProgress)
        {
            var form = new WWWForm();
            if (array != null) foreach (var ___x in array) form.AddField("array", ___x);
            if (array2 != null) foreach (var ___x in array2) form.AddField("array2", ___x.ToString());
            if (array3 != null) foreach (var ___x in array3) form.AddField("array3", ((System.Int32)___x).ToString());

            return _PostAsync("/Perf/TestArray", form, onError, reportProgress);
        }

        IEnumerator _IPerf.TeVoidAsync(Action<Exception> onError, Action<float> reportProgress)
        {
            var form = new WWWForm();

            return _PostAsync("/Perf/TeVoid", form, onError, reportProgress);
        }

        IEnumerator _IPerf.Te4Async(System.String xs, Action<System.String> onCompleted, Action<Exception> onError, Action<float> reportProgress)
        {
            var form = new WWWForm();
            if (xs != null) form.AddField("xs", xs);

            return _PostAsync<System.String>("/Perf/Te4", form, onCompleted, onError, reportProgress);
        }

        #endregion

    }

    public interface _IPerf
    {
        IEnumerator EchoAsync(System.String name, System.Int32 x, System.Int32 y, LightNode.Performance.MyEnum e, Action<LightNode.Performance.MyClass> onCompleted, Action<Exception> onError = null, Action<float> reportProgress = null);
        IEnumerator TestAsync(System.String a = null, System.Nullable<System.Int32> x = null, System.Nullable<LightNode.Performance.MyEnum2> z = null, Action<Exception> onError = null, Action<float> reportProgress = null);
        IEnumerator TeAsync(Action<Exception> onError = null, Action<float> reportProgress = null);
        IEnumerator TestArrayAsync(System.String[] array, System.Int32[] array2, LightNode.Performance.MyEnum[] array3, Action<Exception> onError = null, Action<float> reportProgress = null);
        IEnumerator TeVoidAsync(Action<Exception> onError = null, Action<float> reportProgress = null);
        IEnumerator Te4Async(System.String xs, Action<System.String> onCompleted, Action<Exception> onError = null, Action<float> reportProgress = null);
    }

}

