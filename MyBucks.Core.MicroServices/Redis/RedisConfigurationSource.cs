using System;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using MyBucks.Core.MicroServices.Abstractions;
using MyBucks.Core.MicroServices.ConfigurationModels;

namespace MyBucks.Core.MicroServices.Redis
{
    public class RedisConfigurationSource : JsonConfigurationSource, IRedisConfigurationSource
    {
        public RedisConfigurationSource(CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
        }
        
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            this.EnsureDefaults(builder);
            var client = new RedisConfigurationClient(this);
            return new RedisConfigurationProvider(this, client);
        }
        
        public RedisConfig RedisConfig { get; set; }
        
        public CancellationToken CancellationToken { get; set; }
        
        public Action<RedisWatchExceptionContext> OnWatchException { get; set; }
        
        public Action OnReload { get; set; }
    }
}