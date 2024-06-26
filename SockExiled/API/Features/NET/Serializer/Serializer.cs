using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Roles;
using SockExiled.Extension;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace SockExiled.API.Features.NET.Serializer
{
    internal class Serializer
    {
        private static List<string> Whitelist { get; } = new()
        {
            "Role",
            "Items"
        };

        public static Dictionary<string, Serialized> SerializeElement(object obj, bool ignoreStatic = true, List<string> blacklist = null)
        {
            blacklist ??= new()
            {
                "Footprint"
            };

            Dictionary<string, Serialized> Data = new();

            if (obj is null)
                return Data;

            foreach (PropertyInfo Property in obj.GetType().GetProperties().Where(p => p is not null && p.CanRead && !(p.IsStatic() && ignoreStatic) && !blacklist.Contains(p.Name)))
            {
                var Value = Property.GetValue(obj, null);

                if (Property.Name.Contains("AuthenticationToken"))
                    continue;

                if (Property.Name.Contains("ActiveTime"))
                    continue;

                if (Property.Name.Contains("DeadTime"))
                    continue;

                if (Property.Name is "RelativePosition")
                    continue;

                if (Property.Name is "ActiveEffects")
                    continue;

                if (Property.Name is "Role")
                {
                    try
                    {
                        Role RealRole = (Role)Value;
                        Data.Add(Property.Name, new(Property.PropertyType, SerializeElement(RealRole, true, new()
                        {
                            "Owner",
                            "RandomSpawnLocation",
                            "Value"
                        }).ToObject()));
                    } 
                    catch (Exception) { }

                    continue;
                }

                if (Property.Name is "Items")
                {
                    try
                    {
                        List<Dictionary<string, object>> ItemsInventory = new();
                        foreach (Item Item in (Value as IReadOnlyCollection<Item>).Where(item => item is not null))
                        {
                            ItemsInventory.Add(ClientExtension.CorrectItem(Item));
                        }

                        Data.Add(Property.Name, new(typeof(List<Item>), ItemsInventory));
                    }
                    catch (Exception) { }

                    continue;
                }

                if (Property.PropertyType == typeof(Item))
                {
                    try
                    {
                        Item Item = (Item)Value;
                        Data.Add(Property.Name, new(Property.PropertyType, ClientExtension.CorrectItem(Item)));
                    }
                    catch (Exception) { }

                    continue;
                }

                if (Property.PropertyType.FullName.Contains("Unity") && Value is not null)
                {
                    try
                    {
                        Vector3 Vector = (Vector3)Value;
                        Data.Add(Property.Name, new(Property, $"{Vector.x},{Vector.y},{Vector.z}"));
                    } 
                    catch (Exception)
                    {
                        try
                        {
                            Quaternion Vector = (Quaternion)Value;
                            Data.Add(Property.Name, new(Property, $"{Vector.x},{Vector.y},{Vector.z},{Vector.w}"));
                        }
                        catch (Exception) { }
                    }

                    continue;
                }

                try
                {
                    if (Value is not null && !Property.PropertyType.IsValueType)
                    {
                        if (Value.CheckForReferenceHub(true))
                            continue;
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to check with deep [{e.GetType().Name}]: {e.Message}\n{e.StackTrace}\n\n--> {Property.Name}");
                }

                try
                {
                    if (Value is not null && Value.HasSelfReference())
                    {
                        // Log.Error($"Found SelfReference Loop inside item {Property.Name} - {Property.PropertyType.FullName}");
                        continue;
                    }
                }
                catch (Exception) { }

                try
                {
                    Data.Add(Property.Name, new(Property, Value));
                }
                catch (Exception) { }
            }

            string FileData = "";
            foreach (KeyValuePair<string, Serialized> Serialized in Data.Where(kvp => kvp.Value is not null && kvp.Value.Value is not null))
            {
                FileData += $"{Serialized.Key} -> {Serialized.Value.TypeFullName} (is {Serialized.Value.Value.GetType().FullName})\n";
            }

            File.WriteAllText(Path.Combine(Paths.Plugins, "ksp.txt"), FileData);

            return Data;
        }
    }
}
