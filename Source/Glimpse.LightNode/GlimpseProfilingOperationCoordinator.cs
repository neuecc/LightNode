using Glimpse.Core.Extensibility;
using Glimpse.Core.Framework;
using Glimpse.Core.Message;
using LightNode.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glimpse.LightNode
{
    public class GlimpseProfilingOperationCoordinator : IOperationCoordinator
    {
        static readonly TimelineCategoryItem FilterCategory = new TimelineCategoryItem("LightNodeFilter", "#00a0dc", "#01adee");
        static readonly TimelineCategoryItem OperationCategory = new TimelineCategoryItem("LightNodeOperation", "#00c0dc", "#01adee");

        private IMessageBroker _messageBroker;
        private IExecutionTimer _timerStrategy;

#pragma warning disable 618

        internal IMessageBroker MessageBroker
        {
            get { return _messageBroker ?? (_messageBroker = GlimpseConfiguration.GetConfiguredMessageBroker()); }
            set { _messageBroker = value; }
        }

        internal IExecutionTimer TimerStrategy
        {
            get { return _timerStrategy ?? (_timerStrategy = GlimpseConfiguration.GetConfiguredTimerStrategy()()); }
            set { _timerStrategy = value; }
        }

#pragma warning restore 618


        public IReadOnlyList<LightNodeFilterAttribute> GetFilters(LightNodeOptions options, OperationContext context, IReadOnlyList<LightNodeFilterAttribute> originalFilters)
        {
            if (MessageBroker == null || TimerStrategy == null) return originalFilters;
            MessageBroker.Publish(context);

            if (originalFilters.Count == 0) return originalFilters;

            var array = new LightNodeFilterAttribute[originalFilters.Count];
            for (int i = 0; i < originalFilters.Count; i++)
            {
                array[i] = new FilterWrapper(MessageBroker, TimerStrategy, originalFilters[i]);
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
                catch
                {
                    var msg = new LightNodeFilterResultMessage()
                        {
                            ContractName = operationContext.ContractName,
                            OperationName = operationContext.OperationName,
                            FilterName = filterName,
                            Order = originalFilter.Order,
                            Phase = OperationPhase.Exception,
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

        public async Task<object> ExecuteOperation(LightNodeOptions options, OperationContext context, Func<LightNodeOptions, OperationContext, Task<object>> originalOperation)
        {
            if (MessageBroker == null || TimerStrategy == null) return await originalOperation(options, context);

            object result;
            var start = TimerStrategy.Start();
            try
            {
                result = await originalOperation(options, context);

                var stop = TimerStrategy.Stop(start);

                var message = new LightNodeExecuteResultMessage()
                    {
                        ContractName = context.ContractName,
                        OperationName = context.OperationName,
                        Result = result,
                        Environment = context.Environment,
                        FromRequestStart = TimerStrategy.Point().Offset,
                        Phase = OperationPhase.Operation
                    }
                    .AsTimelineMessage(context.ToString(), OperationCategory)
                    .AsTimedMessage(stop);
                MessageBroker.Publish(message);
                return result;
            }
            catch (Exception ex)
            {
                var stop = TimerStrategy.Stop(start);

                var message = new LightNodeExecuteResultMessage()
                    {
                        ContractName = context.ContractName,
                        OperationName = context.OperationName,
                        Result = ex.ToString(),
                        Environment = context.Environment,
                        FromRequestStart = TimerStrategy.Point().Offset,
                        Phase = OperationPhase.Exception
                    }
                    .AsTimelineMessage(context.ToString(), OperationCategory)
                    .AsTimedMessage(stop);
                MessageBroker.Publish(message);

                throw;
            }
        }
    }
}
