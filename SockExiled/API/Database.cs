using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SockExiled.API
{
    internal class Database
    {
        public static Dictionary<int, Dictionary<string, object>> CachedPlayers = new();
        public static Dictionary<int, Dictionary<string, object>> CachedItems = new();
    }
}
