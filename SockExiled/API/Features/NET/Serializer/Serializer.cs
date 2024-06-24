using Exiled.API.Features.Roles;
using SockExiled.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SockExiled.API.Features.NET.Serializer
{
    internal class Serializer
    {
        public static Dictionary<string, Serialized> SerializeElement(object obj, bool ignoreStatic = true)
        {
            Dictionary<string, Serialized> Data = new();

            if (obj is null)
                return Data;

            foreach (PropertyInfo Property in obj.GetType().GetProperties().Where(p => p is not null && p.CanRead && !(p.IsStatic() && ignoreStatic) && (p.PropertyType.IsTypePrimitive() || p.CanBeSerialized(obj) || p.PropertyType.FullName.Contains("Unity") || p.Name == "Role")))
            {
                if (Property.Name.Contains("AuthenticationToken"))
                    continue;

                if (Property.Name.Contains("ActiveTime"))
                    continue;

                if (Property.Name.Contains("DeadTime"))
                    continue;

                if (Property.Name is "Role")
                {
                    try
                    {
                        Role Role = (Role)(Property?.GetValue(obj, null));
                        Data.Add(Property.Name, new Serialized(Property.PropertyType, SerializeElement(Role).ToObject()));
                    } catch (Exception) { }

                    continue;
                }

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
