using Exiled.Events.EventArgs.Interfaces;
using SockExiled.API.Features.NET;
using SockExiled.API.Features.NET.Serializer.Elements;
using SockExiled.Extension;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SockExiled.API.Core
{
    // This is to avoid event flooding to websocket as if the things are the same we can assume that the answer will be the same
    internal class EventFlood
    {
        public static HashSet<EventFlood> List { get; } = new();

        public Event Source { get; }

        public Event Response { get; private set; }

        public object ArgsResponse { get; private set; }

        public EventBucket Bucket { get; }

        public string Identifier { get; }

        public long ValidUntil { get; }

        public const int DurationTime = 15;

        public EventFlood(object real)
        {
            Source = new(real);
            Bucket = new(Source);
            Response = Bucket.Execute();
            Identifier = EvaluateIdentifier(real);
            ArgsResponse = Response.ToEventArgs(real);
            ValidUntil = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + DurationTime;

            List.Add(this);
        }

        public void Destroy() => List.Remove(this);

        public static string EvaluateIdentifier(object real)
        {
            if (real is IPlayerEvent PlayerEvent)
            {
                return PlayerEvent.Player.ToRappresentativeString();
            }
            else
            {
                return real.GetType().FullName;
            }
        }

        public static bool TryGet(string name, string identifier, out EventFlood eventFlood)
        {
            eventFlood = List.Where(ef => ef.Identifier == identifier && ef.Source.Name == name).FirstOrDefault();
            if (eventFlood is not null && eventFlood.ValidUntil < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                eventFlood.Destroy();
                eventFlood = null;
                return false;
            }
            return eventFlood != null;
        }

        public static bool TryGet(object source, out EventFlood eventFlood) => TryGet(source.GetType().Name.Replace("EventArgs", ""), EvaluateIdentifier(source), out eventFlood);

        public static EventFlood Get(string name, string identifier)
        {
            EventFlood Ef = List.Where(ef => ef.Identifier == identifier && ef.Source.Name == name).FirstOrDefault();
            if (Ef.ValidUntil < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                Ef.Destroy();
                return null;
            }

            return Ef;
        }

        public static EventFlood Get(object source) => Get(source.GetType().Name.Replace("EventArgs", ""), EvaluateIdentifier(source));
    }
}
