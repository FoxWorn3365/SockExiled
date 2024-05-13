using Exiled.API.Features;
using Newtonsoft.Json;
using SockExiled.API.Features.NET;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SockExiled.API.Features
{
    internal class SocketPlugin
    {
        public uint Id { get; }

        public string Name { get; }

        public string Prefix { get; }

        public string Author { get; }

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
    }
}
