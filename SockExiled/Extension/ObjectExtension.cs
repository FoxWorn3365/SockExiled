using Exiled.API.Features;
using Newtonsoft.Json;
using SockExiled.API.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.Animations;

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

        public static bool HasSelfReference(this object obj)
        {
            return ReferenceLoopDetector.HasReferenceLoop(obj);
        }

        public static bool CheckForReferenceHub(this object obj, bool deep = false)
        {
            if (obj.GetType().FullName is "ReferenceHub")
                return true;

            if (obj.GetType().FullName is "Hub")
                return true;

            if (obj.GetType().FullName.Contains("RemoteAdmin"))
                return true;

            if (!deep)
                return obj.GetType().GetProperties().Where(p => p.Name is "ReferenceHub").Count() > 0;

            foreach (PropertyInfo Property in obj.GetType().GetProperties().Where(p => !p.PropertyType.IsValueType && !p.PropertyType.FullName.Contains("Structs")))
            {
                if (Property.Name is "ReferenceHub")
                    return true;

                try
                {
                    object Value = Property.GetValue(obj, null);
                    if (Value is not null && Value.CheckForReferenceHub(false))
                    {
                        return true;
                    }
                }
                catch (Exception)
                {
                    // Log.Warn($"Error {e.GetType().Name} - {e.Message}\n{e.StackTrace}\n\n--> {Property.Name} -- {Property.PropertyType.FullName}");
                }
            }

            return false;
        }
    }
}
