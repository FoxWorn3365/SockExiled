using Exiled.API.Enums;
using Exiled.API.Features;
using SockExiled.API.Features.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayerEvent = Exiled.Events.Handlers.Player;

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

            PlayerEvent.Spawning += Handler.Event;
            PlayerEvent.Spawned += Handler.Event;


            base.OnEnabled();
        }
    }
}
