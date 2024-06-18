using Exiled.API.Features;
using Newtonsoft.Json;
using SockExiled.API.Utilities;
using System;
using System.Collections.Generic;

namespace SockExiled.API.Features.NET
{
    internal class RawSocketMessage
    {
        /// <summary>
        /// The Id of the sender.
        /// <see cref="SocketServer"/> has a readonly Id = 0
        /// </summary>
        public uint Sender { get; }

        /// <summary>
        /// The receiver of the message
        /// </summary>
        public uint? Receiver { get; }

        /// <summary>
        /// The "status code" of the message.
        /// Every message with a different purpouse has a different code.
        /// </summary>
        public int Code { get; }

        /// <summary>
        /// The content of the message as a string
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// The uniqid
        /// </summary>
        public string UniqId { get; }

        public RawSocketMessage(uint sender, uint? receiver, string content, int code, string uniqId = null)
        {
            Sender = sender;
            Receiver = receiver;
            if (Receiver is null && Sender is not 0)
            {
                Receiver = 0;
            }
            Content = content;
            UniqId = uniqId ?? Guid.NewGuid().ToString(); 
            Code = code;
        }

        public RawSocketMessage(string json)
        {
            Dictionary<string, string> Data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            Sender = uint.Parse(Data["sender"]);

            if ((!Data.ContainsKey("receiver") || Data["receiver"] is null || Data["receiver"] == string.Empty) && Sender is not 0)
            {
                Receiver = 0;
            }
            else
            {
                Receiver = uint.Parse(Data["receiver"]);
            }

            Code = int.Parse(Data["code"]);
            Content = Data["content"];
            UniqId = Data["uniq_id"];
        }

        public string Encode() => JsonParser.Encode(this);

        public string Serialize() => Encode();

        public SocketMessage SocketMessage() => new(this);

        public static bool Validate(Dictionary<string, string> data)
        {
            return data.ContainsKey("sender") && data.ContainsKey("code") && data.ContainsKey("content") && data.ContainsKey("uniq_id");
        }
    }
}
