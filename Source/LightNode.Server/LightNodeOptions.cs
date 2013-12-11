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
        public IMediaTypeFormatter DefaultFormatter { get; private set; }
        public IMediaTypeFormatter[] SpecifiedFormatters { get; private set; }

        public LightNodeOptions(AcceptVerbs defaultAcceptVerb, IMediaTypeFormatter defaultFormatter, params IMediaTypeFormatter[] specifiedFormatters)
        {
            DefaultAcceptVerb = defaultAcceptVerb;
            DefaultFormatter = defaultFormatter;
            SpecifiedFormatters = specifiedFormatters;
        }
    }
}