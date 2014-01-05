using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightNode.Performance
{
    public class Startup
    {
        public void Configuration(IAppBuilder appa)
        {
            appa.UseLightNode(new LightNode.Server.LightNodeOptions(Server.AcceptVerbs.Get | Server.AcceptVerbs.Post,
                new LightNode.Formatter.JsonNetContentFormatter()));
        }
    }

    public class Perf : LightNode.Server.LightNodeContract
    {
        public MyClass Echo(string name, int x, int y, MyEnum e)
        {
            return new MyClass { Name = name, Sum = (x + y) * (int)e };
        }

        public void Test(string a = null, int? x = null)
        {
        }

        public System.Threading.Tasks.Task Te()
        {
            return System.Threading.Tasks.Task.FromResult(1);
        }

        public void TestArray(string[] array, int[] array2)
        {
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
}