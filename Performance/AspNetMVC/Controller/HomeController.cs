using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AspNetMVC
{
    public class HomeController : Controller
    {
        public JsonResult Get(string name, int x, int y, MyEnum e)
        {
            return new JsonNetResult(new MyClass { Name = name, Sum = (x + y) * (int)e });
        }
    }

    public class MyClass
    {
        public string Name { get; set; }
        public int Sum { get; set; }
    }

    public enum MyEnum
    {
        A = 2,
        B = 3,
        C = 4
    }

    public class JsonNetResult : JsonResult
    {
        private readonly object _data;

        public JsonNetResult(object data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            _data = data;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var response = context.HttpContext.Response;
            response.ContentType = "application/json";
            var writer = new JsonTextWriter(response.Output);
            var serializer = JsonSerializer.Create(new JsonSerializerSettings());
            serializer.Serialize(writer, _data);
            writer.Flush();
        }
    }
}