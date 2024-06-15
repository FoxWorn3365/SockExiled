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

        public MessageType MessageType { get; }

        public DataType DataType { get; set; }

        public string Content { get; }

        public string UniqId { get; }

        public RawSocketMessage(uint sender, uint? receiver, MessageType messageType, string content, string uniqId)
        {
            Sender = sender;
            Receiver = receiver;
            if (Receiver is null && Sender is not 0)
            {
                Receiver = 0;
            }
            MessageType = messageType;
            Content = content;
            UniqId = uniqId; 
        }

        public RawSocketMessage(uint sender, uint? receiver, MessageType messageType, string content, string uniqId, DataType dataType) : this(sender, receiver, messageType, content, uniqId)
        {
            DataType = dataType;
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

            MessageType = (MessageType)Enum.Parse(typeof(MessageType), Data["message_type"]);

            if (Data.ContainsKey("data_type"))
            {
                DataType = (DataType)Enum.Parse(typeof(DataType), Data["data_type"], true);
            }
            else
            {
                DataType = DataType.Unknown;
            }

            Content = Data["content"];
            UniqId = Data["uniq_id"];
        }

        public string Encode() => JsonParser.Encode(this);

        public string Serialize() => Encode();

        public SocketMessage SocketMessage() => new(this);

        public static bool Validate(Dictionary<string, string> data)
        {
            return data.ContainsKey("sender") && data.ContainsKey("message_type") && data.ContainsKey("content") && data.ContainsKey("uniq_id");
        }
    }
}
