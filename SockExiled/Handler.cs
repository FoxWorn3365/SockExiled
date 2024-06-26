using Exiled.API.Features;
using SockExiled.API.Features;
using SockExiled.API.Features.NET.Serializer.Elements;
using System;
using System.Linq;
using Exiled.Events.EventArgs.Interfaces;
using SockExiled.API.Core;

namespace SockExiled
{
#pragma warning disable IDE0059 // Assegnazione non necessaria di un valore
    internal class Handler
    {
        public void Event(object ev)
        {
            if (SocketPlugin.Plugins.Where(p => p.SubscribedEvents.Contains(ev.GetType().Name.Replace("EventArgs", ""))).Count() == 0)
                return;
            
            // Log.Debug($"Started: {ev.GetType().Name}");
            long start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            if (ev is null)
            {
                Log.Error("Ev is null!");
                return;
            }

            if (EventFlood.TryGet(ev, out EventFlood eventFlood))
            {
                // Log.Info($"Took {DateTimeOffset.Now.ToUnixTimeMilliseconds() - start}ms to parse [cached] event {ev.GetType().Name.Replace("EventArgs", "")}!");
                eventFlood.Response.ToEventArgs(ev);
            }
            else
            {
                EventFlood EventFlood = new(ev);
                if (EventFlood.ArgsResponse is null)
                {
                    Log.Error($"Tried to assign ArgsResponse to the {ev.GetType().Name} event from the {EventFlood.Source.Name} ({EventFlood.Identifier})");
                }
                else
                {
                    Log.Info($"Took {DateTimeOffset.Now.ToUnixTimeMilliseconds() - start}ms to parse [notcached] event {ev.GetType().Name.Replace("EventArgs", "")}!");
                    EventFlood.Response.ToEventArgs(ev);

                    if (ev is IDeniableEvent Deniable)
                    {
                        Log.Warn($"Is event {ev.GetType().Name.Replace("EventArgs", "")} allowed: {Deniable.IsAllowed} - |{ev.GetType().GetProperty("IsAllowed")?.GetValue(ev, null)}|");
                    }
                }
            }

            /*
            // Log.Info($"Took {DateTimeOffset.Now.ToUnixTimeMilliseconds() - start}ms to parse [bfr] event {ev.GetType().Name.Replace("EventArgs", "")}!");

            Event Event = new(ev);

            // Log.Info($"Took {DateTimeOffset.Now.ToUnixTimeMilliseconds() - start}ms to parse [psr] event {ev.GetType().Name.Replace("EventArgs", "")}!");

            EventBucket Bucket = new(Event);

            Event = Bucket.Execute();

            // Log.Info($"Took {DateTimeOffset.Now.ToUnixTimeMilliseconds() - start}ms to parse [rcv] event {ev.GetType().Name.Replace("EventArgs", "")}!");

            if (Event is not null)
            {
                if (ev.GetType().Name.Replace("EventArgs", "") != Event.Name)
                {
                    Log.Warn($"Failed to convert Event object to an EventArgs - Found {ev.GetType().Name.Replace("EventArgs", "")}, expected {Event.Name}");
                    return;
                }

                Log.Info($"Took {DateTimeOffset.Now.ToUnixTimeMilliseconds() - start}ms to parse [fs2] event {ev.GetType().Name.Replace("EventArgs", "")}!");

                Log.Debug($"Evaluated event [FN]: {ev.GetType().Name}");
                ev = Event.ToEventArgs(ev);

                if (ev is IDeniableEvent deEv)
                {
                    Log.Debug($"Is event allowed: {deEv.IsAllowed}");
                }

                Log.Info($"Took {DateTimeOffset.Now.ToUnixTimeMilliseconds() - start}ms to parse [end] event {ev.GetType().Name.Replace("EventArgs", "")}!");
            }
            else
            {
                Log.Error($"Failed to parse event {ev.GetType().Name.Replace("EventArgs", "")}: evaluated event is null!");
            }
            */
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