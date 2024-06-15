using Exiled.API.Features;
using Exiled.Events.Features;
using InventorySystem.Items.MicroHID;
using SockExiled.API.Features.NET;
using SockExiled.API.Features.NET.Communication;
using SockExiled.API.Features.NET.Serializer;
using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SockExiled.Extension
{
    internal static class ClientExtension
    {
        public static void RequestAuthMessage(this SocketClient client, string uniqId = null)
        {
            client.Send(new RawSocketMessage(0, client.Id, MessageType.ServerRequest, "connected - awaithing authentication", uniqId ?? Guid.NewGuid().ToString(), DataType.ConnectionData));
        }

        public static void AuthConfirmMessage(this SocketClient client, string uniqId = null)
        {
            client.Send(new RawSocketMessage(0, client.Id, MessageType.ServerRequest, "authed - awaiting plugin info", uniqId ?? Guid.NewGuid().ToString(), DataType.ConnectionData));
        }

        public static void PluginCreatedMessage(this SocketClient client, string uniqId = null)
        {
            client.Send(new RawSocketMessage(0, client.Id, MessageType.Connection, "created", uniqId ?? Guid.NewGuid().ToString(), DataType.ConnectionData));
        }

        public static void SendEventMessage(this SocketClient client, Event ev, string uniqId)
        {
            client.Send(new RawSocketMessage(0, client.Id, MessageType.ServerEvent, Newtonsoft.Json.JsonConvert.SerializeObject(ev), uniqId));
        }

        public static void SendPong(this SocketClient client, string uniqId = null)
        {
            client.Send(new RawSocketMessage(0, client.Id, MessageType.Ping, null, uniqId ?? Guid.NewGuid().ToString()));
        }

        public static void SendSmartPong(this SocketClient client, string uniqId = null)
        {
            client.Send(new RawSocketMessage(0, client.Id, MessageType.SmartPing, new SmartPing().Encode(), uniqId ?? Guid.NewGuid().ToString()));
        }

        public static void TrySendPlayer(this SocketClient client, uint id, string uniqId = null)
        {
            Log.Debug($"Almost here with uint {id}");
            Log.Debug("RRS SEndPlayer");
            try
            {
                Player Player = Player.Get((int)id);
                if (Player is not null)
                {
                    Log.Debug("Nahh not null");
                    var Element = Serializer.SerializeElement(Player);
                    Log.Debug("Received element");
                    string Data = JsonConvert.SerializeObject(Element, new JsonSerializerSettings()
                    {
                        Error = delegate(object sender, ErrorEventArgs args) 
                        {
                            Log.Error($"Error while parsing member {args.ErrorContext.Member} ({args.ErrorContext.Member.GetType().Name}) {args.ErrorContext.OriginalObject.GetType().Name}: {args.ErrorContext.Error.Source} {args.ErrorContext.Error.Message} - {args.ErrorContext.Error.Data} (Handled: {args.ErrorContext.Handled})");
                        }
                    });
                    Log.Debug("Received data!");
                    client.Send(new RawSocketMessage(0, client.Id, MessageType.Communication, Data, uniqId, DataType.TryGetPlayer));
                }
                else
                {
                    client.Send(new RawSocketMessage(0, client.Id, MessageType.Communication, "not found", uniqId, DataType.TryGetPlayer));
                }
            }
            catch (Exception e)
            {
                Log.Error($"({e.GetType().Name}) Error while parsing player: {e.Message} - {e.InnerException.Message}");
            }

        }
    }
}
