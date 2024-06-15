using Newtonsoft.Json;
using System;
using System.Collections.Generic;

#nullable enable
namespace SockExiled.API.Features.NET
{
    internal class SocketMessage : RawSocketMessage
    {
        public string RawContent { get; }

        public new Dictionary<string, string>? Content { get; }

        public bool SentFromServer { get; } = false;

        public SocketMessage(uint sender, uint? receiver, string content, MessageType messageType, string? uniqId = null) : base(sender, receiver, messageType, content, uniqId ?? Guid.NewGuid().ToString())
        {
            RawContent = base.Content;
            Content = null;
            try
            {
                Content = JsonConvert.DeserializeObject<Dictionary<string, string>>(base.Content);
            }
            catch { }

            if (sender == 0)
            {
                SentFromServer = true;
            }
        }

        public SocketMessage(uint sender, uint? receiver, Dictionary<string, string> content, MessageType messageType, string? uniqId = null) : base(sender, receiver, messageType, JsonConvert.SerializeObject(content), uniqId ?? Guid.NewGuid().ToString())
        {
            RawContent = JsonConvert.SerializeObject(content);
            Content = content;

            if (sender == 0)
            {
                SentFromServer = true;
            }
        }

        public SocketMessage(RawSocketMessage raw) : base(raw.Sender, raw.Receiver, raw.MessageType, raw.Content, raw.UniqId, raw.DataType)
        {
            RawContent = raw.Content;
            Content = null;
            try
            {
                Content = JsonConvert.DeserializeObject<Dictionary<string, string>>(base.Content);
            } catch { }

            if (raw.Sender == 0)
            {
                SentFromServer = true;
            }
        }
    }
}
