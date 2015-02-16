using Glimpse.Core.Extensibility;
using Glimpse.Core.Extensions;
using LightNode.Server;
using System.Collections.Generic;
using System.Linq;

namespace Glimpse.LightNode
{
    public class LightNodeTab : TabBase, ITabSetup, ILayoutControl
    {
        class LightNodeTabResult
        {
            public string Path { get; set; }
            public Dictionary<string, object> Parameters { get; set; }
            public object Result { get; set; }
            public List<Execution> Execution { get; set; }
            public Response Response { get; set; }
            public Options Options { get; set; }
        }

        class Execution
        {
            public int? Order { get; set; }
            public string Phase { get; set; }
            public string Name { get; set; }
            public string FromRequestStart { get; set; }
            public string Elapsed { get; set; }
        }

        class Response
        {
            public string OutputContentFormatter { get; set; }
            public int? StatusCode { get; set; }
            public string ReasonPhrase { get; set; }
            public IDictionary<string, string[]> ResponseHeaders { get; set; }
        }

        class Options
        {
            public string DefaultAcceptVerb { get; set; }
            public string DefaultFormatter { get; set; }
            public string SpecifiedFormatters { get; set; }
            public string GlobalFilters { get; set; }
            public bool UseOtherMiddleware { get; set; }
            public bool ParameterStringImplicitNullAsDefault { get; set; }
            public bool ParameterEnumAllowsFieldNameParse { get; set; }
            public string OperationCoordinatorFactory { get; set; }
            public string StreamWriteOption { get; set; }
            public string ErrorHandlingPolicy { get; set; }
            public string OperationMissingHandlingPolicy { get; set; }
        }

        public override string Name
        {
            get { return "LightNode"; }
        }

        public bool KeysHeadings { get { return true; } }

        public void Setup(ITabSetupContext context)
        {
            context.PersistMessages<ProcessStartMessage>();
            context.PersistMessages<InterruptMessage>();
            context.PersistMessages<OperationContext>();
            context.PersistMessages<LightNodeFilterResultMessage>();
            context.PersistMessages<LightNodeExecuteResultMessage>();
        }

        public override object GetData(ITabContext context)
        {
            var processStart = context.GetMessages<ProcessStartMessage>().FirstOrDefault();
            var interrupt = context.GetMessages<InterruptMessage>().FirstOrDefault();
            var operationContext = context.GetMessages<OperationContext>().FirstOrDefault();
            var filterResult = context.GetMessages<LightNodeFilterResultMessage>();
            var executeResult = context.GetMessages<LightNodeExecuteResultMessage>().FirstOrDefault();

            if (processStart == null) return null;
            var environment = processStart.Environment;

            var response = new Response
            {
                OutputContentFormatter = (executeResult != null) ? executeResult.UsedContentFormatter.GetType().Name : null,
                StatusCode = environment.AsResponseStatusCode(),
                ReasonPhrase = environment.AsResponseReasonPhrase(),
                ResponseHeaders = environment.AsResponseHeaders()
            };

            var execution = new List<Execution>();
            foreach (var item in filterResult.Where(x => x.Phase == OperationPhase.Before))
            {
                execution.Add(new Execution { Order = item.Order, Name = item.FilterName, Phase = item.Phase.ToString(), Elapsed = item.Duration.TotalMilliseconds + " ms", FromRequestStart = "T+ " + item.FromRequestStart.TotalMilliseconds + " ms" });
            };
            if (executeResult != null)
            {
                execution.Add(new Execution { Name = operationContext.ToString(), Elapsed = executeResult.Duration.TotalMilliseconds + " ms", FromRequestStart = "T+ " + executeResult.FromRequestStart.TotalMilliseconds + " ms", Phase = executeResult.Phase.ToString() });
            }
            foreach (var item in filterResult.Where(x => x.Phase == OperationPhase.After || x.Phase == OperationPhase.Exception || x.Phase == OperationPhase.ReturnStatusCode))
            {
                execution.Add(new Execution { Order = item.Order, Name = item.FilterName, Phase = item.Phase.ToString(), Elapsed = item.Duration.TotalMilliseconds + " ms", FromRequestStart = "T+ " + item.FromRequestStart.TotalMilliseconds + " ms" });
            }

            var option = (executeResult != null) ? executeResult.Options : processStart.Options;
            var globalFilter = string.Join(", ", option.Filters.Select(x => x.GetType().Name));
            var specifiedFormatters = string.Join(", ", option.SpecifiedFormatters.Select(x => x.GetType().Name));
            return new LightNodeTabResult
            {
                Path = (operationContext != null) ? operationContext.ToString() : environment.AsRequestPath(),
                Parameters = (operationContext != null) ? operationContext.ParameterNames.Zip(operationContext.Parameters, (x, y) => new { x, y }).ToDictionary(x => x.x, x => x.y) : null,
                Result = (executeResult != null) ? executeResult.Result
                       : (interrupt != null) ? interrupt.Reason + System.Environment.NewLine + interrupt.Detail
                       : null,
                Execution = execution,
                Response = response,
                Options = new Options
                {
                    DefaultAcceptVerb = option.DefaultAcceptVerb.ToString(),
                    ErrorHandlingPolicy = option.ErrorHandlingPolicy.ToString(),
                    DefaultFormatter = option.DefaultFormatter.GetType().Name,
                    GlobalFilters = string.IsNullOrWhiteSpace(globalFilter) ? null : globalFilter,
                    SpecifiedFormatters = string.IsNullOrWhiteSpace(specifiedFormatters) ? null : specifiedFormatters,
                    OperationCoordinatorFactory = option.OperationCoordinatorFactory.GetType().Name,
                    OperationMissingHandlingPolicy = option.OperationMissingHandlingPolicy.ToString(),
                    ParameterEnumAllowsFieldNameParse = option.ParameterEnumAllowsFieldNameParse,
                    ParameterStringImplicitNullAsDefault = option.ParameterStringImplicitNullAsDefault,
                    StreamWriteOption = option.StreamWriteOption.ToString(),
                    UseOtherMiddleware = option.UseOtherMiddleware
                }
            };
        }
    }
}