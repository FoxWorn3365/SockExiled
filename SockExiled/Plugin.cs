using Exiled.API.Enums;
using Exiled.API.Features;
using SockExiled.API.Features.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayerEvent = Exiled.Events.Handlers.Player;
using MapEvent = Exiled.Events.Handlers.Map;
using SockExiled.API.Core;

namespace SockExiled
{
    internal class Plugin : Plugin<Config>
    {
        public override string Name => "SockExiled";

        public override string Author => "FoxWorn3365";

        public override string Prefix => "SockExiled";

        public override Version Version => new(0, 1, 2);

        public override Version RequiredExiledVersion => new(8, 8, 1);

        public override PluginPriority Priority => PluginPriority.High;

        internal static Plugin Instance;

        internal Handler Handler;

        internal static SocketServer Server;

        public override void OnEnabled()
        {
            Instance = this;
            Handler = new();

            Server = new(Config.Port);

            SyncManager.Init();
            // TimeCache.Init();

            // Player event
            PlayerEvent.Verified += Handler.Event;
            PlayerEvent.Spawning += Handler.Event;
            PlayerEvent.Spawned += Handler.Event;
            PlayerEvent.TriggeringTesla += Handler.Event;

            // Map events
            MapEvent.AnnouncingDecontamination += Handler.Event;
            MapEvent.AnnouncingNtfEntrance += Handler.Event;
            MapEvent.AnnouncingScpTermination += Handler.Event;
            MapEvent.ChangedIntoGrenade += Handler.Event;
            MapEvent.Decontaminating += Handler.Event;
            MapEvent.ExplodingGrenade += Handler.Event;
            MapEvent.Generated += Handler.MapGeneratedEvent;
            MapEvent.GeneratorActivating += Handler.Event;
            MapEvent.PickupDestroyed += Handler.Event;
            MapEvent.PlacingBlood += Handler.Event;
            MapEvent.PlacingBulletHole += Handler.Event;
            MapEvent.SpawningTeamVehicle += Handler.Event;
            MapEvent.TurningOffLights += Handler.Event;

            base.OnEnabled();
        }
    }
}
