using Exiled.API.Features;
using Newtonsoft.Json;
using SockExiled.API.Features.NET.Serializer.Elements;
using SockExiled.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SockExiled.API.Features.NET
{
    internal class SocketServer
    {
        public readonly uint Id = 0;

        public int Port { get; }

        public Socket Socket { get; }

        public IPEndPoint IPEndPoint { get; }

        public List<SocketClient> Clients { get; }

        internal Task AcceptTask { get; set; }

        internal Task GarbageCollector {  get; set; }

        public bool IsActive { get; set; }

        public readonly int RefreshRate = 10;

        public SocketServer(int port)
        {
            IsActive = true;
            Port = port;
            Clients = new();

            if (Plugin.Instance.Config.Ip is "0.0.0.0")
            {
                IPEndPoint = new(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0], port);
            } 
            else
            {
                IPEndPoint = new(Dns.GetHostEntry(IPAddress.Parse(Plugin.Instance.Config.Ip)).AddressList[0], port);
            }

            Socket = new(IPEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Socket.Bind(IPEndPoint);
            Socket.Listen(5);

            // Async task handler
            AcceptTask = Task.Run(ConnectionHandler);
            GarbageCollector = Task.Run(GarbageCollectorAction);
        }

        internal void ConnectionHandler()
        {
            while (IsActive)
            {
                Socket ClientSocket = Socket.Accept();
                SocketClient Client = new(this, (uint)Clients.Count() + 2, ClientSocket, SocketStatus.Connecting);
                Clients.Add(Client);
                Client.Send(BuildMessage("Welcome!", 0x00, Client.Id));
            }
        }

        internal async void GarbageCollectorAction()
        {
            while (IsActive)
            {
                foreach (SocketClient Client in Clients)
                {
                    if (!Client.Socket.Connected)
                    {
                        Client.IsActive = false;
                    }

                    if (!Client.IsActive)
                    {
                        Clients.Remove(Client);
                    }
                }

                await Task.Delay(RefreshRate * 5000);
            }
        }

        internal RawSocketMessage BuildMessage(Dictionary<string, string> data, int code, uint receiver) => BuildMessage(JsonConvert.SerializeObject(data), code, receiver);

        internal RawSocketMessage BuildMessage(string content, int code, uint receiver) => new(0, receiver, content, code, Guid.NewGuid().ToString());

        internal RawSocketMessage BuildBroadcastMessage(string content, int code) => new(0, null, content, code, Guid.NewGuid().ToString());
        
        internal void HandleMessage(SocketMessage message, SocketClient sender)
        {
            RawSocketMessage RawMessage = message;

            Log.Debug($"Code: {RawMessage.Code} - Status: {sender.Status}");
            if (sender.Status is SocketStatus.Connecting && message.Code is 0x01)
            {
                sender.RequestAuthMessage(message.UniqId);
                sender.Status = SocketStatus.Authing;
            }
            else if (sender.Status is SocketStatus.Authing && message.Code is 0x02)
            {
                if (message.Content is not null && message.Content.ContainsKey("key") && Plugin.Instance.Config.Keys.Contains(message.Content["key"]))
                {
                    sender.AuthConfirmMessage(message.UniqId);
                    sender.Status = SocketStatus.Negotiating;
                }
                else
                {
                    sender.RequestAuthMessage(message.UniqId);
                }
            }
            else if (sender.Status is SocketStatus.Negotiating && message.Code is 0x03)
            {
                if (message.Content is not null && SocketPlugin.ValidatePluginData(message.Content))
                {
                    // Accept plugin
                    new SocketPlugin(message.Content, sender);
                    sender.PluginCreatedMessage(message.UniqId);
                    sender.Status = SocketStatus.Connected;
                }
                else
                {
                    sender.PluginCreationErrorMessage(message.UniqId);
                }
            } 
            else if (sender.Status is SocketStatus.Connected)
            {
                if (message.Content is not null && message.Code is 0x40 && AsyncEventHandler.List.Where(ev => ev.Id == message.UniqId).Count() > 0 && SocketPlugin.TryGet(sender, out SocketPlugin Plugin))
                {
                    AsyncEventHandler.List.Where(ev => ev.Id == message.UniqId).First().CollectPoolPiece(Plugin, RawMessage.Content);
                }
                else if (message.Code is 0x82)
                {
                    sender.SendPong(message.UniqId);
                }
                else if (message.Code is 0x20 && message.Content is not null && message.Content.ContainsKey("player"))
                {
                    sender.TrySendPlayer(uint.Parse(message.Content["player"]), message.UniqId);
                }
                else if (message.Code is 0x20f && message.Content is not null && message.Content.ContainsKey("player"))
                {
                    sender.TrySendSchemaPlayer(uint.Parse(message.Content["player"]), message.UniqId);
                }
                else if (message.Code is 0x23)
                {
                    sender.TrySendServer(message.UniqId);
                } 
                else if (message.Code is 0x23f)
                {
                    sender.TrySendSchemaServer(message.UniqId);
                }
                else if (message.Code is 0x24)
                {
                    sender.TrySendPlayerList(message.UniqId);
                }
                else if (message.Code is 0x24c)
                {
                    sender.TrySendChunkedPlayerList(message.UniqId);
                }
                else if (message.Code is 0x24cf) 
                {
                    sender.TrySendChunkedSchemaPlayerList(message.UniqId);
                }
                else if (message.Code is 0xe200a && RawMessage.Content is not null)
                {
                    // Is a message for the server, no need to reply!
                    try
                    {
                        Event Event = Event.Decode(RawMessage.Content);
                        
                        if (EventBucket.TrySearch(Event, out EventBucket Bucket))
                        {
                            Bucket.Append(SocketPlugin.Get(sender), Event);
                        }
                    }
                    catch (Exception) { }
                }
                else if (message.Code is 0x21 && message.Content is not null && message.Content.ContainsKey("item"))
                {
                    sender.TrySendItem(int.Parse(message.Content["item"]), message.UniqId);
                }
                else if (message.Code is 0x21f && message.Content is not null && message.Content.ContainsKey("item"))
                {
                    sender.TrySendSchemaItem(uint.Parse(message.Content["item"]), message.UniqId);
                }
            }
        }
    }
}