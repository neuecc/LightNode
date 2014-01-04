using System;

namespace LightNode.Diagnostics
{
    public interface ILogger
    {
        bool IsTraceEnabled { get; }
        bool IsDebugEnabled { get; }
        bool IsInfoEnabled { get; }
        bool IsWarnEnabled { get; }
        bool IsErrorEnabled { get; }
        bool IsFatalEnabled { get; }

        void Trace(string message);

        void Debug(string message);

        void Info(string message);

        void Warn(string message);

        void Error(string message, Exception exception);

        void Fatal(string message, Exception exception);
    }

    public class NullLogger : ILogger
    {
        public NullLogger Instance = new NullLogger();

        private NullLogger()
        {

        }

        public bool IsTraceEnabled
        {
            get { return false; }
        }

        public bool IsDebugEnabled
        {
            get { return false; }
        }

        public bool IsInfoEnabled
        {
            get { return false; }
        }

        public bool IsWarnEnabled
        {
            get { return false; }
        }

        public bool IsErrorEnabled
        {
            get { return false; }
        }

        public bool IsFatalEnabled
        {
            get { return false; }
        }

        public void Trace(string message)
        {
        }

        public void Debug(string message)
        {
        }

        public void Info(string message)
        {
        }

        public void Warn(string message)
        {
        }

        public void Error(string message, Exception exception)
        {
        }

        public void Fatal(string message, Exception exception)
        {
        }
    }
}