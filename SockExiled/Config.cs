using Exiled.API.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SockExiled
{
    internal class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;

        public bool Debug { get; set; } = false;

        public string Ip { get; set; } = "127.0.0.1";

        public int Port { get; set; } = 7778;

        public int SyncTime { get; set; } = 5;

        public List<string> Keys { get; set; } = new();
    }
}
