using Exiled.API.Features;
using Exiled.Events.Features;
using SockExiled.API.Features.NET;
using SockExiled.API.Features.NET.Serializer;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using SockExiled.API;
using System.Linq;
using Exiled.API.Features.Items;
using SockExiled.API.Core;
using System.Threading;
using Interactables.Interobjects.DoorUtils;

namespace SockExiled.Extension
{
    internal static class ClientExtension
    {
        public static void RequestAuthMessage(this SocketClient client, string uniqId = null)
        {
            client.Send(new RawSocketMessage(0, client.Id, "connected - awaithing authentication", 0x10, uniqId ?? Guid.NewGuid().ToString()));
        }

        public static void AuthConfirmMessage(this SocketClient client, string uniqId = null)
        {
            client.Send(new RawSocketMessage(0, client.Id, "authed - awaiting plugin info", 0x11, uniqId ?? Guid.NewGuid().ToString()));
        }

        public static void PluginCreatedMessage(this SocketClient client, string uniqId = null)
        {
            client.Send(new RawSocketMessage(0, client.Id, "created", 0x12, uniqId ?? Guid.NewGuid().ToString()));
        }

        public static void PluginCreationErrorMessage(this SocketClient client, string uniqId = null)
        {
            client.Send(new RawSocketMessage(0, client.Id, "error during creation", 0x12e, uniqId ?? Guid.NewGuid().ToString()));
        }

        public static void SendEventMessage(this SocketClient client, Event ev, string uniqId)
        {
            client.Send(new RawSocketMessage(0, client.Id, JsonConvert.SerializeObject(ev), 0x32, uniqId));
        }

        public static void SendPong(this SocketClient client, string uniqId = null)
        {
            client.Send(new RawSocketMessage(0, client.Id, null, 0x83, uniqId ?? Guid.NewGuid().ToString()));
        }

        //
        // ==================================
        //           SERIALIZATION
        // ==================================
        //

        // SERVER

        public static void TrySendServer(this SocketClient client, string uniqId = null)
        {
            try
            {
                Log.Debug("Received element");
                string Data = JsonConvert.SerializeObject(Serializer.SerializeElement(typeof(Server), false).ToObject(), new JsonSerializerSettings()
                {
                    Error = delegate (object sender, ErrorEventArgs args)
                    {
                        Log.Error($"Error while parsing member {args.ErrorContext.Member} ({args.ErrorContext.Member.GetType().Name}) {args.ErrorContext.OriginalObject.GetType().Name}: {args.ErrorContext.Error.Source} {args.ErrorContext.Error.Message} - {args.ErrorContext.Error.Data} (Handled: {args.ErrorContext.Handled})");
                    }
                });
                Log.Debug("Received data!");
                client.Send(new RawSocketMessage(0, client.Id, Data, 0x33, uniqId));
            }
            catch (Exception e)
            {
                Log.Error($"({e.GetType().Name}) Error while parsing player: {e.Message} - {e.InnerException.Message}");
                client.Send(new RawSocketMessage(0, client.Id, "error while fetching server", 0x33e, uniqId));
            }
        }

        // PLAYERS

        public static void TrySendPlayer(this SocketClient client, uint id, string uniqId = null)
        {
            Log.Debug($"Almost here with uint {id}");
            try
            {
                Player Player = Player.Get((int)id);
                if (Player is not null)
                {
                    Log.Debug("Nahh not null");
                    string Data = JsonConvert.SerializeObject(CorrectPlayer(Player), new JsonSerializerSettings()
                    {
                        Error = delegate (object sender, ErrorEventArgs args)
                        {
                            Log.Error($"Error while parsing member {args.ErrorContext.Member} ({args.ErrorContext.Member.GetType().Name}) {args.ErrorContext.OriginalObject.GetType().Name}: {args.ErrorContext.Error.Source} {args.ErrorContext.Error.Message} - {args.ErrorContext.Error.Data} (Handled: {args.ErrorContext.Handled})");
                        }
                    });
                    Log.Debug("Received data!");
                    client.Send(new RawSocketMessage(0, client.Id, Data, 0x30, uniqId));
                }
                else
                {
                    client.Send(new RawSocketMessage(0, client.Id, "not found", 0x30e, uniqId));
                }
            }
            catch (Exception e)
            {
                Log.Error($"({e.GetType().Name}) Error while parsing player: {e.Message} - {e.InnerException.Message}");
            }
        }

