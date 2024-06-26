using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SockExiled.API.Core
{
    internal class TimeCache
    {
        /// <summary>
        /// The maximum lifetime of the cache, in seconds
        /// </summary>
        public const int CacheTime = 50;

        /// <summary>
        /// The tick time of the garbage collector, in milliseconds.
        /// </summary>
        public const int TickTime = 500;

        public static Task AsyncTask { get; private set; }

        private static Dictionary<string, Dictionary<object, object>> Cache { get; } = new();

        // The KeyValuePair is <parent, key> so we can access the specific object inside Cache
        private static Dictionary<KeyValuePair<string, object>, long> Timer { get; } = new();

        private static async void Refresh()
        {
            while (true)
            {
                foreach (KeyValuePair<string, Dictionary<object, object>> Data in Cache)
                {
                    foreach (KeyValuePair<object, object> InternalData in Data.Value.Where(kvp => HasExpired(Data.Key, kvp.Key)))
                    {
                        Remove(Data.Key, InternalData.Key);
                    }
                }

                await Task.Delay(TickTime);
            }
        }

        public static void Set(string parent, object key, object value)
        {
            if (Cache.ContainsKey(parent))
            {
                if (Cache[parent].ContainsKey(key))
                {
                    Timer[new(parent, key)] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    Cache[parent][key] = value;
                }
                else
                {
                    Timer.Add(new(parent, key), DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                    Cache[parent].Add(key, value);
                }
            }
            else
            {
                Cache.Add(parent, new()
                {
                    {
                        key,
                        value
                    }
                });

                Timer.Add(new(parent, key), DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            }
        }

        public static void Remove(string parent, object key)
        {
            if (Cache.ContainsKey(parent) && Cache[parent].ContainsKey(key))
            {
                Cache[parent].Remove(key);
                Timer.Remove(new(parent, key));
            }
        }

        public static bool HasExpired(string parent, object key)
        {
            if (Timer.ContainsKey(new(parent, key)) && Timer[new(parent, key)] + CacheTime < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                return true;

            if (Timer.ContainsKey(new(parent, key)))
                return false;

            return true;
        }

        public static object TryGet(string parent, object key, object value)
        {
            if (Cache.ContainsKey(parent) && Cache[parent].ContainsKey(key))
                return Cache[parent][key];

            return value;
        }

        public static object Get(string parent, object key)
        {
            if (!Cache.ContainsKey(parent))
                return null;

            if (!Cache[parent].ContainsKey(key))
                return null;

            return Cache[parent][key];
        }

        public static bool Has(string parent, object key)
        {
            return Cache.ContainsKey(parent) && (Cache[parent]?.ContainsKey(key) ?? false);
        }

        public static void Init()
        {
            AsyncTask = Task.Run(Refresh);
        }
    }
}
