using Exiled.API.Features;
using Newtonsoft.Json;
using SockExiled.API.Features.NET.Serializer;
using System.Collections.Generic;

namespace SockExiled.Extension
{
    internal static class DictionaryExtension
    {
        public static Dictionary<string, object> ToObject(this Dictionary<string, Serialized> dictionary)
        {
            Dictionary<string, object> Data = new();
            foreach (KeyValuePair<string, Serialized> pair in dictionary)
            {
                Data.Add(pair.Key, pair.Value.Value);
            }

            return Data;
        }

        public static Dictionary<string, object> Diff(this Dictionary<string, object> dictionary, Dictionary<string, object> element, bool stopIfCountIsWrong = true)
        {
            if (dictionary.Count != element.Count && stopIfCountIsWrong)
            {
                Log.Error($"Failed to 'Diff' two Dictionaries: expect same value, found {dictionary.Count} and {element.Count}");
                return dictionary;
            }

            Dictionary<string, object> Data = new();
            foreach (KeyValuePair<string, object> pair in dictionary)
            {
                if (!element.ContainsKey(pair.Key))
                {
                    Log.Error($"Key {pair.Key} found in the base element but not found in the comparison one!");
                    return dictionary;
                }


                if (element[pair.Key] is null)
                {
                    if (pair.Value is not null)
                    {
                        Data.Add(pair.Key, null);
                    }

                    continue;
                }

                if (!element[pair.Key].Equals(pair.Value))
                {
                    // Some objects are like dictionaries so let's filter them out
                    if (!pair.Value.GetType().IsValueType)
                    {
                        if (JsonConvert.SerializeObject(pair.Value) == JsonConvert.SerializeObject(element[pair.Key]))
                        {
                            continue;
                        }
                    }

                    Log.Debug($"Found uncongruent keys [{pair.Key}]: found {pair.Value} ({pair.Value.GetType().Name}) and {element[pair.Key]} ({element[pair.Key].GetType().Name})");
                    Data.Add(pair.Key, pair.Value);
                }
            }

            Log.Debug($"Returning diff with length: {Data.Count}");
            return Data;
        }

        public static void TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }
    }
}
