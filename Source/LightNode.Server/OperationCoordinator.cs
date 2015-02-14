using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightNode.Server
{
    public interface IOperationCoordinator
    {
        IReadOnlyList<LightNodeFilterAttribute> GetFilters(LightNodeOptions options, OperationContext context, IReadOnlyList<LightNodeFilterAttribute> originalFilters);
        Task<object> ExecuteOperation(LightNodeOptions options, OperationContext context, Func<LightNodeOptions, OperationContext, Task<object>> originalOperation);
    }

    public class DefaultOperationCoordinator : LightNode.Server.IOperationCoordinator
    {
        public IReadOnlyList<LightNodeFilterAttribute> GetFilters(LightNodeOptions options, OperationContext context, IReadOnlyList<LightNodeFilterAttribute> originalFilters)
        {
            return originalFilters;
        }

        public Task<object> ExecuteOperation(LightNodeOptions options, OperationContext context, Func<LightNodeOptions, OperationContext, Task<object>> originalOperation)
        {
            return originalOperation(options, context);
        }
    }
}
