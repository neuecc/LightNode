using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightNode.Server
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public abstract class LightNodeFilterAttribute : Attribute
    {
        int order = int.MaxValue;
        public int Order
        {
            get { return order; }
            set { order = value; }
        }

        public abstract Task Invoke(OperationContext operationContext, Func<Task> next);
    }

    public class LightNodeFilterCollection : IEnumerable<LightNodeFilterAttribute>
    {
        List<LightNodeFilterAttribute> list = new List<LightNodeFilterAttribute>();

        public void Add(LightNodeFilterAttribute filter)
        {
            list.Add(filter);
        }

        public IEnumerator<LightNodeFilterAttribute> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}