using Exiled.API.Features;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SockExiled.API.Features.NET
{
    internal class SocketClient
    {
        public uint Id { get; }

        public Socket Socket { get; }

        public SocketServer Server { get; }

        public Task MessageTask { get; internal set; }

        public SocketStatus Status { get; set; }

        public bool IsActive { get; set; } = false;

        public byte[] Buffer { get; internal set; }

        public SocketClient(SocketServer server, uint id, Socket socket, SocketStatus status)
        {
            Server = server;
            Id = id;
            Socket = socket;
            Status = status;
            Socket.ReceiveTimeout = 25000;
            IsActive = true;
            MessageTask = Task.Run(MessageLoop);
        }

        internal void MessageLoop()
        {
            while (IsActive)
            {
                Log.Warn("TaskManager message encounter for client enabled!");
                // Receiving buffer size and clearing the previous one
                Buffer = new byte[1024];
                string ReadBuffer = string.Empty;
                while (true)
                {
                    ReadBuffer += Encoding.UTF8.GetString(Buffer, 0, Socket.Receive(Buffer));
                    if (ReadBuffer.Contains("<EoM>"))
                    {
                        ReadBuffer = ReadBuffer.Replace("<EoM>", "");
                        break;
                    }
                }

                // Now we have the full message to handle
                Log.Warn($"Received full message from the taskmanager!: '{ReadBuffer}'");
                try
                {
                    RawSocketMessage Message = new(ReadBuffer);
                    Log.Info("Successfully parsed message from ReadBuffer!");
                    Server.HandleMessage(Message.SocketMessage(), this);
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to parse client {Id} message: {e.Message}\n | {e.Source}\n{e.StackTrace}");
                }
            }
        }

        public void Send(RawSocketMessage message)
        {
            if (IsActive)
            {
                Socket.Send(Encoding.UTF8.GetBytes(message.Encode()));
            }
        }

        public void Send(SocketMessage message) => Send(message as RawSocketMessage);

        #nullable enable
        public RawSocketMessage BuildMessage(Dictionary<string, string> data, MessageType type, string? uniqId) => new(Server.Id, Id, type, JsonConvert.SerializeObject(data), uniqId ?? Guid.NewGuid().ToString());

        public RawSocketMessage BuildMessage(string data, MessageType type, string? uniqId) => new(Server.Id, Id, type, data, uniqId ?? Guid.NewGuid().ToString());

        public void Send(Dictionary<string, string> data, MessageType type, string? uniqId) => Send(BuildMessage(data, type, uniqId));

        public void Send(string data, MessageType type, string? uniqid) => Send(BuildMessage(data, type, uniqid));
        // Remember to disable the nullable if you add more methods, ok fox?
    }
}
