using System;
using System.Reflection;

// Thanks https://stackoverflow.com/questions/863881/how-do-i-tell-if-a-type-is-a-simple-type-i-e-holds-a-single-value :P
namespace SockExiled.Extension
{
    internal static class TypeExtension
    {
        public static bool IsTypePrimitive(this Type type)
        {
            TypeInfo typeInfo = type.GetTypeInfo();
            if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // nullable type, check if the nested type is simple.
                return typeInfo.GetGenericArguments()[0].IsTypePrimitive();
            }

            return typeInfo.IsPrimitive
              || typeInfo.IsEnum
              || type.Equals(typeof(string))
              || type.Equals(typeof(decimal));
        }
    }
}
