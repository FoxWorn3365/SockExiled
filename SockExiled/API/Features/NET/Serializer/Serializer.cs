using Exiled.API.Features;
using Newtonsoft.Json;
using SockExiled.Extension;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SockExiled.API.Features.NET.Serializer
{
    internal class Serializer
    {
        public static Dictionary<string, Serialized> SerializeElement(object obj)
        {
            Dictionary<string, Serialized> Data = new();

            foreach (PropertyInfo Property in obj.GetType().GetProperties().Where(p => p is not null && p.CanRead && !p.IsStatic() && (p.PropertyType.IsTypePrimitive() || p.CanBeSerialized(obj) || p.PropertyType.FullName.Contains("Unity"))))
            {
                if (Property.PropertyType.FullName.Contains("Unity"))
                {
                    try
                    {
                        Vector3 Value = (Vector3)(Property?.GetValue(obj, null) ?? new());
                        Data.Add(Property.Name, new(Property, $"{Value.x},{Value.y},{Value.z}"));
                    } 
                    catch (Exception)
                    {
                        try
                        {
                            Quaternion Value = (Quaternion)(Property?.GetValue(obj, null) ?? new());
                            Data.Add(Property.Name, new(Property, $"{Value.x},{Value.y},{Value.z},{Value.w}"));
                        }
                        catch (Exception) { }
                    }
                    continue;
                }

                try
                {
                    Data.Add(Property.Name, new(Property, Property?.GetValue(obj, null) ?? null));
                }
                catch (Exception) { }
            }

            return Data;
        }
    }
}
