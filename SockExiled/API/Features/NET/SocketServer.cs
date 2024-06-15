using Exiled.API.Features;
using MEC;
using Newtonsoft.Json;
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

            // Socket setup
            IPEndPoint = new(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0], port);
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
                Client.Send(BuildMessage("welcome", MessageType.ServerRequest, Client.Id));
            }
        }

        internal async void GarbageCollectorAction()
        {
            while (IsActive)
            {
                foreach (SocketClient Client in Clients)
                {
                    if (!Client.IsActive)
                    {
                        Clients.Remove(Client);
                    }
                }

                await Task.Delay(RefreshRate * 1000);
            }
        }

        internal RawSocketMessage BuildMessage(Dictionary<string, string> data, MessageType type, uint receiver) => BuildMessage(JsonConvert.SerializeObject(data), type, receiver);

        internal RawSocketMessage BuildMessage(string content, MessageType type, uint receiver) => new(0, receiver, type, content, Guid.NewGuid().ToString());

        internal RawSocketMessage BuildBroadcastMessage(string content, MessageType type) => new(0, null, type, content, Guid.NewGuid().ToString());
        
        internal void HandleMessage(SocketMessage message, SocketClient sender)
        {
            RawSocketMessage RawMessage = message;

            Log.Debug($"MessageType: {message.MessageType} && DataType: {message.DataType} && Status: {sender.Status}");
            if (sender.Status is SocketStatus.Connecting && message.MessageType is MessageType.Connection && message.DataType is DataType.ConnectionData)
            {
                sender.RequestAuthMessage(message.UniqId);
                sender.Status = SocketStatus.Authing;
            }
            else if (sender.Status is SocketStatus.Authing && message.MessageType is MessageType.Connection && message.DataType is DataType.ConnectionData)
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
            else if (sender.Status is SocketStatus.Negotiating && message.MessageType is MessageType.Connection && message.DataType is DataType.PluginData)
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
                    sender.AuthConfirmMessage(message.UniqId);
                }
            } 
            else if (sender.Status is SocketStatus.Connected)
            {
                if (message.Content is not null && message.MessageType is MessageType.ClientEventResponse && AsyncEventHandler.List.Where(ev => ev.Id == message.UniqId).Count() > 0 && SocketPlugin.TryGetSocketPlugin(sender, out SocketPlugin Plugin))
                {
                    AsyncEventHandler.List.Where(ev => ev.Id == message.UniqId).First().CollectPoolPiece(Plugin, RawMessage.Content);
                }
                else if (message.MessageType is MessageType.Ping)
                {
                    sender.SendPong(message.UniqId);
                }
                else if (message.MessageType is MessageType.SmartPing)
                {
                    sender.SendSmartPong(message.UniqId);
                }
                else if (message.MessageType is MessageType.Request && message.DataType is DataType.TryGetPlayer && message.Content is not null && message.Content.ContainsKey("player"))
                {
                    sender.TrySendPlayer(uint.Parse(message.Content["player"]), message.UniqId);
                }
            }
        }
    }
}