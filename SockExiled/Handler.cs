using SockExiled.API.Features;

namespace SockExiled
{
    internal class Handler
    {
        public void Event(object ev)
        {
            SocketPlugin.BroadcastEvent(new API.Features.NET.Serializer.Elements.Event(ev));
        }
    }
}
