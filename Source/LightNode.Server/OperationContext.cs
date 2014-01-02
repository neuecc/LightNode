using LightNode.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightNode.Server
{
    public class OperationContext
    {
        public IDictionary<string, object> Environment { get; private set; }

        public string ContractName { get; private set; }
        public string OperationName { get; private set; }

        public AcceptVerbs Verb { get; private set; }

        // internal use

        internal IContentFormatter ContentFormatter { get; set; }

        internal object[] Parameters { get; set; }

        // Type as typeof(LightNodeFilterAttribute)
        internal ILookup<Type, LightNodeFilterAttribute> Filters { get; set; }

        internal OperationContext(IDictionary<string, object> environment, string contractName, string operationName, AcceptVerbs verb)
        {
            Environment = environment;
            ContractName = contractName;
            OperationName = operationName;
            Verb = verb;
        }

        public bool IsDefined(Type attributeType)
        {
            return Filters.Contains(attributeType);
        }

        public bool IsDefined<T>() where T : LightNodeFilterAttribute
        {
            return Filters.Contains(typeof(T));
        }

        public IEnumerable<Attribute> GetFilterAttributes(Type attributeType)
        {
            return Filters[attributeType];
        }

        public IEnumerable<T> GetFilterAttributes<T>() where T : LightNodeFilterAttribute
        {
            return Filters[typeof(T)].Cast<T>();
        }
    }
}