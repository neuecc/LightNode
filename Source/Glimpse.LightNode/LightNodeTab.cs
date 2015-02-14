using Glimpse.Core.Extensibility;
using LightNode.Server.Utility;
using Glimpse.Core.Framework;
using Glimpse.Core.Message;
using LightNode.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glimpse.Core.Extensions;

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
            public int? StatusCode { get; set; }
            public string ReasonPhrase { get; set; }
            public IDictionary<string, string[]> ResponseHeaders { get; set; }
        }

        public override string Name
        {
            get { return "LightNode"; }
        }

        public bool KeysHeadings { get { return true; } }

        public void Setup(ITabSetupContext context)
        {
            context.PersistMessages<LightNodeFilterResultMessage>();
            context.PersistMessages<LightNodeExecuteResultMessage>();
            context.PersistMessages<OperationContext>();
        }

        public override object GetData(ITabContext context)
        {
            var operationContext = context.GetMessages<OperationContext>().FirstOrDefault();
            var filterResult = context.GetMessages<LightNodeFilterResultMessage>();
            var executeResult = context.GetMessages<LightNodeExecuteResultMessage>().FirstOrDefault();

            if (operationContext == null) return null;
            var environment = operationContext.Environment;

            var response = new Response
            {
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
                execution.Add(new Execution { Name = operationContext.ToString(), Elapsed = executeResult.Duration.TotalMilliseconds + " ms", FromRequestStart = "T+ " + executeResult.FromRequestStart.TotalMilliseconds + " ms" });
            }
            foreach (var item in filterResult.Where(x => x.Phase == OperationPhase.After))
            {
                execution.Add(new Execution { Order = item.Order, Name = item.FilterName, Phase = item.Phase.ToString(), Elapsed = item.Duration.TotalMilliseconds + " ms", FromRequestStart = "T+ " + item.FromRequestStart.TotalMilliseconds + " ms" });
            }

            return new LightNodeTabResult
            {
                Path = operationContext.ToString(),
                Parameters = operationContext.ParameterNames.Zip(operationContext.Parameters, (x, y) => new { x, y }).ToDictionary(x => x.x, x => x.y),
                Result = (executeResult != null) ? executeResult.Result : null,
                Execution = execution,
                Response = response
            };
        }
    }
}
