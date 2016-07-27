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

        public void Add(Func<OperationContext, Func<Task>, Task> invoke, int order = int.MaxValue)
        {
            list.Add(new AnonymousLightNodeFilter(invoke, order));
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

    internal class AnonymousLightNodeFilter : LightNodeFilterAttribute
    {
        readonly Func<OperationContext, Func<Task>, Task> invoke;

        public AnonymousLightNodeFilter(Func<OperationContext, Func<Task>, Task> invoke, int order)
        {
            this.invoke = invoke;
            this.Order = order;
        }

        public override Task Invoke(OperationContext operationContext, Func<Task> next)
        {
            return this.invoke(operationContext, next);
        }
    }
}