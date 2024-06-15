using Exiled.API.Features;
using Exiled.API.Features.Doors;
using SockExiled.API.Features;
using SockExiled.API.Features.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SockExiled.API
{
    internal class Storage
    {
        public static readonly Dictionary<uint, Room> Rooms = new();

        public static readonly Dictionary<uint, Door> Doors = new();
    }
}
