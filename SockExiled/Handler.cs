using Exiled.API.Features;
using SockExiled.API.Features;
using SockExiled.API.Features.NET;
using SockExiled.API.Features.NET.Serializer.Elements;

namespace SockExiled
{
#pragma warning disable IDE0059 // Assegnazione non necessaria di un valore
    internal class Handler
    {
        public void Event(object ev)
        {
            ev = new EventBucket(new Event(ev)).Execute();
            // da fixare: ev non è l'object dell'evento - bisogna modificare ev tramite reflection via new EventBucket... ecc
            Log.Debug($"STARTED ALLOWED EVENTS EDITED {ev.GetType().Name}");
        }

        public void Event(string name, EventType type = EventType.Unknown)
        {
            SocketPlugin.BroadcastEvent(new Event(name, type));
        }

        public void MapGeneratedEvent()
        {
            SocketPlugin.BroadcastEvent(new Event("Generated", EventType.MapEvent));
        }

        public void RoudnStartedEvent()
        {
            SocketPlugin.BroadcastEvent(new Event("RoundStarted", EventType.ServerEvent));
        }
    }
}