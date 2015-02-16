using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightNode.Server
{
    public interface IOperationCoordinatorFactory
    {
        IOperationCoordinator Create();
    }

    public interface IOperationCoordinator
    {
        void OnProcessInterrupt(ILightNodeOptions options, IDictionary<string, object> environment, InterruptReason reason, string detail);
        bool OnStartProcessRequest(ILightNodeOptions options, IDictionary<string, object> environment);
        IReadOnlyList<LightNodeFilterAttribute> GetFilters(ILightNodeOptions options, OperationContext context, IReadOnlyList<LightNodeFilterAttribute> originalFilters);
        Task<object> ExecuteOperation(ILightNodeOptions options, OperationContext context, Func<ILightNodeOptions, OperationContext, Task<object>> originalOperation);
    }

    public class DefaultOperationCoordinatorFactory : IOperationCoordinatorFactory
    {
        public IOperationCoordinator Create()
        {
            return new OperationCoordinator();
        }
    }

    public class OperationCoordinator : LightNode.Server.IOperationCoordinator
    {
        public virtual bool OnStartProcessRequest(ILightNodeOptions options, IDictionary<string, object> environment)
        {
            return true;
        }

        public virtual void OnProcessInterrupt(ILightNodeOptions options, IDictionary<string, object> environment, InterruptReason reason, string detail)
        {
        }

        public virtual IReadOnlyList<LightNodeFilterAttribute> GetFilters(ILightNodeOptions options, OperationContext context, IReadOnlyList<LightNodeFilterAttribute> originalFilters)
        {
            return originalFilters;
        }

        public virtual Task<object> ExecuteOperation(ILightNodeOptions options, OperationContext context, Func<ILightNodeOptions, OperationContext, Task<object>> originalOperation)
        {
            return originalOperation(options, context);
        }
    }

    public enum InterruptReason
    {
        MethodNotAllowed,
        OperationNotFound,
        ParameterBindMissing,
        NegotiateFormatFailed,
        ExecuteFailed
    }
}