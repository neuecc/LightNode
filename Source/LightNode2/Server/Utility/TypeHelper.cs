using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LightNode.Server
{
    public static class TypeHelper
    {
        public static bool IsEnum(this Type type)
        {
            return type.GetTypeInfo().IsEnum;
        }

        public static bool IsAbstract(this Type type)
        {
            return type.GetTypeInfo().IsAbstract;
        }

        public static Type[] GetGenericArguments(this Type type)
        {
            return type.GetTypeInfo().GetGenericArguments();
        }

        public static ConstructorInfo[] GetConstructors(this Type type)
        {
            return type.GetTypeInfo().GetConstructors();
        }

        public static MethodInfo[] GetMethods(this Type type, BindingFlags flags)
        {
            return type.GetTypeInfo().GetMethods(flags);
        }

        public static T GetCustomAttribute<T>(this Type type, bool inherit)
            where T : Attribute
        {
            return type.GetTypeInfo().GetCustomAttribute<T>(inherit);
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this Type type, bool inherit)
            where T : Attribute
        {
            return type.GetTypeInfo().GetCustomAttributes<T>(inherit);
        }

        public static bool IsAssignableFrom(this Type type, Type c)
        {
            return type.GetTypeInfo().IsAssignableFrom(c);
        }
    }
}
