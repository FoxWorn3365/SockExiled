using Exiled.API.Enums;
using Exiled.API.Features;
using SockExiled.API.Features.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static Plugin Instance;

        public static SocketServer Server;

        public override void OnEnabled()
        {
            Instance = this;

            Server = new(7778);


            base.OnEnabled();
        }
    }
}
