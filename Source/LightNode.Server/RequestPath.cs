using System;

namespace LightNode.Server
{
    internal class RequestPath : IEquatable<RequestPath>
    {
        readonly string className;
        readonly string methodName;
        readonly static StringComparer comparer = StringComparer.OrdinalIgnoreCase;

        public RequestPath(string className, string methodName)
        {
            this.className = className;
            this.methodName = methodName;
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

            return comparer.Equals(this.className, other.className) && comparer.Equals(this.methodName, other.methodName);
        }

        public override int GetHashCode()
        {
            return CombineHashCodes(comparer.GetHashCode(className), comparer.GetHashCode(methodName));
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
