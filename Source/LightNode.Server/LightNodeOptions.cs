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
        public bool ParameterEnumAllowsFieldNameParse { get; set; }
        public IOperationCoordinator OperationCoordinator { get; set; }

        /// <summary>
        /// <pre>Use buffering when content formatter serialize, Default is BufferAndWrite.</pre>
        /// <pre>If you use top level stream buffering, set to DirectWrite for avoid double buffering.</pre>
        /// </summary>
        public StreamWriteOption StreamWriteOption { get; set; }

        public ErrorHandlingPolicy ErrorHandlingPolicy { get; set; }
        public OperationMissingHandlingPolicy OperationMissingHandlingPolicy { get; set; }

        public LightNodeFilterCollection Filters { get; private set; }

        public LightNodeOptions(AcceptVerbs defaultAcceptVerb, IContentFormatter defaultFormatter, params IContentFormatter[] specifiedFormatters)
        {
            DefaultAcceptVerb = defaultAcceptVerb;
            DefaultFormatter = defaultFormatter;
            SpecifiedFormatters = specifiedFormatters;
            UseOtherMiddleware = false;
            ParameterStringImplicitNullAsDefault = false;
            ParameterEnumAllowsFieldNameParse = false;
            StreamWriteOption = Server.StreamWriteOption.BufferAndWrite;
            ErrorHandlingPolicy = Server.ErrorHandlingPolicy.ThrowException;
            OperationMissingHandlingPolicy = Server.OperationMissingHandlingPolicy.ReturnErrorStatusCode;
            Filters = new LightNodeFilterCollection();
            OperationCoordinator = new DefaultOperationCoordinator();
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

    public enum StreamWriteOption
    {
        BufferAndWrite = 0,
        BufferAndAsynchronousWrite = 1,
        DirectWrite = 2
    }

    public enum ErrorHandlingPolicy
    {
        /// <summary>Do Nothing, throw next pipeline.</summary>
        ThrowException = 0,
        /// <summary>Response StatusCode is 500.</summary>
        ReturnInternalServerError = 1,
        /// <summary>Response StatusCode is 500 and ResponseBody includes error details for debugging.</summary>
        ReturnInternalServerErrorIncludeErrorDetails = 2
    }

    public enum OperationMissingHandlingPolicy
    {
        /// <summary>Return StatusCode(4xx).</summary>
        ReturnErrorStatusCode = 0,
        /// <summary>Return StatusCode(4xx) and ResponseBody includes error details for debugging.</summary>
        ReturnErrorStatusCodeIncludeErrorDetails = 1,
        /// <summary>Do Nothing, throw ParametertMissingException.</summary>
        ThrowException = 2,
    }

    public enum OperationMissingKind
    {
        LackOfParameter = 0,
        MissmatchParameterType = 1,
        NegotiateFormatFailed = 2,
        MethodNotAllowed = 3,
        OperationNotFound = 4
    }

    public abstract class OperationMissingException : Exception
    {
        public OperationMissingKind Kind { get; private set; }

        public OperationMissingException(OperationMissingKind kind, string message)
        {
            this.Kind = kind;
        }
    }

    public class ParameterMissingException : OperationMissingException
    {
        public string ParameterName { get; private set; }

        public ParameterMissingException(OperationMissingKind kind, string parameterName)
            : base(kind, kind.ToString() + ", ParameterName:" + parameterName)
        {
            this.ParameterName = parameterName;
        }
    }

    public class NegotiateFormatFailedException : OperationMissingException
    {
        public string Ext { get; private set; }

        public NegotiateFormatFailedException(OperationMissingKind kind, string ext)
            : base(kind, kind.ToString() + ", Ext:" + ext)
        {
            this.Ext = ext;
        }
    }

    public class MethodNotAllowdException : OperationMissingException
    {
        public string Path { get; private set; }
        public string Method { get; private set; }

        public MethodNotAllowdException(OperationMissingKind kind, string path, string method)
            : base(kind, kind.ToString() + ", Path:" + path + " Method:" + method)
        {
            this.Path = path;
            this.Method = method;
        }
    }

    public class OperationNotFoundException : OperationMissingException
    {
        public string Path { get; private set; }

        public OperationNotFoundException(OperationMissingKind kind, string path)
            : base(kind, kind.ToString() + ", Path:" + path)
        {
            this.Path = path;
        }
    }
}