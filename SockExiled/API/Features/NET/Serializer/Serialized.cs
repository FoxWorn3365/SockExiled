using System;
using System.Linq;
using System.Reflection;

namespace SockExiled.API.Features.NET.Serializer
{
    internal class Serialized
    {
        public string TypeName { get; }

        public string TypeFullName { get; }

        public object Value { get; }

        public Serialized(Type type, object value)
        {
            TypeName = FixName(type.Name);
            TypeFullName = FixName(type.FullName ?? TypeName);
            Value = value;
        }

        public Serialized(PropertyInfo type, object value) : this(type.PropertyType, value) { }

        private string FixName(string name) => name.Contains("`") ? name.Split('`')[0] : name;
    }
}
