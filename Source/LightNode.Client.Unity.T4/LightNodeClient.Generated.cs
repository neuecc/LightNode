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

        public _IPerf Perf { get { return this; } }

        public LightNodeClient(string rootEndPoint)
        {
            this.rootEndPoint = rootEndPoint.TrimEnd('/');
            this.ContentFormatter = defaultContentFormatter;
        }

        protected virtual IEnumerator _PostAsync(string method, WWWForm content)
        {
            using (var www = new WWW(rootEndPoint + method, content))
            {
                // TODO:Progress?
                yield return www;
                if (www.isDone && www.error != null)
                {
                    // as void
                }
                else
                {
                    // TODO:other exception?
                    throw new Exception(www.error ?? "");
                }
            }
        }

        protected virtual IEnumerator _PostAsync<T>(string method, WWWForm content, Action<T> onCompleted)
        {
            using (var www = new WWW(rootEndPoint + method, content))
            {
                // TODO:Progress?
                yield return www;
                if (www.isDone && www.error != null)
                {
                    // bytes? text?
                    using (var ms = new MemoryStream(www.bytes))
                    {
                        var value = (T)ContentFormatter.Deserialize(typeof(T), ms);
                        onCompleted(value);
                    }
                }
                else
                {
                    // TODO:other exception?
                    throw new Exception(www.error ?? "");
                }
            }
        }

        #region _IPerf

        IEnumerator _IPerf.EchoAsync(System.String name, System.Int32 x, System.Int32 y, LightNode.Performance.MyEnum e, Action<LightNode.Performance.MyClass> onCompleted)
        {
            var form = new WWWForm();
            if (name != null) form.AddField("name", name.ToString());
            form.AddField("x", x.ToString());
            form.AddField("y", y.ToString());
            form.AddField("e", e.ToString());

            return _PostAsync<LightNode.Performance.MyClass>("/Perf/Echo", form, onCompleted);
        }

        IEnumerator _IPerf.TestAsync(System.String a, System.Nullable<System.Int32> x)
        {
            var form = new WWWForm();
            if (a != null) form.AddField("a", a.ToString());
            if (x != null) form.AddField("x", x.ToString());

            return _PostAsync("/Perf/Test", form);
        }

        IEnumerator _IPerf.TeAsync()
        {
            var form = new WWWForm();

            return _PostAsync("/Perf/Te", form);
        }

        IEnumerator _IPerf.TestArrayAsync(System.String[] array, System.Int32[] array2)
        {
            var form = new WWWForm();
            if (array != null) foreach (var ___x in array) form.AddField("array", ___x.ToString());
            if (array2 != null) foreach (var ___x in array2) form.AddField("array2", ___x.ToString());

            return _PostAsync("/Perf/TestArray", form);
        }

        #endregion

    }

    public interface _IPerf
    {
        IEnumerator EchoAsync(System.String name, System.Int32 x, System.Int32 y, LightNode.Performance.MyEnum e, Action<LightNode.Performance.MyClass> onCompleted);
        IEnumerator TestAsync(System.String a = null, System.Nullable<System.Int32> x = null);
        IEnumerator TeAsync();
        IEnumerator TestArrayAsync(System.String[] array, System.Int32[] array2);
    }

}