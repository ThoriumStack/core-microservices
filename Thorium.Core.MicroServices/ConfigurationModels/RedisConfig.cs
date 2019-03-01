namespace Thorium.Core.MicroServices.ConfigurationModels
{
    public class RedisConfig
    {
        public string Url { get; set; }
        public string[] Keys { get; set; }
        public bool Upload { get; set; }
        public int PollingInterval { get; set; }
    }
}