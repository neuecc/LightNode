using LightNode.Core;
using System;
using System.Collections.Generic;

namespace LightNode.Server
{
    public class LightNodeOptions
    {
        public AcceptVerbs DefaultAcceptVerb { get; private set; }
        public IContentFormatter DefaultFormatter { get; private set; }
        public IContentFormatter[] SpecifiedFormatters { get; private set; }

        public bool UseOtherMiddleware { get; set; }
        public bool ParameterStringImplicitNullAsDefault { get; set; }

        public LightNodeFilterCollection Filters { get; private set; }

        internal ParameterBinder parametertBinder = ParameterBinder.Default;

        public LightNodeOptions(AcceptVerbs defaultAcceptVerb, IContentFormatter defaultFormatter, params IContentFormatter[] specifiedFormatters)
        {
            DefaultAcceptVerb = defaultAcceptVerb;
            DefaultFormatter = defaultFormatter;
            SpecifiedFormatters = specifiedFormatters;
            UseOtherMiddleware = false;
            ParameterStringImplicitNullAsDefault = false;
            Filters = new LightNodeFilterCollection();
        }

        public LightNodeOptions ConfigureWith(Action<LightNodeOptions> @this)
        {
            @this(this);
            return this;
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