using System.Collections.Generic;

namespace MyBucks.Core.MicroServices.Redis
{
    public sealed class ConfigQueryResult
    {
        public int Version { get; set; }

        public bool Exists { get; set; }

        public Dictionary<string, string> Value { get; set; }
    }
}