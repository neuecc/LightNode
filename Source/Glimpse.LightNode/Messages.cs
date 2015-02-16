using Glimpse.Core.Message;
using LightNode.Core;
using LightNode.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glimpse.LightNode
{
    internal enum OperationPhase
    {
        Before, After, Operation, Exception, ReturnStatusCode
    }

    internal abstract class TimelineMessageBase : MessageBase, ITimelineMessage
    {
        public TimelineCategoryItem EventCategory { get; set; }
        public string EventName { get; set; }
        public string EventSubText { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan Offset { get; set; }
        public DateTime StartTime { get; set; }
    }

    internal class ProcessStartMessage
    {
        public ILightNodeOptions Options { get; set; }
        public IDictionary<string, object> Environment { get; set; }
    }

    internal class InterruptMessage
    {
        public InterruptReason Reason { get; set; }
        public string Detail { get; set; }
    }

    internal class LightNodeFilterResultMessage : TimelineMessageBase
    {
        public string OperationName { get; set; }
        public string ContractName { get; set; }
        public string FilterName { get; set; }
        public int Order { get; set; }
        public OperationPhase Phase { get; set; }
        public TimeSpan FromRequestStart { get; set; }
    }

    internal class LightNodeExecuteResultMessage : TimelineMessageBase
    {
        public string OperationName { get; set; }
        public string ContractName { get; set; }
        public object Result { get; set; }
        public IDictionary<string, object> Environment { get; set; }
        public ILightNodeOptions Options { get; set; }
        public IContentFormatter UsedContentFormatter { get; set; }
        public TimeSpan FromRequestStart { get; set; }
        public OperationPhase Phase { get; set; }
    }
}
