using Exiled.API.Features;
using Newtonsoft.Json;
using System;
using System.Reflection;

namespace SockExiled.Extension
{
    internal static class ObjectExtension
    {
        public static bool CanBeSerialized(this object obj)
        {
            if (obj is null)
                return false;

            try
            {
                JsonConvert.SerializeObject(obj, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Error
                });
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool CanBeSerialized(this PropertyInfo obj, object parent)
        {
            try
            {
                return obj.GetValue(parent).CanBeSerialized();
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
