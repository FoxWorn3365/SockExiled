using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SockExiled.API.Features
{
    internal class Editor
    {
        public static bool EditRequiredValue(DataType type, Dictionary<string, string> values, ushort target = 0) => type switch
        {
            DataType.UpdateItemData => DynamicEditor(Item.Get(target), values),
            DataType.UpdatePlayerData => DynamicEditor(Player.Get(target), values),
            DataType.UpdatePickupData => DynamicEditor(Pickup.Get(target), values),
            DataType.UpdateServerData => StaticEditor(typeof(Server), values),
            _ => false,
        };

        public static bool DynamicEditor(object data, Dictionary<string, string> values)
        {
            foreach (KeyValuePair<string, string> Elements in values)
            {
                if (data.GetType().GetProperty(Elements.Key) is not null && data.GetType().GetProperty(Elements.Key).CanWrite)
                {
                    data.GetType().GetProperty(Elements.Key).SetValue(data, TypeDescriptor.GetConverter(data.GetType().GetProperty(Elements.Key).PropertyType).ConvertFromInvariantString(Elements.Value), null);
                }
            }

            return true;
        }

        public static bool StaticEditor(Type type, Dictionary<string, string> values)
        {
            foreach (KeyValuePair<string, string> Elements in values)
            {
                if (type.GetProperty(Elements.Key) is not null && type.GetProperty(Elements.Key).CanWrite)
                {
                    type.GetProperty(Elements.Key).SetValue(null, TypeDescriptor.GetConverter(type.GetProperty(Elements.Key).PropertyType).ConvertFromInvariantString(Elements.Value), null);
                }
            }

            return true;
        }
    }
}
