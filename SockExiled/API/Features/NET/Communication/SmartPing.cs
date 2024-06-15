using Exiled.API.Features;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace SockExiled.API.Features.NET.Communication
{
    internal class SmartPing
    {
        public List<Player> Players { get; }

        public SmartPing()
        {
            Players = Player.List.ToList();
        }

        public string Encode()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
