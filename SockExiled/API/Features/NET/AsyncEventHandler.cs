using Exiled.Events.EventArgs.Interfaces;
using Exiled.Events.Features;
using Newtonsoft.Json;
using SockExiled.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using YamlDotNet.Serialization.NamingConventions;

namespace SockExiled.API.Features.NET
{
    internal class AsyncEventHandler
    {
        public static HashSet<AsyncEventHandler> List = new();

        public readonly Dictionary<SocketPlugin, Event> Collector;

        public readonly Event Event;

        public readonly string Name;

        public readonly bool Deniable;

        public readonly string Id;

        public Task<Event> Task { get; internal set; }

        public uint TickCount { get; internal set; } = 0;

        public AsyncEventHandler(Event ev)
        {
            Event = ev;
            Name = ev.GetType().Name;
            Deniable = Event is IDeniableEvent;
            Collector = new();
            Id = Guid.NewGuid().ToString();

            foreach (SocketPlugin Plugin in SocketPlugin.Plugins.Where(plugin => plugin.SocketClient.Status is SocketStatus.Connected && plugin.SubscribedEvents.Contains(Name)))
            {
                Collector.Add(Plugin, null);
                Plugin.SocketClient.SendEventMessage(ev, Id);
            }

            Task = System.Threading.Tasks.Task.Run(TaskHandler);

            List.Add(this);
        }

        public void CollectPoolPiece(SocketPlugin plugin, Event response)
        {
            Collector[plugin] = response;
        }

        public void CollectPoolPiece(SocketPlugin plugin, string response)
        {
            // Decode the json
            Event Target = (Event)typeof(JsonConvert).GetMethod("DeserializeObject").MakeGenericMethod(Event.GetType()).Invoke(null, new object[]
            {
                response
            });
            CollectPoolPiece(plugin, Target);
        }

        internal Event TaskHandler()
        {
            while (Collector.ContainsValue(null) && TickCount < 1500)
            {
                TickCount++;
            }

            return HandleProperties();
        }

        internal Event HandleProperties()
        {
            Event Source = Event;
            foreach (KeyValuePair<SocketPlugin, Event> Data in Collector.Where(l => l.Value is not null && l.GetType().Name == Source.GetType().Name).OrderBy(l => l.Key.Priority))
            {
                foreach (PropertyInfo Property in Source.GetType().GetProperties().Where(p => p.CanWrite))
                {
                    string Name = PascalCaseNamingConvention.Instance.Apply(Property.Name);

                    if (Property.GetValue(Source, null) != Data.Value.GetType().GetProperty(Name).GetValue(Data.Value, null))
                    {
                        Property.SetValue(Source, Data.Value.GetType().GetProperty(Name).GetValue(Data.Value), null);
                    }
                }
            }

            return Source;
        }

        public Event Execute()
        {
            Task.Wait();
            List.Remove(this);
            return Task.Result;
        }
    }
}
