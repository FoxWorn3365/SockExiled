using Exiled.API.Features;
using Exiled.API.Features.Items;
using SockExiled.API.Features.NET.Serializer;
using SockExiled.API.Storage;
using SockExiled.Extension;
using System.Collections.Generic;

namespace SockExiled.API.Core
{
    internal class Utilities
    {
        internal static Dictionary<string, object> CorrectPlayer(Player Player)
        {
            if (PlayerStorage.Database.TryGet(Player.Id, out Dictionary<string, object> Data))
                return Data;

            return Serializer.SerializeElement(Player).ToObject();
        }

        internal static Dictionary<string, object> CorrectItem(Item Item)
        {
            return Serializer.SerializeElement(Item).ToObject();
        }
    }
}
