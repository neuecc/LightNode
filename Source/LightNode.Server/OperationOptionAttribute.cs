using LightNode.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightNode.Server
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class OperationOptionAttribute : Attribute
    {
        internal AcceptVerbs? AcceptVerbs { get; private set; }
        internal IContentFormatter ContentFormatter { get; private set; }

        /// <summary>Append operation specific option.</summary>
        /// <param name="acceptVerbs">Overwrite AcceptVerb.</param>
        public OperationOptionAttribute(AcceptVerbs acceptVerbs)
        {
            this.AcceptVerbs = acceptVerbs;
        }

        /// <summary>Append operation specific option.</summary>
        /// <param name="contentFormatterFactory">Ignore default formatter and specifiedFormatters. Force use this contentFormatter.</param>
        public OperationOptionAttribute(Type contentFormatterFactory)
        {
            try
            {
                this.ContentFormatter = (Activator.CreateInstance(contentFormatterFactory) as IContentFormatterFactory).CreateFormatter();
            }
            catch (Exception ex)
            {
                throw new ArgumentException("contentFormatterFactory must inherits IContentFormatterFactory and has parameterless constructor", ex);
            }
        }

        /// <summary>Append operation specific option.</summary>
        /// <param name="acceptVerbs">Overwrite AcceptVerb.</param>
        /// <param name="contentFormatterFactory">Ignore default formatter and specifiedFormatters. Force use this contentFormatter.</param>
        public OperationOptionAttribute(AcceptVerbs acceptVerbs, Type contentFormatterFactory)
        {
            this.AcceptVerbs = acceptVerbs;
            try
            {
                this.ContentFormatter = (Activator.CreateInstance(contentFormatterFactory) as IContentFormatterFactory).CreateFormatter();
            }
            catch (Exception ex)
            {
                throw new ArgumentException("contentFormatterFactory must inherits IContentFormatterFactory and has parameterless constructor", ex);
            }
        }
    }
}