using Exiled.API.Features;
using Newtonsoft.Json;
using SockExiled.API.Features;
using SockExiled.API.Features.NET;
using SockExiled.API.Features.NET.Serializer;
using SockExiled.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SockExiled.API.Storage
{
    internal class PlayerStorage
    {
        internal static Dictionary<int, Dictionary<string, object>> Cache { get; } = new();

        public static Dictionary<int, Dictionary<string, object>> Database { get; } = new();

        public static void Populate()
        {
            foreach (Player Player in Player.List)
            {
                Dictionary<string, object> Serialized = Serializer.SerializeElement(Player).ToObject();

                Database.Clear();

                if (!Cache.ContainsKey(Player.Id))
                {
                    Cache.Add(Player.Id, Serialized);
                    Database.TryAdd(Player.Id, Serialized);
                }
                else
                {
                    try
                    {
                        Dictionary<string, object> Diff = Serialized.Diff(Cache[Player.Id], false);
                        Diff.Add("Partial", true);
                        Diff.Add("Id", Player.Id);
                        Database.TryAdd(Player.Id, Diff);
                    }
                    catch (Exception) { }

                    Cache[Player.Id] = Serialized;
                }

                // Count
                Log.Warn($"Database is populated with: {Database.Count}");
            }

            // Share
            Task.Run(() =>
            {
                try
                {
                    Log.Info($"{Cache.Count} in cache, {Database.Count} in database");
                    foreach (SocketPlugin SocketPlugin in SocketPlugin.Plugins)
                    {
                        if (SocketPlugin is not null && SocketPlugin.SocketClient is not null && SocketPlugin.SocketClient.IsActive && SocketPlugin.SocketClient.Status is SocketStatus.Connected)
                        {
                            SocketPlugin.SocketClient.Send(new RawSocketMessage(0, SocketPlugin.SocketClient.Id, JsonConvert.SerializeObject(Database.Values.ToList()), 0xc20));
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"Encountered {e.GetType().FullName}: {e.Message}\n{e.StackTrace}");
                }
            });
        }
    }
}