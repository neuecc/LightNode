using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceStack
{
    [Route("/Get/{Name}/{x}/{y}/{e}")]
    public class Dto
    {
        public string Name { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public MyEnum e { get; set; }
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
    public class HelloService : Service
    {
        public MyClass Any(Dto request)
        {
            return new MyClass { Name = request.Name, Sum = (request.x + request.y) * (int)request.e };
        }
    }
}