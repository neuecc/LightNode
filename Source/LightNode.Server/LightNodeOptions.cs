using LightNode.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightNode.Server
{
    public class LightNodeOptions
    {
        public AcceptVerbs DefaultAcceptVerb { get; private set; }
        public bool UseOtherMiddleware { get; private set; }
        public IContentFormatter DefaultFormatter { get; private set; }
        public IContentFormatter[] SpecifiedFormatters { get; private set; }

        public LightNodeOptions(AcceptVerbs defaultAcceptVerb, IContentFormatter defaultFormatter, params IContentFormatter[] specifiedFormatters)
        {
            DefaultAcceptVerb = defaultAcceptVerb;
            DefaultFormatter = defaultFormatter;
            SpecifiedFormatters = specifiedFormatters;
            UseOtherMiddleware = false;
        }
    }

    [Flags]
    public enum AcceptVerbs
    {
        Get = 1,
        Post = 2
    }

    // TODO:Attribute Configuration?

    //public class IgnoreOperationAttribute : Attribute
    //{

    //}

    //// TODO:Option?
    //public class ContractOptionAttribute : Attribute
    //{
    //    public AcceptVerbs AcceptVerb { get; private set; }

    //    public IContentFormatter OutputContentFormatter { get; set; }
    //}
}