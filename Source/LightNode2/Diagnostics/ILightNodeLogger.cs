using System;
using LightNode.Server;

namespace LightNode.Diagnostics
{
    public interface ILightNodeLogger
    {
        void ExecuteFinished(string path, bool interrupted, double elapsed);
        void ExecuteStart(string path);
        void InitializeComplete(double elapsed);
        void MethodNotAllowed(OperationMissingKind kind, string path, string method);
        void NegotiateFormatFailed(OperationMissingKind kind, string ext);
        void OperationNotFound(OperationMissingKind kind, string path);
        void ParameterBindMissing(OperationMissingKind kind, string parameterName);
        void ProcessRequestStart(string path);
        void RegisiterOperation(string className, string methodName, double elapsed);
    }

    internal class NullLightNodeLogger : ILightNodeLogger
    {
        internal static readonly ILightNodeLogger Instance = new NullLightNodeLogger();

        NullLightNodeLogger()
        {

        }

        public void ExecuteFinished(string path, bool interrupted, double elapsed)
        {
        }

        public void ExecuteStart(string path)
        {
        }

        public void InitializeComplete(double elapsed)
        {
        }

        public void MethodNotAllowed(OperationMissingKind kind, string path, string method)
        {
        }

        public void NegotiateFormatFailed(OperationMissingKind kind, string ext)
        {
        }

        public void OperationNotFound(OperationMissingKind kind, string path)
        {
        }

        public void ParameterBindMissing(OperationMissingKind kind, string parameterName)
        {
        }

        public void ProcessRequestStart(string path)
        {
        }

        public void RegisiterOperation(string className, string methodName, double elapsed)
        {
        }
    }
}