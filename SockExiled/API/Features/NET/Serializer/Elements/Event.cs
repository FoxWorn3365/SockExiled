using Exiled.API.Features;
using Exiled.Events.EventArgs.Interfaces;
using Newtonsoft.Json;
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

        public object Data { get; internal set; }

        public Event(object main) 
        {
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

            Log.Debug("Event partially created, going through reflection...");

            EncodeTypes(main);
        }

        private void EncodeTypes(object main)
        {
            Dictionary<string, object> Encoded = new();
            foreach (PropertyInfo Property in main.GetType().GetProperties().Where(p => p is not null && p.CanRead))
            {
                var Value = Property.GetValue(main, null);
                if (Property.PropertyType == typeof(Player) && Value is not null)
                {
                    Encoded.Add(Property.Name, ClientExtension.CorrectPlayer((Player)Value));
                }
                else
                {
                    // Try encode with json
                    if (Value.CanBeSerialized())
                    {
                        Encoded.Add(Property.Name, JsonConvert.SerializeObject(Value));
                    }
                    else
                    {
                        Encoded.Add(Property.Name, null);
                    }
                }
            }

            Name = main.GetType().Name.Replace("EventArgs", "");
            Data = Encoded;
        }

        public string Encode() => JsonConvert.SerializeObject(this);
    }
}
