using Exiled.API.Features;
using Exiled.Events.EventArgs.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SockExiled.API.Storage;
using SockExiled.Extension;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SockExiled.API.Features.NET.Serializer.Elements
{
    internal class Event
    {
        public string Name { get; internal set; }
        
        public EventType EventType { get; }

        public bool Nullable { get; }

        public Dictionary<string, object> Data { get; internal set; }

        public string UniqId { get; }

        public Event(object main) 
        {
            UniqId = Guid.NewGuid().ToString();

            if (main is null)
            {
                Log.Error("Failed to create a SockExiled Event instance: the given object is null");
                return;
            }

            Nullable = main is IDeniableEvent;
            
            if (main is IPlayerEvent)
            {
                EventType = EventType.PlayerEvent;
            }
            else if (main is IItemEvent)
            {
                EventType = EventType.ItemEvent;
            }
            else if (main is IPickupEvent)
            {
                EventType = EventType.PickupEvent;
            }
            else if (main is IScp0492Event)
            {
                EventType = EventType.Scp0492Event;
            }
            else if (main is IScp049Event)
            {
                EventType = EventType.Scp049Event;
            }
            else if (main is IScp079Event)
            {
                EventType = EventType.Scp079Event;
            }
            else if (main is IScp106Event)
            {
                EventType = EventType.Scp106Event;
            }
            else if (main is IScp173Event)
            {
                EventType = EventType.Scp173Event;
            }
            else if (main is IScp096Event)
            {
                EventType = EventType.Scp096Event;
            }
            else if (main is IScp939Event)
            {
                EventType = EventType.Scp939Event;
            }
            else if (main is IScp3114Event)
            {
                EventType = EventType.Scp3114Event;
            }
            else
            {
                EventType = EventType.Unknown;

                // Try to get the name by the namespace
                foreach (EventType eventType in Enum.GetValues(typeof(EventType)))
                {
                    if (main.GetType().FullName.Contains($".{eventType.ToString().Replace("Event", "")}."))
                    {
                        EventType = eventType;
                        break;
                    }
                }
            }

            Log.Debug($"Event '{main.GetType().Name}' partially created, going through reflection...");

            EncodeTypes(main);
        }

        public Event(string name, EventType eventType, bool nullable, Dictionary<string, object> data, string uniqid = null)
        {
            Name = name;
            EventType = eventType;
            Nullable = nullable;
            Data = data;
            UniqId = uniqid ?? Guid.NewGuid().ToString();
        }

        public Event(string name, EventType eventType, bool nullable = false) : this(name, eventType, nullable, new()) { }

        private void EncodeTypes(object main)
        {
            Dictionary<string, object> Encoded = new();
            foreach (PropertyInfo Property in main.GetType().GetProperties().Where(p => p is not null && p.CanRead))
            {
                var Value = Property.GetValue(main, null);

                if (Value is null)
                    continue;

                if (Property.PropertyType == typeof(Player) && Value is Player Player)
                {
                    Dictionary<string, object> Element = Serializer.SerializeElement(Player).ToObject().Diff(PlayerStorage.Cache[Player.Id], false);
                    Element.Add("Id", Player.Id);
                    Encoded.Add(Property.Name, Element);
                }
                else if (Property.PropertyType.IsValueType)
                {
                    Encoded.Add(Property.Name, Value);
                }
                else
                {
                    /*try
                    {
                        Encoded.Add(Property.Name, JsonConvert.SerializeObject(Value));
                    } catch(Exception)
                    {
                        Encoded.Add(Property.Name, null);
                    }*/
                }
            }

            Name = main.GetType().Name.Replace("EventArgs", "");
            Data = Encoded;
        }

        public string Encode() => JsonConvert.SerializeObject(this, new JsonSerializerSettings()
        {
            ContractResolver = new DefaultContractResolver()
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        });

        public object ToEventArgs(object element)
        {
            if (element is null)
            {
                Log.Error("ToEventArgs error: provided element is null!");
                return null;
            }

            if (element.GetType().Name.Replace("EventArgs", "") != Name)
            {
                Log.Warn($"Failed to convert Event object to an EventArgs - Found {element.GetType().Name.Replace("EventArgs", "")}, expected {Name}");
                return null;
            }

            // Log.Info("Started to edit the main object...");
            foreach (PropertyInfo Property in element.GetType().GetProperties().Where(p => p is not null && !p.IsStatic() && p.CanRead && p.CanWrite && Data.ContainsKey(p.Name)))
            {
                // Update the element if present in the data
                try
                {
                    // Log.Info($"Trying to change property {Property.Name} to {Data[Property.Name]}");
                    Property.SetValue(element, Data[Property.Name], null);
                } 
                catch (Exception)
                {
                    Log.Warn($"Failed to update a value inside the original object {element.GetType().FullName}.\nError while changing type from {Data[Property.Name].GetType().FullName} to {Property.PropertyType.Namespace}.");
                }
            }

            return element;
        }

        public static Event Decode(string message)
        {
            try
            {
                Dictionary<string, object> Data = JsonConvert.DeserializeObject<Dictionary<string, object>>(message);

                if (Data.ContainsKey("Name") && Data.ContainsKey("EventType") && Data.ContainsKey("Nullable") && Data.ContainsKey("Data") && Data.ContainsKey("UniqId"))
                {
                    // Let's import it
                    Enum.TryParse(Data["EventType"].ToString(), out EventType EventType);

                    Data["Data"] = JsonConvert.DeserializeObject<Dictionary<string, object>>(Data["Data"].ToString());

                    /*if (Data["Data"] is Dictionary<string, object> Dictionary && Dictionary.ContainsKey("IsAllowed"))
                    {
                        Log.Warn("Key IsAllowed is contained, content: " + Dictionary["IsAllowed"]);
                    }*/

                    return new(Data["Name"].ToString(), EventType, bool.Parse(Data["Nullable"].ToString()), Data["Data"] as Dictionary<string, object>, Data["UniqId"].ToString());
                }
            }
            catch (Exception) { }

            return null;
        }
    }
}
