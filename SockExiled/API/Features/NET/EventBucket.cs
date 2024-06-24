using SockExiled.API.Features.NET.Serializer.Elements;
using SockExiled.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Assertions.Must;

namespace SockExiled.API.Features.NET
{
    internal class EventBucket
    {
        public static List<EventBucket> List { get; } = new();

        public Event Submitted { get; }

        private Dictionary<SocketPlugin, Event> Response { get; } = new();

        public const int Tick = 5;

        public const int Timeout = 1000;

        private int Counter = 0;

        public bool IsCompleted { get; private set; } = false;

        private Task<Event> AsyncTask { get; set; }

        public EventBucket(Event submitted)
        {
            Submitted = submitted;

            AsyncTask = Task.Run(TickAction);

            // Send async events with a task to interested elements
            foreach (SocketPlugin Plugin in SocketPlugin.Plugins.Where(pl => pl.SubscribedEvents.Contains(Submitted.Name)))
            {
                Response.Add(Plugin, null);
                Task.Run(() =>
                {
                    Plugin.HandleEvent(Submitted);
                });
            }

            List.Add(this);
        }

        public static EventBucket Search(Event source)
        {
            return List.Where(evb => evb.Submitted.Name == source.Name && evb.Submitted.UniqId == source.UniqId).FirstOrDefault();
        }

        public static bool TrySearch(Event source, out EventBucket response)
        {
            response = Search(source);
            return response is not null;
        }

        public void Append(SocketPlugin plugin, Event modifiedEvent)
        {
            if (modifiedEvent.Name != Submitted.Name)
                return;

            if (modifiedEvent.UniqId != Submitted.UniqId)
                return;

            Response.TryAdd(plugin, modifiedEvent);
        }

        public Event Execute()
        {
            if (!IsCompleted) 
                return null;

            if (AsyncTask is null)
                return null;

            AsyncTask.Wait();
            return AsyncTask.Result;
        }

        private async Task<Event> TickAction()
        {
            while (!IsCompleted)
            {
                if (Response.Where(kvp => kvp.Value is null).Count() == 0)
                {
                    IsCompleted = true;
                    break;
                }

                if (Counter > Timeout)
                {
                    break;
                }

                Counter += Tick;
                await Task.Delay(Tick);
            }

            return Evaluate();
        }

        private Event Evaluate()
        {
            foreach (KeyValuePair<SocketPlugin, Event> Data in Response.Where(kvp => kvp.Value is not null).OrderBy(kvp => kvp.Key.Priority))
            {
                foreach (KeyValuePair<string, object> Element in Data.Value.Data.Where(kvp => kvp.Value is not null && Submitted.Data[kvp.Key] is not null))
                {
                    if (!Submitted.Data[Element.Key].Equals(Element.Value) && Submitted.Data[Element.Key].GetType() == Element.Value.GetType())
                    {
                        // Edit the thing
                        Submitted.Data[Element.Key] = Element.Value;
                    }
                }
            }

            List.Remove(this);
            return Submitted;
        }
    }
}
