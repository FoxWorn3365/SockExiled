using SockExiled.API.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SockExiled.API.Core
{
    /// <summary>
    /// Handle garbage collectors and Exiled.API.Features.X syncronization to the clients
    /// </summary>
    internal class SyncManager
    {
        private static Task GarbageCollector { get; set; }

        private static Task SyncHandlerTask { get; set; }

        private const int CollectorTickTime = 500;

        public static void Init()
        {
            GarbageCollector = Task.Run(GarbageHandler);
            SyncHandlerTask = Task.Run(SyncHandler);
        }

        private static async void GarbageHandler()
        {
            while (true)
            {
                Plugin.Server.GarbageCollectorAction();

                await Task.Delay(CollectorTickTime);
            }
        }

        private static async void SyncHandler()
        {
            while (true)
            {
                // Load players
                PlayerStorage.Populate();

                await Task.Delay(Plugin.Instance.Config.SyncTime * 1000);
            }
        }
    }
}
