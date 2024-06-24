using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Reflection;

namespace SockExiled.API.Utilities
{
    internal class JsonParser
    {
        public static string Encode(object obj)
        {
            Dictionary<string, string> Data = new();

            SnakeCaseNamingStrategy NamingStrategy = new();
            foreach (PropertyInfo Property in obj.GetType().GetProperties())
            {
                Data.Add(NamingStrategy.GetPropertyName(Property.Name, false), (Property.GetValue(obj, null) ?? "error").ToString());
            }
            return JsonConvert.SerializeObject(Data);
        }
    }
}
