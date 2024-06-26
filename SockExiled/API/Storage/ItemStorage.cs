using System.Collections.Generic;

namespace SockExiled.API.Storage
{
    internal class ItemStorage : Storage
    {
        public static new Dictionary<int, Dictionary<string, object>> Database { get; }
    }
}
