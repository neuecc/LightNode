using Glimpse.Core.Extensibility;
using Glimpse.Core.Framework;
using Glimpse.Core.Message;
using LightNode.Server;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Glimpse.LightNode
{
    public class GlimpseProfilingOperationCoordinatorFactory : IOperationCoordinatorFactory
    {
        public IOperationCoordinator Create()
        {
            return new GlimpseProfilingOperationCoordinator();
        }
    }

    public class GlimpseProfilingOperationCoordinator : IOperationCoordinator
    {
        static readonly TimelineCategoryItem FilterCategory = new TimelineCategoryItem("LightNodeFilter", "#dcc000", "#dcc000");
        static readonly TimelineCategoryItem OperationCategory = new TimelineCategoryItem("LightNodeOperation", "#dc9f00", "#dc9f00");

        private IMessageBroker _messageBroker;
        private IExecutionTimer _timer;

#pragma warning disable 618

        internal IMessageBroker MessageBroker
        {
            get { return _messageBroker ?? (_messageBroker = GlimpseConfiguration.GetConfiguredMessageBroker()); }
            set { _messageBroker = value; }
        }

        internal IExecutionTimer Timer
        {
            get { return _timer ?? (_timer = GlimpseConfiguration.GetConfiguredTimerStrategy()()); }
            set { _timer = value; }
        }

#pragma warning restore 618

        public bool OnStartProcessRequest(ILightNodeOptions options, IDictionary<string, object> environment)
        {
            MessageBroker.Publish(new ProcessStartMessage() { Options = options, Environment = environment });
            return true;
        }

        public void OnProcessInterrupt(ILightNodeOptions options, IDictionary<string, object> environment, InterruptReason reason, string detail)
        {
            MessageBroker.Publish(new InterruptMessage() { Reason = reason, Detail = detail });
        }

        public IReadOnlyList<LightNodeFilterAttribute> GetFilters(ILightNodeOptions options, OperationContext context, IReadOnlyList<LightNodeFilterAttribute> originalFilters)
        {
            if (MessageBroker == null || Timer == null) return originalFilters;
            MessageBroker.Publish(context);

            if (originalFilters.Count == 0) return originalFilters;

            var array = new LightNodeFilterAttribute[originalFilters.Count];
            for (int i = 0; i < originalFilters.Count; i++)
            {
                array[i] = new FilterWrapper(MessageBroker, Timer, originalFilters[i]);
            }

            return array;
        }

        class FilterWrapper : LightNodeFilterAttribute
        {
            readonly LightNodeFilterAttribute originalFilter;
            readonly IMessageBroker messageBroker;
            readonly IExecutionTimer timer;

            public FilterWrapper(IMessageBroker messageBroker, IExecutionTimer timer, LightNodeFilterAttribute originalFilter)
            {
                this.originalFilter = originalFilter;
                this.messageBroker = messageBroker;
                this.timer = timer;
            }

            public override async Task Invoke(OperationContext operationContext, Func<Task> next)
            {
                var filterName = originalFilter.GetType().Name;
                var executingStart = timer.Start();
                var executedStart = default(TimeSpan);
                var firstPhase = true;
                try
                {
                    await originalFilter.Invoke(operationContext, async () =>
                    {
                        firstPhase = false;
                        var executingTimerResult = timer.Stop(executingStart);
                        var msg = new LightNodeFilterResultMessage()
                            {
                                ContractName = operationContext.ContractName,
                                OperationName = operationContext.OperationName,
                                FilterName = filterName,
                                Order = originalFilter.Order,
                                Phase = OperationPhase.Before,
                                FromRequestStart = timer.Point().Offset
                            }
                            .AsTimelineMessage(filterName, FilterCategory, "Before")
                            .AsTimedMessage(executingTimerResult);
                        messageBroker.Publish(msg);
                        try
                        {
                            await next();
                        }
                        finally
                        {
                            executedStart = timer.Start();
                        }
                    });
                }
                catch (Exception ex)
                {
                    var msg = new LightNodeFilterResultMessage()
                        {
                            ContractName = operationContext.ContractName,
                            OperationName = operationContext.OperationName,
                            FilterName = filterName,
                            Order = originalFilter.Order,
                            Phase = (ex is ReturnStatusCodeException) ? OperationPhase.ReturnStatusCode : OperationPhase.Exception,
                            FromRequestStart = timer.Point().Offset
                        }
                        .AsTimelineMessage(filterName, FilterCategory, "Exception")
                        .AsTimedMessage((firstPhase) ? timer.Stop(executingStart) : timer.Stop(executedStart));
                    messageBroker.Publish(msg);
                    throw;
                }

                var executedTimerResult = timer.Stop(executedStart);
                var msgFinish = new LightNodeFilterResultMessage()
                    {
                        ContractName = operationContext.ContractName,
                        OperationName = operationContext.OperationName,
                        FilterName = originalFilter.GetType().Name,
                        Order = originalFilter.Order,
                        Phase = OperationPhase.After,
                        FromRequestStart = timer.Point().Offset
                    }
                    .AsTimelineMessage(filterName, FilterCategory, "After")
                    .AsTimedMessage(executedTimerResult);
                messageBroker.Publish(msgFinish);
            }
        }

        public async Task<object> ExecuteOperation(ILightNodeOptions options, OperationContext context, Func<ILightNodeOptions, OperationContext, Task<object>> originalOperation)
        {
            if (MessageBroker == null || Timer == null) return await originalOperation(options, context);
            var timer = Timer;

            object result;
            var start = timer.Start();
            try
            {
                result = await originalOperation(options, context);

                var stop = timer.Stop(start);

                var message = new LightNodeExecuteResultMessage()
                    {
                        ContractName = context.ContractName,
                        OperationName = context.OperationName,
                        Result = result,
                        Environment = context.Environment,
                        UsedContentFormatter = context.ContentFormatter,
                        Options = options,
                        FromRequestStart = timer.Point().Offset,
                        Phase = OperationPhase.Operation
                    }
                    .AsTimelineMessage(context.ToString(), OperationCategory)
                    .AsTimedMessage(stop);
                MessageBroker.Publish(message);
                return result;
            }
            catch (Exception ex)
            {
                var stop = timer.Stop(start);

                var message = new LightNodeExecuteResultMessage()
                    {
                        ContractName = context.ContractName,
                        OperationName = context.OperationName,
                        Result = ex.ToString(),
                        Environment = context.Environment,
                        UsedContentFormatter = context.ContentFormatter,
                        Options = options,
                        FromRequestStart = timer.Point().Offset,
                        Phase = (ex is ReturnStatusCodeException) ? OperationPhase.ReturnStatusCode : OperationPhase.Exception
                    }
                    .AsTimelineMessage(context.ToString(), OperationCategory)
                    .AsTimedMessage(stop);
                MessageBroker.Publish(message);

                throw;
            }
        }
    }
}
