using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightNode.Server
{
    public abstract class LightNodeContract
    {
        public IDictionary<string, object> Environment { get; set; }
    }
}