using Glimpse.Core.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glimpse.LightNode
{
    public class LightNodeFilterResultMessage : MessageBase, ITimelineMessage
    {
        public string OperationName { get; set; }
        public string ContractName { get; set; }
        public string FilterName { get; set; }
        public int Order { get; set; }
        public OperationPhase Phase { get; set; }
        public TimeSpan FromRequestStart { get; set; }

        public TimelineCategoryItem EventCategory { get; set; }
        public string EventName { get; set; }
        public string EventSubText { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan Offset { get; set; }
        public DateTime StartTime { get; set; }
    }

    public enum OperationPhase
    {
        Before, After, Operation, Exception
    }

    public class LightNodeExecuteResultMessage : MessageBase, ITimelineMessage
    {
        public string OperationName { get; set; }
        public string ContractName { get; set; }
        public object Result { get; set; }
        public IDictionary<string, object> Environment { get; set; }
        public TimeSpan FromRequestStart { get; set; }
        public OperationPhase Phase { get; set; }

        public TimelineCategoryItem EventCategory { get; set; }
        public string EventName { get; set; }
        public string EventSubText { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan Offset { get; set; }
        public DateTime StartTime { get; set; }
    }
}
