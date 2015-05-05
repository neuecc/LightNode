using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Collections.Generic;
using Microsoft.Owin.Testing;
using Owin;
using LightNode.Formatter;
using System.IO;
using LightNode.Server;

namespace LightNode.Server.Tests
{
    [TestClass]
    public class NegotiateFormatterTest
    {
        [TestMethod]
        public void ExtMatch()
        {
            var testServer = TestServer.Create(app =>
            {
                app.UseLightNode(
                    new LightNodeOptions(AcceptVerbs.Get | AcceptVerbs.Post,
                        new FormatterB(), new FormatterC(), new FormatterA(), new FormatterA2())
                    , typeof(MockEnv).Assembly);
            });

            {
                var client = new HttpClient(testServer.Handler);
                client.BaseAddress = new Uri("http://localhost/");
                client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip;q=1.0, identity; q=0.5,FormatterA;q=0.7,FormatterA2, *;q=0");

                var response = client.GetAsync("/FormatterCheck/Echo.a").Result;
                response.Content.Headers.ContentEncoding.First().Is("FormatterA2");
            }

            {
                var client = new HttpClient(testServer.Handler);
                client.BaseAddress = new Uri("http://localhost/");
                // client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip;q=1.0, identity; q=0.5,FormatterA;q=0.7,FormatterA2, *;q=0");

                var response = client.GetAsync("/FormatterCheck/Echo.a").Result;
                response.Content.Headers.ContentEncoding.First().Is("FormatterA");
            }

            {
                var client = new HttpClient(testServer.Handler);
                client.BaseAddress = new Uri("http://localhost/");
                client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip;q=1.0, identity; q=0.5,FormatterA;q=0.7,FormatterA2, *;q=0");

                var response = client.GetAsync("/FormatterCheck/Echo.c").Result;
                response.Content.Headers.ContentEncoding.First().Is("FormatterC");
            }
        }

        [TestMethod]
        public void Accept()
        {
            var testServer = TestServer.Create(app =>
            {
                app.UseLightNode(
                    new LightNodeOptions(AcceptVerbs.Get | AcceptVerbs.Post,
                        new JavaScriptContentFormatter(), new GZipJavaScriptContentFormatter())
                    , typeof(MockEnv).Assembly);
            });
            {
                // realcase
                var client = new HttpClient(testServer.Handler);
                client.BaseAddress = new Uri("http://localhost/");
                client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip;q=1.0, identity; q=0.5, *;q=0");

                var response = client.GetAsync("/FormatterCheck/Echo").Result;
                response.Content.Headers.ContentEncoding.First().Is("gzip");
            }
            {
                // ungzip
                var client = new HttpClient(testServer.Handler);
                client.BaseAddress = new Uri("http://localhost/");
                client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");

                var response = client.GetAsync("/FormatterCheck/Echo").Result;
                response.Content.Headers.ContentEncoding.Count.Is(0);
            }
            {
                // application/json
                var client = new HttpClient(testServer.Handler);
                client.BaseAddress = new Uri("http://localhost/");
                client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/json,application/xml;q=0.9,image/webp,*/*;q=0.8");
                client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip;q=1.0, identity; q=0.5, *;q=0");

                var response = client.GetAsync("/FormatterCheck/Echo").Result;
                response.Content.Headers.ContentEncoding.First().Is("gzip");
            }
            {
                // application/json,ungzip
                var client = new HttpClient(testServer.Handler);
                client.BaseAddress = new Uri("http://localhost/");
                client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/json,application/xml;q=0.9,image/webp,*/*;q=0.8");

                var response = client.GetAsync("/FormatterCheck/Echo").Result;
                response.Content.Headers.ContentEncoding.Count.Is(0);
            }
            {
                // unity
                var client = new HttpClient(testServer.Handler);
                client.BaseAddress = new Uri("http://localhost/");
                client.DefaultRequestHeaders.Accept.ParseAdd("*/*");
                client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip");

                var response = client.GetAsync("/FormatterCheck/Echo").Result;
                response.Content.Headers.ContentEncoding.First().Is("gzip");
            }
        }
    }

    public class FormatterA : ContentFormatterBase
    {
        public FormatterA()
            : base(mediaType: "", ext: "a", encoding: System.Text.Encoding.UTF8)
        {

        }

        public override string ContentEncoding
        {
            get
            {
                return "FormatterA";
            }
        }

        public override object Deserialize(Type type, Stream stream)
        {
            return null;
        }

        public override void Serialize(Stream stream, object obj)
        {

        }
    }
    public class FormatterA2 : ContentFormatterBase
    {
        public FormatterA2()
                : base(mediaType: "", ext: "a", encoding: System.Text.Encoding.UTF8)
        {

        }

        public override string ContentEncoding
        {
            get
            {
                return "FormatterA2";
            }
        }

        public override object Deserialize(Type type, Stream stream)
        {
            return null;
        }

        public override void Serialize(Stream stream, object obj)
        {

        }
    }

    public class FormatterB : ContentFormatterBase
    {
        public FormatterB()
            : base(mediaType: "", ext: "b", encoding: System.Text.Encoding.UTF8)
        {

        }

        public override string ContentEncoding
        {
            get
            {
                return "FormatterB";
            }
        }

        public override object Deserialize(Type type, Stream stream)
        {
            return null;
        }

        public override void Serialize(Stream stream, object obj)
        {

        }
    }

    public class FormatterC : ContentFormatterBase
    {
        public FormatterC()
            : base(mediaType: "", ext: "c", encoding: System.Text.Encoding.UTF8)
        {

        }

        public override string ContentEncoding
        {
            get
            {
                return "FormatterC";
            }
        }

        public override object Deserialize(Type type, Stream stream)
        {
            return null;
        }

        public override void Serialize(Stream stream, object obj)
        {

        }
    }

    public class FormatterCheck : LightNodeContract
    {
        public string Echo()
        {
            return "void";
        }
    }
}