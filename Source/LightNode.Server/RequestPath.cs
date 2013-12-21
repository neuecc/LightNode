using System;

namespace LightNode.Server
{
    internal class RequestPath : IEquatable<RequestPath>
    {
        readonly string className;
        readonly string methodName;

        public RequestPath(string className, string methodName)
        {
            this.className = className.ToLowerInvariant();
            this.methodName = methodName.ToLowerInvariant();
        }

        public override bool Equals(object obj)
        {
            var _obj = obj as RequestPath;
            if (_obj != null) return Equals(_obj);

            return base.Equals(obj);
        }

        public bool Equals(RequestPath other)
        {
            if (other == null) return false;

            return (this.className == other.className) && (this.methodName == other.methodName);
        }

        public override int GetHashCode()
        {
            return CombineHashCodes(className.GetHashCode(), methodName.GetHashCode());
        }

        public override string ToString()
        {
            return "/" + className + "/" + methodName;
        }

        internal static int CombineHashCodes(int h1, int h2)
        {
            return (h1 << 5) + h1 ^ h2;
        }
    }
}