        public static void TrySendPlayerList(this SocketClient client, string uniqId = null)
        {
            List<int> Ids = new();

            foreach (Player Player in Player.List)
            {
                Ids.Add(Player.Id);
            }

            client.Send(new RawSocketMessage(0, client.Id, JsonConvert.SerializeObject(Ids), 0x34, uniqId));
        }

        public static void TrySendChunkedPlayerList(this SocketClient client, string uniqId = null)
        {
            List<Dictionary<string, object>> Players = new();

            try
            {
                foreach (Player Player in Player.List)
                {
                    Players.Add(CorrectPlayer(Player));
                }

                string Data = JsonConvert.SerializeObject(Players, new JsonSerializerSettings()
                {
                    Error = delegate (object sender, ErrorEventArgs args)
                    {
                        Log.Error($"Error while parsing member {args.ErrorContext.Member} ({args.ErrorContext.Member.GetType().Name}) {args.ErrorContext.OriginalObject.GetType().Name}: {args.ErrorContext.Error.Source} {args.ErrorContext.Error.Message} - {args.ErrorContext.Error.Data} (Handled: {args.ErrorContext.Handled})");
                    }
                });

                client.Send(new RawSocketMessage(0, client.Id, Data, 0x34c, uniqId));
            }
            catch (Exception e)
            {
                Log.Error($"({e.GetType().Name}) Error while parsing player list: {e.Message} - {e.InnerException.Message}");
                client.Send(new RawSocketMessage(0, client.Id, "error while fetching server", 0x34ce, uniqId));
            }
        }

        // ITEMS

        public static void TrySendItem(this SocketClient client, int id, string uniqId = null)
        {
            Log.Debug($"Almost here with uint {id}");
            try
            {
                Item Item = Item.Get((ushort)id);

                if (Item is not null)
                {
                    Log.Debug("Nahh not null");
                    string Data = JsonConvert.SerializeObject(CorrectItem(Item), new JsonSerializerSettings()
                    {
                        Error = delegate (object sender, ErrorEventArgs args)
                        {
                            Log.Error($"Error while parsing member {args.ErrorContext.Member} ({args.ErrorContext.Member.GetType().Name}) {args.ErrorContext.OriginalObject.GetType().Name}: {args.ErrorContext.Error.Source} {args.ErrorContext.Error.Message} - {args.ErrorContext.Error.Data} (Handled: {args.ErrorContext.Handled})");
                        }
                    });
                    Log.Debug("Received data!");
                    client.Send(new RawSocketMessage(0, client.Id, Data, 0x31, uniqId));
                }
                else
                {
                    client.Send(new RawSocketMessage(0, client.Id, "not found", 0x31e, uniqId));
                }
            }
            catch (Exception e)
            {
                Log.Error($"({e.GetType().Name}) Error while parsing player: {e.Message} - {e.InnerException.Message}");
            }
        }

        internal static Dictionary<string, object> CorrectItem(Item item) => Utilities.CorrectItem(item);

        internal static Dictionary<string, object> CorrectPlayer(Player player) => Utilities.CorrectPlayer(player);

        //
        // ==================================
        //             SCHEMAs
        // ==================================
        //

        // SERVER

        public static void TrySendSchemaServer(this SocketClient client, string uniqId = null)
        {
            try
            {
                Log.Debug("Received element");
                string Data = JsonConvert.SerializeObject(Serializer.SerializeElement(typeof(Server), false), new JsonSerializerSettings()
                {
                    Error = delegate (object sender, ErrorEventArgs args)
                    {
                        Log.Error($"Error while parsing member {args.ErrorContext.Member} ({args.ErrorContext.Member.GetType().Name}) {args.ErrorContext.OriginalObject.GetType().Name}: {args.ErrorContext.Error.Source} {args.ErrorContext.Error.Message} - {args.ErrorContext.Error.Data} (Handled: {args.ErrorContext.Handled})");
                    }
                });
                Log.Debug("Received data!");
                client.Send(new RawSocketMessage(0, client.Id, Data, 0x33f, uniqId));
            }
            catch (Exception e)
            {
                Log.Error($"({e.GetType().Name}) Error while parsing player: {e.Message} - {e.InnerException.Message}");
                client.Send(new RawSocketMessage(0, client.Id, "error while fetching server", 0x33e, uniqId));
            }
        }

