using System.Diagnostics.Tracing;
using LightNode.Server;

namespace LightNode.Diagnostics
{
    // EventSource is "Public", you can enable SLAB's ObservableEventListener
    [EventSource(Name = "LightNode")]
    public sealed class LightNodeEventSource : EventSource, ILightNodeLogger
    {
        public static readonly LightNodeEventSource Log = new LightNodeEventSource();

        private LightNodeEventSource()
        {

        }
        public class Keywords
        {
            public const EventKeywords Regisiter = (EventKeywords)1;
            public const EventKeywords OperationMissing = (EventKeywords)2;
            public const EventKeywords ProcessRequest = (EventKeywords)4;
        }

        [Event(1, Level = EventLevel.Verbose, Keywords = Keywords.Regisiter)]
        public void RegisiterOperation(string className, string methodName, double elapsed)
        {
            WriteEvent(1, className ?? "", methodName ?? "", elapsed);
        }

        [Event(2, Level = EventLevel.Verbose, Keywords = Keywords.Regisiter)]
        public void InitializeComplete(double elapsed)
        {
            WriteEvent(2, elapsed);
        }

        [Event(3, Level = EventLevel.Informational, Keywords = Keywords.OperationMissing)]
        public void ParameterBindMissing(OperationMissingKind kind, string parameterName)
        {
            WriteEvent(3, kind, parameterName ?? "");
        }

        [Event(4, Level = EventLevel.Informational, Keywords = Keywords.OperationMissing)]
        public void NegotiateFormatFailed(OperationMissingKind kind, string ext)
        {
            WriteEvent(4, kind, ext ?? "");
        }

        [Event(5, Level = EventLevel.Informational, Keywords = Keywords.OperationMissing)]
        public void MethodNotAllowed(OperationMissingKind kind, string path, string method)
        {
            WriteEvent(5, kind, path ?? "", method ?? "");
        }

        [Event(6, Level = EventLevel.Informational, Keywords = Keywords.OperationMissing)]
        public void OperationNotFound(OperationMissingKind kind, string path)
        {
            WriteEvent(6, kind, path ?? "");
        }

        [Event(7, Level = EventLevel.Verbose, Keywords = Keywords.ProcessRequest)]
        public void ProcessRequestStart(string path)
        {
            WriteEvent(7, path ?? "");
        }

        [Event(8, Level = EventLevel.Verbose, Keywords = Keywords.ProcessRequest)]
        public void ExecuteStart(string path)
        {
            WriteEvent(8, path ?? "");
        }

        [Event(9, Level = EventLevel.Verbose, Keywords = Keywords.ProcessRequest)]
        public void ExecuteFinished(string path, bool interrupted, double elapsed)
        {
            WriteEvent(9, path ?? "", interrupted, elapsed);
        }
    }
}