using Exiled.API.Features;
using Newtonsoft.Json;
using SockExiled.API.Features.NET;
using SockExiled.API.Features.NET.Serializer.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SockExiled.API.Features
{
    internal class SocketPlugin
    {
        /// <summary>
        /// Get a list of every <see cref="SocketPlugin"/> registered
        /// </summary>
        public static HashSet<SocketPlugin> Plugins { get; } = new();

        public uint Id { get; }

        public string Name { get; }

        public string Prefix { get; }

        public string Author { get; }

        public uint Priority { get; internal set; } = 1;

        public Version Version { get; }

        public List<string> SubscribedEvents { get; }

        public SocketClient SocketClient {  get; }

        public SocketPlugin(uint id, string name, string prefix, string author, Version version, List<string> subscribedEvents, SocketClient client)
        {
            Id = id;
            Name = name;
            Prefix = prefix;
            Author = author;
            Version = version;
            SubscribedEvents = subscribedEvents;
            SocketClient = client;
            Plugins.Add(this);
        }

        public SocketPlugin(uint id, string name, string prefix, string author, string version, List<string> subscribedEvents, SocketClient client)
        {
            Id = id;
            Name = name;
            Prefix = prefix;
            Author = author;
            Version = new(version);
            SubscribedEvents = subscribedEvents;
            SocketClient = client;
            Plugins.Add(this);
        }

        public SocketPlugin(uint id, string name, string prefix, string author, Version version, string subscribedEvents, SocketClient client)
        {
            Id = id;
            Name = name;
            Prefix = prefix;
            Author = author;
            Version = version;
            SubscribedEvents = subscribedEvents.Split('|').ToList();
            SocketClient = client;
            Plugins.Add(this);
        }

        public SocketPlugin(uint id, string name, string prefix, string author, string version, string subscribedEvents, SocketClient client)
        {
            Id = id;
            Name = name;
            Prefix = prefix;
            Author = author;
            Version = new(version);
            SubscribedEvents = subscribedEvents.Split('|').ToList();
            SocketClient = client;
            Plugins.Add(this);
        }

        internal SocketPlugin(Dictionary<string, string> data, SocketClient client)
        {
            Id = uint.Parse(data["id"]);
            Name = data["name"];
            Prefix = data["prefix"];
            Author = data["author"];
            Version = new(data["version"]);
            SubscribedEvents = data["subscribed_events"].Split('|').ToList();
            SocketClient = client;
            Plugins.Add(this);
        }

        public static SocketPlugin Summon(string data, SocketClient client)
        {
            try
            {
                Dictionary<string, string> Info = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
                
                if (Info is not null && ValidatePluginData(Info))
                {
                    return new(Info, client);
                }
                else
                {
                    Log.Warn("Failed to load a plugin!\nPlugin INFO are not correct!");
                    return null;
                }
            } 
            catch (Exception e)
            {
                Log.Error($"Error while deserializing a plugin data: {e.Message}\n | {e.Source}\n - \n | {e.InnerException}");
                return null;
            }
        }

        public static bool ValidatePluginData(Dictionary<string, string> data)
        {
            return data.ContainsKey("id") && data.ContainsKey("name") && data.ContainsKey("prefix") && data.ContainsKey("author") && data.ContainsKey("version") && data.ContainsKey("subscribed_events");
        }

        public static bool TryGetSocketPlugin(SocketClient client, out SocketPlugin plugin)
        {
            plugin = null;
            if (Plugins.Where(pl => pl.SocketClient == client).Count() > 0)
            {
                plugin = Plugins.Where(pl => pl.SocketClient == client).First();
                return true;
            }

            return false;
        }

        public static SocketPlugin GetHigherPriority() => Plugins.OrderBy(p => p.Priority).Last();

        // From less to high
        public static HashSet<SocketPlugin> OrderByPriority() => Plugins.OrderBy(x => x.Priority).ToHashSet();

        public static void BroadcastEvent(Event ev)
        {
            Log.Debug($"Proposed the event {ev.Name} to every connected plugin...");

            foreach (SocketPlugin Plugin in Plugins)
            {
                Plugin.HandleEvent(ev);
            }
        }

        internal void HandleEvent(Event ev)
        {
            if (SubscribedEvents.Contains(ev.Name))
            {
                // Send the event
                SocketClient.Send(new RawSocketMessage(0, SocketClient.Id, ev.Encode(), 0xe200));
            }
        }

        public void Destroy()
        {
            SocketClient.Close();
            Plugins.Remove(this);
        }

        public void Close() => Destroy();

        public void UpdatePriority(uint priority)
        {
            Priority = priority;
        }
    }
}
