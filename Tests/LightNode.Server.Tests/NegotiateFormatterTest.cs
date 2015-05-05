using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Collections.Generic;

namespace LightNode.Server.Tests
{
    [TestClass]
    public class NegotiateFormatterTest
    {
        [TestMethod]
        public void MediaType()
        {
            // MockEnv.CreateRequest("/Hello/Say.txt").GetString();

            var client = MockEnv.CreateHttpClient();
            client.BaseAddress = new Uri("http://localhost/");
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var response = client.PostAsync("/ComplexContract/CreatePerson", new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                {"Age", "14"}, {"Name", "hogehoge"}
            })).Result;
            response.Content.ReadAsStringAsync().Result.Is(@"{""Age"":14,""Name"":""hogehoge""}");
        }
    }

    public class ComplexContract : LightNodeContract
    {
        public Person CreatePerson(int age, string name)
        {
            return new Person { Age = age, Name = name };
        }
    }

    public class Person
    {
        public int Age { get; set; }
        public string Name { get; set; }
    }
}