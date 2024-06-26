using System.Collections.Generic;

namespace SockExiled.API.Storage
{
    internal class Storage
    {
        public static Dictionary<int, Dictionary<string, object>> Cache { get; } = new();

        public static Dictionary<int, Dictionary<string, object>> Database { get; } = new();

        public static bool Has(int id) => Database.ContainsKey(id);

        public static Dictionary<string, object> Get(int id) => Database.ContainsKey(id) ? Database[id] : null;

        public static bool TryGet(int id, out Dictionary<string, object> result)
        {
            result = Get(id);
            return result != null;
        }

        public static void Add(int id, Dictionary<string, object> data)
        {
            if (!Has(id))
                Database.Add(id, data);
            else
                Database[id] = data;
        }
    }
}