        // PLAYERS

        public static void TrySendChunkedSchemaPlayerList(this SocketClient client, string uniqId = null)
        {
            List<Dictionary<string, Serialized>> Players = new();

            try
            {
                foreach (Player Player in Player.List)
                {
                    Players.Add(Serializer.SerializeElement(Player));
                }

                string Data = JsonConvert.SerializeObject(Players, new JsonSerializerSettings()
                {
                    Error = delegate (object sender, ErrorEventArgs args)
                    {
                        Log.Error($"Error while parsing member {args.ErrorContext.Member} ({args.ErrorContext.Member.GetType().Name}) {args.ErrorContext.OriginalObject.GetType().Name}: {args.ErrorContext.Error.Source} {args.ErrorContext.Error.Message} - {args.ErrorContext.Error.Data} (Handled: {args.ErrorContext.Handled})");
                    }
                });

                client.Send(new RawSocketMessage(0, client.Id, Data, 0x34cf, uniqId));
            }
            catch (Exception e)
            {
                Log.Error($"({e.GetType().Name}) Error while parsing player list: {e.Message} - {e.InnerException.Message}");
                client.Send(new RawSocketMessage(0, client.Id, "error while fetching server", 0x34cfe, uniqId));
            }
        }
        public static void TrySendSchemaPlayer(this SocketClient client, uint id, string uniqId = null)
        {
            Log.Debug($"Almost here with uint {id}");
            try
            {
                Player Player = Player.Get((int)id);
                if (Player is not null)
                {
                    Log.Debug("Nahh not null");
                    string Data = JsonConvert.SerializeObject(Serializer.SerializeElement(Player), new JsonSerializerSettings()
                    {
                        Error = delegate (object sender, ErrorEventArgs args)
                        {
                            Log.Error($"Error while parsing member {args.ErrorContext.Member} ({args.ErrorContext.Member.GetType().Name}) {args.ErrorContext.OriginalObject.GetType().Name}: {args.ErrorContext.Error.Source} {args.ErrorContext.Error.Message} - {args.ErrorContext.Error.Data} (Handled: {args.ErrorContext.Handled})");
                        }
                    });
                    Log.Debug("Received data!");
                    client.Send(new RawSocketMessage(0, client.Id, Data, 0x30f, uniqId));
                }
                else
                {
                    client.Send(new RawSocketMessage(0, client.Id, "not found", 0x30e, uniqId));
                }
            }
            catch (Exception e)
            {
                Log.Error($"({e.GetType().Name}) Error while parsing player: {e.Message} - {e.InnerException.Message}");
            }
        }

        // ITEMS

        public static void TrySendSchemaItem(this SocketClient client, uint id, string uniqId = null)
        {
            Log.Debug($"Almost here with uint {id}");
            try
            {
                Item Item = Item.Get((ushort)id);
                if (Item is not null)
                {
                    Log.Debug("Nahh not null");
                    string Data = JsonConvert.SerializeObject(Serializer.SerializeElement(Item), new JsonSerializerSettings()
                    {
                        Error = delegate (object sender, ErrorEventArgs args)
                        {
                            Log.Error($"Error while parsing member {args.ErrorContext.Member} ({args.ErrorContext.Member.GetType().Name}) {args.ErrorContext.OriginalObject.GetType().Name}: {args.ErrorContext.Error.Source} {args.ErrorContext.Error.Message} - {args.ErrorContext.Error.Data} (Handled: {args.ErrorContext.Handled})");
                        }
                    });
                    Log.Debug("Received data!");
                    client.Send(new RawSocketMessage(0, client.Id, Data, 0x31f, uniqId));
                }
                else
                {
                    client.Send(new RawSocketMessage(0, client.Id, "not found", 0x31e, uniqId));
                }
            }
            catch (Exception e)
            {
                Log.Error($"({e.GetType().Name}) Error while parsing player: {e.Message} - {e.InnerException.Message}");
            }
        }
    }
}
